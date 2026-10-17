using CentroMedico.Core.Servicios;
using Oracle.ManagedDataAccess.Client;

namespace CentroMedico.Datos.Oracle;

/// <summary>Convierte las violaciones de restricciones en mensajes para el usuario.</summary>
internal static class ErroresOracle
{
    private static readonly Dictionary<string, string> Mensajes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["UQ_PERSONAS_CEDULA"] = "La cédula ya está registrada para otra persona.",
        ["UQ_PACIENTES_PERSONA"] = "Esa persona ya está registrada como paciente.",
        ["UQ_EMPLEADOS_PERSONA"] = "Esa persona ya está registrada como empleado.",
        ["UQ_USUARIOS_USUARIO"] = "El nombre de usuario ya existe.",
        ["UQ_USUARIOS_EMPLEADO"] = "El empleado ya tiene una cuenta.",
        ["UQ_SERVICIOS_NOMBRE"] = "Ya existe un servicio con ese nombre.",
        ["UX_CITAS_MEDICO_TURNO"] = "El médico ya tiene una cita en ese turno.",
        ["UQ_FACTURAS_NUMERO"] = "Otro equipo emitió una factura al mismo tiempo; vuelva a intentarlo.",
        ["UQ_DETALLES_CITA"] = "Una de las citas ya fue facturada."
    };

    public static void Traducir(Action accion)
    {
        try
        {
            accion();
        }
        catch (OracleException ex) when (ex.Number == 1)
        {
            // ORA-00001: unique constraint (CENTROMEDICO.UQ_...) violated
            var restriccion = Mensajes.Keys.FirstOrDefault(k => ex.Message.Contains(k, StringComparison.OrdinalIgnoreCase));
            throw new ReglaNegocioException(restriccion is null ? "El registro ya existe." : Mensajes[restriccion]);
        }
    }
}
