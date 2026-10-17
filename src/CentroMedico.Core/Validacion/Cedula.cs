namespace CentroMedico.Core.Validacion;

/// <summary>Cédula de identidad ecuatoriana (10 dígitos, verificador módulo 10).</summary>
public static class Cedula
{
    public static bool EsValida(string? cedula)
    {
        if (cedula is null || cedula.Length != 10 || !cedula.All(char.IsAsciiDigit))
            return false;

        int provincia = int.Parse(cedula[..2]);
        if (provincia is < 1 or > 24 && provincia != 30)
            return false;

        // El tercer dígito menor a 6 identifica a personas naturales.
        if (cedula[2] - '0' >= 6)
            return false;

        int suma = 0;
        for (int i = 0; i < 9; i++)
        {
            int digito = cedula[i] - '0';
            if (i % 2 == 0)
            {
                digito *= 2;
                if (digito > 9) digito -= 9;
            }
            suma += digito;
        }

        int verificador = (10 - suma % 10) % 10;
        return verificador == cedula[9] - '0';
    }
}
