using CentroMedico.ViewModels;

namespace CentroMedico.Tests.ViewModels;

public class ServiciosViewModelTests
{
    private readonly Contexto _contexto = Demo.Crear();
    private readonly ServiciosViewModel _vm;

    public ServiciosViewModelTests()
    {
        _vm = new ServiciosViewModel(_contexto);
    }

    [Theory]
    [InlineData("12,50", 12.50)]
    [InlineData("12.50", 12.50)]
    [InlineData("$ 8", 8)]
    public void AceptaComaOPuntoEnElPrecio(string texto, decimal esperado)
    {
        Assert.True(ServiciosViewModel.TryLeerPrecio(texto, out var precio));
        Assert.Equal(esperado, precio);
    }

    [Fact]
    public void MuestraPrecioEIvaConFormatoLocal()
    {
        var curacion = _vm.Servicios.Single(s => s.Nombre == "Curación con insumos");

        Assert.Equal("$ 12,50", curacion.Precio);
        Assert.Equal("15 %", curacion.Iva);
    }

    [Fact]
    public void AgregaUnServicioNuevo()
    {
        _vm.Nombre = "Electrocardiograma";
        _vm.Precio = "35,00";
        _vm.GuardarCommand.Execute(null);

        Assert.Equal("Servicio agregado.", _vm.Mensaje);
        Assert.Equal(6, _vm.Servicios.Count);
        Assert.Equal("Editar servicio", _vm.TituloFormulario);
    }

    [Fact]
    public void RechazaPrecioQueNoEsNumero()
    {
        _vm.Nombre = "Ecografía";
        _vm.Precio = "veinte";
        _vm.GuardarCommand.Execute(null);

        Assert.Equal("Escriba el precio como número, por ejemplo 25,00.", _vm.Error);
    }

    [Fact]
    public void EditarCargaLosDatosDelServicio()
    {
        _vm.Seleccionado = _vm.Servicios.Single(s => s.Nombre == "Consulta pediátrica");

        Assert.Equal("30,00", _vm.Precio);
        _vm.Precio = "32,5";
        _vm.GuardarCommand.Execute(null);

        Assert.Equal("$ 32,50", _vm.Seleccionado!.Precio);
    }
}
