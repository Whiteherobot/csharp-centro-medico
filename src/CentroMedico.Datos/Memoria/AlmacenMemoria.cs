using CentroMedico.Core.Modelos;

namespace CentroMedico.Datos.Memoria;

/// <summary>
/// Tablas en memoria compartidas por los repositorios del modo demo y las pruebas.
/// Cada repositorio guarda copias, igual que una base real, para que modificar un
/// objeto en pantalla no cambie el dato hasta que se llame a Actualizar.
/// </summary>
public class AlmacenMemoria
{
    internal readonly List<Paciente> Pacientes = [];
    internal readonly List<Empleado> Empleados = [];
    internal readonly List<Usuario> Usuarios = [];
    internal readonly List<Servicio> Servicios = [];
    internal readonly List<Cita> Citas = [];
    internal readonly List<Factura> Facturas = [];

    private int _ultimoId;
    internal readonly object Candado = new();

    internal int NuevoId() => ++_ultimoId;
}
