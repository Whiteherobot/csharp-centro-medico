using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CentroMedico.App.Converters;

public class NegarConverter : IValueConverter
{
    public object Convert(object valor, Type tipo, object? parametro, CultureInfo cultura) => valor is not true;
    public object ConvertBack(object valor, Type tipo, object? parametro, CultureInfo cultura) => valor is not true;
}

/// <summary>Visible cuando el valor es false (lo contrario de BooleanToVisibilityConverter).</summary>
public class OcultarSiVerdaderoConverter : IValueConverter
{
    public object Convert(object valor, Type tipo, object? parametro, CultureInfo cultura) =>
        valor is true ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object valor, Type tipo, object? parametro, CultureInfo cultura) =>
        throw new NotSupportedException();
}
