using System.Globalization;

namespace CentroMedico.ViewModels;

public static class Formatos
{
    public static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-EC");

    /// <summary>Ecuador usa dólares; se muestra "$ 1.234,50" con la convención local.</summary>
    public static string Dinero(decimal valor) => valor.ToString("$ #,##0.00", Cultura);
}
