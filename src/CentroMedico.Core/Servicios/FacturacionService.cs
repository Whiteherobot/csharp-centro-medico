using CentroMedico.Core.Modelos;
using CentroMedico.Core.Repositorios;

namespace CentroMedico.Core.Servicios;

/// <summary>Línea que el usuario agrega a la factura antes de emitirla.</summary>
public record LineaFactura(int ServicioId, int Cantidad, int? CitaId = null);

public class FacturacionService(
    IFacturaRepository facturas,
    ICitaRepository citas,
    IPacienteRepository pacientes,
    IServicioRepository servicios,
    TimeProvider reloj)
{
    /// <summary>Tarifa general del IVA en Ecuador desde abril de 2024.</summary>
    public const decimal TarifaIva = 0.15m;

    public const string Establecimiento = "001";
    public const string PuntoEmision = "001";

    /// <summary>Citas atendidas del paciente que todavía no se cobraron.</summary>
    public IReadOnlyList<Cita> CitasPendientes(int pacienteId) =>
        citas.ListarPorPaciente(pacienteId).Where(c => c.PendienteDeFacturar).ToList();

    /// <summary>Arma la factura sin guardarla, para mostrar los totales en pantalla.</summary>
    public Factura Calcular(int pacienteId, IEnumerable<LineaFactura> lineas)
    {
        var factura = new Factura { PacienteId = pacienteId };

        foreach (var linea in lineas)
        {
            if (linea.Cantidad <= 0)
                throw new ReglaNegocioException("La cantidad debe ser mayor a cero.");

            var servicio = servicios.ObtenerPorId(linea.ServicioId)
                ?? throw new ReglaNegocioException("El servicio no existe.");

            if (linea.CitaId is { } citaId)
            {
                var cita = citas.ObtenerPorId(citaId);
                if (cita is null || cita.PacienteId != pacienteId)
                    throw new ReglaNegocioException("La cita no pertenece al paciente.");
                if (!cita.PendienteDeFacturar)
                    throw new ReglaNegocioException("La cita no está atendida o ya se facturó.");
                if (linea.Cantidad != 1 || cita.ServicioId != servicio.Id)
                    throw new ReglaNegocioException("Una cita se factura una vez y con su propio servicio.");
            }
            else if (!servicio.Activo)
            {
                throw new ReglaNegocioException($"El servicio {servicio.Nombre} está desactivado.");
            }

            factura.Detalles.Add(new DetalleFactura
            {
                ServicioId = servicio.Id,
                Descripcion = servicio.Nombre,
                Cantidad = linea.Cantidad,
                PrecioUnitario = servicio.Precio,
                GravaIva = servicio.GravaIva,
                CitaId = linea.CitaId
            });
        }

        bool citaRepetida = factura.Detalles
            .Where(d => d.CitaId is not null)
            .GroupBy(d => d.CitaId)
            .Any(g => g.Count() > 1);
        if (citaRepetida)
            throw new ReglaNegocioException("La misma cita está dos veces en la factura.");

        factura.SubtotalTarifa0 = factura.Detalles.Where(d => !d.GravaIva).Sum(d => d.Subtotal);
        factura.SubtotalGravado = factura.Detalles.Where(d => d.GravaIva).Sum(d => d.Subtotal);
        factura.Iva = Redondear(factura.SubtotalGravado * TarifaIva);
        factura.Total = factura.SubtotalTarifa0 + factura.SubtotalGravado + factura.Iva;
        return factura;
    }

    public Factura Emitir(int pacienteId, IEnumerable<LineaFactura> lineas)
    {
        var paciente = pacientes.ObtenerPorId(pacienteId)
            ?? throw new ReglaNegocioException("Seleccione un paciente.");

        var factura = Calcular(paciente.Id, lineas);
        if (factura.Detalles.Count == 0)
            throw new ReglaNegocioException("La factura no tiene servicios.");

        factura.Fecha = reloj.GetLocalNow().DateTime;
        factura.Numero = $"{Establecimiento}-{PuntoEmision}-{facturas.SiguienteSecuencial():D9}";
        facturas.Agregar(factura);
        return factura;
    }

    private static decimal Redondear(decimal valor) =>
        Math.Round(valor, 2, MidpointRounding.AwayFromZero);
}
