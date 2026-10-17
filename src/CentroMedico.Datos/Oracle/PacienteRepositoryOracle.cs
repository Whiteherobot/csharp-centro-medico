using System.Data;
using CentroMedico.Core.Modelos;
using CentroMedico.Core.Repositorios;

namespace CentroMedico.Datos.Oracle;

public class PacienteRepositoryOracle(ConexionOracle db) : IPacienteRepository
{
    private const string Select = $"""
        SELECT pac.pac_id, pac.pac_fecha_nacimiento, pac.pac_activo, {PersonasOracle.Columnas}
          FROM idm_pacientes pac
          JOIN idm_personas per ON per.per_id = pac.idm_personas_per_id
        """;

    public IReadOnlyList<Paciente> Listar(string? filtro = null)
    {
        using var cx = db.Abrir();
        using var c = ConexionOracle.Comando(cx, $"""
            {Select}
             WHERE :filtro IS NULL
                OR per.per_cedula LIKE :filtro || '%'
                OR UPPER(per.per_nombre || ' ' || per.per_apellido) LIKE '%' || UPPER(:filtro) || '%'
             ORDER BY per.per_apellido, per.per_nombre
            """).Con("filtro", filtro?.Trim());
        return c.Leer(Mapear);
    }

    public Paciente? ObtenerPorId(int id) => Uno("pac.pac_id = :valor", id);

    public Paciente? ObtenerPorCedula(string cedula) => Uno("per.per_cedula = :valor", cedula);

    public void Agregar(Paciente p)
    {
        using var cx = db.Abrir();
        using var tx = cx.BeginTransaction();
        ErroresOracle.Traducir(() =>
        {
            int perId = PersonasOracle.Guardar(cx, tx, p);
            using var c = ConexionOracle.Comando(cx, """
                INSERT INTO idm_pacientes (idm_personas_per_id, pac_fecha_nacimiento, pac_activo)
                VALUES (:perId, :nacimiento, :activo)
                RETURNING pac_id INTO :id
                """, tx).Con("perId", perId).Con("nacimiento", p.FechaNacimiento).Con("activo", p.Activo);
            p.Id = c.InsertarRetornandoId();
        });
        tx.Commit();
    }

    public void Actualizar(Paciente p)
    {
        using var cx = db.Abrir();
        using var tx = cx.BeginTransaction();
        ErroresOracle.Traducir(() =>
        {
            using var c = ConexionOracle.Comando(cx, """
                UPDATE idm_pacientes SET pac_fecha_nacimiento = :nacimiento, pac_activo = :activo
                 WHERE pac_id = :id
                RETURNING idm_personas_per_id INTO :perId
                """, tx).Con("nacimiento", p.FechaNacimiento).Con("activo", p.Activo).Con("id", p.Id);
            var perId = c.SalidaEntera("perId");
            if (c.ExecuteNonQuery() == 0) throw new KeyNotFoundException("No existe el paciente.");
            PersonasOracle.Actualizar(cx, tx, perId.ValorEntero(), p);
        });
        tx.Commit();
    }

    private Paciente? Uno(string condicion, object valor)
    {
        using var cx = db.Abrir();
        using var c = ConexionOracle.Comando(cx, $"{Select} WHERE {condicion}").Con("valor", valor);
        return c.Leer(Mapear).FirstOrDefault();
    }

    private static Paciente Mapear(IDataRecord r)
    {
        var p = new Paciente
        {
            Id = r.Entero("pac_id"),
            Activo = r.Booleano("pac_activo"),
            FechaNacimiento = r["pac_fecha_nacimiento"] is DateTime d ? DateOnly.FromDateTime(d) : null
        };
        PersonasOracle.Mapear(r, p);
        return p;
    }
}
