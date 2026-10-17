namespace CentroMedico.Core.Servicios;

/// <summary>Error de negocio que se muestra tal cual al usuario.</summary>
public class ReglaNegocioException(string mensaje) : Exception(mensaje);
