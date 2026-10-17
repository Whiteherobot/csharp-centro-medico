using System.Data;
using CentroMedico.Core.Modelos;
using CentroMedico.Core.Repositorios;

namespace CentroMedico.Datos.Oracle;

public class CitaRepositoryOracle(ConexionOracle db) : ICitaRepository
{
    private const string Select = """
        SELECT cit_id, idm_pacientes_pac_id, idm_empleados_emp_id, idm_servicios_ser_id,
               cit_fecha_hora, cit_estado, idm_facturas_fac_id
          FROM idm_citas
        """;

    // Rango [día, día + 1) para que Oracle pueda usar el índice de cit_fecha_hora.
    public IReadOnlyList<Cita> ListarPorMedicoYFecha(int medicoId, DateOnly fecha) =>
        Buscar("idm_empleados_emp_id = :medico AND cit_fecha_hora >= :dia AND cit_fecha_hora < :dia + 1",
            ("medico", medicoId), ("dia", fecha));

    public IReadOnlyList<Cita> ListarPorPaciente(int pacienteId) =>
        Buscar("idm_pacientes_pac_id = :paciente", ("paciente", pacienteId));

    public IReadOnlyList<Cita> ListarPorFecha(DateOnly fecha) =>
        Buscar("cit_fecha_hora >= :dia AND cit_fecha_hora < :dia + 1", ("dia", fecha));

    public Cita? ObtenerPorId(int id) => Buscar("cit_id = :id", ("id", id)).FirstOrDefault();

    public void Agregar(Cita cita)
    {
        using var cx = db.Abrir();
        ErroresOracle.Traducir(() =>
        {
            using var c = ConexionOracle.Comando(cx, """
                INSERT INTO idm_citas (idm_pacientes_pac_id, idm_empleados_emp_id, idm_servicios_ser_id, cit_fecha_hora, cit_estado)
                VALUES (:paciente, :medico, :servicio, :fecha, :estado)
                RETURNING cit_id INTO :id
                """)
                .Con("paciente", cita.PacienteId).Con("medico", cita.MedicoId).Con("servicio", cita.ServicioId)
                .Con("fecha", cita.FechaHora).Con("estado", cita.Estado);
            cita.Id = c.InsertarRetornandoId();
        });
    }

    public void Actualizar(Cita cita)
    {
        using var cx = db.Abrir();
        ErroresOracle.Traducir(() =>
        {
            using var c = ConexionOracle.Comando(cx,
                "UPDATE idm_citas SET cit_estado = :estado, cit_fecha_hora = :fecha WHERE cit_id = :id")
                .Con("estado", cita.Estado).Con("fecha", cita.FechaHora).Con("id", cita.Id);
            if (c.ExecuteNonQuery() == 0) throw new KeyNotFoundException("No existe la cita.");
        });
    }

    private List<Cita> Buscar(string condicion, params (string Nombre, object Valor)[] parametros)
    {
        using var cx = db.Abrir();
        using var c = ConexionOracle.Comando(cx, $"{Select} WHERE {condicion} ORDER BY cit_fecha_hora");
        foreach (var (nombre, valor) in parametros) c.Con(nombre, valor);
        return c.Leer(Mapear);
    }

    private static Cita Mapear(IDataRecord r) => new()
    {
        Id = r.Entero("cit_id"),
        PacienteId = r.Entero("idm_pacientes_pac_id"),
        MedicoId = r.Entero("idm_empleados_emp_id"),
        ServicioId = r.Entero("idm_servicios_ser_id"),
        FechaHora = (DateTime)r["cit_fecha_hora"],
        Estado = r.Enum<EstadoCita>("cit_estado"),
        FacturaId = r.EnteroONulo("idm_facturas_fac_id")
    };
}
