using AutoMapper;
using DentalPro.Application.Interfaces;
using DentalPro.Application.Interfaces.IRepositories;
using DentalPro.Domain.Entities;

namespace DentalPro.Infrastructure.Services;

/// <summary>
/// Implementación del servicio para la gestión de consultorios
/// </summary>
public class ConsultorioService : IConsultorioService
{
    private readonly IConsultorioRepository _consultorioRepository;
    private readonly IMapper _mapper;

    public ConsultorioService(IConsultorioRepository consultorioRepository, IMapper mapper)
    {
        _consultorioRepository = consultorioRepository;
        _mapper = mapper;
    }

    /// <summary>
    /// Obtiene todos los consultorios
    /// </summary>
    public async Task<IEnumerable<Consultorio>> GetAllAsync()
    {
        return await _consultorioRepository.GetAllAsync();
    }

    /// <summary>
    /// Obtiene un consultorio por su ID
    /// </summary>
    public async Task<Consultorio?> GetByIdAsync(Guid id)
    {
        return await _consultorioRepository.GetByIdAsync(id);
    }

    /// <summary>
    /// Verifica si existe un consultorio con el ID especificado
    /// </summary>
    /// <param name="id">ID del consultorio a verificar</param>
    /// <returns>True si existe, False en caso contrario</returns>
    public async Task<bool> ExistsByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            return false;
        }
        
        var consultorio = await _consultorioRepository.GetByIdAsync(id);
        return consultorio != null;
    }
}
