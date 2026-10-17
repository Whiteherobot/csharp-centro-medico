using System.Globalization;
using System.Windows.Input;
using CentroMedico.Core.Modelos;
using CentroMedico.Core.Servicios;

namespace CentroMedico.ViewModels;

public record ServicioFila(Servicio Servicio)
{
    public string Nombre => Servicio.Nombre;
    public string Precio => Formatos.Dinero(Servicio.Precio);
    public string Iva => Servicio.GravaIva ? "15 %" : "0 %";
    public string Estado => Servicio.Activo ? "Activo" : "Inactivo";
}

/// <summary>Catálogo de servicios que se reservan y se facturan.</summary>
public class ServiciosViewModel : ViewModelBase
{
    private readonly Contexto _c;
    private IReadOnlyList<ServicioFila> _servicios = [];
    private ServicioFila? _seleccionado;
    private string _nombre = "";
    private string _precio = "";
    private bool _gravaIva;
    private bool _activo = true;

    public ServiciosViewModel(Contexto contexto)
    {
        _c = contexto;
        NuevoCommand = Comando(() => Seleccionado = null);
        GuardarCommand = Comando(Guardar);
        Buscar();
    }

    public IReadOnlyList<ServicioFila> Servicios
    {
        get => _servicios;
        private set => Set(ref _servicios, value);
    }

    public ServicioFila? Seleccionado
    {
        get => _seleccionado;
        set
        {
            if (!Set(ref _seleccionado, value)) return;
            Error = Mensaje = null;
            var s = value?.Servicio;
            Nombre = s?.Nombre ?? "";
            Precio = s?.Precio.ToString("0.00", Formatos.Cultura) ?? "";
            GravaIva = s?.GravaIva ?? false;
            Activo = s?.Activo ?? true;
            OnPropertyChanged(nameof(TituloFormulario));
        }
    }

    public string TituloFormulario => Seleccionado is null ? "Nuevo servicio" : "Editar servicio";

    public string Nombre { get => _nombre; set => Set(ref _nombre, value); }

    /// <summary>Texto del campo; se acepta coma o punto como separador decimal.</summary>
    public string Precio { get => _precio; set => Set(ref _precio, value); }

    public bool GravaIva { get => _gravaIva; set => Set(ref _gravaIva, value); }
    public bool Activo { get => _activo; set => Set(ref _activo, value); }

    public ICommand NuevoCommand { get; }
    public ICommand GuardarCommand { get; }

    private void Buscar() => Servicios = _c.Servicios.Listar().Select(s => new ServicioFila(s)).ToList();

    public static bool TryLeerPrecio(string texto, out decimal precio) =>
        decimal.TryParse(texto.Replace("$", "").Replace(',', '.').Trim(),
            NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out precio);

    private void Guardar()
    {
        var servicio = Seleccionado is { } fila ? _c.Servicios.ObtenerPorId(fila.Servicio.Id) ?? new Servicio() : new Servicio();
        bool eraNuevo = servicio.Id == 0;

        bool listo = Intentar(() =>
        {
            if (!TryLeerPrecio(Precio, out var precio))
                throw new ReglaNegocioException("Escriba el precio como número, por ejemplo 25,00.");
            servicio.Nombre = Nombre;
            servicio.Precio = precio;
            servicio.GravaIva = GravaIva;
            servicio.Activo = Activo;
            _c.Registro.GuardarServicio(servicio);
        }, eraNuevo ? "Servicio agregado." : "Cambios guardados.");

        if (!listo) return;
        string? mensaje = Mensaje;
        Buscar();
        Seleccionado = Servicios.FirstOrDefault(f => f.Servicio.Id == servicio.Id);
        Mensaje = mensaje;
    }
}
