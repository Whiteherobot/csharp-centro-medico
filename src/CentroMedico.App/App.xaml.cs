using System.Globalization;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Threading;
using CentroMedico.App.Views;
using CentroMedico.Datos.Oracle;
using CentroMedico.ViewModels;
using Oracle.ManagedDataAccess.Client;

namespace CentroMedico.App;

public partial class App : Application
{
    private Contexto _contexto = null!;

    static App()
    {
        // Sin esto WPF formatea fechas y números como en-US (8/20/1990) aunque Windows esté en español.
        var cultura = CultureInfo.GetCultureInfo("es-EC");
        CultureInfo.DefaultThreadCurrentCulture = cultura;
        CultureInfo.DefaultThreadCurrentUICulture = cultura;
        CultureInfo.CurrentCulture = cultura;
        CultureInfo.CurrentUICulture = cultura;
        FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement),
            new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(cultura.IetfLanguageTag)));
    }

    private void ApplicationStart(object sender, StartupEventArgs e)
    {
        DispatcherUnhandledException += ErrorInesperado;

        var dialogos = new Dialogos();
        var cadena = Configuracion.CadenaOracle();
        if (cadena is not null && ErrorDeConexion(cadena) is { } error)
        {
            // Si Oracle no responde, se ofrece seguir en modo demo en vez de cerrar.
            if (!dialogos.Confirmar($"No se pudo conectar a Oracle:\n{error}\n\n¿Quiere abrir la aplicación en modo demo?"))
            {
                Shutdown();
                return;
            }
            cadena = null;
        }

        _contexto = cadena is null
            ? Contexto.Demo(TimeProvider.System, dialogos)
            : Contexto.Oracle(cadena, TimeProvider.System, dialogos);

        MostrarLogin();
    }

    private void MostrarLogin()
    {
        var vm = new LoginViewModel(_contexto);
        var login = new LoginView(vm);
        vm.SesionIniciada += sesion =>
        {
            var principal = new MainViewModel(_contexto, sesion);
            var ventana = new MainView(principal);
            principal.SesionCerrada += () =>
            {
                ventana.Close();
                MostrarLogin();
            };
            ventana.Show();
            login.Close();
        };
        login.Show();
    }

    private static string? ErrorDeConexion(string cadena)
    {
        try
        {
            using var conexion = new ConexionOracle(cadena).Abrir();
            return null;
        }
        catch (OracleException ex)
        {
            return ex.Message;
        }
    }

    private void ErrorInesperado(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show($"Ocurrió un error inesperado:\n{e.Exception.Message}", "Centro Médico",
            MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }
}
