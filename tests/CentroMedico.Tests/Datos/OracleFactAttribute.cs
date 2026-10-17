namespace CentroMedico.Tests.Datos;

/// <summary>
/// Prueba que necesita una base Oracle real. Se salta si no está definida la variable
/// CENTROMEDICO_ORACLE_PRUEBAS. Usar un esquema propio: cada prueba vacía las tablas.
/// </summary>
public sealed class OracleFactAttribute : FactAttribute
{
    public const string Variable = "CENTROMEDICO_ORACLE_PRUEBAS";

    public static string? Cadena => Environment.GetEnvironmentVariable(Variable);

    public OracleFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Cadena))
            Skip = $"Defina {Variable} para correr las pruebas contra Oracle.";
    }
}
