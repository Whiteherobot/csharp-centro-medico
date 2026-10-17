using CentroMedico.ViewModels;

namespace CentroMedico.Tests.ViewModels;

public class FacturacionViewModelTests : IDisposable
{
    private readonly DialogosFalsos _dialogos = new();
    private readonly Contexto _contexto;
    private readonly FacturacionViewModel _vm;
    private readonly string _carpeta = Directory.CreateTempSubdirectory("centromedico-").FullName;

    public FacturacionViewModelTests()
    {
        _contexto = Demo.Crear(dialogos: _dialogos);
        _vm = new FacturacionViewModel(_contexto);
    }

    public void Dispose() => Directory.Delete(_carpeta, recursive: true);

    private int Servicio(string nombre) => _vm.Servicios.Single(s => s.Texto.StartsWith(nombre)).Valor;

    [Fact]
    public void AlElegirPacienteTraeSusCitasPorCobrarYCalculaElTotal()
    {
        _vm.BuscarPaciente = "1309873212";

        var cita = Assert.Single(_vm.CitasPorCobrar);
        Assert.True(cita.Incluir);
        Assert.Equal("$ 8,00", _vm.Total);

        cita.Incluir = false;
        Assert.Equal("$ 0,00", _vm.Total);
        Assert.False(_vm.EmitirCommand.CanExecute(null));
    }

    [Fact]
    public void SumarServiciosSueltosAgrupaLaMismaLinea()
    {
        _vm.BuscarPaciente = "0104567896";
        Assert.True(_vm.SinCitasPorCobrar);

        _vm.ServicioId = Servicio("Certificado médico");
        _vm.Cantidad = "1";
        _vm.AgregarCommand.Execute(null);
        _vm.ServicioId = Servicio("Certificado médico");
        _vm.Cantidad = "2";
        _vm.AgregarCommand.Execute(null);

        var linea = Assert.Single(_vm.LineasExtra);
        Assert.Equal(3, linea.Cantidad);
        Assert.Equal("$ 30,00", _vm.SubtotalGravado);
        Assert.Equal("$ 4,50", _vm.Iva);
        Assert.Equal("$ 34,50", _vm.Total);

        _vm.QuitarCommand.Execute(linea);
        Assert.Equal("$ 0,00", _vm.Total);
    }

    [Fact]
    public void LaCantidadDebeSerUnNumeroPositivo()
    {
        _vm.ServicioId = Servicio("Certificado médico");
        _vm.Cantidad = "0";
        Assert.False(_vm.AgregarCommand.CanExecute(null));
        _vm.Cantidad = "dos";
        Assert.False(_vm.AgregarCommand.CanExecute(null));
    }

    [Fact]
    public void EmitirDejaLaFacturaEnElHistorialYLimpiaLoCobrado()
    {
        _vm.BuscarPaciente = "1309873212";
        _vm.ServicioId = Servicio("Curación con insumos");
        _vm.AgregarCommand.Execute(null);

        _vm.EmitirCommand.Execute(null);

        Assert.Equal("Factura 001-001-000000001 emitida por $ 22,38.", _vm.Mensaje);
        Assert.Empty(_vm.CitasPorCobrar);
        Assert.Empty(_vm.LineasExtra);
        Assert.Equal("Carmen Zambrano Vélez", Assert.Single(_vm.Facturas).Paciente);
        Assert.NotNull(_vm.FacturaSeleccionada);
    }

    [Fact]
    public void ExportaElComprobanteConColumnasAlineadas()
    {
        _vm.BuscarPaciente = "1309873212";
        _vm.ServicioId = Servicio("Certificado médico");
        _vm.AgregarCommand.Execute(null);
        _vm.EmitirCommand.Execute(null);
        _dialogos.Ruta = Path.Combine(_carpeta, "factura.txt");

        _vm.ExportarCommand.Execute(null);

        var lineas = File.ReadAllLines(_dialogos.Ruta);
        Assert.Equal("Factura 001-001-000000001", lineas[1]);
        Assert.Contains(lineas, l => l.StartsWith("1     Control de presión y glucosa") && l.EndsWith("8,00"));
        Assert.Contains(lineas, l => l.StartsWith("1     Certificado médico *") && l.EndsWith("10,00"));
        var totales = lineas.Where(l => l.TrimStart().StartsWith("Subtotal") || l.TrimStart().StartsWith("IVA") || l.TrimStart().StartsWith("TOTAL")).ToList();
        Assert.Equal(4, totales.Count);
        Assert.All(totales, l => Assert.Equal(64, l.Length));
        Assert.EndsWith("19,50", totales[^1]);
        Assert.Equal("Comprobante guardado en factura.txt.", _vm.Mensaje);
    }

    [Fact]
    public void SiSeCancelaElDialogoNoSeEscribeNada()
    {
        _vm.BuscarPaciente = "1309873212";
        _vm.EmitirCommand.Execute(null);
        _dialogos.Ruta = null;

        _vm.ExportarCommand.Execute(null);

        Assert.Empty(Directory.GetFiles(_carpeta));
    }
}
