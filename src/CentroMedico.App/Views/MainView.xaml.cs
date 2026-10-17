using System.Windows;
using System.Windows.Input;
using CentroMedico.ViewModels;

namespace CentroMedico.App.Views;

public partial class MainView : Window
{
    public MainView(MainViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
        // Sin esto, al maximizar una ventana sin borde tapa la barra de tareas.
        MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
    }

    private void Arrastrar(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2) Maximizar(sender, e);
        else if (e.LeftButton == MouseButtonState.Pressed) DragMove();
    }

    private void Minimizar(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void Maximizar(object sender, RoutedEventArgs e) =>
        WindowState = WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;

    private void Cerrar(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
}
