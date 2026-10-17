using Oracle.ManagedDataAccess.Client;
using System.Data;
using CentroMedico.Core.Modelos;
using CentroMedico.Core.Repositorios;

namespace CentroMedico.Datos.Oracle;

public class ServicioRepositoryOracle(ConexionOracle db) : IServicioRepository
{
    private const string Select = """
        SELECT ser_id, ser_nombre, ser_precio, ser_grava_iva, ser_duracion, ser_activo
          FROM idm_servicios
        """;

    public IReadOnlyList<Servicio> Listar(bool soloActivos = false)
    {
        using var cx = db.Abrir();
        using var c = ConexionOracle.Comando(cx, $"{Select} WHERE :todos = 1 OR ser_activo = 1 ORDER BY ser_nombre")
            .Con("todos", !soloActivos);
        return c.Leer(Mapear);
    }

    public Servicio? ObtenerPorId(int id)
    {
        using var cx = db.Abrir();
        using var c = ConexionOracle.Comando(cx, $"{Select} WHERE ser_id = :id").Con("id", id);
        return c.Leer(Mapear).FirstOrDefault();
    }

    public void Agregar(Servicio s)
    {
        using var cx = db.Abrir();
        ErroresOracle.Traducir(() =>
        {
            using var c = ConexionOracle.Comando(cx, """
                INSERT INTO idm_servicios (ser_nombre, ser_precio, ser_grava_iva, ser_duracion, ser_activo)
                VALUES (:nombre, :precio, :iva, :duracion, :activo)
                RETURNING ser_id INTO :id
                """);
            s.Id = ConDatos(c, s).InsertarRetornandoId();
        });
    }

    public void Actualizar(Servicio s)
    {
        using var cx = db.Abrir();
        ErroresOracle.Traducir(() =>
        {
            using var c = ConexionOracle.Comando(cx, """
                UPDATE idm_servicios
                   SET ser_nombre = :nombre, ser_precio = :precio, ser_grava_iva = :iva,
                       ser_duracion = :duracion, ser_activo = :activo
                 WHERE ser_id = :id
                """);
            if (ConDatos(c, s).Con("id", s.Id).ExecuteNonQuery() == 0)
                throw new KeyNotFoundException("No existe el servicio.");
        });
    }

    private static OracleCommand ConDatos(
        OracleCommand c, Servicio s) => c
        .Con("nombre", s.Nombre).Con("precio", s.Precio).Con("iva", s.GravaIva)
        .Con("duracion", s.DuracionMinutos).Con("activo", s.Activo);

    private static Servicio Mapear(IDataRecord r) => new()
    {
        Id = r.Entero("ser_id"),
        Nombre = r.Texto("ser_nombre"),
        Precio = r.Decimal("ser_precio"),
        GravaIva = r.Booleano("ser_grava_iva"),
        DuracionMinutos = r.Entero("ser_duracion"),
        Activo = r.Booleano("ser_activo")
    };
}
