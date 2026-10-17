using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using CentroMedico.ViewModels;

namespace CentroMedico.App.Views;

public partial class LoginView : Window
{
    private readonly LoginViewModel _vm;

    public LoginView(LoginViewModel vm)
    {
        InitializeComponent();
        DataContext = _vm = vm;
        // El ViewModel borra la clave tras un intento fallido; el PasswordBox no se enlaza solo.
        _vm.PropertyChanged += LimpiarContrasena;
        Loaded += (_, _) => txtUsuario.Focus();
    }

    private void LimpiarContrasena(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LoginViewModel.Contrasena) && _vm.Contrasena.Length == 0)
            txtContrasena.Clear();
    }

    private void TxtContrasena_PasswordChanged(object sender, RoutedEventArgs e) =>
        _vm.Contrasena = txtContrasena.Password;

    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
            DragMove();
    }

    private void BtnMinimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void BtnClose_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
}
