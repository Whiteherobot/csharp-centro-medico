using System.Data;
using CentroMedico.Core.Modelos;
using Oracle.ManagedDataAccess.Client;

namespace CentroMedico.Datos.Oracle;

/// <summary>
/// Lectura y guardado de idm_personas, compartido por pacientes y empleados.
/// Si la cédula ya está registrada (por ejemplo, un empleado que ahora es paciente)
/// se reutiliza la misma fila.
/// </summary>
internal static class PersonasOracle
{
    public const string Columnas =
        "per.per_id, per.per_cedula, per.per_nombre, per.per_apellido, per.per_direccion, per.per_telefono, per.per_email";

    public static void Mapear(IDataRecord r, Persona p)
    {
        p.Cedula = r.Texto("per_cedula");
        p.Nombres = r.Texto("per_nombre");
        p.Apellidos = r.Texto("per_apellido");
        p.Direccion = r.Texto("per_direccion");
        p.Telefono = r.Texto("per_telefono");
        p.Email = r.Texto("per_email");
    }

    public static int Guardar(OracleConnection cx, OracleTransaction tx, Persona p)
    {
        using var buscar = ConexionOracle.Comando(cx, "SELECT per_id FROM idm_personas WHERE per_cedula = :cedula", tx)
            .Con("cedula", p.Cedula);
        object? existente = buscar.ExecuteScalar();

        if (existente is null)
        {
            using var insertar = ConexionOracle.Comando(cx, """
                INSERT INTO idm_personas (per_cedula, per_nombre, per_apellido, per_direccion, per_telefono, per_email)
                VALUES (:cedula, :nombre, :apellido, :direccion, :telefono, :email)
                RETURNING per_id INTO :id
                """, tx);
            return ConDatos(insertar, p).InsertarRetornandoId();
        }

        int perId = Convert.ToInt32(existente);
        Actualizar(cx, tx, perId, p);
        return perId;
    }

    /// <summary>Cambia los datos de una persona ya vinculada (incluida la cédula).</summary>
    public static void Actualizar(OracleConnection cx, OracleTransaction tx, int perId, Persona p)
    {
        using var comando = ConexionOracle.Comando(cx, """
            UPDATE idm_personas
               SET per_cedula = :cedula, per_nombre = :nombre, per_apellido = :apellido,
                   per_direccion = :direccion, per_telefono = :telefono, per_email = :email
             WHERE per_id = :perId
            """, tx);
        ConDatos(comando, p).Con("perId", perId).ExecuteNonQuery();
    }

    private static OracleCommand ConDatos(OracleCommand c, Persona p) => c
        .Con("cedula", p.Cedula)
        .Con("nombre", p.Nombres)
        .Con("apellido", p.Apellidos)
        .Con("direccion", p.Direccion)
        .Con("telefono", p.Telefono)
        .Con("email", p.Email);
}
