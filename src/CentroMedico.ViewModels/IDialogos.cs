namespace CentroMedico.ViewModels;

/// <summary>Ventanas del sistema que los ViewModels necesitan sin depender de WPF.</summary>
public interface IDialogos
{
    bool Confirmar(string pregunta);

    /// <summary>Pide la ruta donde guardar un archivo; null si el usuario cancela.</summary>
    string? RutaParaGuardar(string nombreSugerido, string filtro);
}
