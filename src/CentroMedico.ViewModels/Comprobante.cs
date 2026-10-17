using System.Text;
using CentroMedico.Core.Modelos;
using CentroMedico.Core.Servicios;

namespace CentroMedico.ViewModels;

/// <summary>Comprobante en texto plano con columnas alineadas, para imprimir o adjuntar.</summary>
public static class Comprobante
{
    private const int Ancho = 64;

    public static string Generar(Factura f, Paciente paciente)
    {
        var sb = new StringBuilder();
        sb.AppendLine("CENTRO MÉDICO");
        sb.AppendLine($"Factura {f.Numero}");
        sb.AppendLine($"Fecha:    {f.Fecha.ToString("dd/MM/yyyy HH:mm", Formatos.Cultura)}");
        sb.AppendLine($"Paciente: {paciente.NombreCompleto}");
        sb.AppendLine($"Cédula:   {paciente.Cedula}");
        sb.AppendLine(new string('-', Ancho));
        sb.AppendLine($"{"Cant.",-6}{"Descripción",-34}{"P. unit.",11}{"Subtotal",13}");
        foreach (var d in f.Detalles)
        {
            string descripcion = d.Descripcion + (d.GravaIva ? " *" : "");
            if (descripcion.Length > 33) descripcion = descripcion[..32] + "…";
            sb.AppendLine($"{d.Cantidad,-6}{descripcion,-34}{Dinero(d.PrecioUnitario),11}{Dinero(d.Subtotal),13}");
        }
        sb.AppendLine(new string('-', Ancho));
        Total(sb, "Subtotal 0 %", f.SubtotalTarifa0);
        string tarifa = $"{FacturacionService.TarifaIva * 100:0} %";
        Total(sb, $"Subtotal {tarifa}", f.SubtotalGravado);
        Total(sb, $"IVA {tarifa}", f.Iva);
        Total(sb, "TOTAL", f.Total);
        if (f.Detalles.Any(d => d.GravaIva))
        {
            sb.AppendLine();
            sb.AppendLine("* Grava IVA. Los servicios de salud tienen tarifa 0 %.");
        }
        return sb.ToString();
    }

    private static void Total(StringBuilder sb, string etiqueta, decimal valor) =>
        sb.AppendLine($"{etiqueta,51}{Dinero(valor),13}");

    private static string Dinero(decimal valor) => valor.ToString("#,##0.00", Formatos.Cultura);
}
