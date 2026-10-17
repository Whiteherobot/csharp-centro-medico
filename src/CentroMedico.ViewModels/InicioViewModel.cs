using CentroMedico.Core.Modelos;

namespace CentroMedico.ViewModels;

/// <summary>Fila de la agenda con los nombres ya resueltos para mostrar.</summary>
public record CitaFila(Cita Cita, string Paciente, string Medico, string Servicio)
{
    public string Hora => Cita.FechaHora.ToString("HH:mm");
    public string Fecha => Cita.FechaHora.ToString("dd/MM/yyyy");
    public string Estado => Cita.Estado switch
    {
        EstadoCita.Reservada => "Reservada",
        EstadoCita.Atendida => Cita.FacturaId is null ? "Atendida (por facturar)" : "Atendida",
        EstadoCita.Cancelada => "Cancelada",
        EstadoCita.NoAsistio => "No asistió",
        _ => Cita.Estado.ToString()
    };
}

public class InicioViewModel : ViewModelBase
{
    public InicioViewModel(Contexto c)
    {
        Fecha = c.Hoy.ToString("dddd d 'de' MMMM 'de' yyyy", Formatos.Cultura);
        var citas = c.Citas.ListarPorFecha(c.Hoy);
        CitasDeHoy = Filas(c, citas);

        Reservadas = citas.Count(x => x.Estado == EstadoCita.Reservada);
        Atendidas = citas.Count(x => x.Estado == EstadoCita.Atendida);
        PacientesActivos = c.Pacientes.Listar().Count(p => p.Activo);
        FacturadoHoy = c.Facturas.Listar()
            .Where(f => DateOnly.FromDateTime(f.Fecha) == c.Hoy)
            .Sum(f => f.Total);
    }

    public string Fecha { get; }
    public IReadOnlyList<CitaFila> CitasDeHoy { get; }
    public int Reservadas { get; }
    public int Atendidas { get; }
    public int PacientesActivos { get; }
    public decimal FacturadoHoy { get; }
    public string FacturadoHoyTexto => Formatos.Dinero(FacturadoHoy);
    public bool SinCitas => CitasDeHoy.Count == 0;

    internal static List<CitaFila> Filas(Contexto c, IEnumerable<Cita> citas)
    {
        // Pocas consultas: se cachean los nombres en lugar de buscar fila por fila.
        var pacientes = new Dictionary<int, string>();
        var medicos = new Dictionary<int, string>();
        var servicios = c.Servicios.Listar().ToDictionary(s => s.Id, s => s.Nombre);

        return citas.Select(cita => new CitaFila(cita,
            Nombre(pacientes, cita.PacienteId, id => c.Pacientes.ObtenerPorId(id)?.NombreCompleto),
            Nombre(medicos, cita.MedicoId, id => c.Empleados.ObtenerPorId(id)?.NombreCompleto),
            servicios.GetValueOrDefault(cita.ServicioId, "")))
            .ToList();
    }

    private static string Nombre(Dictionary<int, string> cache, int id, Func<int, string?> buscar)
    {
        if (!cache.TryGetValue(id, out var nombre))
            cache[id] = nombre = buscar(id) ?? "(no existe)";
        return nombre;
    }
}
