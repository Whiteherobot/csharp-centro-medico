using CentroMedico.ViewModels;

namespace CentroMedico.Tests.ViewModels;

public class PacientesViewModelTests
{
    private readonly Contexto _contexto = Demo.Crear();
    private readonly PacientesViewModel _vm;

    public PacientesViewModelTests()
    {
        _vm = new PacientesViewModel(_contexto);
    }

    [Fact]
    public void ListaOrdenadoPorApellidoYCalculaLaEdad()
    {
        Assert.Equal(4, _vm.Pacientes.Count);
        Assert.Equal("Guamán Tenesaca Jorge", _vm.Pacientes[0].Nombre);
        Assert.Equal("36 años", _vm.Pacientes.Single(p => p.Cedula == "0104567896").EdadTexto);
    }

    [Fact]
    public void FiltrarActualizaLaLista()
    {
        _vm.Filtro = "ochoa";
        Assert.Equal("0923456784", Assert.Single(_vm.Pacientes).Cedula);
    }

    [Fact]
    public void RegistraUnPacienteNuevoYLoDejaSeleccionado()
    {
        _vm.NuevoCommand.Execute(null);
        _vm.Datos.Cedula = "1802345676";
        _vm.Datos.Nombres = "Gabriela";
        _vm.Datos.Apellidos = "Rocano";
        _vm.Datos.Telefono = "0998877665";
        _vm.FechaNacimiento = new DateTime(2001, 5, 10);

        _vm.GuardarCommand.Execute(null);

        Assert.Equal("Paciente registrado.", _vm.Mensaje);
        Assert.Equal(5, _vm.Pacientes.Count);
        Assert.Equal("1802345676", _vm.Seleccionado?.Cedula);
        Assert.Equal("Editar paciente", _vm.TituloFormulario);
    }

    [Fact]
    public void SiLaValidacionFallaNoSeModificaLaFila()
    {
        _vm.Seleccionado = _vm.Pacientes.Single(p => p.Cedula == "0104567896");
        _vm.Datos.Telefono = "123";

        _vm.GuardarCommand.Execute(null);

        Assert.Contains("teléfono", _vm.Error);
        Assert.Equal("0987654321", _contexto.Pacientes.ObtenerPorCedula("0104567896")!.Telefono);
        Assert.Equal("0987654321", _vm.Seleccionado!.Telefono);
    }

    [Fact]
    public void DesactivarYMostrarHistorial()
    {
        _vm.Seleccionado = _vm.Pacientes.Single(p => p.Cedula == "1309873212");
        Assert.Single(_vm.Historial);

        _vm.Datos.Activo = false;
        _vm.GuardarCommand.Execute(null);

        Assert.Equal("Inactivo", _vm.Seleccionado!.Estado);
    }

    [Fact]
    public void NoAceptaFechaDeNacimientoFutura()
    {
        _vm.Seleccionado = _vm.Pacientes[0];
        _vm.FechaNacimiento = Escenario.Lunes.AddDays(3);

        _vm.GuardarCommand.Execute(null);

        Assert.Equal("La fecha de nacimiento no puede ser futura.", _vm.Error);
    }
}
