using System.Windows.Input;
using CentroMedico.Core.Modelos;

namespace CentroMedico.ViewModels;

public record PacienteFila(Paciente Paciente, int? Edad)
{
    public string Cedula => Paciente.Cedula;
    public string Nombre => $"{Paciente.Apellidos} {Paciente.Nombres}";
    public string Telefono => Paciente.Telefono;
    public string EdadTexto => Edad is { } e ? $"{e} años" : "";
    public string Estado => Paciente.Activo ? "Activo" : "Inactivo";
}

public class PacientesViewModel : ViewModelBase
{
    private readonly Contexto _c;
    private string _filtro = "";
    private IReadOnlyList<PacienteFila> _pacientes = [];
    private PacienteFila? _seleccionado;
    private DateTime? _fechaNacimiento;
    private IReadOnlyList<CitaFila> _historial = [];

    public PacientesViewModel(Contexto contexto)
    {
        _c = contexto;
        NuevoCommand = Comando(Nuevo);
        GuardarCommand = Comando(Guardar);
        Buscar();
    }

    public DatosPersona Datos { get; } = new();

    public string Filtro
    {
        get => _filtro;
        set { if (Set(ref _filtro, value)) Buscar(); }
    }

    public IReadOnlyList<PacienteFila> Pacientes
    {
        get => _pacientes;
        private set => Set(ref _pacientes, value);
    }

    public PacienteFila? Seleccionado
    {
        get => _seleccionado;
        set
        {
            if (!Set(ref _seleccionado, value)) return;
            Error = Mensaje = null;
            Cargar(value?.Paciente);
            OnPropertyChanged(nameof(EsNuevo));
            OnPropertyChanged(nameof(TituloFormulario));
        }
    }

    /// <summary>El DatePicker trabaja con DateTime; el modelo guarda DateOnly.</summary>
    public DateTime? FechaNacimiento
    {
        get => _fechaNacimiento;
        set => Set(ref _fechaNacimiento, value);
    }

    public IReadOnlyList<CitaFila> Historial
    {
        get => _historial;
        private set => Set(ref _historial, value);
    }

    public bool EsNuevo => Seleccionado is null;
    public string TituloFormulario => EsNuevo ? "Nuevo paciente" : "Editar paciente";
    public DateTime FechaMaxima => _c.Ahora.Date;

    public ICommand NuevoCommand { get; }
    public ICommand GuardarCommand { get; }

    private void Buscar()
    {
        var hoy = _c.Hoy;
        Pacientes = _c.Pacientes.Listar(Filtro).Select(p => new PacienteFila(p, p.Edad(hoy))).ToList();
    }

    private void Nuevo()
    {
        Seleccionado = null;
        Cargar(null);
    }

    private void Cargar(Paciente? p)
    {
        Datos.Cargar(p);
        FechaNacimiento = p?.FechaNacimiento?.ToDateTime(TimeOnly.MinValue);
        Historial = p is null ? [] : InicioViewModel.Filas(_c, _c.Citas.ListarPorPaciente(p.Id).Reverse());
    }

    private void Guardar()
    {
        // Se edita una copia fresca: si la validación falla, la fila de la tabla no queda a medias.
        var paciente = Seleccionado is { } fila ? _c.Pacientes.ObtenerPorId(fila.Paciente.Id) ?? new Paciente() : new Paciente();
        bool eraNuevo = paciente.Id == 0;

        bool listo = Intentar(() =>
        {
            if (FechaNacimiento is { } f && f.Date > _c.Ahora.Date)
                throw new Core.Servicios.ReglaNegocioException("La fecha de nacimiento no puede ser futura.");
            Datos.Aplicar(paciente);
            paciente.FechaNacimiento = FechaNacimiento is { } fecha ? DateOnly.FromDateTime(fecha) : null;
            _c.Registro.GuardarPaciente(paciente);
        }, eraNuevo ? "Paciente registrado." : "Cambios guardados.");

        if (!listo) return;
        string mensaje = Mensaje!;
        Buscar();
        Seleccionado = Pacientes.FirstOrDefault(f => f.Paciente.Id == paciente.Id);
        Mensaje = mensaje;
    }
}
