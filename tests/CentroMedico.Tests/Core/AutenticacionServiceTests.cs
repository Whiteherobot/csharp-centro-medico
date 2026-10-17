using CentroMedico.Core.Modelos;
using CentroMedico.Core.Seguridad;
using CentroMedico.Core.Servicios;

namespace CentroMedico.Tests.Core;

public class AutenticacionServiceTests
{
    private readonly Escenario _e = new();
    private readonly AutenticacionService _auth;

    public AutenticacionServiceTests()
    {
        _auth = new AutenticacionService(_e.Usuarios, _e.Empleados);
    }

    [Fact]
    public void ElHashNoGuardaLaContrasenaYCambiaConCadaSal()
    {
        string a = HashContrasena.Generar("Clinica2026");
        string b = HashContrasena.Generar("Clinica2026");

        Assert.DoesNotContain("Clinica2026", a);
        Assert.NotEqual(a, b);
        Assert.True(HashContrasena.Verificar("Clinica2026", a));
        Assert.False(HashContrasena.Verificar("clinica2026", a));
        Assert.False(HashContrasena.Verificar("Clinica2026", "123456"));  // texto plano del sistema viejo
    }

    [Fact]
    public void IniciaSesionConLaCuentaCreada()
    {
        _auth.CrearCuenta(_e.Recepcionista.Id, "RCabrera", "Turnos2026", Rol.Recepcion);

        var sesion = _auth.IniciarSesion("rcabrera", "Turnos2026");

        Assert.Equal("rcabrera", sesion.Usuario.NombreUsuario);
        Assert.Equal("Rosa Cabrera", sesion.Empleado.NombreCompleto);
        Assert.False(sesion.EsAdministrador);
    }

    [Theory]
    [InlineData("rcabrera", "Otra2026")]
    [InlineData("noexiste", "Turnos2026")]
    public void UsuarioOClaveMalDanElMismoMensaje(string usuario, string clave)
    {
        _auth.CrearCuenta(_e.Recepcionista.Id, "rcabrera", "Turnos2026", Rol.Recepcion);

        var error = Assert.Throws<ReglaNegocioException>(() => _auth.IniciarSesion(usuario, clave));
        Assert.Equal("Usuario o contraseña incorrectos.", error.Message);
    }

    [Fact]
    public void UnEmpleadoDesactivadoNoEntra()
    {
        _auth.CrearCuenta(_e.Recepcionista.Id, "rcabrera", "Turnos2026", Rol.Recepcion);
        _e.Recepcionista.Activo = false;
        _e.Empleados.Actualizar(_e.Recepcionista);

        var error = Assert.Throws<ReglaNegocioException>(() => _auth.IniciarSesion("rcabrera", "Turnos2026"));
        Assert.Contains("desactivada", error.Message);
    }

    [Theory]
    [InlineData("abc", "Turnos2026")]        // usuario corto
    [InlineData("rosa cabrera", "Turnos2026")] // espacio
    [InlineData("rcabrera", "corta1")]
    [InlineData("rcabrera", "soloLetras")]
    [InlineData("rcabrera", "12345678")]
    public void ValidaUsuarioYContrasena(string usuario, string clave) =>
        Assert.Throws<ReglaNegocioException>(() =>
            _auth.CrearCuenta(_e.Recepcionista.Id, usuario, clave, Rol.Recepcion));

    [Fact]
    public void UnaCuentaPorEmpleadoYNombresUnicos()
    {
        _auth.CrearCuenta(_e.Recepcionista.Id, "rcabrera", "Turnos2026", Rol.Recepcion);

        Assert.Throws<ReglaNegocioException>(() =>
            _auth.CrearCuenta(_e.Recepcionista.Id, "rosa.c", "Turnos2026", Rol.Recepcion));
        Assert.Throws<ReglaNegocioException>(() =>
            _auth.CrearCuenta(_e.Medico.Id, "RCABRERA", "Turnos2026", Rol.Recepcion));
    }

    [Fact]
    public void CambiarContrasenaPideLaActual()
    {
        _auth.CrearCuenta(_e.Recepcionista.Id, "rcabrera", "Turnos2026", Rol.Recepcion);
        var sesion = _auth.IniciarSesion("rcabrera", "Turnos2026");

        Assert.Throws<ReglaNegocioException>(() => _auth.CambiarContrasena(sesion, "mal", "Nueva2027"));
        _auth.CambiarContrasena(sesion, "Turnos2026", "Nueva2027");

        Assert.Throws<ReglaNegocioException>(() => _auth.IniciarSesion("rcabrera", "Turnos2026"));
        _auth.IniciarSesion("rcabrera", "Nueva2027");
    }

    [Fact]
    public void ElAdministradorRestableceYDesactivaCuentas()
    {
        _auth.CrearCuenta(_e.Medico.Id, "admin", "Admin2026", Rol.Administrador);
        _auth.CrearCuenta(_e.Recepcionista.Id, "rcabrera", "Turnos2026", Rol.Recepcion);
        var admin = _auth.IniciarSesion("admin", "Admin2026");

        _auth.RestablecerContrasena(admin, _e.Recepcionista.Id, "Olvido2026");
        _auth.IniciarSesion("rcabrera", "Olvido2026");

        _auth.ConfigurarCuenta(admin, _e.Recepcionista.Id, Rol.Recepcion, activa: false);
        Assert.Throws<ReglaNegocioException>(() => _auth.IniciarSesion("rcabrera", "Olvido2026"));
    }

    [Fact]
    public void NadieSeQuitaSuPropioPermisoNiRecepcionAdministra()
    {
        _auth.CrearCuenta(_e.Medico.Id, "admin", "Admin2026", Rol.Administrador);
        _auth.CrearCuenta(_e.Recepcionista.Id, "rcabrera", "Turnos2026", Rol.Recepcion);
        var admin = _auth.IniciarSesion("admin", "Admin2026");
        var recepcion = _auth.IniciarSesion("rcabrera", "Turnos2026");

        Assert.Throws<ReglaNegocioException>(() =>
            _auth.ConfigurarCuenta(admin, _e.Medico.Id, Rol.Recepcion, activa: true));
        Assert.Throws<ReglaNegocioException>(() =>
            _auth.RestablecerContrasena(recepcion, _e.Medico.Id, "Robada2026"));
    }
}
