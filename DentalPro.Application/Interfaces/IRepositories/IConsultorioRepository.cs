using DentalPro.Domain.Entities;

namespace DentalPro.Application.Interfaces.IRepositories
{
    /// <summary>
    /// Repositorio para la entidad Consultorio
    /// </summary>
    public interface IConsultorioRepository
    {
        /// <summary>
        /// Obtiene todos los consultorios
        /// </summary>
        Task<IEnumerable<Consultorio>> GetAllAsync();
        
        /// <summary>
        /// Obtiene un consultorio por su ID
        /// </summary>
        Task<Consultorio?> GetByIdAsync(Guid id);
        
        /// <summary>
        /// Agrega un nuevo consultorio
        /// </summary>
        Task<Consultorio> AddAsync(Consultorio consultorio);
        
        /// <summary>
        /// Actualiza un consultorio existente
        /// </summary>
        Task UpdateAsync(Consultorio consultorio);
        
        /// <summary>
        /// Elimina un consultorio
        /// </summary>
        Task RemoveAsync(Consultorio consultorio);
        
        /// <summary>
        /// Guarda los cambios en la base de datos
        /// </summary>
        Task<int> SaveChangesAsync();
    }
}
