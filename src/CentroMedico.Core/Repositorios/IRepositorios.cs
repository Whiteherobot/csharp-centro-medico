using CentroMedico.Core.Modelos;

namespace CentroMedico.Core.Repositorios;

public interface IPacienteRepository
{
    IReadOnlyList<Paciente> Listar(string? filtro = null);
    Paciente? ObtenerPorId(int id);
    Paciente? ObtenerPorCedula(string cedula);
    void Agregar(Paciente paciente);
    void Actualizar(Paciente paciente);
}

public interface IEmpleadoRepository
{
    IReadOnlyList<Empleado> Listar(string? filtro = null);
    IReadOnlyList<Empleado> ListarMedicos();
    Empleado? ObtenerPorId(int id);
    Empleado? ObtenerPorCedula(string cedula);
    void Agregar(Empleado empleado);
    void Actualizar(Empleado empleado);
}

public interface IUsuarioRepository
{
    Usuario? ObtenerPorNombre(string nombreUsuario);
    Usuario? ObtenerPorEmpleado(int empleadoId);
    void Agregar(Usuario usuario);
    void Actualizar(Usuario usuario);
}

public interface IServicioRepository
{
    IReadOnlyList<Servicio> Listar(bool soloActivos = false);
    Servicio? ObtenerPorId(int id);
    void Agregar(Servicio servicio);
    void Actualizar(Servicio servicio);
}

public interface ICitaRepository
{
    IReadOnlyList<Cita> ListarPorMedicoYFecha(int medicoId, DateOnly fecha);
    IReadOnlyList<Cita> ListarPorPaciente(int pacienteId);
    IReadOnlyList<Cita> ListarPorFecha(DateOnly fecha);
    Cita? ObtenerPorId(int id);
    void Agregar(Cita cita);
    void Actualizar(Cita cita);
}

public interface IFacturaRepository
{
    IReadOnlyList<Factura> Listar();
    Factura? ObtenerPorId(int id);
    int SiguienteSecuencial();

    /// <summary>Guarda la factura y marca sus citas como facturadas en una sola operación.</summary>
    void Agregar(Factura factura);
}
