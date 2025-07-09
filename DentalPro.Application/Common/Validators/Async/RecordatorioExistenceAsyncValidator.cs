using DentalPro.Application.Interfaces.IRepositories;

namespace DentalPro.Application.Common.Validators.Async;

/// <summary>
/// Validador asíncrono para verificar la existencia de un recordatorio por su ID
/// </summary>
public class RecordatorioExistenceAsyncValidator
{
    private readonly IRecordatorioRepository _recordatorioRepository;

    public RecordatorioExistenceAsyncValidator(IRecordatorioRepository recordatorioRepository)
    {
        _recordatorioRepository = recordatorioRepository;
    }

    /// <summary>
    /// Verifica si existe un recordatorio con el ID especificado
    /// </summary>
    /// <param name="idRecordatorio">ID del recordatorio a verificar</param>
    /// <returns>True si existe, False en caso contrario</returns>
    public async Task<bool> ExistsAsync(Guid idRecordatorio)
    {
        if (idRecordatorio == Guid.Empty)
            return false;

        return await _recordatorioRepository.ExistsByIdAsync(idRecordatorio);
    }
}
