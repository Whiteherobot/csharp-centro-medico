using CentroMedico.Core.Modelos;
using CentroMedico.Core.Validacion;

namespace CentroMedico.Tests.Core;

public class ValidadorPersonaTests
{
    private static Paciente PacienteValido() => new()
    {
        Cedula = "0104567896",
        Nombres = "María José",
        Apellidos = "Quizhpi Ortega",
        Telefono = "0987654321",
        Email = "mjquizhpi@correo.ec"
    };

    [Fact]
    public void PacienteCompletoNoTieneErrores() =>
        Assert.Empty(ValidadorPersona.Validar(PacienteValido()));

    [Theory]
    [InlineData("0987654321")]  // celular
    [InlineData("072845123")]   // fijo de Cuenca
    public void AceptaCelularYFijo(string telefono)
    {
        var paciente = PacienteValido();
        paciente.Telefono = telefono;
        Assert.Empty(ValidadorPersona.Validar(paciente));
    }

    [Fact]
    public void ReportaCadaCampoMal()
    {
        var paciente = new Paciente { Cedula = "123", Telefono = "12345", Email = "sin-arroba" };

        var errores = ValidadorPersona.Validar(paciente);

        Assert.Equal(5, errores.Count);
    }

    [Fact]
    public void ElCorreoEsOpcional()
    {
        var paciente = PacienteValido();
        paciente.Email = "";
        Assert.Empty(ValidadorPersona.Validar(paciente));
    }

    [Fact]
    public void MedicoSinEspecialidadEsInvalido()
    {
        var medico = new Empleado
        {
            Cedula = "1710034065",
            Nombres = "Andrés",
            Apellidos = "Vintimilla",
            Telefono = "0991234567",
            Cargo = Cargo.Medico
        };

        Assert.Contains("Un médico debe tener especialidad.", ValidadorPersona.Validar(medico));

        medico.Especialidad = "Pediatría";
        Assert.Empty(ValidadorPersona.Validar(medico));
    }

    [Fact]
    public void EdadConsideraSiYaCumplioAnios()
    {
        var paciente = new Paciente { FechaNacimiento = new DateOnly(1990, 8, 20) };

        Assert.Equal(35, paciente.Edad(new DateOnly(2026, 8, 19)));
        Assert.Equal(36, paciente.Edad(new DateOnly(2026, 8, 20)));
    }
}
