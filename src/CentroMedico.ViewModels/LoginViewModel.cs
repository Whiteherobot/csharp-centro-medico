using System.Windows.Input;
using CentroMedico.Core.Servicios;

namespace CentroMedico.ViewModels;

public class LoginViewModel : ViewModelBase
{
    private readonly Contexto _contexto;
    private string _usuario = "";
    private string _contrasena = "";

    public LoginViewModel(Contexto contexto)
    {
        _contexto = contexto;
        IngresarCommand = Comando(Ingresar,
            () => !string.IsNullOrWhiteSpace(Usuario) && Contrasena.Length > 0);
    }

    /// <summary>Se dispara al validar las credenciales; la vista abre la ventana principal.</summary>
    public event Action<Sesion>? SesionIniciada;

    public string Usuario
    {
        get => _usuario;
        set => Set(ref _usuario, value);
    }

    /// <summary>La vista la copia desde el PasswordBox, que no permite enlazar.</summary>
    public string Contrasena
    {
        get => _contrasena;
        set => Set(ref _contrasena, value);
    }

    public bool ModoDemo => _contexto.ModoDemo;

    public string AyudaDemo =>
        $"Modo demo, sin base de datos. Entre con {DatosDemo.UsuarioAdmin} / {DatosDemo.ClaveAdmin} " +
        $"o con {DatosDemo.UsuarioRecepcion} / {DatosDemo.ClaveRecepcion}.";

    public ICommand IngresarCommand { get; }

    private void Ingresar()
    {
        Sesion? sesion = null;
        if (Intentar(() => sesion = _contexto.Autenticacion.IniciarSesion(Usuario, Contrasena)))
            SesionIniciada?.Invoke(sesion!);
        else
            Contrasena = "";
    }
}
