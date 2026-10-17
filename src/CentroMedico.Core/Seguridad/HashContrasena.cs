using System.Security.Cryptography;

namespace CentroMedico.Core.Seguridad;

/// <summary>
/// PBKDF2-SHA256 con sal aleatoria. Se guarda como "pbkdf2$iteraciones$sal$hash"
/// (Base64) para poder subir las iteraciones más adelante sin romper las cuentas viejas.
/// </summary>
public static class HashContrasena
{
    private const int Iteraciones = 210_000;
    private const int BytesSal = 16;
    private const int BytesHash = 32;

    public static string Generar(string contrasena)
    {
        byte[] sal = RandomNumberGenerator.GetBytes(BytesSal);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(contrasena, sal, Iteraciones, HashAlgorithmName.SHA256, BytesHash);
        return $"pbkdf2${Iteraciones}${Convert.ToBase64String(sal)}${Convert.ToBase64String(hash)}";
    }

    public static bool Verificar(string contrasena, string guardado)
    {
        var partes = guardado.Split('$');
        if (partes.Length != 4 || partes[0] != "pbkdf2" || !int.TryParse(partes[1], out int iteraciones))
            return false;

        try
        {
            byte[] sal = Convert.FromBase64String(partes[2]);
            byte[] esperado = Convert.FromBase64String(partes[3]);
            byte[] calculado = Rfc2898DeriveBytes.Pbkdf2(contrasena, sal, iteraciones, HashAlgorithmName.SHA256, esperado.Length);
            return CryptographicOperations.FixedTimeEquals(calculado, esperado);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
