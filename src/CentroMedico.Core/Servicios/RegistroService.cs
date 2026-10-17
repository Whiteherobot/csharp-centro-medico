using CentroMedico.Core.Modelos;
using CentroMedico.Core.Repositorios;
using CentroMedico.Core.Validacion;

namespace CentroMedico.Core.Servicios;

/// <summary>Altas y cambios de pacientes, personal y servicios.</summary>
public class RegistroService(
    IPacienteRepository pacientes,
    IEmpleadoRepository empleados,
    IServicioRepository servicios)
{
    public void GuardarPaciente(Paciente paciente)
    {
        Normalizar(paciente);
        Validar(paciente);

        var existente = pacientes.ObtenerPorCedula(paciente.Cedula);
        if (existente is not null && existente.Id != paciente.Id)
            throw new ReglaNegocioException($"Ya existe un paciente con la cédula {paciente.Cedula}.");

        if (paciente.Id == 0) pacientes.Agregar(paciente);
        else pacientes.Actualizar(paciente);
    }

    public void GuardarEmpleado(Empleado empleado)
    {
        Normalizar(empleado);
        if (!empleado.EsMedico) empleado.Especialidad = null;
        Validar(empleado);

        var existente = empleados.ObtenerPorCedula(empleado.Cedula);
        if (existente is not null && existente.Id != empleado.Id)
            throw new ReglaNegocioException($"Ya existe un empleado con la cédula {empleado.Cedula}.");

        if (empleado.Id == 0) empleados.Agregar(empleado);
        else empleados.Actualizar(empleado);
    }

    public void GuardarServicio(Servicio servicio)
    {
        servicio.Nombre = servicio.Nombre.Trim();
        if (servicio.Nombre.Length < 3)
            throw new ReglaNegocioException("El nombre del servicio es obligatorio.");
        if (servicio.Precio <= 0 || decimal.Round(servicio.Precio, 2) != servicio.Precio)
            throw new ReglaNegocioException("El precio debe ser mayor a cero y con máximo dos decimales.");

        bool repetido = servicios.Listar().Any(s =>
            s.Id != servicio.Id && string.Equals(s.Nombre, servicio.Nombre, StringComparison.CurrentCultureIgnoreCase));
        if (repetido)
            throw new ReglaNegocioException($"Ya existe el servicio {servicio.Nombre}.");

        if (servicio.Id == 0) servicios.Agregar(servicio);
        else servicios.Actualizar(servicio);
    }

    private static void Normalizar(Persona persona)
    {
        persona.Cedula = persona.Cedula.Trim();
        persona.Nombres = persona.Nombres.Trim();
        persona.Apellidos = persona.Apellidos.Trim();
        persona.Direccion = persona.Direccion.Trim();
        persona.Telefono = persona.Telefono.Trim().Replace(" ", "").Replace("-", "");
        persona.Email = persona.Email.Trim().ToLowerInvariant();
    }

    private static void Validar(Persona persona)
    {
        var errores = ValidadorPersona.Validar(persona);
        if (errores.Count > 0)
            throw new ReglaNegocioException(string.Join(Environment.NewLine, errores));
    }
}
