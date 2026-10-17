using System.Windows.Input;
using CentroMedico.Core.Modelos;

namespace CentroMedico.ViewModels;

public class CitasViewModel : ViewModelBase
{
    private readonly Contexto _c;
    private DateTime _fechaAgenda;
    private int _medicoFiltro;
    private IReadOnlyList<CitaFila> _citas = [];
    private CitaFila? _citaSeleccionada;

    private string _buscarPaciente = "";
    private IReadOnlyList<Opcion<int>> _pacientesEncontrados = [];
    private int? _pacienteId;
    private int? _medicoId;
    private int? _servicioId;
    private DateTime? _fechaCita;
    private IReadOnlyList<Opcion<TimeOnly>> _horarios = [];
    private TimeOnly? _horario;

    public const int TodosLosMedicos = 0;

    public CitasViewModel(Contexto contexto)
    {
        _c = contexto;
        var medicos = _c.Empleados.ListarMedicos()
            .Select(m => new Opcion<int>(m.Id, $"{m.NombreCompleto} ({m.Especialidad})"))
            .ToList();
        Medicos = medicos;
        FiltroMedicos = [new Opcion<int>(TodosLosMedicos, "Todos los médicos"), .. medicos];
        Servicios = _c.Servicios.Listar(soloActivos: true).Select(s => new Opcion<int>(s.Id, s.Nombre)).ToList();

        ReservarCommand = Comando(Reservar, () => PacienteId is not null && MedicoId is not null && ServicioId is not null && Horario is not null);
        AtenderCommand = Comando(() => CambiarEstado(EstadoCita.Atendida, "Cita marcada como atendida."), PuedeCambiarEstado);
        NoAsistioCommand = Comando(() => CambiarEstado(EstadoCita.NoAsistio, "Se registró que el paciente no asistió."), PuedeCambiarEstado);
        CancelarCommand = Comando(Cancelar, PuedeCambiarEstado);

        _fechaAgenda = _c.Ahora.Date;
        _fechaCita = _c.Ahora.Date;
        CargarAgenda();
        BuscarPacientes();
    }

    // Agenda

    public DateTime FechaAgenda
    {
        get => _fechaAgenda;
        set { if (Set(ref _fechaAgenda, value.Date)) CargarAgenda(); }
    }

    public IReadOnlyList<Opcion<int>> FiltroMedicos { get; }

    public int MedicoFiltro
    {
        get => _medicoFiltro;
        set { if (Set(ref _medicoFiltro, value)) CargarAgenda(); }
    }

    public IReadOnlyList<CitaFila> Citas
    {
        get => _citas;
        private set => Set(ref _citas, value);
    }

    public CitaFila? CitaSeleccionada
    {
        get => _citaSeleccionada;
        set => Set(ref _citaSeleccionada, value);
    }

    public bool AgendaVacia => Citas.Count == 0;

    // Reserva

    public string BuscarPaciente
    {
        get => _buscarPaciente;
        set { if (Set(ref _buscarPaciente, value)) BuscarPacientes(); }
    }

    public IReadOnlyList<Opcion<int>> PacientesEncontrados
    {
        get => _pacientesEncontrados;
        private set => Set(ref _pacientesEncontrados, value);
    }

    public int? PacienteId { get => _pacienteId; set => Set(ref _pacienteId, value); }

    public IReadOnlyList<Opcion<int>> Medicos { get; }

    public int? MedicoId
    {
        get => _medicoId;
        set { if (Set(ref _medicoId, value)) CargarHorarios(); }
    }

    public IReadOnlyList<Opcion<int>> Servicios { get; }

    public int? ServicioId { get => _servicioId; set => Set(ref _servicioId, value); }

    public DateTime? FechaCita
    {
        get => _fechaCita;
        set { if (Set(ref _fechaCita, value)) CargarHorarios(); }
    }

    public DateTime FechaMinima => _c.Ahora.Date;

    public IReadOnlyList<Opcion<TimeOnly>> Horarios
    {
        get => _horarios;
        private set => Set(ref _horarios, value);
    }

    public TimeOnly? Horario { get => _horario; set => Set(ref _horario, value); }

    public string AvisoHorarios =>
        MedicoId is null || FechaCita is null ? "Elija médico y fecha para ver los turnos libres."
        : Horarios.Count == 0 ? "No hay turnos libres ese día."
        : $"{Horarios.Count} turnos libres de 30 minutos.";

    public ICommand ReservarCommand { get; }
    public ICommand AtenderCommand { get; }
    public ICommand NoAsistioCommand { get; }
    public ICommand CancelarCommand { get; }

    private void CargarAgenda()
    {
        var fecha = DateOnly.FromDateTime(FechaAgenda);
        var citas = MedicoFiltro == TodosLosMedicos
            ? _c.Citas.ListarPorFecha(fecha)
            : _c.Citas.ListarPorMedicoYFecha(MedicoFiltro, fecha);
        Citas = InicioViewModel.Filas(_c, citas);
        OnPropertyChanged(nameof(AgendaVacia));
    }

    private void BuscarPacientes()
    {
        PacientesEncontrados = _c.Pacientes.Listar(BuscarPaciente)
            .Where(p => p.Activo)
            .Take(30)
            .Select(p => new Opcion<int>(p.Id, $"{p.Cedula}  {p.Apellidos} {p.Nombres}"))
            .ToList();
        // Con un único resultado (por ejemplo, la cédula completa) se elige solo.
        if (PacientesEncontrados.Count == 1) PacienteId = PacientesEncontrados[0].Valor;
        else if (PacientesEncontrados.All(p => p.Valor != PacienteId)) PacienteId = null;
    }

    private void CargarHorarios()
    {
        Horarios = MedicoId is { } medico && FechaCita is { } fecha
            ? _c.Agenda.HorariosDisponibles(medico, DateOnly.FromDateTime(fecha))
                .Select(h => new Opcion<TimeOnly>(h, h.ToString("HH:mm"))).ToList()
            : [];
        Horario = null;
        OnPropertyChanged(nameof(AvisoHorarios));
    }

    private void Reservar()
    {
        var fechaHora = DateOnly.FromDateTime(FechaCita!.Value).ToDateTime(Horario!.Value);
        string medico = Medicos.First(m => m.Valor == MedicoId).Texto;

        bool listo = Intentar(
            () => _c.Agenda.Reservar(PacienteId!.Value, MedicoId!.Value, ServicioId!.Value, fechaHora),
            $"Cita reservada el {fechaHora.ToString("dddd d 'de' MMMM 'a las' HH:mm", Formatos.Cultura)} con {medico}.");
        if (!listo) return;

        string? mensaje = Mensaje;
        _fechaAgenda = fechaHora.Date;
        OnPropertyChanged(nameof(FechaAgenda));
        CargarAgenda();
        CargarHorarios();
        Mensaje = mensaje;
    }

    private bool PuedeCambiarEstado() => CitaSeleccionada?.Cita.Estado == EstadoCita.Reservada;

    private void Cancelar()
    {
        var fila = CitaSeleccionada!;
        if (!_c.Dialogos.Confirmar($"¿Cancelar la cita de {fila.Paciente} a las {fila.Hora}?")) return;
        CambiarEstado(EstadoCita.Cancelada, "Cita cancelada; el turno quedó libre.");
    }

    private void CambiarEstado(EstadoCita estado, string mensaje)
    {
        int id = CitaSeleccionada!.Cita.Id;
        if (!Intentar(() => _c.Agenda.CambiarEstado(id, estado), mensaje)) return;

        string? texto = Mensaje;
        CargarAgenda();
        CargarHorarios();
        CitaSeleccionada = Citas.FirstOrDefault(c => c.Cita.Id == id);
        Mensaje = texto;
    }
}
