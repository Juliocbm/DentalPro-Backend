namespace DentalPro.Domain.Constants;

/// <summary>
/// Define los códigos de permisos disponibles en el sistema.
/// Los permisos están organizados por módulos.
/// </summary>
public static class PermisoCodigos
{
    // Módulo: Sistema
    public static class Sistema
    {
        public const string Modulo = "Sistema";
        
        public const string AccesoSistema = "Sistema.Acceso";
        public const string AdministrarRoles = "Sistema.AdministrarRoles";
        public const string AdministrarPermisos = "Sistema.AdministrarPermisos";
        public const string AdministrarUsuarios = "Sistema.AdministrarUsuarios";
        public const string ConfiguracionGeneral = "Sistema.ConfiguracionGeneral";
    }

    // Módulo: Consultorio
    public static class Consultorio
    {
        public const string Modulo = "Consultorio";
        
        public const string VerConsultorio = "Consultorio.Ver";
        public const string CrearConsultorio = "Consultorio.Crear";
        public const string EditarConsultorio = "Consultorio.Editar";
        public const string EliminarConsultorio = "Consultorio.Eliminar";
        public const string AdministrarSuscripcion = "Consultorio.AdministrarSuscripcion";
    }

    // Módulo: Pacientes
    public static class Pacientes
    {
        public const string Modulo = "Pacientes";
        
        public const string VerPacientes = "Pacientes.Ver";
        public const string CrearPaciente = "Pacientes.Crear";
        public const string EditarPaciente = "Pacientes.Editar";
        public const string EliminarPaciente = "Pacientes.Eliminar";
        public const string GestionarExpediente = "Pacientes.GestionarExpediente";
        public const string VerExpediente = "Pacientes.VerExpediente";
    }

    // Módulo: Citas
    public static class Citas
    {
        public const string Modulo = "Citas";
        
        public const string VerCitas = "Citas.Ver";
        public const string CrearCita = "Citas.Crear";
        public const string EditarCita = "Citas.Editar";
        public const string EliminarCita = "Citas.Eliminar";
        public const string ConfirmarCita = "Citas.Confirmar";
        public const string CancelarCita = "Citas.Cancelar";
        public const string GestionarRecordatorios = "Citas.GestionarRecordatorios";
    }

    // Módulo: Tratamientos
    public static class Tratamientos
    {
        public const string Modulo = "Tratamientos";
        
        public const string VerTratamientos = "Tratamientos.Ver";
        public const string CrearTratamiento = "Tratamientos.Crear";
        public const string EditarTratamiento = "Tratamientos.Editar";
        public const string EliminarTratamiento = "Tratamientos.Eliminar";
        public const string GestionarSeguimiento = "Tratamientos.GestionarSeguimiento";
    }

    // Módulo: Servicios
    public static class Servicios
    {
        public const string Modulo = "Servicios";
        
        public const string VerServicios = "Servicios.Ver";
        public const string CrearServicio = "Servicios.Crear";
        public const string EditarServicio = "Servicios.Editar";
        public const string EliminarServicio = "Servicios.Eliminar";
    }

    // Módulo: Finanzas
    public static class Finanzas
    {
        public const string Modulo = "Finanzas";
        
        public const string VerPagos = "Finanzas.VerPagos";
        public const string RegistrarPago = "Finanzas.RegistrarPago";
        public const string EditarPago = "Finanzas.EditarPago";
        public const string AnularPago = "Finanzas.AnularPago";
        public const string GestionarFacturas = "Finanzas.GestionarFacturas";
        public const string VerReportes = "Finanzas.VerReportes";
        public const string ExportarReportes = "Finanzas.ExportarReportes";
    }

    // Módulo: Doctores
    public static class Doctores
    {
        public const string Modulo = "Doctores";
        
        public const string VerDoctores = "Doctores.Ver";
        public const string CrearDoctor = "Doctores.Crear";
        public const string EditarDoctor = "Doctores.Editar";
        public const string GestionarEspecialidades = "Doctores.GestionarEspecialidades";
        public const string GestionarDisponibilidad = "Doctores.GestionarDisponibilidad";
    }

    // Módulo: Reportes
    public static class Reportes
    {
        public const string Modulo = "Reportes";
        
        public const string VerReportesCitas = "Reportes.VerCitas";
        public const string VerReportesPacientes = "Reportes.VerPacientes";
        public const string VerReportesTratamientos = "Reportes.VerTratamientos";
        public const string VerReportesFinanzas = "Reportes.VerFinanzas";
        public const string ExportarReportes = "Reportes.Exportar";
    }

    // Obtener todos los módulos definidos
    public static string[] ObtenerTodosModulos()
    {
        return new[]
        {
            Sistema.Modulo,
            Consultorio.Modulo,
            Pacientes.Modulo,
            Citas.Modulo,
            Tratamientos.Modulo,
            Servicios.Modulo,
            Finanzas.Modulo,
            Doctores.Modulo,
            Reportes.Modulo
        };
    }
}
