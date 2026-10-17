namespace CentroMedico.Core.Modelos;

public enum EstadoCita
{
    Reservada,
    Atendida,
    Cancelada,
    NoAsistio
}

public class Cita
{
    public int Id { get; set; }
    public int PacienteId { get; set; }
    public int MedicoId { get; set; }
    public int ServicioId { get; set; }
    public DateTime FechaHora { get; set; }
    public EstadoCita Estado { get; set; } = EstadoCita.Reservada;

    /// <summary>Factura en la que se cobró la cita, si ya se facturó.</summary>
    public int? FacturaId { get; set; }

    public bool PendienteDeFacturar => Estado == EstadoCita.Atendida && FacturaId is null;
}
