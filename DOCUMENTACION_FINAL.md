# Documentación Completa DentalProWeb API

## Tabla de Contenidos
1. [Arquitectura del Proyecto](#1-arquitectura-del-proyecto)
2. [Proyectos y Objetivos](#2-proyectos-y-objetivos)
3. [Convenciones de Nomenclatura](#3-convenciones-de-nomenclatura)
4. [Sistema Global de Excepciones](#4-sistema-global-de-excepciones-personalizadas)
5. [Validación con FluentValidation](#5-validación-con-fluentvalidation)
6. [Perfiles de Mapping (AutoMapper)](#6-perfiles-de-mapping-automapper)
7. [Entidades y DTOs](#7-entidades-y-dtos)
8. [Sistema de Auditoría e Interceptores](#8-sistema-de-auditoría-e-interceptores)
9. [Sistema de Autenticación y Autorización](#9-sistema-de-autenticación-y-autorización)
10. [Servicios Principales](#10-servicios-principales)
11. [Base de Datos](#11-base-de-datos)

## 1. Arquitectura del Proyecto

DentalProWeb API implementa una **arquitectura de capas** siguiendo los principios de Clean Architecture:

- **Capa de Presentación**: API REST con controladores ASP.NET Core
- **Capa de Aplicación**: Servicios, DTOs, Validación y Mapeos
- **Capa de Dominio**: Entidades y Reglas de Negocio
- **Capa de Infraestructura**: Persistencia, Seguridad y Servicios Externos

El proyecto utiliza **inyección de dependencias** extensivamente para mantener un bajo acoplamiento entre componentes, facilitando pruebas unitarias y mantenibilidad.

## 2. Proyectos y Objetivos

La solución se divide en los siguientes proyectos:

1. **DentalPro.Api**: Punto de entrada de la aplicación, controladores y configuración de API
   - *Objetivo*: Exponer endpoints REST y manejar comunicación HTTP

2. **DentalPro.Application**: Lógica de aplicación y casos de uso
   - *Objetivo*: Implementar la lógica de negocio sin dependencias directas de infraestructura

3. **DentalPro.Domain**: Entidades y reglas de negocio core
   - *Objetivo*: Definir el modelo de dominio y reglas invariantes

4. **DentalPro.Infrastructure**: Implementaciones concretas de interfaces definidas en capas superiores
   - *Objetivo*: Proporcionar implementaciones para persistencia, autenticación y servicios externos

5. **DentalPro.Tests**: Pruebas unitarias e integración
   - *Objetivo*: Validar el comportamiento del sistema

## 3. Convenciones de Nomenclatura

### 3.1. Convenciones Generales
- Usar **PascalCase** para nombres de clases, interfaces, propiedades y métodos
- Usar **camelCase** para variables locales y parámetros
- Usar sustantivos para clases y objetos, verbos para métodos
- Nombrar las interfaces con prefijo "I" (ej: `IUsuarioRepository`)
- Usar nombres descriptivos y evitar abreviaturas no estándar

### 3.2. DTOs (Data Transfer Objects)
Formato: `[Entidad][Operación/Tipo]Dto`

- **DTOs de Operación**:
  - `UsuarioCreateDto`: Para crear un usuario
  - `UsuarioUpdateDto`: Para actualizar un usuario
  - `UsuarioChangePasswordDto`: Para cambiar contraseña

- **DTOs de Respuesta**:
  - `AuthLoginResponseDto`: Respuesta después del login
  - `AuthRegisterResponseDto`: Respuesta después del registro

- **DTOs de Detalle/Información**:
  - `UsuarioDto`: Información completa de un usuario
  - `RolDetailDto`: Información detallada de un rol

Organización en carpetas por entidad:
```
/DTOs
  /Usuario
    UsuarioCreateDto.cs
    UsuarioUpdateDto.cs
    UsuarioDto.cs
  /Rol
    RolCreateDto.cs
    RolUpdateDto.cs
    RolDetailDto.cs
```

### 3.3. Validadores
Formato: `[Entidad][Operación/Tipo]DtoValidator`

- `UsuarioCreateDtoValidator`: Validador para `UsuarioCreateDto`
- `UsuarioUpdateDtoValidator`: Validador para `UsuarioUpdateDto`
- `AuthLoginDtoValidator`: Validador para `AuthLoginDto`

Para validadores asíncronos: `[Entidad]Existence[Criterio]AsyncValidator`
- `ConsultorioExistenceAsyncValidator`: Verifica existencia de consultorio por ID
- `RolExistenceByIdAsyncValidator`: Verifica existencia de rol por ID

Organización:
```
/Common/Validators
  /Usuarios
    UsuarioCreateDtoValidator.cs
    UsuarioUpdateDtoValidator.cs
  /Async
    ConsultorioExistenceAsyncValidator.cs
    RolExistenceByIdAsyncValidator.cs
```

### 3.4. Servicios
Formato: `[Entidad]Service` con interfaces `I[Entidad]Service`

- `UsuarioService` e `IUsuarioService`
- `ConsultorioService` e `IConsultorioService`
- `AuthService` e `IAuthService`

### 3.5. Repositorios
Formato: `[Entidad]Repository` con interfaces `I[Entidad]Repository`

- `UsuarioRepository` e `IUsuarioRepository`
- `ConsultorioRepository` e `IConsultorioRepository`

### 3.6. Controladores
Formato: `[Entidad]Controller`

- `UsuariosController`: Controlador para gestión de usuarios
- `RolesController`: Controlador para gestión de roles
- `AuthController`: Controlador para autenticación

Métodos estándar:
- **Controladores**:
  - `Get[Entidad]s` o `GetAll`: Listar todos
  - `Get[Entidad](id)`: Obtener por ID
  - `Create[Entidad]`: Crear nuevo
  - `Update[Entidad]`: Actualizar
  - `Delete[Entidad]`: Eliminar

### 3.7. Excepciones
Formato: `[Tipo]Exception`

- `NotFoundException`: Para recursos no encontrados
- `ValidationException`: Para errores de validación
- `ForbiddenAccessException`: Para problemas de autorización

### 3.8. Políticas de Autorización
Formato: `Require[Rol]Role` o `[Requisito]`

- `RequireAdminRole`: Para usuarios con rol de Administrador
- `RequireDoctorRole`: Para usuarios con rol de Doctor
- `RequireAssistantRole`: Para usuarios con rol de Asistente
- `RequireAuthenticatedUser`: Para cualquier usuario autenticado
- `RequireConsultorioAccess`: Para validar pertenencia al mismo consultorio

## 4. Sistema Global de Excepciones Personalizadas

El sistema implementa un middleware centralizado (`ExceptionHandlingMiddleware`) que captura todas las excepciones y retorna respuestas HTTP estandarizadas:

### 4.1. Componentes Principales
- **ApplicationException**: Clase base abstracta para todas las excepciones personalizadas
- **Excepciones Específicas**:
  - `NotFoundException`: Código 404, para recursos no encontrados
  - `ValidationException`: Código 400, para errores de validación
  - `ForbiddenAccessException`: Código 403, para problemas de autorización
  - `BadRequestException`: Código 400, para solicitudes incorrectas

### 4.2. Estructura de Respuesta de Error
```json
{
  "statusCode": 400,
  "errorCode": "2001",
  "message": "Mensaje de error estandarizado",
  "details": "Detalles adicionales si existen",
  "validationErrors": [
    { "field": "campo", "message": "mensaje de error" }
  ]
}
```

### 4.3. Catálogo de Errores
- **ErrorCodes**: Códigos organizados por rangos (1000-general, 2000-autenticación, etc.)
- **ErrorMessages**: Mensajes estandarizados para diferentes situaciones

## 5. Validación con FluentValidation

### 5.1. Nomenclatura de Validadores
- Validadores estándar: `[Entidad][Operación/Tipo]DtoValidator`
  - Ejemplos: `UsuarioCreateDtoValidator`, `CitaUpdateDtoValidator`

- Validadores asíncronos: `[Entidad]Existence[Criterio]AsyncValidator`
  - Ejemplos: `ConsultorioExistenceAsyncValidator`, `RolExistenceByIdAsyncValidator`

### 5.2. Ubicación
- Los validadores se encuentran en `DentalPro.Application/Validators`
- Agrupados por funcionalidad (carpetas: Usuarios, Citas, etc.)

### 5.3. Integración
- Registrados mediante extensión de servicios en `Program.cs`
- Integrados con el filtro de validación de modelos de ASP.NET Core

## 6. Perfiles de Mapping (AutoMapper)

Los perfiles se definen en `DentalPro.Application/Mapping`:

### 6.1. Nomenclatura
- `[Módulo]MappingProfile`: Ejemplo: `UsuariosMappingProfile`

### 6.2. Estructura
```csharp
public class UsuariosMappingProfile : Profile
{
    public UsuariosMappingProfile()
    {
        CreateMap<Usuario, UsuarioDetailDto>()
            .ForMember(dest => dest.RolNombres, opt => opt.MapFrom(src => src.Roles.Select(r => r.Rol.Nombre)));

        CreateMap<UsuarioCreateDto, Usuario>();
        
        // Más mappings...
    }
}
```

### 6.3. Registro
- Todos los perfiles se registran automáticamente en `Program.cs` mediante Assembly scanning

## 7. Entidades y DTOs

### 7.1. Entidades
- Ubicadas en `DentalPro.Domain/Entities`
- Organizadas por esquema de base de datos
- Incluyen propiedades de navegación para relaciones
- Usan clases de configuración en `Infrastructure/Persistence/Configuration` para FluentAPI

### 7.2. DTOs
- Ubicados en `DentalPro.Application/DTOs`
- Organizados por funcionalidad
- Nomenclatura: `[Entidad][Operación/Tipo]Dto`
  - **CreateDto**: Para creación
  - **UpdateDto**: Para actualización
  - **DetailDto**: Para respuestas detalladas
  - **ListDto**: Para listas/resúmenes

## 8. Sistema de Auditoría e Interceptores

### 8.1. AuditLogs
- Tabla `auditoria.AuditLogs` para registro de acciones
- Campos clave: Action, EntityType, EntityId, UserId, Timestamp, Details, IpAddress, IdConsultorio

### 8.2. Servicios
- `IAuditService` y `AuditService`: Para registro explícito de auditoría
  - `RegisterActionAsync`: Método principal para registro de acciones de auditoría

### 8.3. Interceptores
1. **DirectAuditInterceptor**:
   - Audita cambios directos a entidades
   - Implementa `SaveChangesInterceptor`
   - Crea registros `AuditLog` para cada entidad modificada

2. **SimpleAuditInterceptor**:
   - Maneja auditoría simple (CreatedAt, ModifiedAt)
   - Actualiza automáticamente propiedades de auditoría en entidades

### 8.4. Integración
- Registrados en `ApplicationDbContext` para ejecución automática
- Configurados en `Program.cs` como servicios Scoped
## 9. Sistema de Autenticación y Autorización

### 9.1. Autenticación - Componentes Principales

#### 9.1.1. Servicios de Autenticación

**AuthService**
- **Responsabilidades**:
  - Login/logout de usuarios
  - Registro de nuevos usuarios
  - Generación y validación de JWT tokens
  - Refresh tokens
  - Cambio de contraseñas
  - Bloqueo de cuentas tras intentos fallidos
  - Auditoría de eventos de autenticación

- **Métodos principales**:
  ```csharp
  Task<AuthLoginResponseDto> LoginAsync(AuthLoginDto request);
  Task<AuthRegisterResponseDto> RegisterAsync(AuthRegisterDto request);
  Task<AuthLoginResponseDto> RefreshTokenAsync(string refreshToken);
  Task LogoutAsync(string refreshToken);
  Task ChangePasswordAsync(UsuarioChangePasswordDto request);
  Task<bool> ValidateTokenAsync(string token);
  ```

**JwtService**
- **Responsabilidades**:
  - Generación de tokens JWT
  - Inclusión de claims personalizadas (rol, consultorio, permisos)
  - Configuración de expiración y firma

#### 9.1.2. DTOs de Autenticación

- **AuthLoginDto**:
  ```csharp
  public class AuthLoginDto
  {
      public string Correo { get; set; }
      public string Password { get; set; }
  }
  ```

- **AuthLoginResponseDto**:
  ```csharp
  public class AuthLoginResponseDto
  {
      public Guid UserId { get; set; }
      public string Nombre { get; set; }
      public string Correo { get; set; }
      public List<string> Roles { get; set; }
      public string Token { get; set; }
      public string RefreshToken { get; set; }
      public DateTime Expiration { get; set; }
      public Guid IdConsultorio { get; set; }
  }
  ```

#### 9.1.3. Validación y Seguridad

- **Hashing de contraseñas**: Se utiliza BCrypt para el almacenamiento seguro
  ```csharp
  public class PasswordHasher : IPasswordHasher
  {
      public string HashPassword(string password)
      {
          return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt());
      }

      public bool VerifyPassword(string password, string passwordHash)
      {
          return BCrypt.Net.BCrypt.Verify(password, passwordHash);
      }
  }
  ```

- **Protección contra intentos de fuerza bruta**:
  - Bloqueo de cuenta después de 5 intentos fallidos
  - Duración de bloqueo exponencial según número de intentos

#### 9.1.4. Gestión de RefreshTokens

- **Tabla dedicada**: `seguridad.RefreshToken` para almacenar tokens
- **Características de seguridad**:
  - Tokens revocables
  - Expiración configurable
  - Un token por dispositivo/sesión

### 9.2. Sistema de Autorización Basado en Políticas

#### 9.2.1. Políticas Basadas en Roles

- **Políticas predefinidas**:
  ```csharp
  services.AddAuthorization(options =>
  {
      options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Administrador"));
      options.AddPolicy("RequireDoctorRole", policy => policy.RequireRole("Doctor"));
      options.AddPolicy("RequireAssistantRole", policy => policy.RequireRole("Asistente"));
      options.AddPolicy("RequireAuthenticatedUser", policy => policy.RequireAuthenticatedUser());
  });
  ```

#### 9.2.2. Política de Consultorio

La política `RequireConsultorioAccess` reemplazó a las anteriores políticas `ConsultorioAccess` y `SameConsultorio` para unificar y estandarizar la nomenclatura.

- **RequireConsultorioAccessAttribute**: Atributo que hereda de AuthorizeAttribute
  ```csharp
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
  public class RequireConsultorioAccessAttribute : AuthorizeAttribute
  {
      public RequireConsultorioAccessAttribute() : base("RequireConsultorioAccess")
      {
      }
  }
  ```

- **ConsultorioAccessRequirement**: Requisito para la política
- **ConsultorioAccessHandler**: Implementa la lógica de validación que verifica:
  - Si el consultorio de la ruta coincide con el del usuario
  - Si el usuario es administrador (bypass)

#### 9.2.3. Sistema de Permisos Granulares

**Definición de Permisos**:
- Organizados en clases estáticas por módulo
  ```csharp
  public static class CitasPermissions
  {
      public const string View = "citas.view";
      public const string ViewAll = "citas.view.all";
      public const string ViewDoctor = "citas.view.doctor";
      public const string ViewPaciente = "citas.view.paciente";
      public const string Create = "citas.create";
      public const string Update = "citas.update";
      public const string Cancel = "citas.cancel";
      public const string Delete = "citas.delete";
  }
  ```

**Componentes de Autorización**:

- **RequirePermisoAttribute**: Atributo para aplicar políticas dinámicas
- **PermissionPolicyProvider**: Proveedor dinámico de políticas
- **PermisoHandler**: Verifica si el usuario tiene el permiso requerido

**Verificación de Permisos en Servicios**:

- **IPermissionValidator**: Interfaz para validar permisos
  ```csharp
  public interface IPermissionValidator
  {
      Task ValidatePermissionAsync(string permission, CancellationToken cancellationToken = default);
      Task<bool> HasPermissionAsync(string permission, CancellationToken cancellationToken = default);
  }
  ```

- **Uso en servicios**:
  ```csharp
  // Validación de permisos antes de operación
  await _permissionValidator.ValidatePermissionAsync(CitasPermissions.Update, cancellationToken);
  
  // Lanza ForbiddenAccessException si no tiene permiso
  ```

### 9.3. CurrentUserService - Backbone del Sistema de Seguridad

**Interfaz y Funcionalidades**:
```csharp
public interface ICurrentUserService
{
    Guid? GetCurrentUserId();
    Task<Usuario> GetCurrentUserAsync(CancellationToken cancellationToken = default);
    Guid? GetCurrentConsultorioId();
    Task<bool> IsInRoleAsync(string roleName);
    Task<bool> HasPermissionAsync(string permission);
    Task<IEnumerable<string>> GetUserPermissionsAsync();
    void InvalidateCache();
}
```

**Características implementadas**:
- Cache en memoria para mejorar rendimiento
- Logging detallado de eventos de seguridad
- Uso de excepciones personalizadas (ForbiddenAccessException, NotFoundException)
- Validaciones adicionales como verificar si el usuario está activo
- Método para invalidar caché después de actualizaciones

**Implementación**:
El servicio obtiene la información del usuario desde las claims del token JWT y proporciona métodos para:
- Obtener el ID del usuario actual
- Obtener el objeto de usuario completo (con caché)
- Verificar roles y permisos
- Obtener el ID del consultorio asociado

### 9.4. Gestión de Roles y Permisos

**Estructura de Base de Datos**:
- **seguridad.Rol**: Roles del sistema
- **seguridad.Permiso**: Permisos disponibles
- **seguridad.RolPermiso**: Relación muchos a muchos entre roles y permisos
- **seguridad.UsuarioRol**: Asignación de roles a usuarios
- **seguridad.UsuarioPermiso**: Permisos específicos por usuario (sobreescrituras)

**Servicios Especializados**:

**UsuarioRoleService**: 
- Gestión específica de roles de usuario
- Métodos para asignar/revocar roles
- Verificación de pertenencia a roles

**RolService**: 
- Gestión de roles y sus permisos
- CRUD de roles
- Asignación de permisos a roles

### 9.5. Flujo Completo de Autenticación y Autorización

1. **Autenticación**:
   - Cliente envía credenciales a `/api/auth/login`
   - `AuthService` valida credenciales y genera token JWT
   - Token incluye claims de: userId, consultorioId, roles
   - Cliente almacena token y refresh token

2. **Autorización por Política/Rol**:
   - Cliente incluye token en cabecera Authorization
   - Middleware JWT valida token
   - Cuando llega a endpoint con `[Authorize(Policy = "RequireAdminRole")]`
   - ASP.NET Core verifica si el usuario tiene el rol requerido

3. **Autorización por Permiso**:
   - Cliente llama endpoint con `[RequirePermiso("citas.view")]`
   - `PermissionPolicyProvider` crea política dinámica
   - `PermisoHandler` verifica si usuario tiene el permiso
   - `CurrentUserService` consulta los permisos del usuario (directos + heredados de roles)

4. **Autorización por Consultorio**:
   - Cliente llama endpoint con `[RequireConsultorioAccess]`
   - `ConsultorioAccessHandler` extrae consultorioId de la ruta
   - Verifica que coincida con el consultorioId del usuario actual
   - Permite acceso si coinciden o si el usuario es Administrador

## 10. Servicios Principales

### 10.1. Servicios de Usuario
La gestión de usuarios se ha separado en servicios especializados para mejorar la mantenibilidad:

1. **UsuarioManagementService**: Operaciones CRUD de usuarios
   - Creación, actualización y eliminación de usuarios
   - Gestión de estados de usuario (activo/inactivo)

2. **UsuarioService**: Fachada general que delega en servicios especializados
   - Punto de entrada principal para operaciones de usuario
   - Coordinación entre servicios especializados

3. **UsuarioRoleService**: Gestión específica de roles de usuario
   - Asignación y revocación de roles
   - Consulta de roles de usuario

4. **AuthService**: Autenticación y gestión de tokens
   - Login y registro
   - Gestión de tokens JWT y refresh

### 10.2. CurrentUserService
- Obtiene información del usuario actual desde claims del token JWT
- Implementa caché en memoria para mejorar rendimiento
- Proporciona métodos como:
  - `GetCurrentUserId()`
  - `GetCurrentUserAsync()`
  - `GetCurrentConsultorioId()`
  - `IsInRoleAsync()`
  - `HasPermissionAsync()`

### 10.3. Servicios de Funcionalidad
- **CitaService**: Gestión de citas médicas
- **PacienteService**: Gestión de pacientes
- **RecordatorioService**: Gestión de recordatorios de citas
- **ConsultorioService**: Configuración de consultorios
- **RolService**: Administración de roles y permisos

## 11. Base de Datos

- **Motor**: SQL Server
- **Estructura**: Organizada por esquemas funcionales:
  - `seguridad`: Usuarios, roles, permisos
  - `pacientes`: Información de pacientes
  - `citas`: Citas médicas y recordatorios
  - `tratamientos`: Servicios y tratamientos
  - `finanzas`: Pagos y facturas
  - `configuracion`: Configuraciones generales
  - `auditoria`: Registros de auditoría

- **Migraciones**: Gestionadas con Entity Framework Core
- **Seeders**: Datos iniciales configurados en `Infrastructure/Persistence/Seeders`

## 12. Historial de Cambios

### 12.1. Estandarización de Nomenclatura (DTOs y Validadores)
- Se estandarizó la nomenclatura de DTOs, especialmente en módulos de autenticación:
  - `LoginRequest` → `AuthLoginDto`
  - `LoginResponse` → `AuthLoginResponseDto`
  - `RegisterRequest` → `AuthRegisterDto`
  - `ChangePasswordRequest` → `UsuarioChangePasswordDto`
- Se estandarizaron los validadores siguiendo el mismo patrón
- Se actualizaron todas las referencias en controladores y servicios

### 12.2. Implementación de FluentValidation
- Se agregaron validadores para DTOs principales
- Se creó un filtro personalizado (ValidationFilter) para integración con el sistema de excepciones
- Se actualizó ValidationException para soportar FormErrors compatibles con Angular

### 12.3. Sistema de Autorización
- Se estandarizaron los nombres de políticas de autorización
- Se unificaron las políticas de acceso a consultorio con el nombre `RequireConsultorioAccess`
- Se implementó un sistema de permisos granulares para el módulo de Citas

### 12.4. Refactorización de Servicios de Usuario
- Se separaron responsabilidades en servicios especializados
- Se implementó un CurrentUserService robusto con caché
- Se agregaron permisos granulares y se corrigieron referencias incorrectas
