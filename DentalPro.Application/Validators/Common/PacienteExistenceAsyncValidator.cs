using DentalPro.Application.Interfaces.IRepositories;

namespace DentalPro.Application.Validators.Common;

/// <summary>
/// Validador asíncrono para verificar la existencia de un paciente por su ID
/// </summary>
public class PacienteExistenceAsyncValidator
{
    private readonly IPacienteRepository _pacienteRepository;

    public PacienteExistenceAsyncValidator(IPacienteRepository pacienteRepository)
    {
        _pacienteRepository = pacienteRepository;
    }

    /// <summary>
    /// Verifica si existe un paciente con el ID especificado
    /// </summary>
    /// <param name="idPaciente">ID del paciente a verificar</param>
    /// <returns>True si existe, False en caso contrario</returns>
    public async Task<bool> ExistsAsync(Guid idPaciente)
    {
        if (idPaciente == Guid.Empty)
            return false;

        return await _pacienteRepository.ExistsByIdAsync(idPaciente);
    }
}
