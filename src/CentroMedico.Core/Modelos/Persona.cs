namespace CentroMedico.Core.Modelos;

/// <summary>Datos comunes de pacientes y empleados (tabla idm_personas).</summary>
public abstract class Persona
{
    public int Id { get; set; }
    public string Cedula { get; set; } = "";
    public string Nombres { get; set; } = "";
    public string Apellidos { get; set; } = "";
    public string Direccion { get; set; } = "";
    public string Telefono { get; set; } = "";
    public string Email { get; set; } = "";
    public bool Activo { get; set; } = true;

    public string NombreCompleto => $"{Nombres} {Apellidos}".Trim();
}
