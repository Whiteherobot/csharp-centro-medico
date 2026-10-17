using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using CentroMedico.App.Views;
using CentroMedico.ViewModels;

namespace CentroMedico.App.Tests;

/// <summary>
/// Arma cada pantalla con los datos del modo demo, sin mostrar ventanas, y la dibuja a PNG.
/// Falla si el XAML tiene un recurso o un enlace que no se puede resolver; las imágenes
/// quedan en artifacts/capturas para revisarlas (el CI las publica).
/// </summary>
public class VistasTests
{
    private static readonly string Carpeta = Path.Combine(
        AppContext.BaseDirectory, "..", "..", "..", "..", "..", "artifacts", "capturas");

    private class SinDialogos : IDialogos
    {
        public bool Confirmar(string pregunta) => true;
        public string? RutaParaGuardar(string nombreSugerido, string filtro) => null;
    }

    /// <summary>Un lunes a media mañana, para que la agenda del demo tenga citas en el día.</summary>
    private class Reloj : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(2026, 10, 19, 10, 15, 0, TimeSpan.FromHours(-5));
        public override TimeZoneInfo LocalTimeZone =>
            TimeZoneInfo.CreateCustomTimeZone("ECT", TimeSpan.FromHours(-5), "Ecuador", "Ecuador");
    }

    private static void ConApp(Action<Contexto> accion) => HiloSta.Ejecutar(() =>
    {
        // Los recursos de App.xaml (colores, estilos y la fuente) se cargan una vez por hilo.
        if (Application.Current is null)
        {
            // Las rutas relativas como /Styles/... se buscan en el ensamblado de recursos, que en
            // el proceso de pruebas sería este y no el de la app.
            // No se puede leer antes de asignarlo: el getter fija el ensamblado de entrada.
            Application.ResourceAssembly = typeof(App).Assembly;
            var app = new App { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            app.InitializeComponent();
        }
        accion(Contexto.Demo(new Reloj(), new SinDialogos()));
    });

    private static void Capturar(FrameworkElement elemento, string nombre, double ancho, double alto)
    {
        var fallos = new List<string>();
        var escucha = new ErroresDeEnlace(fallos);
        PresentationTraceSources.DataBindingSource.Listeners.Add(escucha);
        // Sin depurador conectado WPF no emite estos mensajes hasta que se refresca la configuración.
        PresentationTraceSources.Refresh();
        PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Error;
        try
        {
            elemento.Measure(new Size(ancho, alto));
            elemento.Arrange(new Rect(0, 0, ancho, alto));
            elemento.UpdateLayout();
            // Deja que corran los enlaces y las plantillas pendientes antes de dibujar.
            Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
            elemento.UpdateLayout();

            var imagen = new RenderTargetBitmap((int)ancho, (int)alto, 96, 96, PixelFormats.Pbgra32);
            imagen.Render(elemento);
            var png = new PngBitmapEncoder();
            png.Frames.Add(BitmapFrame.Create(imagen));
            Directory.CreateDirectory(Carpeta);
            using var archivo = File.Create(Path.Combine(Carpeta, nombre + ".png"));
            png.Save(archivo);
        }
        finally
        {
            PresentationTraceSources.DataBindingSource.Listeners.Remove(escucha);
        }
        Assert.True(fallos.Count == 0, $"Errores de enlace en {nombre}:\n" + string.Join("\n", fallos));
    }

    private class ErroresDeEnlace(List<string> fallos) : TraceListener
    {
        public override void Write(string? mensaje) { }
        public override void WriteLine(string? mensaje) { if (mensaje is not null) fallos.Add(mensaje); }
    }

    /// <summary>Saca el contenido de la ventana para dibujarlo sin mostrarla.</summary>
    private static FrameworkElement Contenido(Window ventana)
    {
        var raiz = (FrameworkElement)ventana.Content;
        ventana.Content = null;
        raiz.DataContext = ventana.DataContext;
        foreach (var clave in ventana.Resources.Keys) raiz.Resources[clave] = ventana.Resources[clave];
        return raiz;
    }

    [Fact]
    public void Login() => ConApp(c =>
    {
        var vista = new LoginView(new LoginViewModel(c));
        Capturar(Contenido(vista), "01-login", 800, 550);
    });

    [Theory]
    [InlineData("Inicio", "02-inicio")]
    [InlineData("Pacientes", "03-pacientes")]
    [InlineData("Citas", "04-citas")]
    [InlineData("Facturación", "05-facturacion")]
    [InlineData("Servicios", "06-servicios")]
    [InlineData("Personal", "07-personal")]
    [InlineData("Mi cuenta", "08-cuenta")]
    public void Secciones(string titulo, string archivo) => ConApp(c =>
    {
        var sesion = c.Autenticacion.IniciarSesion(DatosDemo.UsuarioAdmin, DatosDemo.ClaveAdmin);
        var vm = new MainViewModel(c, sesion);
        vm.IrA(vm.Opciones.Single(o => o.Titulo == titulo));
        Preparar(vm.VistaActual);

        Capturar(Contenido(new MainView(vm)), archivo, 1150, 700);
    });

    /// <summary>Deja cada pantalla con algo seleccionado para que la captura muestre el formulario lleno.</summary>
    private static void Preparar(ViewModelBase vista)
    {
        switch (vista)
        {
            case PacientesViewModel p:
                p.Seleccionado = p.Pacientes.First(f => f.Cedula == "0104567896");
                break;
            case PersonalViewModel p:
                p.Seleccionado = p.Empleados.First(f => f.Cedula == "1710034065");
                break;
            case ServiciosViewModel s:
                s.Seleccionado = s.Servicios.First(f => f.Nombre.StartsWith("Curación"));
                break;
            case CitasViewModel c:
                c.BuscarPaciente = "1309873212";
                c.MedicoId = c.Medicos[0].Valor;
                c.ServicioId = c.Servicios[0].Valor;
                c.Horario = c.Horarios.FirstOrDefault()?.Valor;
                break;
            case FacturacionViewModel f:
                f.BuscarPaciente = "1309873212";
                f.ServicioId = f.Servicios.First(s => s.Texto.StartsWith("Certificado")).Valor;
                f.AgregarCommand.Execute(null);
                break;
        }
    }

    [Fact]
    public void TodasLasOpcionesDelMenuTienenVista() => ConApp(c =>
    {
        var sesion = c.Autenticacion.IniciarSesion(DatosDemo.UsuarioAdmin, DatosDemo.ClaveAdmin);
        var vm = new MainViewModel(c, sesion);
        var ventana = new MainView(vm);

        foreach (var opcion in vm.Opciones)
        {
            vm.IrA(opcion);
            var plantilla = (DataTemplate?)ventana.TryFindResource(new DataTemplateKey(vm.VistaActual.GetType()));
            Assert.True(plantilla is not null, $"Falta la vista de {opcion.Titulo}");
        }
    });
}
