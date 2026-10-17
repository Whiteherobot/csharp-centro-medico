using System.Text.RegularExpressions;
using CentroMedico.Core.Modelos;

namespace CentroMedico.Core.Validacion;

public static partial class ValidadorPersona
{
    /// <summary>Devuelve la lista de errores; vacía si los datos son correctos.</summary>
    public static List<string> Validar(Persona persona)
    {
        var errores = new List<string>();

        if (!Cedula.EsValida(persona.Cedula))
            errores.Add("La cédula no es válida.");
        if (string.IsNullOrWhiteSpace(persona.Nombres))
            errores.Add("Los nombres son obligatorios.");
        if (string.IsNullOrWhiteSpace(persona.Apellidos))
            errores.Add("Los apellidos son obligatorios.");
        if (!TelefonoRegex().IsMatch(persona.Telefono))
            errores.Add("El teléfono debe ser un celular (09XXXXXXXX) o un fijo con código de área (0XXXXXXXX).");
        if (!string.IsNullOrEmpty(persona.Email) && !EmailRegex().IsMatch(persona.Email))
            errores.Add("El correo electrónico no es válido.");

        if (persona is Empleado { EsMedico: true, Especialidad: var especialidad }
            && string.IsNullOrWhiteSpace(especialidad))
            errores.Add("Un médico debe tener especialidad.");

        return errores;
    }

    [GeneratedRegex(@"^0(9\d{8}|[2-7]\d{7})$")]
    private static partial Regex TelefonoRegex();

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();
}
