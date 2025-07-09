using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DentalPro.Domain.Entities;

namespace DentalPro.Application.Interfaces.IRepositories
{
    public interface IPacienteRepository
    {
        Task<IEnumerable<Paciente>> GetAllByConsultorioAsync(Guid idConsultorio);
        Task<Paciente?> GetByIdAsync(Guid id);
        Task<Paciente?> GetByCorreoAsync(string correo);
        Task<Paciente> CreateAsync(Paciente paciente);
        Task UpdateAsync(Paciente paciente);
        Task<bool> DeleteAsync(Guid id);
        Task SaveChangesAsync();
        
        /// <summary>
        /// Verifica si existe un paciente con el ID especificado
        /// </summary>
        /// <param name="id">ID del paciente a verificar</param>
        /// <returns>True si existe, False en caso contrario</returns>
        Task<bool> ExistsByIdAsync(Guid id);
    }
}
