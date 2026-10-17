namespace CentroMedico.Core.Modelos;

public class Factura
{
    public int Id { get; set; }

    /// <summary>Número con el formato del SRI: establecimiento-punto-secuencial.</summary>
    public string Numero { get; set; } = "";

    public DateTime Fecha { get; set; }
    public int PacienteId { get; set; }
    public List<DetalleFactura> Detalles { get; set; } = [];

    public decimal SubtotalTarifa0 { get; set; }
    public decimal SubtotalGravado { get; set; }
    public decimal Iva { get; set; }
    public decimal Total { get; set; }
}

public class DetalleFactura
{
    public int ServicioId { get; set; }
    public string Descripcion { get; set; } = "";
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public bool GravaIva { get; set; }

    /// <summary>Cita que origina la línea (null si es un servicio suelto).</summary>
    public int? CitaId { get; set; }

    public decimal Subtotal => Cantidad * PrecioUnitario;
}
