namespace CentroMedico.Core.Modelos;

public class Servicio
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public decimal Precio { get; set; }

    /// <summary>
    /// Los servicios de salud tienen tarifa 0 % de IVA en Ecuador; solo algunos
    /// (certificados, insumos, estética) gravan la tarifa general.
    /// </summary>
    public bool GravaIva { get; set; }

    public int DuracionMinutos { get; set; } = 30;
    public bool Activo { get; set; } = true;
}
