namespace DentalPro.Application.Common.Exceptions;

/// <summary>
/// Excepción lanzada cuando un recurso no es encontrado
/// </summary>
public class NotFoundException : ApplicationException
{
    public NotFoundException(string resourceName, object resourceKey)
        : base($"El recurso '{resourceName}' ({resourceKey}) no fue encontrado.", "RESOURCE_NOT_FOUND")
    {
        ResourceName = resourceName;
        ResourceKey = resourceKey;
    }

    public string ResourceName { get; }
    public object ResourceKey { get; }
}
