namespace CentroMedico.Core.Modelos;

public enum Cargo
{
    Atencion,
    Medico,
    Administracion
}

public class Empleado : Persona
{
    public Cargo Cargo { get; set; } = Cargo.Atencion;

    /// <summary>Solo aplica a médicos (por ejemplo, "Medicina general").</summary>
    public string? Especialidad { get; set; }

    public bool EsMedico => Cargo == Cargo.Medico;
}
