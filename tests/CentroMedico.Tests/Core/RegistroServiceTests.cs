using CentroMedico.Core.Modelos;
using CentroMedico.Core.Servicios;

namespace CentroMedico.Tests.Core;

public class RegistroServiceTests
{
    private readonly Escenario _e = new();
    private readonly RegistroService _registro;

    public RegistroServiceTests()
    {
        _registro = new RegistroService(_e.Pacientes, _e.Empleados, _e.Servicios);
    }

    [Fact]
    public void LimpiaLosDatosAntesDeGuardar()
    {
        var paciente = new Paciente
        {
            Cedula = " 1710034065 ",
            Nombres = " Diego ",
            Apellidos = "Ochoa",
            Telefono = "099 123-4567",
            Email = " DOchoa@Correo.EC "
        };

        _registro.GuardarPaciente(paciente);

        var guardado = _e.Pacientes.ObtenerPorCedula("1710034065")!;
        Assert.Equal("0991234567", guardado.Telefono);
        Assert.Equal("dochoa@correo.ec", guardado.Email);
        Assert.Equal("Diego Ochoa", guardado.NombreCompleto);
    }

    [Fact]
    public void NoRepiteCedulaPeroPermiteEditarAlMismoPaciente()
    {
        var copia = new Paciente { Cedula = _e.Paciente.Cedula, Nombres = "Otra", Apellidos = "Persona", Telefono = "0991234567" };
        Assert.Throws<ReglaNegocioException>(() => _registro.GuardarPaciente(copia));

        var mismo = _e.Pacientes.ObtenerPorId(_e.Paciente.Id)!;
        mismo.Telefono = "072845123";
        _registro.GuardarPaciente(mismo);
        Assert.Equal("072845123", _e.Pacientes.ObtenerPorId(mismo.Id)!.Telefono);
    }

    [Fact]
    public void JuntaTodosLosErroresEnUnMensaje()
    {
        var error = Assert.Throws<ReglaNegocioException>(() =>
            _registro.GuardarPaciente(new Paciente { Cedula = "1", Telefono = "1" }));

        Assert.Equal(4, error.Message.Split(Environment.NewLine).Length);
    }

    [Fact]
    public void QuitaLaEspecialidadSiYaNoEsMedico()
    {
        var empleado = new Empleado
        {
            Cedula = "0923456784", Nombres = "Carla", Apellidos = "Jara", Telefono = "0981112233",
            Cargo = Cargo.Atencion, Especialidad = "Pediatría"
        };

        _registro.GuardarEmpleado(empleado);

        Assert.Null(_e.Empleados.ObtenerPorId(empleado.Id)!.Especialidad);
    }

    [Theory]
    [InlineData("Ecografía", 0)]
    [InlineData("Ecografía", -5)]
    [InlineData("Ecografía", 12.345)]
    [InlineData("  ", 20)]
    [InlineData("consulta GENERAL", 30)]  // ya existe "Consulta general"
    public void ValidaLosServicios(string nombre, decimal precio) =>
        Assert.Throws<ReglaNegocioException>(() =>
            _registro.GuardarServicio(new Servicio { Nombre = nombre, Precio = precio }));
}
