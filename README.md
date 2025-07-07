# DentalPro API

API para la gestión de consultorios dentales.

## Sistema de Autenticación y Autorización

### Autenticación

La API utiliza JWT (JSON Web Tokens) para la autenticación. Los endpoints para autenticación son:

- `POST /api/Auth/login`: Para iniciar sesión con credenciales
- `POST /api/Auth/register`: Para registrar nuevos usuarios

### Autorización basada en Políticas

El proyecto implementa un sistema de autorización basado en políticas (Authorization Policies) en lugar de usar atributos de roles directos. Esto proporciona mayor flexibilidad y mantenibilidad al código.

#### Políticas configuradas:

1. **RequireAdminRole**: Para usuarios con rol de Administrador
2. **RequireDoctorRole**: Para usuarios con rol de Doctor
3. **RequireAssistantRole**: Para usuarios con rol de Asistente
4. **RequireAuthenticatedUser**: Para cualquier usuario autenticado
5. **ConsultorioAccess**: Política avanzada para validar que el usuario pertenece al mismo consultorio

#### Uso de políticas en los controladores:

```csharp
[Authorize(Policy = "RequireAdminRole")]
public async Task<ActionResult> SomeAdminMethod()
{
    // Solo accesible por administradores
}
```

#### Política personalizada para acceso a consultorios

Se implementó una política personalizada para validar automáticamente el acceso basado en el consultorio del usuario:

```csharp
[RequireConsultorioAccess]
public async Task<ActionResult> GetConsultorioData(Guid idConsultorio)
{
    // La validación de acceso al consultorio se maneja automáticamente
}
```

#### Componentes del sistema de autorización por consultorios:

- **ConsultorioAccessRequirement**: Requisito para la política de autorización
- **ConsultorioAccessHandler**: Manejador que implementa la lógica de validación
- **RequireConsultorioAccessAttribute**: Atributo para aplicar la validación a nivel de controlador/acción

## Manejo de Excepciones

La API implementa un sistema centralizado para el manejo de excepciones, siguiendo los principios de responsabilidad única y consistencia.

### Middleware Global

Se utiliza un middleware (`ExceptionHandlingMiddleware`) que captura todas las excepciones no controladas y produce respuestas estandarizadas:

- Formato JSON consistente para todos los errores
- Códigos de error específicos para diferentes situaciones
- Mensajes de error amigables para el usuario
- Detalles técnicos solo en entorno de desarrollo

### Excepciones Personalizadas

Se implementaron las siguientes excepciones de aplicación:

- `NotFoundException`: Para recursos que no existen
- `ValidationException`: Para errores de validación de modelos
- `ForbiddenAccessException`: Para problemas de permisos
- `BadRequestException`: Para solicitudes incorrectas

### Catálogo de Errores Centralizado

Se mantiene un catálogo centralizado de códigos y mensajes de error en:

- `ErrorCodes`: Catálogo de códigos de error con rangos específicos
- `ErrorMessages`: Mensajes estandarizados para diferentes situaciones

### Formato de Respuesta de Error

La API devuelve un formato de error optimizado para compatibilidad con Angular:

```json
{
  "statusCode": 400,
  "errorCode": "ERR-4000",
  "error": "Validation Failed",
  "message": "Error de validación",
  "details": "Detalles adicionales (solo en desarrollo)",
  "validationErrors": [
    {
      "property": "Email",
      "message": "El formato del correo electrónico es inválido",
      "attemptedValue": "correo-invalido"
    }
  ],
  "formErrors": {
    "email": ["El formato del correo electrónico es inválido"]
  },
  "timestamp": "2023-07-07T14:35:45.123Z"
}
```

#### Campos especiales para Angular

- `error`: Compatible con los interceptores HTTP de Angular
- `formErrors`: Formato compatible con Angular FormGroup para asociar errores directamente con los controles del formulario

## Estructura del Proyecto

El proyecto sigue una arquitectura limpia (Clean Architecture):

- **DentalPro.Domain**: Entidades y reglas de negocio
- **DentalPro.Application**: Lógica de aplicación, DTOs e interfaces
- **DentalPro.Infrastructure**: Implementaciones concretas y acceso a datos
- **DentalPro.Api**: Controladores y configuración de la API
