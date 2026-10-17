using System.IO;
using System.Text.Json;

namespace CentroMedico.App;

internal static class Configuracion
{
    public const string Variable = "CENTROMEDICO_ORACLE";

    /// <summary>Cadena de conexión a Oracle, o null para el modo demo.</summary>
    public static string? CadenaOracle()
    {
        var cadena = Environment.GetEnvironmentVariable(Variable);
        if (string.IsNullOrWhiteSpace(cadena))
        {
            var ruta = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            if (File.Exists(ruta))
            {
                using var json = JsonDocument.Parse(File.ReadAllText(ruta));
                if (json.RootElement.TryGetProperty("Oracle", out var valor))
                    cadena = valor.GetString();
            }
        }
        return string.IsNullOrWhiteSpace(cadena) ? null : cadena;
    }
}
