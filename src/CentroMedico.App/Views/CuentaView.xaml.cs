using System.Windows;
using System.Windows.Controls;
using CentroMedico.ViewModels;

namespace CentroMedico.App.Views;

public partial class CuentaView : UserControl
{
    public CuentaView()
    {
        InitializeComponent();
        DataContextChanged += (_, e) =>
        {
            if (e.NewValue is CuentaViewModel vm)
                vm.ContrasenaCambiada += () =>
                {
                    txtActual.Clear();
                    txtNueva.Clear();
                    txtConfirmacion.Clear();
                };
        };
    }

    private CuentaViewModel? Vm => DataContext as CuentaViewModel;

    private void Actual_Changed(object sender, RoutedEventArgs e) { if (Vm is { } vm) vm.Actual = txtActual.Password; }
    private void Nueva_Changed(object sender, RoutedEventArgs e) { if (Vm is { } vm) vm.Nueva = txtNueva.Password; }
    private void Confirmacion_Changed(object sender, RoutedEventArgs e) { if (Vm is { } vm) vm.Confirmacion = txtConfirmacion.Password; }
}
