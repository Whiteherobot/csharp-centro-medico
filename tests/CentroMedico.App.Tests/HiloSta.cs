using System.Runtime.ExceptionServices;
using System.Windows.Threading;

namespace CentroMedico.App.Tests;

/// <summary>
/// WPF exige un hilo STA y el objeto Application queda atado al hilo que lo crea,
/// así que todas las pruebas corren en un mismo hilo con su propio Dispatcher.
/// </summary>
internal static class HiloSta
{
    private static readonly Lazy<Dispatcher> Despachador = new(() =>
    {
        Dispatcher? listo = null;
        using var creado = new ManualResetEventSlim();
        var hilo = new Thread(() =>
        {
            listo = Dispatcher.CurrentDispatcher;
            creado.Set();
            Dispatcher.Run();
        }) { IsBackground = true, Name = "Pruebas WPF" };
        hilo.SetApartmentState(ApartmentState.STA);
        hilo.Start();
        creado.Wait();
        return listo!;
    });

    public static void Ejecutar(Action accion)
    {
        ExceptionDispatchInfo? error = null;
        Despachador.Value.Invoke(() =>
        {
            try { accion(); }
            catch (Exception ex) { error = ExceptionDispatchInfo.Capture(ex); }
        });
        error?.Throw();
    }
}
