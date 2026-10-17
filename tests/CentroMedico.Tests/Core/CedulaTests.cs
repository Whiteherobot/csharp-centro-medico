using CentroMedico.Core.Validacion;

namespace CentroMedico.Tests.Core;

public class CedulaTests
{
    [Theory]
    [InlineData("1710034065")]
    [InlineData("0104567896")]
    [InlineData("0923456784")]
    [InlineData("2409876543")]
    public void AceptaCedulasConVerificadorCorrecto(string cedula) =>
        Assert.True(Cedula.EsValida(cedula));

    [Theory]
    [InlineData("1710034066")]   // verificador cambiado
    [InlineData("171003406")]    // 9 dígitos
    [InlineData("17100340655")]  // 11 dígitos
    [InlineData("17100340a5")]   // letra
    [InlineData("2510034060")]   // provincia 25 no existe
    [InlineData("1760034060")]   // tercer dígito 6 = sociedad pública, no persona
    [InlineData("")]
    [InlineData(null)]
    public void RechazaCedulasInvalidas(string? cedula) =>
        Assert.False(Cedula.EsValida(cedula));
}
