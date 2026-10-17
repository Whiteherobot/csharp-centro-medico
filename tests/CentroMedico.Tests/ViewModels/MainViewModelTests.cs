using CentroMedico.ViewModels;

namespace CentroMedico.Tests.ViewModels;

public class MainViewModelTests
{
    private readonly Contexto _contexto = Demo.Crear();

    private MainViewModel Entrar(string usuario, string clave) =>
        new(_contexto, _contexto.Autenticacion.IniciarSesion(usuario, clave));

    [Fact]
    public void AbreEnInicioYCambiaDeVista()
    {
        var vm = Entrar(DatosDemo.UsuarioAdmin, DatosDemo.ClaveAdmin);

        Assert.IsType<InicioViewModel>(vm.VistaActual);
        Assert.Equal("Paola Andrade Mora", vm.NombreUsuario);
        Assert.Equal("Administrador", vm.Rol);

        var cuenta = vm.Opciones.Single(o => o.Titulo == "Mi cuenta");
        vm.IrACommand.Execute(cuenta);

        Assert.IsType<CuentaViewModel>(vm.VistaActual);
        Assert.Same(cuenta, vm.OpcionActual);
    }

    [Fact]
    public void CerrarSesionAvisaALaVentana()
    {
        var vm = Entrar(DatosDemo.UsuarioRecepcion, DatosDemo.ClaveRecepcion);
        bool cerrada = false;
        vm.SesionCerrada += () => cerrada = true;

        vm.CerrarSesionCommand.Execute(null);

        Assert.True(cerrada);
    }

    [Fact]
    public void ElInicioMuestraLaAgendaDelDia()
    {
        var inicio = new InicioViewModel(_contexto);

        Assert.Equal("lunes 19 de octubre de 2026", inicio.Fecha);
        Assert.Equal(3, inicio.Reservadas);
        Assert.Equal(4, inicio.PacientesActivos);
        Assert.Equal("$ 0,00", inicio.FacturadoHoyTexto);
        Assert.All(inicio.CitasDeHoy, f => Assert.Equal("Reservada", f.Estado));
        Assert.Contains(inicio.CitasDeHoy, f => f.Paciente == "Diego Ochoa Pesántez" && f.Medico == "Ana Lucía Medina Coronel");
    }

    [Fact]
    public void CambiarContrasenaExigeConfirmacionIgual()
    {
        var sesion = _contexto.Autenticacion.IniciarSesion(DatosDemo.UsuarioRecepcion, DatosDemo.ClaveRecepcion);
        var cuenta = new CuentaViewModel(_contexto, sesion);
        bool limpiar = false;
        cuenta.ContrasenaCambiada += () => limpiar = true;

        cuenta.Actual = DatosDemo.ClaveRecepcion;
        cuenta.Nueva = "Nueva2027";
        cuenta.Confirmacion = "Nueva2028";
        cuenta.CambiarCommand.Execute(null);
        Assert.Equal("La confirmación no coincide con la contraseña nueva.", cuenta.Error);
        Assert.False(limpiar);

        cuenta.Confirmacion = "Nueva2027";
        cuenta.CambiarCommand.Execute(null);
        Assert.Equal("Contraseña actualizada.", cuenta.Mensaje);
        Assert.True(limpiar);
        Assert.False(cuenta.CambiarCommand.CanExecute(null));
        _contexto.Autenticacion.IniciarSesion(DatosDemo.UsuarioRecepcion, "Nueva2027");
    }
}
