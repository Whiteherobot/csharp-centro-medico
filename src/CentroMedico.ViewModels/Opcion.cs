namespace CentroMedico.ViewModels;

/// <summary>Valor de un ComboBox con el texto que ve el usuario.</summary>
public record Opcion<T>(T Valor, string Texto);
