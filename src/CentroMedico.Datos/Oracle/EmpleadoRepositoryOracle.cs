using System.Data;
using CentroMedico.Core.Modelos;
using CentroMedico.Core.Repositorios;

namespace CentroMedico.Datos.Oracle;

public class EmpleadoRepositoryOracle(ConexionOracle db) : IEmpleadoRepository
{
    private const string Select = $"""
        SELECT emp.emp_id, emp.emp_tipo, emp.emp_especialidad, emp.emp_activo, {PersonasOracle.Columnas}
          FROM idm_empleados emp
          JOIN idm_personas per ON per.per_id = emp.idm_personas_per_id
        """;

    public IReadOnlyList<Empleado> Listar(string? filtro = null)
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

    public IReadOnlyList<Empleado> ListarMedicos()
    {
        using var cx = db.Abrir();
        using var c = ConexionOracle.Comando(cx, $"""
            {Select}
             WHERE emp.emp_tipo = 'Medico' AND emp.emp_activo = 1
             ORDER BY per.per_apellido, per.per_nombre
            """);
        return c.Leer(Mapear);
    }

    public Empleado? ObtenerPorId(int id) => Uno("emp.emp_id = :valor", id);

    public Empleado? ObtenerPorCedula(string cedula) => Uno("per.per_cedula = :valor", cedula);

    public void Agregar(Empleado e)
    {
        using var cx = db.Abrir();
        using var tx = cx.BeginTransaction();
        ErroresOracle.Traducir(() =>
        {
            int perId = PersonasOracle.Guardar(cx, tx, e);
            using var c = ConexionOracle.Comando(cx, """
                INSERT INTO idm_empleados (idm_personas_per_id, emp_tipo, emp_especialidad, emp_activo)
                VALUES (:perId, :tipo, :especialidad, :activo)
                RETURNING emp_id INTO :id
                """, tx)
                .Con("perId", perId).Con("tipo", e.Cargo).Con("especialidad", e.Especialidad).Con("activo", e.Activo);
            e.Id = c.InsertarRetornandoId();
        });
        tx.Commit();
    }

    public void Actualizar(Empleado e)
    {
        using var cx = db.Abrir();
        using var tx = cx.BeginTransaction();
        ErroresOracle.Traducir(() =>
        {
            using var c = ConexionOracle.Comando(cx, """
                UPDATE idm_empleados
                   SET emp_tipo = :tipo, emp_especialidad = :especialidad, emp_activo = :activo
                 WHERE emp_id = :id
                RETURNING idm_personas_per_id INTO :perId
                """, tx).Con("tipo", e.Cargo).Con("especialidad", e.Especialidad).Con("activo", e.Activo).Con("id", e.Id);
            var perId = c.SalidaEntera("perId");
            if (c.ExecuteNonQuery() == 0) throw new KeyNotFoundException("No existe el empleado.");
            PersonasOracle.Actualizar(cx, tx, perId.ValorEntero(), e);
        });
        tx.Commit();
    }

    private Empleado? Uno(string condicion, object valor)
    {
        using var cx = db.Abrir();
        using var c = ConexionOracle.Comando(cx, $"{Select} WHERE {condicion}").Con("valor", valor);
        return c.Leer(Mapear).FirstOrDefault();
    }

    private static Empleado Mapear(IDataRecord r)
    {
        var e = new Empleado
        {
            Id = r.Entero("emp_id"),
            Cargo = r.Enum<Cargo>("emp_tipo"),
            Especialidad = r.TextoONulo("emp_especialidad"),
            Activo = r.Booleano("emp_activo")
        };
        PersonasOracle.Mapear(r, e);
        return e;
    }
}
