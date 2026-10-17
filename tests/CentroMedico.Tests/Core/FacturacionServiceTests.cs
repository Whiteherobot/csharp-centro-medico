using CentroMedico.Core.Modelos;
using CentroMedico.Core.Servicios;

namespace CentroMedico.Tests.Core;

public class FacturacionServiceTests
{
    private readonly Escenario _e = new();
    private readonly FacturacionService _facturacion;

    public FacturacionServiceTests()
    {
        _facturacion = new FacturacionService(_e.Facturas, _e.Citas, _e.Pacientes, _e.Servicios, _e.Reloj);
    }

    private Cita CitaAtendida()
    {
        var cita = new Cita
        {
            PacienteId = _e.Paciente.Id,
            MedicoId = _e.Medico.Id,
            ServicioId = _e.Consulta.Id,
            FechaHora = Escenario.Lunes.AddHours(-24),
            Estado = EstadoCita.Atendida
        };
        _e.Citas.Agregar(cita);
        return cita;
    }

    [Fact]
    public void SeparaTarifaCeroDeLoGravadoYCalculaElIva()
    {
        var cita = CitaAtendida();

        var factura = _facturacion.Calcular(_e.Paciente.Id,
        [
            new LineaFactura(_e.Consulta.Id, 1, cita.Id),
            new LineaFactura(_e.Certificado.Id, 2)
        ]);

        Assert.Equal(25.00m, factura.SubtotalTarifa0);
        Assert.Equal(20.00m, factura.SubtotalGravado);
        Assert.Equal(3.00m, factura.Iva);
        Assert.Equal(48.00m, factura.Total);
    }

    [Fact]
    public void RedondeaElIvaADosDecimales()
    {
        var curacion = new Servicio { Nombre = "Curación con insumos", Precio = 7.35m, GravaIva = true };
        _e.Servicios.Agregar(curacion);

        var factura = _facturacion.Calcular(_e.Paciente.Id, [new LineaFactura(curacion.Id, 1)]);

        Assert.Equal(1.10m, factura.Iva);   // 7,35 × 0,15 = 1,1025
        Assert.Equal(8.45m, factura.Total);
    }

    [Fact]
    public void EmitirNumeraConElFormatoDelSriYMarcaLaCita()
    {
        var cita = CitaAtendida();

        var primera = _facturacion.Emitir(_e.Paciente.Id, [new LineaFactura(_e.Consulta.Id, 1, cita.Id)]);
        var segunda = _facturacion.Emitir(_e.Paciente.Id, [new LineaFactura(_e.Certificado.Id, 1)]);

        Assert.Equal("001-001-000000001", primera.Numero);
        Assert.Equal("001-001-000000002", segunda.Numero);
        Assert.Empty(_facturacion.CitasPendientes(_e.Paciente.Id));
    }

    [Fact]
    public void UnaCitaNoSeCobraDosVeces()
    {
        var cita = CitaAtendida();
        _facturacion.Emitir(_e.Paciente.Id, [new LineaFactura(_e.Consulta.Id, 1, cita.Id)]);

        var error = Assert.Throws<ReglaNegocioException>(() =>
            _facturacion.Emitir(_e.Paciente.Id, [new LineaFactura(_e.Consulta.Id, 1, cita.Id)]));
        Assert.Contains("ya se facturó", error.Message);
    }

    [Fact]
    public void NoFacturaCitasReservadasNiDeOtroPaciente()
    {
        var cita = CitaAtendida();
        var otro = new Paciente { Cedula = "1710034065", Nombres = "Diego", Apellidos = "Ochoa" };
        _e.Pacientes.Agregar(otro);

        Assert.Throws<ReglaNegocioException>(() =>
            _facturacion.Calcular(otro.Id, [new LineaFactura(_e.Consulta.Id, 1, cita.Id)]));

        var reservada = new Cita { PacienteId = _e.Paciente.Id, ServicioId = _e.Consulta.Id };
        _e.Citas.Agregar(reservada);
        Assert.Throws<ReglaNegocioException>(() =>
            _facturacion.Calcular(_e.Paciente.Id, [new LineaFactura(_e.Consulta.Id, 1, reservada.Id)]));
    }

    [Fact]
    public void LaMismaCitaNoPuedeIrEnDosLineas()
    {
        var cita = CitaAtendida();

        var error = Assert.Throws<ReglaNegocioException>(() => _facturacion.Calcular(_e.Paciente.Id,
            [new LineaFactura(_e.Consulta.Id, 1, cita.Id), new LineaFactura(_e.Consulta.Id, 1, cita.Id)]));
        Assert.Contains("dos veces", error.Message);
    }

    [Fact]
    public void RechazaFacturaVaciaYCantidadesInvalidas()
    {
        Assert.Throws<ReglaNegocioException>(() => _facturacion.Emitir(_e.Paciente.Id, []));
        Assert.Throws<ReglaNegocioException>(() =>
            _facturacion.Calcular(_e.Paciente.Id, [new LineaFactura(_e.Certificado.Id, 0)]));
    }

    [Fact]
    public void UnServicioDesactivadoNoSeVendeSuelto()
    {
        _e.Certificado.Activo = false;
        _e.Servicios.Actualizar(_e.Certificado);

        Assert.Throws<ReglaNegocioException>(() =>
            _facturacion.Calcular(_e.Paciente.Id, [new LineaFactura(_e.Certificado.Id, 1)]));
    }
}
