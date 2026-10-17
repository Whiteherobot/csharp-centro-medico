using CentroMedico.Core.Modelos;
using CentroMedico.Core.Repositorios;

namespace CentroMedico.Core.Servicios;

/// <summary>
/// Reserva de citas. Atención de lunes a sábado; entre semana de 08:00 a 17:00
/// con almuerzo de 13:00 a 14:00, y los sábados de 08:00 a 12:00. Turnos de 30 minutos.
/// </summary>
public class AgendaService(
    ICitaRepository citas,
    IPacienteRepository pacientes,
    IEmpleadoRepository empleados,
    IServicioRepository servicios,
    TimeProvider reloj)
{
    public static readonly TimeSpan DuracionTurno = TimeSpan.FromMinutes(30);

    private DateTime Ahora => reloj.GetLocalNow().DateTime;

    public IReadOnlyList<TimeOnly> HorariosDisponibles(int medicoId, DateOnly fecha)
    {
        var ocupados = citas.ListarPorMedicoYFecha(medicoId, fecha)
            .Where(c => c.Estado != EstadoCita.Cancelada)
            .Select(c => TimeOnly.FromDateTime(c.FechaHora))
            .ToHashSet();

        return Turnos(fecha)
            .Where(hora => !ocupados.Contains(hora) && fecha.ToDateTime(hora) > Ahora)
            .ToList();
    }

    public Cita Reservar(int pacienteId, int medicoId, int servicioId, DateTime fechaHora)
    {
        var paciente = pacientes.ObtenerPorId(pacienteId)
            ?? throw new ReglaNegocioException("El paciente no existe.");
        if (!paciente.Activo)
            throw new ReglaNegocioException("El paciente está desactivado.");

        var medico = empleados.ObtenerPorId(medicoId);
        if (medico is not { EsMedico: true, Activo: true })
            throw new ReglaNegocioException("Seleccione un médico activo.");

        var servicio = servicios.ObtenerPorId(servicioId);
        if (servicio is not { Activo: true })
            throw new ReglaNegocioException("Seleccione un servicio activo.");

        if (fechaHora <= Ahora)
            throw new ReglaNegocioException("No se puede reservar en una fecha u hora que ya pasó.");

        var fecha = DateOnly.FromDateTime(fechaHora);
        var hora = TimeOnly.FromDateTime(fechaHora);
        if (!Turnos(fecha).Contains(hora))
            throw new ReglaNegocioException("La hora está fuera del horario de atención.");

        if (!HorariosDisponibles(medicoId, fecha).Contains(hora))
            throw new ReglaNegocioException($"El médico ya tiene una cita a las {hora:HH\\:mm}.");

        bool pacienteOcupado = citas.ListarPorPaciente(pacienteId)
            .Any(c => c.Estado == EstadoCita.Reservada && c.FechaHora == fechaHora);
        if (pacienteOcupado)
            throw new ReglaNegocioException("El paciente ya tiene otra cita a esa hora.");

        var cita = new Cita
        {
            PacienteId = pacienteId,
            MedicoId = medicoId,
            ServicioId = servicioId,
            FechaHora = fechaHora
        };
        citas.Agregar(cita);
        return cita;
    }

    /// <summary>Solo una cita reservada puede pasar a atendida, cancelada o no asistió.</summary>
    public void CambiarEstado(int citaId, EstadoCita nuevo)
    {
        var cita = citas.ObtenerPorId(citaId)
            ?? throw new ReglaNegocioException("La cita no existe.");

        if (cita.Estado != EstadoCita.Reservada)
            throw new ReglaNegocioException($"La cita ya está {Texto(cita.Estado)} y no se puede cambiar.");
        if (nuevo == EstadoCita.Reservada)
            throw new ReglaNegocioException("La cita ya está reservada.");
        if (nuevo is EstadoCita.Atendida or EstadoCita.NoAsistio && cita.FechaHora > Ahora)
            throw new ReglaNegocioException("La cita todavía no ocurre.");

        cita.Estado = nuevo;
        citas.Actualizar(cita);
    }

    public static IEnumerable<TimeOnly> Turnos(DateOnly fecha)
    {
        var (inicio, fin) = fecha.DayOfWeek switch
        {
            DayOfWeek.Sunday => (TimeOnly.MinValue, TimeOnly.MinValue),
            DayOfWeek.Saturday => (new TimeOnly(8, 0), new TimeOnly(12, 0)),
            _ => (new TimeOnly(8, 0), new TimeOnly(17, 0))
        };

        for (var hora = inicio; hora < fin; hora = hora.Add(DuracionTurno))
        {
            if (hora.Hour == 13 && fecha.DayOfWeek != DayOfWeek.Saturday) continue;
            yield return hora;
        }
    }

    public static string Texto(EstadoCita estado) => estado switch
    {
        EstadoCita.Reservada => "reservada",
        EstadoCita.Atendida => "atendida",
        EstadoCita.Cancelada => "cancelada",
        EstadoCita.NoAsistio => "marcada como no asistió",
        _ => estado.ToString()
    };
}
