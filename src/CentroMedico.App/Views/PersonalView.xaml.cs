using System.Windows;
using System.Windows.Controls;
using CentroMedico.ViewModels;

namespace CentroMedico.App.Views;

public partial class PersonalView : UserControl
{
    public PersonalView()
    {
        InitializeComponent();
        DataContextChanged += (_, e) =>
        {
            if (e.NewValue is PersonalViewModel vm)
                vm.ClaveUsada += () =>
                {
                    txtClaveNueva.Clear();
                    txtClaveRestablecer.Clear();
                };
        };
    }

    private void Clave_Changed(object sender, RoutedEventArgs e)
    {
        if (DataContext is PersonalViewModel vm && sender is PasswordBox caja)
            vm.Clave = caja.Password;
    }
}
