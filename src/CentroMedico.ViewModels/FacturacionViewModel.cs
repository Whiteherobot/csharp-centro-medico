using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Windows.Input;
using CentroMedico.Core.Modelos;
using CentroMedico.Core.Servicios;

namespace CentroMedico.ViewModels;

/// <summary>Cita atendida que se puede incluir en la factura.</summary>
public class CitaPorCobrar(Cita cita, string texto, decimal precio) : ViewModelBase
{
    private bool _incluir = true;

    public Cita Cita { get; } = cita;
    public string Texto { get; } = texto;
    public string Precio { get; } = Formatos.Dinero(precio);

    public bool Incluir { get => _incluir; set => Set(ref _incluir, value); }
}

/// <summary>Servicio que se cobra sin cita (un certificado, una curación).</summary>
public record LineaExtra(int ServicioId, string Descripcion, int Cantidad, decimal PrecioUnitario)
{
    public string Subtotal => Formatos.Dinero(Cantidad * PrecioUnitario);
}

public record FacturaFila(Factura Factura, string Paciente)
{
    public string Numero => Factura.Numero;
    public string Fecha => Factura.Fecha.ToString("dd/MM/yyyy HH:mm");
    public string Total => Formatos.Dinero(Factura.Total);
}

public class FacturacionViewModel : ViewModelBase
{
    private readonly Contexto _c;
    private string _buscarPaciente = "";
    private IReadOnlyList<Opcion<int>> _pacientesEncontrados = [];
    private int? _pacienteId;
    private IReadOnlyList<CitaPorCobrar> _citasPorCobrar = [];
    private int? _servicioId;
    private string _cantidad = "1";
    private Factura? _vistaPrevia;
    private IReadOnlyList<FacturaFila> _facturas = [];
    private FacturaFila? _facturaSeleccionada;

    public FacturacionViewModel(Contexto contexto)
    {
        _c = contexto;
        Servicios = _c.Servicios.Listar(soloActivos: true)
            .Select(s => new Opcion<int>(s.Id, $"{s.Nombre} ({Formatos.Dinero(s.Precio)}{(s.GravaIva ? " + IVA" : "")})"))
            .ToList();
        LineasExtra.CollectionChanged += (_, _) => Recalcular();

        AgregarCommand = Comando(Agregar, () => ServicioId is not null && int.TryParse(Cantidad, out int n) && n > 0);
        QuitarCommand = Comando<LineaExtra>(l => LineasExtra.Remove(l));
        EmitirCommand = Comando(Emitir, () => PacienteId is not null && Lineas().Any());
        ExportarCommand = Comando(Exportar, () => FacturaSeleccionada is not null);

        BuscarPacientes();
        CargarFacturas();
    }

    // Paciente

    public string BuscarPaciente
    {
        get => _buscarPaciente;
        set { if (Set(ref _buscarPaciente, value)) BuscarPacientes(); }
    }

    public IReadOnlyList<Opcion<int>> PacientesEncontrados
    {
        get => _pacientesEncontrados;
        private set => Set(ref _pacientesEncontrados, value);
    }

    public int? PacienteId
    {
        get => _pacienteId;
        set
        {
            if (!Set(ref _pacienteId, value)) return;
            LineasExtra.Clear();
            CargarCitas();
        }
    }

    // Detalle

    public IReadOnlyList<CitaPorCobrar> CitasPorCobrar
    {
        get => _citasPorCobrar;
        private set => Set(ref _citasPorCobrar, value);
    }

    public bool SinCitasPorCobrar => PacienteId is not null && CitasPorCobrar.Count == 0;

    public IReadOnlyList<Opcion<int>> Servicios { get; }
    public int? ServicioId { get => _servicioId; set => Set(ref _servicioId, value); }
    public string Cantidad { get => _cantidad; set => Set(ref _cantidad, value); }
    public ObservableCollection<LineaExtra> LineasExtra { get; } = [];

    // Totales

    public string SubtotalTarifa0 => Formatos.Dinero(_vistaPrevia?.SubtotalTarifa0 ?? 0);
    public string SubtotalGravado => Formatos.Dinero(_vistaPrevia?.SubtotalGravado ?? 0);
    public string Iva => Formatos.Dinero(_vistaPrevia?.Iva ?? 0);
    public string Total => Formatos.Dinero(_vistaPrevia?.Total ?? 0);

    // Historial

    public IReadOnlyList<FacturaFila> Facturas
    {
        get => _facturas;
        private set => Set(ref _facturas, value);
    }

    public FacturaFila? FacturaSeleccionada
    {
        get => _facturaSeleccionada;
        set => Set(ref _facturaSeleccionada, value);
    }

    public ICommand AgregarCommand { get; }
    public ICommand QuitarCommand { get; }
    public ICommand EmitirCommand { get; }
    public ICommand ExportarCommand { get; }

    private void BuscarPacientes()
    {
        PacientesEncontrados = _c.Pacientes.Listar(BuscarPaciente)
            .Take(30)
            .Select(p => new Opcion<int>(p.Id, $"{p.Cedula}  {p.Apellidos} {p.Nombres}"))
            .ToList();
        if (PacientesEncontrados.Count == 1) PacienteId = PacientesEncontrados[0].Valor;
        else if (PacientesEncontrados.All(p => p.Valor != PacienteId)) PacienteId = null;
    }

    private void CargarCitas()
    {
        foreach (var c in CitasPorCobrar) c.PropertyChanged -= CitaCambiada;

        var servicios = _c.Servicios.Listar().ToDictionary(s => s.Id);
        CitasPorCobrar = PacienteId is { } id
            ? _c.Facturacion.CitasPendientes(id).Select(cita =>
            {
                var s = servicios[cita.ServicioId];
                return new CitaPorCobrar(cita, $"{cita.FechaHora:dd/MM/yyyy HH:mm}  {s.Nombre}", s.Precio);
            }).ToList()
            : [];

        foreach (var c in CitasPorCobrar) c.PropertyChanged += CitaCambiada;
        OnPropertyChanged(nameof(SinCitasPorCobrar));
        Recalcular();
    }

    private void CitaCambiada(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CitaPorCobrar.Incluir)) Recalcular();
    }

    private IEnumerable<LineaFactura> Lineas() =>
        CitasPorCobrar.Where(c => c.Incluir).Select(c => new LineaFactura(c.Cita.ServicioId, 1, c.Cita.Id))
            .Concat(LineasExtra.Select(l => new LineaFactura(l.ServicioId, l.Cantidad)));

    private void Recalcular()
    {
        try
        {
            _vistaPrevia = PacienteId is { } id ? _c.Facturacion.Calcular(id, Lineas()) : null;
        }
        catch (ReglaNegocioException ex)
        {
            // Por ejemplo, alguien desactivó un servicio de la lista mientras se armaba la factura.
            _vistaPrevia = null;
            Error = ex.Message;
        }
        OnPropertyChanged(nameof(SubtotalTarifa0));
        OnPropertyChanged(nameof(SubtotalGravado));
        OnPropertyChanged(nameof(Iva));
        OnPropertyChanged(nameof(Total));
    }

    private void Agregar()
    {
        var servicio = _c.Servicios.ObtenerPorId(ServicioId!.Value)!;
        int cantidad = int.Parse(Cantidad);

        // Si el servicio ya está en la lista se suma la cantidad en vez de repetir la línea.
        var existente = LineasExtra.FirstOrDefault(l => l.ServicioId == servicio.Id);
        if (existente is not null)
        {
            LineasExtra[LineasExtra.IndexOf(existente)] = existente with { Cantidad = existente.Cantidad + cantidad };
        }
        else
        {
            LineasExtra.Add(new LineaExtra(servicio.Id, servicio.Nombre, cantidad, servicio.Precio));
        }
        ServicioId = null;
        Cantidad = "1";
    }

    private void Emitir()
    {
        Factura? factura = null;
        if (!Intentar(() => factura = _c.Facturacion.Emitir(PacienteId!.Value, Lineas()))) return;

        Mensaje = $"Factura {factura!.Numero} emitida por {Formatos.Dinero(factura.Total)}.";
        string? mensaje = Mensaje;
        LineasExtra.Clear();
        CargarCitas();
        CargarFacturas();
        FacturaSeleccionada = Facturas.FirstOrDefault(f => f.Factura.Id == factura.Id);
        Mensaje = mensaje;
    }

    private void CargarFacturas()
    {
        var nombres = new Dictionary<int, string>();
        Facturas = _c.Facturas.Listar().Select(f =>
        {
            if (!nombres.TryGetValue(f.PacienteId, out var nombre))
                nombres[f.PacienteId] = nombre = _c.Pacientes.ObtenerPorId(f.PacienteId)?.NombreCompleto ?? "";
            return new FacturaFila(f, nombre);
        }).ToList();
    }

    private void Exportar()
    {
        var factura = FacturaSeleccionada!.Factura;
        var ruta = _c.Dialogos.RutaParaGuardar($"factura-{factura.Numero}.txt", "Texto (*.txt)|*.txt");
        if (ruta is null) return;

        var paciente = _c.Pacientes.ObtenerPorId(factura.PacienteId)!;
        File.WriteAllText(ruta, Comprobante.Generar(factura, paciente));
        Error = null;
        Mensaje = $"Comprobante guardado en {Path.GetFileName(ruta)}.";
    }
}
