using System.Runtime.ExceptionServices;

namespace CentroMedico.App.Tests;

/// <summary>WPF exige un hilo STA; xUnit corre las pruebas en hilos MTA.</summary>
internal static class HiloSta
{
    public static void Ejecutar(Action accion)
    {
        ExceptionDispatchInfo? error = null;
        var hilo = new Thread(() =>
        {
            try { accion(); }
            catch (Exception ex) { error = ExceptionDispatchInfo.Capture(ex); }
        });
        hilo.SetApartmentState(ApartmentState.STA);
        hilo.Start();
        hilo.Join();
        error?.Throw();
    }
}
