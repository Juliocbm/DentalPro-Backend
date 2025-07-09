# Convenciones de Nomenclatura para DentalProWeb API

Este documento define las convenciones oficiales de nomenclatura para el proyecto DentalProWeb API. Estas convenciones están diseñadas para garantizar la coherencia, claridad y mantenibilidad del código en todo el proyecto.

## Tabla de Contenidos

1. [Estructura del Proyecto](#estructura-del-proyecto)
2. [Convenciones Generales](#convenciones-generales)
3. [DTOs](#dtos)
4. [Validadores](#validadores)
5. [Servicios](#servicios)
6. [Repositorios](#repositorios)
7. [Controladores](#controladores)
8. [Manejo de Errores](#manejo-de-errores)
9. [Autorización](#autorización)

## Estructura del Proyecto

El proyecto sigue una arquitectura limpia con las siguientes capas:

- **DentalPro.Domain**: Modelos de dominio y lógica de negocio central
- **DentalPro.Application**: DTOs, interfaces, servicios y validadores
- **DentalPro.Infrastructure**: Implementación de repositorios y servicios externos
- **DentalPro.Api**: Controladores, configuración y punto de entrada de la aplicación

## Convenciones Generales

- Usar **PascalCase** para nombres de clases, interfaces, propiedades y métodos
- Usar **camelCase** para variables locales y parámetros
- Usar sustantivos para clases y objetos, verbos para métodos
- Nombrar las interfaces con prefijo "I" (ej: `IUsuarioRepository`)
- Usar nombres descriptivos y evitar abreviaturas no estándar

## DTOs

Los DTOs (Data Transfer Objects) siguen un patrón de nomenclatura consistente:

```
[Entidad][Operación/Tipo]Dto
```

Donde:
- **Entidad**: Representa el modelo de dominio relacionado (ej: `Usuario`, `Rol`, `Consultorio`)
- **Operación/Tipo**: Describe la acción o el propósito del DTO

### Ejemplos:

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

### Organización:

Los DTOs se organizan en carpetas por entidad:
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

## Validadores

Los validadores siguen un patrón de nomenclatura consistente que refleja el DTO que validan:

```
[Entidad][Operación/Tipo]DtoValidator
```

### Ejemplos:

- `UsuarioCreateDtoValidator`: Validador para `UsuarioCreateDto`
- `UsuarioUpdateDtoValidator`: Validador para `UsuarioUpdateDto`
- `AuthLoginDtoValidator`: Validador para `AuthLoginDto`

### Validadores Asíncronos:

Para validadores asíncronos que verifican existencia en base de datos:

```
[Entidad]Existence[Criterio]AsyncValidator
```

### Ejemplos:

- `ConsultorioExistenceAsyncValidator`: Verifica existencia de consultorio por ID
- `RolExistenceByIdAsyncValidator`: Verifica existencia de rol por ID
- `RolExistenceByNameAsyncValidator`: Verifica existencia de rol por nombre

### Organización:

Los validadores se organizan en carpetas por entidad:
```
/Common/Validators
  /Usuarios
    UsuarioCreateDtoValidator.cs
    UsuarioUpdateDtoValidator.cs
  /Async
    ConsultorioExistenceAsyncValidator.cs
    RolExistenceByIdAsyncValidator.cs
    RolExistenceByNameAsyncValidator.cs
```

## Servicios

Los servicios siguen un patrón de nomenclatura consistente:

```
[Entidad]Service
```

Las interfaces de servicios utilizan el prefijo "I":

```
I[Entidad]Service
```

### Ejemplos:

- `UsuarioService` e `IUsuarioService`
- `ConsultorioService` e `IConsultorioService`
- `AuthService` e `IAuthService`

### Organización:

```
/Interfaces/IServices
  IUsuarioService.cs
  IConsultorioService.cs
  
/Services
  UsuarioService.cs
  ConsultorioService.cs
```

## Repositorios

Los repositorios siguen un patrón de nomenclatura consistente:

```
[Entidad]Repository
```

Las interfaces de repositorios utilizan el prefijo "I":

```
I[Entidad]Repository
```

### Ejemplos:

- `UsuarioRepository` e `IUsuarioRepository`
- `ConsultorioRepository` e `IConsultorioRepository`

### Organización:

```
/Interfaces/IRepositories
  IUsuarioRepository.cs
  IConsultorioRepository.cs
  
/Repositories
  UsuarioRepository.cs
  ConsultorioRepository.cs
```

## Controladores

Los controladores siguen un patrón de nomenclatura consistente:

```
[Entidad]Controller
```

### Ejemplos:

- `UsuariosController`: Controlador para gestión de usuarios
- `RolesController`: Controlador para gestión de roles
- `AuthController`: Controlador para autenticación

### Organización:

Los controladores se colocan en la raíz de la carpeta Controllers:
```
/Controllers
  UsuariosController.cs
  RolesController.cs
  AuthController.cs
```

## Manejo de Errores

Las excepciones personalizadas siguen un patrón de nomenclatura descriptivo:

```
[Tipo]Exception
```

### Ejemplos:

- `NotFoundException`: Para recursos no encontrados
- `ValidationException`: Para errores de validación
- `ForbiddenAccessException`: Para problemas de autorización

## Autorización

Las políticas de autorización utilizan el siguiente formato:

```
Require[Rol]Role o [Requisito]
```

### Ejemplos:

- `RequireAdminRole`: Para usuarios con rol de Administrador
- `RequireDoctorRole`: Para usuarios con rol de Doctor
- `RequireAssistantRole`: Para usuarios con rol de Asistente
- `RequireAuthenticatedUser`: Para cualquier usuario autenticado
- `SameConsultorio`: Para validar pertenencia al mismo consultorio

---

Este documento será actualizado según evolucionen las necesidades del proyecto. Todos los miembros del equipo deben adherirse a estas convenciones para mantener la consistencia del código.

## Historial de Cambios

- **08/07/2025**: Versión inicial del documento de convenciones
