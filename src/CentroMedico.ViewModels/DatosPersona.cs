using CentroMedico.Core.Modelos;

namespace CentroMedico.ViewModels;

/// <summary>Campos de formulario comunes a pacientes y empleados.</summary>
public class DatosPersona : ViewModelBase
{
    private string _cedula = "";
    private string _nombres = "";
    private string _apellidos = "";
    private string _direccion = "";
    private string _telefono = "";
    private string _email = "";
    private bool _activo = true;

    public string Cedula { get => _cedula; set => Set(ref _cedula, value); }
    public string Nombres { get => _nombres; set => Set(ref _nombres, value); }
    public string Apellidos { get => _apellidos; set => Set(ref _apellidos, value); }
    public string Direccion { get => _direccion; set => Set(ref _direccion, value); }
    public string Telefono { get => _telefono; set => Set(ref _telefono, value); }
    public string Email { get => _email; set => Set(ref _email, value); }
    public bool Activo { get => _activo; set => Set(ref _activo, value); }

    public void Cargar(Persona? p)
    {
        Cedula = p?.Cedula ?? "";
        Nombres = p?.Nombres ?? "";
        Apellidos = p?.Apellidos ?? "";
        Direccion = p?.Direccion ?? "";
        Telefono = p?.Telefono ?? "";
        Email = p?.Email ?? "";
        Activo = p?.Activo ?? true;
    }

    public void Aplicar(Persona p)
    {
        p.Cedula = Cedula;
        p.Nombres = Nombres;
        p.Apellidos = Apellidos;
        p.Direccion = Direccion;
        p.Telefono = Telefono;
        p.Email = Email;
        p.Activo = Activo;
    }
}
