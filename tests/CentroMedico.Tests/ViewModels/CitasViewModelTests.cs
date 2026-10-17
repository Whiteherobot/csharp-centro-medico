using CentroMedico.Core.Modelos;
using CentroMedico.ViewModels;

namespace CentroMedico.Tests.ViewModels;

public class CitasViewModelTests
{
    private readonly RelojFijo _reloj = new(Escenario.Lunes);
    private readonly DialogosFalsos _dialogos = new();
    private readonly Contexto _contexto;
    private readonly CitasViewModel _vm;

    public CitasViewModelTests()
    {
        _contexto = Demo.Crear(_reloj, _dialogos);
        _vm = new CitasViewModel(_contexto);
    }

    private int Medico(string apellido) => _vm.Medicos.Single(m => m.Texto.Contains(apellido)).Valor;

    [Fact]
    public void AbreConLaAgendaDeHoyYFiltraPorMedico()
    {
        Assert.Equal(3, _vm.Citas.Count);

        _vm.MedicoFiltro = Medico("Medina");
        Assert.Equal("Diego Ochoa Pesántez", Assert.Single(_vm.Citas).Paciente);

        _vm.FechaAgenda = Escenario.Lunes.AddDays(1);
        Assert.True(_vm.AgendaVacia);
    }

    [Fact]
    public void LaCedulaCompletaEligeAlPaciente()
    {
        _vm.BuscarPaciente = "0104567896";
        Assert.NotNull(_vm.PacienteId);

        _vm.BuscarPaciente = "zzz";
        Assert.Null(_vm.PacienteId);
    }

    [Fact]
    public void LosHorariosSeCalculanPorMedicoYFecha()
    {
        Assert.Empty(_vm.Horarios);
        Assert.Equal("Elija médico y fecha para ver los turnos libres.", _vm.AvisoHorarios);

        _vm.MedicoId = Medico("Vintimilla");

        Assert.Equal(14, _vm.Horarios.Count);   // 16 turnos menos 2 ya reservados en el demo
        _vm.FechaCita = Escenario.Lunes.AddDays(6);  // domingo
        Assert.Equal("No hay turnos libres ese día.", _vm.AvisoHorarios);
    }

    [Fact]
    public void ReservarAgregaLaCitaYQuitaElTurno()
    {
        _vm.BuscarPaciente = "1309873212";
        _vm.MedicoId = Medico("Vintimilla");
        _vm.ServicioId = _vm.Servicios.First().Valor;
        _vm.FechaCita = Escenario.Lunes.AddDays(1);
        Assert.False(_vm.ReservarCommand.CanExecute(null));
        _vm.Horario = new TimeOnly(15, 30);

        _vm.ReservarCommand.Execute(null);

        Assert.Equal("Cita reservada el martes 20 de octubre a las 15:30 con Andrés Vintimilla Ríos (Medicina general).", _vm.Mensaje);
        Assert.Equal(Escenario.Lunes.AddDays(1).Date, _vm.FechaAgenda);
        Assert.Single(_vm.Citas);
        Assert.DoesNotContain(_vm.Horarios, h => h.Valor == new TimeOnly(15, 30));
        Assert.Null(_vm.Horario);
    }

    [Fact]
    public void CancelarPideConfirmacionYLiberaElTurno()
    {
        _vm.CitaSeleccionada = _vm.Citas[0];
        _dialogos.Respuesta = false;
        _vm.CancelarCommand.Execute(null);
        Assert.Equal(EstadoCita.Reservada, _contexto.Citas.ObtenerPorId(_vm.Citas[0].Cita.Id)!.Estado);

        _dialogos.Respuesta = true;
        _vm.CancelarCommand.Execute(null);

        Assert.Equal("Cancelada", _vm.CitaSeleccionada!.Estado);
        Assert.False(_vm.CancelarCommand.CanExecute(null));
        Assert.Equal(2, _dialogos.Preguntas.Count);
    }

    [Fact]
    public void SoloSeAtiendeCuandoLlegaLaHora()
    {
        _vm.CitaSeleccionada = _vm.Citas[0];
        _vm.AtenderCommand.Execute(null);
        Assert.Equal("La cita todavía no ocurre.", _vm.Error);

        _reloj.Ahora = _vm.Citas[0].Cita.FechaHora.AddMinutes(10);
        _vm.AtenderCommand.Execute(null);
        Assert.Equal("Atendida (por facturar)", _vm.CitaSeleccionada!.Estado);
    }
}
