namespace CentroMedico.Core.Modelos;

public enum Rol
{
    Recepcion,
    Administrador
}

/// <summary>Cuenta de acceso al sistema; siempre pertenece a un empleado.</summary>
public class Usuario
{
    public int Id { get; set; }
    public string NombreUsuario { get; set; } = "";
    public string HashContrasena { get; set; } = "";
    public Rol Rol { get; set; } = Rol.Recepcion;
    public bool Activo { get; set; } = true;
    public int EmpleadoId { get; set; }
}
