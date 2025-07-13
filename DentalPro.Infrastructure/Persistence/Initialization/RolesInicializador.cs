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
/// Clase encargada de inicializar los roles predefinidos del sistema y asignar sus permisos por defecto
/// </summary>
public class RolesInicializador
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<RolesInicializador> _logger;

    public RolesInicializador(ApplicationDbContext context, ILogger<RolesInicializador> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Inicializa los roles predeterminados del sistema y asigna permisos
    /// </summary>
    public async Task InicializarRolesAsync()
    {
        try
        {
            // Obtener permisos existentes (deben haber sido inicializados previamente)
            var permisos = await _context.Permisos.ToListAsync();
            
            if (!permisos.Any())
            {
                _logger.LogWarning("No se encontraron permisos para asignar a los roles. Ejecute primero la inicialización de permisos.");
                return;
            }

            // Verificar si ya existen roles predefinidos
            if (await _context.Roles.AnyAsync(r => r.EsSistema))
            {
                _logger.LogInformation("Los roles predefinidos ya están inicializados.");
                return;
            }

            _logger.LogInformation("Iniciando la creación de roles predefinidos...");

            // Crear roles predefinidos
            var roles = new List<Rol>
            {
                new Rol
                {
                    IdRol = Guid.NewGuid(),
                    Nombre = "SuperUsuario",
                    Descripcion = "Super usuario con acceso a todo el sistema, incluyendo datos de todos los consultorios",
                    EsSistema = true,
                    Activo = true
                },
                new Rol
                {
                    IdRol = Guid.NewGuid(),
                    Nombre = "Administrador",
                    Descripcion = "Administrador con acceso completo limitado a su propio consultorio",
                    EsSistema = true,
                    Activo = true
                },
                new Rol
                {
                    IdRol = Guid.NewGuid(),
                    Nombre = "Odontólogo",
                    Descripcion = "Doctor que realiza procedimientos dentales",
                    EsSistema = true,
                    Activo = true
                },
                new Rol
                {
                    IdRol = Guid.NewGuid(),
                    Nombre = "Recepcionista",
                    Descripcion = "Encargado de agenda, citas y atención inicial",
                    EsSistema = true,
                    Activo = true
                },
                new Rol
                {
                    IdRol = Guid.NewGuid(),
                    Nombre = "Asistente",
                    Descripcion = "Asistente dental que apoya en procedimientos",
                    EsSistema = true,
                    Activo = true
                },
                new Rol
                {
                    IdRol = Guid.NewGuid(),
                    Nombre = "AuxiliarRecepcion",
                    Descripcion = "Auxiliar con acceso limitado a funciones de recepción",
                    EsSistema = true,
                    Activo = true
                }
            };

            // Guardar roles en base de datos
            await _context.Roles.AddRangeAsync(roles);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Se han creado {roles.Count} roles predefinidos en el sistema.");

            // Asignar permisos a roles
            await AsignarPermisosARolesAsync(roles, permisos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al inicializar los roles: {Mensaje}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Asigna permisos predeterminados a los roles del sistema
    /// </summary>
    private async Task AsignarPermisosARolesAsync(List<Rol> roles, List<Permiso> permisos)
    {
        try
        {
            var rolPermisos = new List<RolPermiso>();
            
            // 1. Rol: SuperUsuario - Absolutamente todos los permisos sin restricción
            var rolSuperUsuario = roles.FirstOrDefault(r => r.Nombre == "SuperUsuario");
            if (rolSuperUsuario != null)
            {
                // Para el SuperUsuario, asignar todos los permisos disponibles sin restricciones
                foreach (var permiso in permisos)
                {
                    rolPermisos.Add(new RolPermiso
                    {
                        IdRol = rolSuperUsuario.IdRol,
                        IdPermiso = permiso.IdPermiso
                    });
                }
            }
            
            // 2. Rol: Administrador - Todos los permisos excepto los que trascienden consultorios
            var rolAdmin = roles.First(r => r.Nombre == "Administrador");
            
            // Lista de permisos que trascienden límites de consultorio y no deberían asignarse a Administrador
            var permisosExcluidosAdmin = new[] {
                "usuarios.view.all-consultorios",
                "consultorios.view.all"
            };
            
            // Para el administrador, asignar todos los permisos EXCEPTO los excluidos
            foreach (var permiso in permisos.Where(p => !permisosExcluidosAdmin.Contains(p.Codigo)))
            {
                rolPermisos.Add(new RolPermiso
                {
                    IdRol = rolAdmin.IdRol,
                    IdPermiso = permiso.IdPermiso
                });
            }
            
            // 2. Rol: Odontólogo
            var rolOdontologo = roles.First(r => r.Nombre == "Odontólogo");
            AsignarPermisosARol(rolPermisos, rolOdontologo, permisos, new[]
            {
                // Acceso a módulos
                "Sistema.Acceso",
                "Pacientes.Acceso",
                "Citas.Acceso",
                "Tratamientos.Acceso",
                "Servicios.Acceso",
                "Doctores.Acceso",
                
                // Permisos específicos
                "Sistema.AccesoSistema",
                
                // Pacientes
                "Pacientes.VerPacientes",
                "Pacientes.CrearPaciente",
                "Pacientes.EditarPaciente",
                "Pacientes.GestionarExpediente",
                "Pacientes.VerExpediente",
                
                // Citas
                "Citas.VerCitas",
                "Citas.CrearCita",
                "Citas.EditarCita",
                "Citas.ConfirmarCita",
                "Citas.CancelarCita",
                
                // Tratamientos
                "Tratamientos.VerTratamientos",
                "Tratamientos.CrearTratamiento",
                "Tratamientos.EditarTratamiento",
                "Tratamientos.GestionarSeguimiento",
                
                // Servicios
                "Servicios.VerServicios",
                
                // Doctores
                "Doctores.VerDoctores",
                "Doctores.GestionarDisponibilidad"
            });
            
            // 3. Rol: Recepcionista
            var rolRecepcionista = roles.First(r => r.Nombre == "Recepcionista");
            AsignarPermisosARol(rolPermisos, rolRecepcionista, permisos, new[]
            {
                // Acceso a módulos
                "Sistema.Acceso",
                "Pacientes.Acceso",
                "Citas.Acceso",
                "Finanzas.Acceso",
                
                // Permisos específicos
                "Sistema.AccesoSistema",
                
                // Pacientes
                "Pacientes.VerPacientes",
                "Pacientes.CrearPaciente",
                "Pacientes.EditarPaciente",
                "Pacientes.VerExpediente",
                
                // Citas
                "Citas.VerCitas",
                "Citas.CrearCita",
                "Citas.EditarCita",
                "Citas.ConfirmarCita",
                "Citas.CancelarCita",
                "Citas.GestionarRecordatorios",
                
                // Finanzas
                "Finanzas.VerPagos",
                "Finanzas.RegistrarPago"
            });
            
            // 4. Rol: Asistente
            var rolAsistente = roles.First(r => r.Nombre == "Asistente");
            AsignarPermisosARol(rolPermisos, rolAsistente, permisos, new[]
            {
                // Acceso a módulos
                "Sistema.Acceso",
                "Pacientes.Acceso",
                "Citas.Acceso",
                "Tratamientos.Acceso",
                
                // Permisos específicos
                "Sistema.AccesoSistema",
                
                // Pacientes
                "Pacientes.VerPacientes",
                "Pacientes.VerExpediente",
                
                // Citas
                "Citas.VerCitas",
                "Citas.ConfirmarCita",
                
                // Tratamientos
                "Tratamientos.VerTratamientos"
            });
            
            // 5. Rol: AuxiliarRecepcion
            var rolAuxiliar = roles.First(r => r.Nombre == "AuxiliarRecepcion");
            AsignarPermisosARol(rolPermisos, rolAuxiliar, permisos, new[]
            {
                // Acceso a módulos
                "Sistema.Acceso",
                "Pacientes.Acceso",
                "Citas.Acceso",
                
                // Permisos específicos
                "Sistema.AccesoSistema",
                
                // Pacientes
                "Pacientes.VerPacientes",
                
                // Citas
                "Citas.VerCitas",
                "Citas.ConfirmarCita",
                "Citas.GestionarRecordatorios"
            });
            
            // Guardar asignaciones de permisos a roles
            await _context.RolesPermisos.AddRangeAsync(rolPermisos);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"Se han asignado {rolPermisos.Count} permisos a los roles predefinidos.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al asignar permisos a roles: {Mensaje}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Asigna permisos específicos a un rol
    /// </summary>
    private void AsignarPermisosARol(List<RolPermiso> rolPermisos, Rol rol, List<Permiso> permisos, string[] codigosPermiso)
    {
        foreach (var codigo in codigosPermiso)
        {
            var permiso = permisos.FirstOrDefault(p => p.Codigo == codigo);
            if (permiso != null)
            {
                rolPermisos.Add(new RolPermiso
                {
                    IdRol = rol.IdRol,
                    IdPermiso = permiso.IdPermiso
                });
            }
            else
            {
                _logger.LogWarning($"No se encontró el permiso con código {codigo} para asignar al rol {rol.Nombre}");
            }
        }
    }
}
