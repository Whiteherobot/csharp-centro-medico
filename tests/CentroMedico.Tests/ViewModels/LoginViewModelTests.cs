using CentroMedico.Core.Servicios;
using CentroMedico.ViewModels;

namespace CentroMedico.Tests.ViewModels;

public class LoginViewModelTests
{
    private readonly LoginViewModel _vm = new(Demo.Crear());

    [Fact]
    public void ElBotonSeHabilitaConUsuarioYContrasena()
    {
        int avisos = 0;
        _vm.IngresarCommand.CanExecuteChanged += (_, _) => avisos++;

        Assert.False(_vm.IngresarCommand.CanExecute(null));
        _vm.Usuario = "admin";
        Assert.False(_vm.IngresarCommand.CanExecute(null));
        _vm.Contrasena = "x";

        Assert.True(_vm.IngresarCommand.CanExecute(null));
        Assert.True(avisos >= 2);
    }

    [Fact]
    public void ConCredencialesDelDemoAbreLaSesion()
    {
        Sesion? sesion = null;
        _vm.SesionIniciada += s => sesion = s;

        _vm.Usuario = DatosDemo.UsuarioAdmin;
        _vm.Contrasena = DatosDemo.ClaveAdmin;
        _vm.IngresarCommand.Execute(null);

        Assert.NotNull(sesion);
        Assert.True(sesion.EsAdministrador);
        Assert.Null(_vm.Error);
    }

    [Fact]
    public void ConClaveIncorrectaMuestraErrorYBorraLaClave()
    {
        _vm.Usuario = DatosDemo.UsuarioAdmin;
        _vm.Contrasena = "otra";
        _vm.IngresarCommand.Execute(null);

        Assert.Equal("Usuario o contraseña incorrectos.", _vm.Error);
        Assert.Equal("", _vm.Contrasena);
    }

    [Fact]
    public void ElModoDemoTieneAgendaYAlgoPorFacturar()
    {
        var contexto = Demo.Crear();
        var proximo = Escenario.Lunes.Date;

        Assert.True(_vm.ModoDemo);
        Assert.Equal(3, contexto.Citas.ListarPorFecha(DateOnly.FromDateTime(proximo)).Count);
        var carmen = contexto.Pacientes.ObtenerPorCedula("1309873212")!;
        Assert.Single(contexto.Facturacion.CitasPendientes(carmen.Id));
    }
}
