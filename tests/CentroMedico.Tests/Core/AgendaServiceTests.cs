using CentroMedico.Core.Modelos;
using CentroMedico.Core.Servicios;

namespace CentroMedico.Tests.Core;

public class AgendaServiceTests
{
    private readonly Escenario _e = new();
    private readonly AgendaService _agenda;
    private static readonly DateOnly Lunes = DateOnly.FromDateTime(Escenario.Lunes);

    public AgendaServiceTests()
    {
        _agenda = new AgendaService(_e.Citas, _e.Pacientes, _e.Empleados, _e.Servicios, _e.Reloj);
    }

    private Cita Reservar(int hora, int minuto = 0) =>
        _agenda.Reservar(_e.Paciente.Id, _e.Medico.Id, _e.Consulta.Id, Lunes.ToDateTime(new TimeOnly(hora, minuto)));

    [Fact]
    public void EntreSemanaHay16TurnosSinLaHoraDeAlmuerzo()
    {
        var turnos = AgendaService.Turnos(Lunes).ToList();

        Assert.Equal(16, turnos.Count);
        Assert.Equal(new TimeOnly(8, 0), turnos[0]);
        Assert.Equal(new TimeOnly(16, 30), turnos[^1]);
        Assert.DoesNotContain(new TimeOnly(13, 0), turnos);
        Assert.DoesNotContain(new TimeOnly(13, 30), turnos);
    }

    [Fact]
    public void SabadoSoloEnLaMananaYDomingoCerrado()
    {
        Assert.Equal(8, AgendaService.Turnos(Lunes.AddDays(5)).Count());
        Assert.Empty(AgendaService.Turnos(Lunes.AddDays(6)));
    }

    [Fact]
    public void UnaCitaReservadaOcupaElTurnoDelMedico()
    {
        Reservar(9);

        var libres = _agenda.HorariosDisponibles(_e.Medico.Id, Lunes);

        Assert.Equal(15, libres.Count);
        Assert.DoesNotContain(new TimeOnly(9, 0), libres);
    }

    [Fact]
    public void UnaCitaCanceladaLiberaElTurno()
    {
        var cita = Reservar(9);
        _agenda.CambiarEstado(cita.Id, EstadoCita.Cancelada);

        Assert.Contains(new TimeOnly(9, 0), _agenda.HorariosDisponibles(_e.Medico.Id, Lunes));
        Reservar(9);
    }

    [Fact]
    public void NoOfreceTurnosQueYaPasaron()
    {
        _e.Reloj.Ahora = Lunes.ToDateTime(new TimeOnly(10, 15));

        var libres = _agenda.HorariosDisponibles(_e.Medico.Id, Lunes);

        Assert.Equal(new TimeOnly(10, 30), libres[0]);
        var error = Assert.Throws<ReglaNegocioException>(() => Reservar(10));
        Assert.Contains("ya pasó", error.Message);
    }

    [Fact]
    public void NoPermiteDosCitasDelMedicoALaMismaHora()
    {
        Reservar(9);
        var otro = new Paciente { Cedula = "1710034065", Nombres = "Diego", Apellidos = "Ochoa" };
        _e.Pacientes.Agregar(otro);

        var error = Assert.Throws<ReglaNegocioException>(() =>
            _agenda.Reservar(otro.Id, _e.Medico.Id, _e.Consulta.Id, Lunes.ToDateTime(new TimeOnly(9, 0))));
        Assert.Equal("El médico ya tiene una cita a las 09:00.", error.Message);
    }

    [Fact]
    public void NoPermiteAlPacienteDosCitasALaMismaHoraConDistintosMedicos()
    {
        Reservar(9);
        var otroMedico = new Empleado { Nombres = "Ana", Apellidos = "Medina", Cargo = Cargo.Medico, Especialidad = "Pediatría" };
        _e.Empleados.Agregar(otroMedico);

        var error = Assert.Throws<ReglaNegocioException>(() =>
            _agenda.Reservar(_e.Paciente.Id, otroMedico.Id, _e.Consulta.Id, Lunes.ToDateTime(new TimeOnly(9, 0))));
        Assert.Contains("otra cita", error.Message);
    }

    [Theory]
    [InlineData(13, 0)]
    [InlineData(17, 0)]
    [InlineData(9, 15)]
    public void RechazaHorasFueraDelHorario(int hora, int minuto)
    {
        var error = Assert.Throws<ReglaNegocioException>(() => Reservar(hora, minuto));
        Assert.Contains("fuera del horario", error.Message);
    }

    [Fact]
    public void SoloSeReservaConMedicosYPacientesActivos()
    {
        Assert.Throws<ReglaNegocioException>(() =>
            _agenda.Reservar(_e.Paciente.Id, _e.Recepcionista.Id, _e.Consulta.Id, Lunes.ToDateTime(new TimeOnly(9, 0))));

        _e.Paciente.Activo = false;
        _e.Pacientes.Actualizar(_e.Paciente);
        var error = Assert.Throws<ReglaNegocioException>(() => Reservar(9));
        Assert.Equal("El paciente está desactivado.", error.Message);
    }

    [Fact]
    public void NoSeMarcaAtendidaUnaCitaFutura()
    {
        var cita = Reservar(9);

        Assert.Throws<ReglaNegocioException>(() => _agenda.CambiarEstado(cita.Id, EstadoCita.Atendida));

        _e.Reloj.Ahora = Lunes.ToDateTime(new TimeOnly(9, 20));
        _agenda.CambiarEstado(cita.Id, EstadoCita.Atendida);
        Assert.True(_e.Citas.ObtenerPorId(cita.Id)!.PendienteDeFacturar);
    }

    [Fact]
    public void UnaCitaCerradaNoCambiaDeEstado()
    {
        var cita = Reservar(9);
        _agenda.CambiarEstado(cita.Id, EstadoCita.Cancelada);

        var error = Assert.Throws<ReglaNegocioException>(() => _agenda.CambiarEstado(cita.Id, EstadoCita.Atendida));
        Assert.Equal("La cita ya está cancelada y no se puede cambiar.", error.Message);
    }
}
