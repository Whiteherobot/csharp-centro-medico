using CentroMedico.ViewModels;

namespace CentroMedico.Tests.ViewModels;

public class DialogosFalsos : IDialogos
{
    public bool Respuesta { get; set; } = true;
    public string? Ruta { get; set; }
    public List<string> Preguntas { get; } = [];

    public bool Confirmar(string pregunta)
    {
        Preguntas.Add(pregunta);
        return Respuesta;
    }

    public string? RutaParaGuardar(string nombreSugerido, string filtro) => Ruta;
}

public static class Demo
{
    /// <summary>Contexto demo con el reloj fijo en el lunes de prueba.</summary>
    public static Contexto Crear(RelojFijo? reloj = null, DialogosFalsos? dialogos = null) =>
        Contexto.Demo(reloj ?? new RelojFijo(Escenario.Lunes), dialogos ?? new DialogosFalsos());
}
