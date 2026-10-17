using Oracle.ManagedDataAccess.Client;
using System.Data;
using CentroMedico.Core.Modelos;
using CentroMedico.Core.Repositorios;
using CentroMedico.Core.Servicios;

namespace CentroMedico.Datos.Oracle;

public class FacturaRepositoryOracle(ConexionOracle db) : IFacturaRepository
{
    private const string Select = """
        SELECT fac_id, fac_numero, fac_fecha, idm_pacientes_pac_id,
               fac_subtotal_0, fac_subtotal_iva, fac_iva, fac_total
          FROM idm_facturas
        """;

    public IReadOnlyList<Factura> Listar()
    {
        using var cx = db.Abrir();
        using var c = ConexionOracle.Comando(cx, $"{Select} ORDER BY fac_fecha DESC, fac_id DESC");
        var facturas = c.Leer(Mapear);
        foreach (var f in facturas) f.Detalles = Detalles(cx, f.Id);
        return facturas;
    }

    public Factura? ObtenerPorId(int id)
    {
        using var cx = db.Abrir();
        using var c = ConexionOracle.Comando(cx, $"{Select} WHERE fac_id = :id").Con("id", id);
        var factura = c.Leer(Mapear).FirstOrDefault();
        if (factura is not null) factura.Detalles = Detalles(cx, factura.Id);
        return factura;
    }

    public int SiguienteSecuencial()
    {
        // El índice único de fac_numero evita duplicados si dos equipos emiten a la vez.
        using var cx = db.Abrir();
        using var c = ConexionOracle.Comando(cx, "SELECT COUNT(*) + 1 FROM idm_facturas");
        return Convert.ToInt32(c.ExecuteScalar());
    }

    public void Agregar(Factura f)
    {
        using var cx = db.Abrir();
        using var tx = cx.BeginTransaction();
        ErroresOracle.Traducir(() =>
        {
            using (var c = ConexionOracle.Comando(cx, """
                INSERT INTO idm_facturas (fac_numero, fac_fecha, idm_pacientes_pac_id,
                                          fac_subtotal_0, fac_subtotal_iva, fac_iva, fac_total)
                VALUES (:numero, :fecha, :paciente, :sub0, :subIva, :iva, :total)
                RETURNING fac_id INTO :id
                """, tx))
            {
                c.Con("numero", f.Numero).Con("fecha", f.Fecha).Con("paciente", f.PacienteId)
                 .Con("sub0", f.SubtotalTarifa0).Con("subIva", f.SubtotalGravado).Con("iva", f.Iva).Con("total", f.Total);
                f.Id = c.InsertarRetornandoId();
            }

            foreach (var d in f.Detalles)
            {
                using var c = ConexionOracle.Comando(cx, """
                    INSERT INTO idm_detalles_factura (idm_facturas_fac_id, idm_servicios_ser_id, det_descripcion,
                                                      det_cantidad, det_precio, det_grava_iva, idm_citas_cit_id)
                    VALUES (:factura, :servicio, :descripcion, :cantidad, :precio, :iva, :cita)
                    """, tx)
                    .Con("factura", f.Id).Con("servicio", d.ServicioId).Con("descripcion", d.Descripcion)
                    .Con("cantidad", d.Cantidad).Con("precio", d.PrecioUnitario).Con("iva", d.GravaIva).Con("cita", d.CitaId);
                c.ExecuteNonQuery();

                if (d.CitaId is { } citaId)
                {
                    // Solo se marca si sigue sin facturar; si otro equipo la cobró antes, se revierte todo.
                    using var marcar = ConexionOracle.Comando(cx, """
                        UPDATE idm_citas SET idm_facturas_fac_id = :factura
                         WHERE cit_id = :cita AND idm_facturas_fac_id IS NULL
                        """, tx).Con("factura", f.Id).Con("cita", citaId);
                    if (marcar.ExecuteNonQuery() == 0)
                        throw new ReglaNegocioException("Una de las citas ya fue facturada.");
                }
            }
        });
        tx.Commit();
    }

    private static List<DetalleFactura> Detalles(OracleConnection cx, int facturaId)
    {
        using var c = ConexionOracle.Comando(cx, """
            SELECT idm_servicios_ser_id, det_descripcion, det_cantidad, det_precio, det_grava_iva, idm_citas_cit_id
              FROM idm_detalles_factura
             WHERE idm_facturas_fac_id = :id
             ORDER BY det_id
            """).Con("id", facturaId);
        return c.Leer(r => new DetalleFactura
        {
            ServicioId = r.Entero("idm_servicios_ser_id"),
            Descripcion = r.Texto("det_descripcion"),
            Cantidad = r.Entero("det_cantidad"),
            PrecioUnitario = r.Decimal("det_precio"),
            GravaIva = r.Booleano("det_grava_iva"),
            CitaId = r.EnteroONulo("idm_citas_cit_id")
        });
    }

    private static Factura Mapear(IDataRecord r) => new()
    {
        Id = r.Entero("fac_id"),
        Numero = r.Texto("fac_numero"),
        Fecha = (DateTime)r["fac_fecha"],
        PacienteId = r.Entero("idm_pacientes_pac_id"),
        SubtotalTarifa0 = r.Decimal("fac_subtotal_0"),
        SubtotalGravado = r.Decimal("fac_subtotal_iva"),
        Iva = r.Decimal("fac_iva"),
        Total = r.Decimal("fac_total")
    };
}
