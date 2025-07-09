using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DentalPro.Domain.Constants;
using DentalPro.Domain.Entities;

namespace DentalPro.Infrastructure.Persistence.Initialization;

/// <summary>
/// Clase encargada de inicializar los permisos predefinidos en el sistema
/// </summary>
public class PermisosInicializador
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PermisosInicializador> _logger;

    public PermisosInicializador(ApplicationDbContext context, ILogger<PermisosInicializador> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Inicializa los permisos predeterminados del sistema
    /// </summary>
    public async Task InicializarPermisosAsync()
    {
        try
        {
            // Verificar si ya existen permisos
            if (await _context.Permisos.AnyAsync())
            {
                _logger.LogInformation("Los permisos ya están inicializados.");
                return;
            }

            _logger.LogInformation("Iniciando la creación de permisos predefinidos...");

            // Crear permisos del sistema agrupados por módulo
            var permisos = new List<Permiso>();

            // Módulo: Sistema
            AgregarPermisosModulo(permisos, PermisoCodigos.Sistema.Modulo, new Dictionary<string, string>
            {
                { PermisoCodigos.Sistema.AccesoSistema, "Acceso al sistema" },
                { PermisoCodigos.Sistema.AdministrarRoles, "Administrar roles del sistema" },
                { PermisoCodigos.Sistema.AdministrarPermisos, "Administrar permisos y asignaciones" },
                { PermisoCodigos.Sistema.AdministrarUsuarios, "Administrar usuarios del sistema" },
                { PermisoCodigos.Sistema.ConfiguracionGeneral, "Configuración general del sistema" }
            }, esOperacion: false);

            // Módulo: Consultorio
            AgregarPermisosModulo(permisos, PermisoCodigos.Consultorio.Modulo, new Dictionary<string, string>
            {
                { PermisoCodigos.Consultorio.VerConsultorio, "Ver información del consultorio" },
                { PermisoCodigos.Consultorio.CrearConsultorio, "Crear nuevo consultorio" },
                { PermisoCodigos.Consultorio.EditarConsultorio, "Editar información del consultorio" },
                { PermisoCodigos.Consultorio.EliminarConsultorio, "Eliminar consultorio" },
                { PermisoCodigos.Consultorio.AdministrarSuscripcion, "Administrar suscripción del consultorio" }
            });

            // Módulo: Pacientes
            AgregarPermisosModulo(permisos, PermisoCodigos.Pacientes.Modulo, new Dictionary<string, string>
            {
                { PermisoCodigos.Pacientes.VerPacientes, "Ver listado de pacientes" },
                { PermisoCodigos.Pacientes.CrearPaciente, "Crear nuevo paciente" },
                { PermisoCodigos.Pacientes.EditarPaciente, "Editar información de paciente" },
                { PermisoCodigos.Pacientes.EliminarPaciente, "Eliminar paciente" },
                { PermisoCodigos.Pacientes.GestionarExpediente, "Gestionar expediente clínico" },
                { PermisoCodigos.Pacientes.VerExpediente, "Ver expediente clínico" }
            });

            // Módulo: Citas
            AgregarPermisosModulo(permisos, PermisoCodigos.Citas.Modulo, new Dictionary<string, string>
            {
                { PermisoCodigos.Citas.VerCitas, "Ver agenda de citas" },
                { PermisoCodigos.Citas.CrearCita, "Agendar nueva cita" },
                { PermisoCodigos.Citas.EditarCita, "Modificar cita existente" },
                { PermisoCodigos.Citas.EliminarCita, "Eliminar cita" },
                { PermisoCodigos.Citas.ConfirmarCita, "Confirmar asistencia a cita" },
                { PermisoCodigos.Citas.CancelarCita, "Cancelar cita programada" },
                { PermisoCodigos.Citas.GestionarRecordatorios, "Gestionar recordatorios de citas" }
            });

            // Módulo: Tratamientos
            AgregarPermisosModulo(permisos, PermisoCodigos.Tratamientos.Modulo, new Dictionary<string, string>
            {
                { PermisoCodigos.Tratamientos.VerTratamientos, "Ver tratamientos de pacientes" },
                { PermisoCodigos.Tratamientos.CrearTratamiento, "Crear nuevo tratamiento" },
                { PermisoCodigos.Tratamientos.EditarTratamiento, "Editar tratamiento existente" },
                { PermisoCodigos.Tratamientos.EliminarTratamiento, "Eliminar tratamiento" },
                { PermisoCodigos.Tratamientos.GestionarSeguimiento, "Gestionar seguimiento de tratamientos" }
            });

            // Módulo: Servicios
            AgregarPermisosModulo(permisos, PermisoCodigos.Servicios.Modulo, new Dictionary<string, string>
            {
                { PermisoCodigos.Servicios.VerServicios, "Ver catálogo de servicios" },
                { PermisoCodigos.Servicios.CrearServicio, "Crear nuevo servicio" },
                { PermisoCodigos.Servicios.EditarServicio, "Editar servicio existente" },
                { PermisoCodigos.Servicios.EliminarServicio, "Eliminar servicio" }
            });

            // Módulo: Finanzas
            AgregarPermisosModulo(permisos, PermisoCodigos.Finanzas.Modulo, new Dictionary<string, string>
            {
                { PermisoCodigos.Finanzas.VerPagos, "Ver registro de pagos" },
                { PermisoCodigos.Finanzas.RegistrarPago, "Registrar nuevo pago" },
                { PermisoCodigos.Finanzas.EditarPago, "Editar información de pago" },
                { PermisoCodigos.Finanzas.AnularPago, "Anular pago registrado" },
                { PermisoCodigos.Finanzas.GestionarFacturas, "Gestionar facturas" },
                { PermisoCodigos.Finanzas.VerReportes, "Ver reportes financieros" },
                { PermisoCodigos.Finanzas.ExportarReportes, "Exportar reportes financieros" }
            });

            // Módulo: Doctores
            AgregarPermisosModulo(permisos, PermisoCodigos.Doctores.Modulo, new Dictionary<string, string>
            {
                { PermisoCodigos.Doctores.VerDoctores, "Ver información de doctores" },
                { PermisoCodigos.Doctores.CrearDoctor, "Registrar nuevo doctor" },
                { PermisoCodigos.Doctores.EditarDoctor, "Editar información de doctor" },
                { PermisoCodigos.Doctores.GestionarEspecialidades, "Gestionar especialidades médicas" },
                { PermisoCodigos.Doctores.GestionarDisponibilidad, "Gestionar disponibilidad de doctores" }
            });

            // Módulo: Reportes
            AgregarPermisosModulo(permisos, PermisoCodigos.Reportes.Modulo, new Dictionary<string, string>
            {
                { PermisoCodigos.Reportes.VerReportesCitas, "Ver reportes de citas" },
                { PermisoCodigos.Reportes.VerReportesPacientes, "Ver reportes de pacientes" },
                { PermisoCodigos.Reportes.VerReportesTratamientos, "Ver reportes de tratamientos" },
                { PermisoCodigos.Reportes.VerReportesFinanzas, "Ver reportes financieros" },
                { PermisoCodigos.Reportes.ExportarReportes, "Exportar reportes a diferentes formatos" }
            });

            // Agregar permisos de acceso a módulo (permisos generales)
            foreach (var modulo in PermisoCodigos.ObtenerTodosModulos())
            {
                permisos.Add(new Permiso
                {
                    IdPermiso = Guid.NewGuid(),
                    Codigo = $"{modulo}.Acceso",
                    Nombre = $"Acceso al módulo {modulo}",
                    Descripcion = $"Permite acceder al módulo {modulo}",
                    Modulo = modulo,
                    EsOperacion = false,
                    PredeterminadoSistema = true
                });
            }

            // Guardar permisos en base de datos
            await _context.Permisos.AddRangeAsync(permisos);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Se han creado {permisos.Count} permisos predefinidos en el sistema.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al inicializar los permisos: {Mensaje}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Agrega permisos de un módulo específico a la lista
    /// </summary>
    private void AgregarPermisosModulo(List<Permiso> permisos, string modulo, Dictionary<string, string> operaciones, bool esOperacion = true)
    {
        foreach (var op in operaciones)
        {
            permisos.Add(new Permiso
            {
                IdPermiso = Guid.NewGuid(),
                Codigo = op.Key,
                Nombre = op.Value,
                Descripcion = $"Permiso para {op.Value.ToLower()} en el módulo {modulo}",
                Modulo = modulo,
                EsOperacion = esOperacion,
                PredeterminadoSistema = true
            });
        }
    }
}
