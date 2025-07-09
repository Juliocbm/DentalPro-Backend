using DentalPro.Application.Interfaces.IRepositories;

namespace DentalPro.Application.Validators.Common;

/// <summary>
/// Validador asíncrono para verificar la existencia de una cita por su ID
/// </summary>
public class CitaExistenceAsyncValidator
{
    private readonly ICitaRepository _citaRepository;

    public CitaExistenceAsyncValidator(ICitaRepository citaRepository)
    {
        _citaRepository = citaRepository;
    }

    /// <summary>
    /// Verifica si existe una cita con el ID especificado
    /// </summary>
    /// <param name="idCita">ID de la cita a verificar</param>
    /// <returns>True si existe, False en caso contrario</returns>
    public async Task<bool> ExistsAsync(Guid idCita)
    {
        if (idCita == Guid.Empty)
            return false;

        return await _citaRepository.ExistsByIdAsync(idCita);
    }
}
