using System.Windows.Input;
using CentroMedico.Core.Modelos;
using CentroMedico.Core.Servicios;

namespace CentroMedico.ViewModels;

public record EmpleadoFila(Empleado Empleado, Usuario? Cuenta)
{
    public string Cedula => Empleado.Cedula;
    public string Nombre => $"{Empleado.Apellidos} {Empleado.Nombres}";
    public string Cargo => Empleado.EsMedico
        ? $"Médico ({Empleado.Especialidad})"
        : PersonalViewModel.TextoCargo(Empleado.Cargo);
    public string CuentaTexto => Cuenta is null ? "Sin cuenta" : Cuenta.Activo ? Cuenta.NombreUsuario : $"{Cuenta.NombreUsuario} (desactivada)";
    public string Estado => Empleado.Activo ? "Activo" : "Inactivo";
}

/// <summary>Empleados del centro y sus cuentas de acceso. Solo para administradores.</summary>
public class PersonalViewModel : ViewModelBase
{
    private readonly Contexto _c;
    private readonly Sesion _sesion;
    private string _filtro = "";
    private IReadOnlyList<EmpleadoFila> _empleados = [];
    private EmpleadoFila? _seleccionado;
    private Cargo _cargo = Cargo.Atencion;
    private string _especialidad = "";
    private string _nuevoUsuario = "";
    private string _clave = "";
    private Rol _rolCuenta = Rol.Recepcion;
    private bool _cuentaActiva = true;

    public PersonalViewModel(Contexto contexto, Sesion sesion)
    {
        _c = contexto;
        _sesion = sesion;
        NuevoCommand = Comando(Nuevo);
        GuardarCommand = Comando(Guardar);
        CrearCuentaCommand = Comando(CrearCuenta, () => Seleccionado is { Cuenta: null } && NuevoUsuario.Length > 0 && Clave.Length > 0);
        GuardarCuentaCommand = Comando(GuardarCuenta, () => TieneCuenta);
        RestablecerCommand = Comando(Restablecer, () => TieneCuenta && Clave.Length > 0);
        Buscar();
    }

    /// <summary>La vista vacía el PasswordBox cuando la clave se usó.</summary>
    public event Action? ClaveUsada;

    public DatosPersona Datos { get; } = new();

    public IReadOnlyList<Opcion<Cargo>> Cargos { get; } =
        Enum.GetValues<Cargo>().Select(c => new Opcion<Cargo>(c, TextoCargo(c))).ToList();

    public IReadOnlyList<Opcion<Rol>> Roles { get; } =
    [
        new(Rol.Recepcion, "Recepción (pacientes, citas y facturas)"),
        new(Rol.Administrador, "Administrador (además personal y servicios)")
    ];

    public string Filtro
    {
        get => _filtro;
        set { if (Set(ref _filtro, value)) Buscar(); }
    }

    public IReadOnlyList<EmpleadoFila> Empleados
    {
        get => _empleados;
        private set => Set(ref _empleados, value);
    }

    public EmpleadoFila? Seleccionado
    {
        get => _seleccionado;
        set
        {
            if (!Set(ref _seleccionado, value)) return;
            Error = Mensaje = null;
            Cargar(value);
            OnPropertyChanged(nameof(EsNuevo));
            OnPropertyChanged(nameof(TituloFormulario));
            OnPropertyChanged(nameof(TieneCuenta));
            OnPropertyChanged(nameof(PuedeCrearCuenta));
            OnPropertyChanged(nameof(UsuarioActual));
        }
    }

    public Cargo Cargo
    {
        get => _cargo;
        set { if (Set(ref _cargo, value)) OnPropertyChanged(nameof(EsMedico)); }
    }

    public bool EsMedico => Cargo == Cargo.Medico;

    public string Especialidad { get => _especialidad; set => Set(ref _especialidad, value); }

    public bool EsNuevo => Seleccionado is null;
    public string TituloFormulario => EsNuevo ? "Nuevo empleado" : "Editar empleado";
    public bool TieneCuenta => Seleccionado?.Cuenta is not null;
    public bool PuedeCrearCuenta => Seleccionado is { Cuenta: null };
    public string UsuarioActual => Seleccionado?.Cuenta?.NombreUsuario ?? "";

    public string NuevoUsuario { get => _nuevoUsuario; set => Set(ref _nuevoUsuario, value); }

    /// <summary>Clave para la cuenta nueva o la que asigna el administrador al restablecer.</summary>
    public string Clave { get => _clave; set => Set(ref _clave, value); }

    public Rol RolCuenta { get => _rolCuenta; set => Set(ref _rolCuenta, value); }
    public bool CuentaActiva { get => _cuentaActiva; set => Set(ref _cuentaActiva, value); }

    public ICommand NuevoCommand { get; }
    public ICommand GuardarCommand { get; }
    public ICommand CrearCuentaCommand { get; }
    public ICommand GuardarCuentaCommand { get; }
    public ICommand RestablecerCommand { get; }

    public static string TextoCargo(Cargo cargo) => cargo switch
    {
        Cargo.Medico => "Médico",
        Cargo.Administracion => "Administración",
        _ => "Atención al cliente"
    };

    private void Buscar() =>
        Empleados = _c.Empleados.Listar(Filtro)
            .Select(e => new EmpleadoFila(e, _c.Usuarios.ObtenerPorEmpleado(e.Id)))
            .ToList();

    private void Nuevo() => Seleccionado = null;

    private void Cargar(EmpleadoFila? fila)
    {
        Datos.Cargar(fila?.Empleado);
        Cargo = fila?.Empleado.Cargo ?? Cargo.Atencion;
        Especialidad = fila?.Empleado.Especialidad ?? "";
        NuevoUsuario = Sugerencia(fila?.Empleado);
        Clave = "";
        RolCuenta = fila?.Cuenta?.Rol ?? Rol.Recepcion;
        CuentaActiva = fila?.Cuenta?.Activo ?? true;
        ClaveUsada?.Invoke();
    }

    /// <summary>Inicial del nombre más el primer apellido: "Rosa Cabrera Loja" → "rcabrera".</summary>
    private static string Sugerencia(Empleado? e)
    {
        if (e is null || e.Nombres.Length == 0 || e.Apellidos.Length == 0) return "";
        string texto = (e.Nombres[..1] + e.Apellidos.Split(' ')[0]).ToLowerInvariant();
        var limpio = texto.Normalize(System.Text.NormalizationForm.FormD)
            .Where(ch => char.IsAsciiLetterOrDigit(ch));
        return string.Concat(limpio);
    }

    private void Guardar()
    {
        var empleado = Seleccionado is { } fila ? _c.Empleados.ObtenerPorId(fila.Empleado.Id) ?? new Empleado() : new Empleado();
        bool eraNuevo = empleado.Id == 0;

        bool listo = Intentar(() =>
        {
            if (empleado.Id == _sesion.Empleado.Id && !Datos.Activo)
                throw new ReglaNegocioException("No puede desactivarse a sí mismo.");
            Datos.Aplicar(empleado);
            empleado.Cargo = Cargo;
            empleado.Especialidad = EsMedico ? Especialidad.Trim() : null;
            _c.Registro.GuardarEmpleado(empleado);
        }, eraNuevo ? "Empleado registrado. Ya puede crearle una cuenta de acceso." : "Cambios guardados.");

        if (listo) Refrescar(empleado.Id);
    }

    private void CrearCuenta()
    {
        int id = Seleccionado!.Empleado.Id;
        if (Intentar(() => _c.Autenticacion.CrearCuenta(id, NuevoUsuario, Clave, RolCuenta), "Cuenta creada."))
            Refrescar(id);
    }

    private void GuardarCuenta()
    {
        int id = Seleccionado!.Empleado.Id;
        if (Intentar(() => _c.Autenticacion.ConfigurarCuenta(_sesion, id, RolCuenta, CuentaActiva), "Cuenta actualizada."))
            Refrescar(id);
    }

    private void Restablecer()
    {
        int id = Seleccionado!.Empleado.Id;
        if (Intentar(() => _c.Autenticacion.RestablecerContrasena(_sesion, id, Clave),
                $"Contraseña restablecida. Entréguela a {Seleccionado.Empleado.Nombres} para que la cambie al entrar."))
        {
            Clave = "";
            ClaveUsada?.Invoke();
        }
    }

    /// <summary>Recarga la tabla y vuelve a seleccionar al empleado sin perder el mensaje.</summary>
    private void Refrescar(int empleadoId)
    {
        string? mensaje = Mensaje;
        Buscar();
        Seleccionado = Empleados.FirstOrDefault(f => f.Empleado.Id == empleadoId);
        Mensaje = mensaje;
    }
}
