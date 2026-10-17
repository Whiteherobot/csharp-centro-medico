using System.Text.RegularExpressions;
using CentroMedico.Core.Modelos;
using CentroMedico.Core.Repositorios;
using CentroMedico.Core.Seguridad;

namespace CentroMedico.Core.Servicios;

public record Sesion(Usuario Usuario, Empleado Empleado)
{
    public bool EsAdministrador => Usuario.Rol == Rol.Administrador;
}

public partial class AutenticacionService(IUsuarioRepository usuarios, IEmpleadoRepository empleados)
{
    private const string CredencialesInvalidas = "Usuario o contraseña incorrectos.";

    public Sesion IniciarSesion(string nombreUsuario, string contrasena)
    {
        var usuario = usuarios.ObtenerPorNombre(nombreUsuario.Trim());
        // Mismo mensaje si el usuario no existe o la clave no coincide, para no revelar cuál falló.
        if (usuario is null || !HashContrasena.Verificar(contrasena, usuario.HashContrasena))
            throw new ReglaNegocioException(CredencialesInvalidas);

        var empleado = empleados.ObtenerPorId(usuario.EmpleadoId);
        if (!usuario.Activo || empleado is not { Activo: true })
            throw new ReglaNegocioException("La cuenta está desactivada.");

        return new Sesion(usuario, empleado);
    }

    /// <summary>Crea la cuenta de acceso de un empleado que todavía no tiene una.</summary>
    public Usuario CrearCuenta(int empleadoId, string nombreUsuario, string contrasena, Rol rol)
    {
        nombreUsuario = nombreUsuario.Trim().ToLowerInvariant();

        var empleado = empleados.ObtenerPorId(empleadoId)
            ?? throw new ReglaNegocioException("El empleado no existe.");
        if (usuarios.ObtenerPorEmpleado(empleado.Id) is not null)
            throw new ReglaNegocioException($"{empleado.NombreCompleto} ya tiene una cuenta.");
        if (!NombreUsuarioRegex().IsMatch(nombreUsuario))
            throw new ReglaNegocioException("El usuario debe tener de 4 a 20 caracteres: letras, números, punto o guion bajo.");
        if (usuarios.ObtenerPorNombre(nombreUsuario) is not null)
            throw new ReglaNegocioException($"El usuario {nombreUsuario} ya existe.");
        ValidarContrasena(contrasena);

        var usuario = new Usuario
        {
            NombreUsuario = nombreUsuario,
            HashContrasena = HashContrasena.Generar(contrasena),
            Rol = rol,
            EmpleadoId = empleado.Id
        };
        usuarios.Agregar(usuario);
        return usuario;
    }

    public void CambiarContrasena(Sesion sesion, string actual, string nueva)
    {
        var usuario = usuarios.ObtenerPorNombre(sesion.Usuario.NombreUsuario)
            ?? throw new ReglaNegocioException("La cuenta ya no existe.");
        if (!HashContrasena.Verificar(actual, usuario.HashContrasena))
            throw new ReglaNegocioException("La contraseña actual no es correcta.");
        ValidarContrasena(nueva);

        usuario.HashContrasena = HashContrasena.Generar(nueva);
        usuarios.Actualizar(usuario);
    }

    /// <summary>El administrador asigna una clave nueva cuando alguien la olvida.</summary>
    public void RestablecerContrasena(Sesion administrador, int empleadoId, string nueva)
    {
        var usuario = CuentaAdministrable(administrador, empleadoId);
        ValidarContrasena(nueva);
        usuario.HashContrasena = HashContrasena.Generar(nueva);
        usuarios.Actualizar(usuario);
    }

    public void ConfigurarCuenta(Sesion administrador, int empleadoId, Rol rol, bool activa)
    {
        var usuario = CuentaAdministrable(administrador, empleadoId);
        if (usuario.Id == administrador.Usuario.Id && (!activa || rol != Rol.Administrador))
            throw new ReglaNegocioException("No puede quitarse el permiso de administrador ni desactivar su propia cuenta.");

        usuario.Rol = rol;
        usuario.Activo = activa;
        usuarios.Actualizar(usuario);
    }

    private Usuario CuentaAdministrable(Sesion administrador, int empleadoId)
    {
        if (!administrador.EsAdministrador)
            throw new ReglaNegocioException("Solo un administrador puede modificar cuentas.");
        return usuarios.ObtenerPorEmpleado(empleadoId)
            ?? throw new ReglaNegocioException("El empleado no tiene cuenta.");
    }

    private static void ValidarContrasena(string contrasena)
    {
        if (contrasena.Length < 8 || !contrasena.Any(char.IsLetter) || !contrasena.Any(char.IsDigit))
            throw new ReglaNegocioException("La contraseña debe tener al menos 8 caracteres, con letras y números.");
    }

    [GeneratedRegex(@"^[a-z0-9._]{4,20}$")]
    private static partial Regex NombreUsuarioRegex();
}
