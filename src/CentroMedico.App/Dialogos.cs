using System.Windows;
using CentroMedico.ViewModels;
using Microsoft.Win32;

namespace CentroMedico.App;

internal class Dialogos : IDialogos
{
    public bool Confirmar(string pregunta) =>
        MessageBox.Show(pregunta, "Centro Médico", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

    public string? RutaParaGuardar(string nombreSugerido, string filtro)
    {
        var dialogo = new SaveFileDialog { FileName = nombreSugerido, Filter = filtro };
        return dialogo.ShowDialog() == true ? dialogo.FileName : null;
    }
}
