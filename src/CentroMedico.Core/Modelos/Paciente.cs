namespace CentroMedico.Core.Modelos;

public class Paciente : Persona
{
    public DateOnly? FechaNacimiento { get; set; }

    public int? Edad(DateOnly hoy)
    {
        if (FechaNacimiento is not { } nacimiento) return null;
        int edad = hoy.Year - nacimiento.Year;
        if (hoy < nacimiento.AddYears(edad)) edad--;
        return edad;
    }
}
