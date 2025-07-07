using DentalPro.Application.Interfaces.IRepositories;
using DentalPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DentalPro.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Implementación del repositorio para la entidad Consultorio
    /// </summary>
    public class ConsultorioRepository : IConsultorioRepository
    {
        private readonly ApplicationDbContext _context;

        public ConsultorioRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtiene todos los consultorios
        /// </summary>
        public async Task<IEnumerable<Consultorio>> GetAllAsync()
        {
            return await _context.Consultorios
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene un consultorio por su ID
        /// </summary>
        public async Task<Consultorio?> GetByIdAsync(Guid id)
        {
            return await _context.Consultorios
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.IdConsultorio == id);
        }

        /// <summary>
        /// Agrega un nuevo consultorio
        /// </summary>
        public async Task<Consultorio> AddAsync(Consultorio consultorio)
        {
            await _context.Consultorios.AddAsync(consultorio);
            return consultorio;
        }

        /// <summary>
        /// Actualiza un consultorio existente
        /// </summary>
        public Task UpdateAsync(Consultorio consultorio)
        {
            _context.Consultorios.Update(consultorio);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Elimina un consultorio
        /// </summary>
        public Task RemoveAsync(Consultorio consultorio)
        {
            _context.Consultorios.Remove(consultorio);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Guarda los cambios en la base de datos
        /// </summary>
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
