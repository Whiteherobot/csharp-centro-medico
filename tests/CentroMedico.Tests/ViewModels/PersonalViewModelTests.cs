using CentroMedico.Core.Modelos;
using CentroMedico.ViewModels;

namespace CentroMedico.Tests.ViewModels;

public class PersonalViewModelTests
{
    private readonly Contexto _contexto = Demo.Crear();
    private readonly PersonalViewModel _vm;

    public PersonalViewModelTests()
    {
        var admin = _contexto.Autenticacion.IniciarSesion(DatosDemo.UsuarioAdmin, DatosDemo.ClaveAdmin);
        _vm = new PersonalViewModel(_contexto, admin);
    }

    [Fact]
    public void SoloLosAdministradoresVenPersonal()
    {
        var recepcion = _contexto.Autenticacion.IniciarSesion(DatosDemo.UsuarioRecepcion, DatosDemo.ClaveRecepcion);
        var admin = _contexto.Autenticacion.IniciarSesion(DatosDemo.UsuarioAdmin, DatosDemo.ClaveAdmin);

        Assert.DoesNotContain(new MainViewModel(_contexto, recepcion).Opciones, o => o.Titulo == "Personal");
        Assert.Contains(new MainViewModel(_contexto, admin).Opciones, o => o.Titulo == "Personal");
    }

    [Fact]
    public void MuestraCargoYCuentaDeCadaEmpleado()
    {
        var rosa = _vm.Empleados.Single(e => e.Cedula == "1103456123");
        var andres = _vm.Empleados.Single(e => e.Cedula == "1710034065");

        Assert.Equal("rcabrera", rosa.CuentaTexto);
        Assert.Equal("Sin cuenta", andres.CuentaTexto);
        Assert.Equal("Médico", andres.Cargo);
    }

    [Fact]
    public void RegistraUnMedicoYLeCreaCuentaConElUsuarioSugerido()
    {
        _vm.NuevoCommand.Execute(null);
        _vm.Datos.Cedula = "0912345675";
        _vm.Datos.Nombres = "Iñaki";
        _vm.Datos.Apellidos = "Muñoz Ortiz";
        _vm.Datos.Telefono = "0961122334";
        _vm.Cargo = Cargo.Medico;
        _vm.GuardarCommand.Execute(null);
        Assert.Equal("Un médico debe tener especialidad.", _vm.Error);

        _vm.Especialidad = "Traumatología";
        _vm.GuardarCommand.Execute(null);
        Assert.StartsWith("Empleado registrado", _vm.Mensaje);
        Assert.True(_vm.PuedeCrearCuenta);
        Assert.Equal("imunoz", _vm.NuevoUsuario);

        Assert.False(_vm.CrearCuentaCommand.CanExecute(null));
        _vm.Clave = "Trauma2026";
        _vm.CrearCuentaCommand.Execute(null);

        Assert.Equal("Cuenta creada.", _vm.Mensaje);
        Assert.Equal("imunoz", _vm.Seleccionado!.CuentaTexto);
        _contexto.Autenticacion.IniciarSesion("imunoz", "Trauma2026");
    }

    [Fact]
    public void DesactivarLaCuentaDeRecepcion()
    {
        _vm.Seleccionado = _vm.Empleados.Single(e => e.Cedula == "1103456123");
        _vm.CuentaActiva = false;
        _vm.GuardarCuentaCommand.Execute(null);

        Assert.Equal("rcabrera (desactivada)", _vm.Seleccionado!.CuentaTexto);
    }

    [Fact]
    public void ElAdministradorNoSeDesactivaASiMismo()
    {
        _vm.Seleccionado = _vm.Empleados.Single(e => e.Cedula == "0102030400");
        _vm.Datos.Activo = false;
        _vm.GuardarCommand.Execute(null);

        Assert.Equal("No puede desactivarse a sí mismo.", _vm.Error);
    }

    [Fact]
    public void RestablecerLimpiaLaClave()
    {
        bool limpiada = false;
        _vm.ClaveUsada += () => limpiada = true;
        _vm.Seleccionado = _vm.Empleados.Single(e => e.Cedula == "1103456123");
        limpiada = false;

        _vm.Clave = "Nueva2026";
        _vm.RestablecerCommand.Execute(null);

        Assert.True(limpiada);
        Assert.Equal("", _vm.Clave);
        _contexto.Autenticacion.IniciarSesion(DatosDemo.UsuarioRecepcion, "Nueva2026");
    }
}
