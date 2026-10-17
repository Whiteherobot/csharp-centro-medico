using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace CentroMedico.Datos.Oracle;

public class ConexionOracle(string cadena)
{
    public OracleConnection Abrir()
    {
        var conexion = new OracleConnection(cadena);
        conexion.Open();
        return conexion;
    }

    /// <summary>
    /// ODP.NET enlaza los parámetros por posición si no se indica lo contrario;
    /// con BindByName el orden en que se agregan deja de importar.
    /// </summary>
    public static OracleCommand Comando(OracleConnection conexion, string sql, OracleTransaction? transaccion = null)
    {
        var comando = conexion.CreateCommand();
        comando.CommandText = sql;
        comando.BindByName = true;
        comando.Transaction = transaccion;
        return comando;
    }
}

internal static class OracleExtensiones
{
    public static OracleCommand Con(this OracleCommand comando, string nombre, object? valor)
    {
        comando.Parameters.Add(new OracleParameter(nombre, valor switch
        {
            null => DBNull.Value,
            bool b => b ? 1 : 0,
            Enum e => e.ToString(),
            DateOnly d => d.ToDateTime(TimeOnly.MinValue),
            string s when s.Length == 0 => DBNull.Value,
            _ => valor
        }));
        return comando;
    }

    public static OracleParameter SalidaEntera(this OracleCommand comando, string nombre)
    {
        var parametro = new OracleParameter(nombre, OracleDbType.Int32, ParameterDirection.Output);
        comando.Parameters.Add(parametro);
        return parametro;
    }

    public static int ValorEntero(this OracleParameter parametro) => Convert.ToInt32(parametro.Value.ToString());

    /// <summary>Ejecuta un INSERT ... RETURNING id INTO :id y devuelve el id generado.</summary>
    public static int InsertarRetornandoId(this OracleCommand comando)
    {
        var id = comando.SalidaEntera("id");
        comando.ExecuteNonQuery();
        return id.ValorEntero();
    }

    public static List<T> Leer<T>(this OracleCommand comando, Func<IDataRecord, T> mapear)
    {
        using var lector = comando.ExecuteReader();
        var lista = new List<T>();
        while (lector.Read()) lista.Add(mapear(lector));
        return lista;
    }

    public static string Texto(this IDataRecord r, string columna) =>
        r[columna] is DBNull ? "" : ((string)r[columna]).TrimEnd();

    public static string? TextoONulo(this IDataRecord r, string columna) =>
        r[columna] is DBNull ? null : (string)r[columna];

    public static int Entero(this IDataRecord r, string columna) => Convert.ToInt32(r[columna]);

    public static int? EnteroONulo(this IDataRecord r, string columna) =>
        r[columna] is DBNull ? null : Convert.ToInt32(r[columna]);

    public static decimal Decimal(this IDataRecord r, string columna) => Convert.ToDecimal(r[columna]);

    public static bool Booleano(this IDataRecord r, string columna) => Convert.ToInt32(r[columna]) == 1;

    public static T Enum<T>(this IDataRecord r, string columna) where T : struct, Enum =>
        System.Enum.Parse<T>((string)r[columna]);
}
