using System.Globalization;
using System.Windows.Data;

namespace CentroMedico.App.Converters;

/// <summary>True si los dos valores enlazados son el mismo objeto (marca la opción activa del menú).</summary>
public class IgualConverter : IMultiValueConverter
{
    public object Convert(object[] valores, Type tipo, object? parametro, CultureInfo cultura) =>
        valores.Length == 2 && ReferenceEquals(valores[0], valores[1]);

    public object[] ConvertBack(object valor, Type[] tipos, object? parametro, CultureInfo cultura) =>
        throw new NotSupportedException();
}
