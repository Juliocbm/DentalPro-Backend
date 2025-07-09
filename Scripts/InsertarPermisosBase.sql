-- Script de inicialización de permisos predefinidos del sistema
-- Este script inserta los permisos base agrupados por módulo

-- Verificar si ya existen permisos para evitar duplicados
IF NOT EXISTS (SELECT TOP 1 1 FROM seguridad.Permiso)
BEGIN
    PRINT 'Iniciando inserción de permisos predefinidos...';
    
    DECLARE @FechaActual DATETIME2 = GETDATE();
    
    -- MÓDULO: SISTEMA
    INSERT INTO seguridad.Permiso (IdPermiso, Codigo, Nombre, Descripcion, Modulo, EsOperacion, PredeterminadoSistema)
    VALUES 
    (NEWID(), 'Sistema.Acceso', 'Acceso al módulo Sistema', 'Permite acceder al módulo Sistema', 'Sistema', 0, 1),
    (NEWID(), 'Sistema.AccesoSistema', 'Acceso al sistema', 'Permiso para acceso al sistema en el módulo Sistema', 'Sistema', 0, 1),
    (NEWID(), 'Sistema.AdministrarRoles', 'Administrar roles del sistema', 'Permiso para administrar roles del sistema en el módulo Sistema', 'Sistema', 1, 1),
    (NEWID(), 'Sistema.AdministrarPermisos', 'Administrar permisos y asignaciones', 'Permiso para administrar permisos y asignaciones en el módulo Sistema', 'Sistema', 1, 1),
    (NEWID(), 'Sistema.AdministrarUsuarios', 'Administrar usuarios del sistema', 'Permiso para administrar usuarios del sistema en el módulo Sistema', 'Sistema', 1, 1),
    (NEWID(), 'Sistema.ConfiguracionGeneral', 'Configuración general del sistema', 'Permiso para configuración general del sistema en el módulo Sistema', 'Sistema', 1, 1);
    
    -- MÓDULO: CONSULTORIO
    INSERT INTO seguridad.Permiso (IdPermiso, Codigo, Nombre, Descripcion, Modulo, EsOperacion, PredeterminadoSistema)
    VALUES 
    (NEWID(), 'Consultorio.Acceso', 'Acceso al módulo Consultorio', 'Permite acceder al módulo Consultorio', 'Consultorio', 0, 1),
    (NEWID(), 'Consultorio.VerConsultorio', 'Ver información del consultorio', 'Permiso para ver información del consultorio en el módulo Consultorio', 'Consultorio', 1, 1),
    (NEWID(), 'Consultorio.CrearConsultorio', 'Crear nuevo consultorio', 'Permiso para crear nuevo consultorio en el módulo Consultorio', 'Consultorio', 1, 1),
    (NEWID(), 'Consultorio.EditarConsultorio', 'Editar información del consultorio', 'Permiso para editar información del consultorio en el módulo Consultorio', 'Consultorio', 1, 1),
    (NEWID(), 'Consultorio.EliminarConsultorio', 'Eliminar consultorio', 'Permiso para eliminar consultorio en el módulo Consultorio', 'Consultorio', 1, 1),
    (NEWID(), 'Consultorio.AdministrarSuscripcion', 'Administrar suscripción del consultorio', 'Permiso para administrar suscripción del consultorio en el módulo Consultorio', 'Consultorio', 1, 1);
    
    -- MÓDULO: PACIENTES
    INSERT INTO seguridad.Permiso (IdPermiso, Codigo, Nombre, Descripcion, Modulo, EsOperacion, PredeterminadoSistema)
    VALUES 
    (NEWID(), 'Pacientes.Acceso', 'Acceso al módulo Pacientes', 'Permite acceder al módulo Pacientes', 'Pacientes', 0, 1),
    (NEWID(), 'Pacientes.VerPacientes', 'Ver listado de pacientes', 'Permiso para ver listado de pacientes en el módulo Pacientes', 'Pacientes', 1, 1),
    (NEWID(), 'Pacientes.CrearPaciente', 'Crear nuevo paciente', 'Permiso para crear nuevo paciente en el módulo Pacientes', 'Pacientes', 1, 1),
    (NEWID(), 'Pacientes.EditarPaciente', 'Editar información de paciente', 'Permiso para editar información de paciente en el módulo Pacientes', 'Pacientes', 1, 1),
    (NEWID(), 'Pacientes.EliminarPaciente', 'Eliminar paciente', 'Permiso para eliminar paciente en el módulo Pacientes', 'Pacientes', 1, 1),
    (NEWID(), 'Pacientes.GestionarExpediente', 'Gestionar expediente clínico', 'Permiso para gestionar expediente clínico en el módulo Pacientes', 'Pacientes', 1, 1),
    (NEWID(), 'Pacientes.VerExpediente', 'Ver expediente clínico', 'Permiso para ver expediente clínico en el módulo Pacientes', 'Pacientes', 1, 1);
    
    -- MÓDULO: CITAS
    INSERT INTO seguridad.Permiso (IdPermiso, Codigo, Nombre, Descripcion, Modulo, EsOperacion, PredeterminadoSistema)
    VALUES 
    (NEWID(), 'Citas.Acceso', 'Acceso al módulo Citas', 'Permite acceder al módulo Citas', 'Citas', 0, 1),
    (NEWID(), 'Citas.VerCitas', 'Ver agenda de citas', 'Permiso para ver agenda de citas en el módulo Citas', 'Citas', 1, 1),
    (NEWID(), 'Citas.CrearCita', 'Agendar nueva cita', 'Permiso para agendar nueva cita en el módulo Citas', 'Citas', 1, 1),
    (NEWID(), 'Citas.EditarCita', 'Modificar cita existente', 'Permiso para modificar cita existente en el módulo Citas', 'Citas', 1, 1),
    (NEWID(), 'Citas.EliminarCita', 'Eliminar cita', 'Permiso para eliminar cita en el módulo Citas', 'Citas', 1, 1),
    (NEWID(), 'Citas.ConfirmarCita', 'Confirmar asistencia a cita', 'Permiso para confirmar asistencia a cita en el módulo Citas', 'Citas', 1, 1),
    (NEWID(), 'Citas.CancelarCita', 'Cancelar cita programada', 'Permiso para cancelar cita programada en el módulo Citas', 'Citas', 1, 1),
    (NEWID(), 'Citas.GestionarRecordatorios', 'Gestionar recordatorios de citas', 'Permiso para gestionar recordatorios de citas en el módulo Citas', 'Citas', 1, 1);
    
    -- MÓDULO: TRATAMIENTOS
    INSERT INTO seguridad.Permiso (IdPermiso, Codigo, Nombre, Descripcion, Modulo, EsOperacion, PredeterminadoSistema)
    VALUES 
    (NEWID(), 'Tratamientos.Acceso', 'Acceso al módulo Tratamientos', 'Permite acceder al módulo Tratamientos', 'Tratamientos', 0, 1),
    (NEWID(), 'Tratamientos.VerTratamientos', 'Ver tratamientos de pacientes', 'Permiso para ver tratamientos de pacientes en el módulo Tratamientos', 'Tratamientos', 1, 1),
    (NEWID(), 'Tratamientos.CrearTratamiento', 'Crear nuevo tratamiento', 'Permiso para crear nuevo tratamiento en el módulo Tratamientos', 'Tratamientos', 1, 1),
    (NEWID(), 'Tratamientos.EditarTratamiento', 'Editar tratamiento existente', 'Permiso para editar tratamiento existente en el módulo Tratamientos', 'Tratamientos', 1, 1),
    (NEWID(), 'Tratamientos.EliminarTratamiento', 'Eliminar tratamiento', 'Permiso para eliminar tratamiento en el módulo Tratamientos', 'Tratamientos', 1, 1),
    (NEWID(), 'Tratamientos.GestionarSeguimiento', 'Gestionar seguimiento de tratamientos', 'Permiso para gestionar seguimiento de tratamientos en el módulo Tratamientos', 'Tratamientos', 1, 1);
    
    -- MÓDULO: SERVICIOS
    INSERT INTO seguridad.Permiso (IdPermiso, Codigo, Nombre, Descripcion, Modulo, EsOperacion, PredeterminadoSistema)
    VALUES 
    (NEWID(), 'Servicios.Acceso', 'Acceso al módulo Servicios', 'Permite acceder al módulo Servicios', 'Servicios', 0, 1),
    (NEWID(), 'Servicios.VerServicios', 'Ver catálogo de servicios', 'Permiso para ver catálogo de servicios en el módulo Servicios', 'Servicios', 1, 1),
    (NEWID(), 'Servicios.CrearServicio', 'Crear nuevo servicio', 'Permiso para crear nuevo servicio en el módulo Servicios', 'Servicios', 1, 1),
    (NEWID(), 'Servicios.EditarServicio', 'Editar servicio existente', 'Permiso para editar servicio existente en el módulo Servicios', 'Servicios', 1, 1),
    (NEWID(), 'Servicios.EliminarServicio', 'Eliminar servicio', 'Permiso para eliminar servicio en el módulo Servicios', 'Servicios', 1, 1);
    
    -- MÓDULO: FINANZAS
    INSERT INTO seguridad.Permiso (IdPermiso, Codigo, Nombre, Descripcion, Modulo, EsOperacion, PredeterminadoSistema)
    VALUES 
    (NEWID(), 'Finanzas.Acceso', 'Acceso al módulo Finanzas', 'Permite acceder al módulo Finanzas', 'Finanzas', 0, 1),
    (NEWID(), 'Finanzas.VerPagos', 'Ver registro de pagos', 'Permiso para ver registro de pagos en el módulo Finanzas', 'Finanzas', 1, 1),
    (NEWID(), 'Finanzas.RegistrarPago', 'Registrar nuevo pago', 'Permiso para registrar nuevo pago en el módulo Finanzas', 'Finanzas', 1, 1),
    (NEWID(), 'Finanzas.EditarPago', 'Editar información de pago', 'Permiso para editar información de pago en el módulo Finanzas', 'Finanzas', 1, 1),
    (NEWID(), 'Finanzas.AnularPago', 'Anular pago registrado', 'Permiso para anular pago registrado en el módulo Finanzas', 'Finanzas', 1, 1),
    (NEWID(), 'Finanzas.GestionarFacturas', 'Gestionar facturas', 'Permiso para gestionar facturas en el módulo Finanzas', 'Finanzas', 1, 1),
    (NEWID(), 'Finanzas.VerReportes', 'Ver reportes financieros', 'Permiso para ver reportes financieros en el módulo Finanzas', 'Finanzas', 1, 1),
    (NEWID(), 'Finanzas.ExportarReportes', 'Exportar reportes financieros', 'Permiso para exportar reportes financieros en el módulo Finanzas', 'Finanzas', 1, 1);
    
    -- MÓDULO: DOCTORES
    INSERT INTO seguridad.Permiso (IdPermiso, Codigo, Nombre, Descripcion, Modulo, EsOperacion, PredeterminadoSistema)
    VALUES 
    (NEWID(), 'Doctores.Acceso', 'Acceso al módulo Doctores', 'Permite acceder al módulo Doctores', 'Doctores', 0, 1),
    (NEWID(), 'Doctores.VerDoctores', 'Ver información de doctores', 'Permiso para ver información de doctores en el módulo Doctores', 'Doctores', 1, 1),
    (NEWID(), 'Doctores.CrearDoctor', 'Registrar nuevo doctor', 'Permiso para registrar nuevo doctor en el módulo Doctores', 'Doctores', 1, 1),
    (NEWID(), 'Doctores.EditarDoctor', 'Editar información de doctor', 'Permiso para editar información de doctor en el módulo Doctores', 'Doctores', 1, 1),
    (NEWID(), 'Doctores.GestionarEspecialidades', 'Gestionar especialidades médicas', 'Permiso para gestionar especialidades médicas en el módulo Doctores', 'Doctores', 1, 1),
    (NEWID(), 'Doctores.GestionarDisponibilidad', 'Gestionar disponibilidad de doctores', 'Permiso para gestionar disponibilidad de doctores en el módulo Doctores', 'Doctores', 1, 1);
    
    -- MÓDULO: REPORTES
    INSERT INTO seguridad.Permiso (IdPermiso, Codigo, Nombre, Descripcion, Modulo, EsOperacion, PredeterminadoSistema)
    VALUES 
    (NEWID(), 'Reportes.Acceso', 'Acceso al módulo Reportes', 'Permite acceder al módulo Reportes', 'Reportes', 0, 1),
    (NEWID(), 'Reportes.VerReportesCitas', 'Ver reportes de citas', 'Permiso para ver reportes de citas en el módulo Reportes', 'Reportes', 1, 1),
    (NEWID(), 'Reportes.VerReportesPacientes', 'Ver reportes de pacientes', 'Permiso para ver reportes de pacientes en el módulo Reportes', 'Reportes', 1, 1),
    (NEWID(), 'Reportes.VerReportesTratamientos', 'Ver reportes de tratamientos', 'Permiso para ver reportes de tratamientos en el módulo Reportes', 'Reportes', 1, 1),
    (NEWID(), 'Reportes.VerReportesFinanzas', 'Ver reportes financieros', 'Permiso para ver reportes financieros en el módulo Reportes', 'Reportes', 1, 1),
    (NEWID(), 'Reportes.ExportarReportes', 'Exportar reportes a diferentes formatos', 'Permiso para exportar reportes a diferentes formatos en el módulo Reportes', 'Reportes', 1, 1);
    
    PRINT 'Inserción de permisos completada. Se han insertado permisos para 9 módulos.';
END
ELSE
BEGIN
    PRINT 'Ya existen permisos en la base de datos. No se realizará la inserción.';
END
