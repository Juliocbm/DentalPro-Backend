using System;
using System.Collections.Generic;

namespace DentalPro.Domain.Entities;

/// <summary>
/// Representa información detallada específica para usuarios con rol de Doctor
/// </summary>
public class DoctorDetail
{
    /// <summary>
    /// ID único para DoctorDetail
    /// </summary>
    public Guid IdDoctorDetail { get; set; }
    
    /// <summary>
    /// ID del usuario asociado (relación 1:1 con Usuario)
    /// </summary>
    public Guid IdUsuario { get; set; }
    
    /// <summary>
    /// Especialidad principal del doctor
    /// </summary>
    public string Especialidad { get; set; } = null!;
    
    /// <summary>
    /// Años de experiencia profesional del doctor
    /// </summary>
    public int AñosExperiencia { get; set; }
    
    /// <summary>
    /// Número de licencia profesional del doctor
    /// </summary>
    public string? NumeroLicencia { get; set; }
    
    /// <summary>
    /// Fecha de graduación del doctor
    /// </summary>
    public DateTime FechaGraduacion { get; set; }
    
    /// <summary>
    /// Certificaciones profesionales del doctor (almacenado como JSON)
    /// </summary>
    public string? Certificaciones { get; set; }
    
    /// <summary>
    /// Navegación a la entidad Usuario relacionada
    /// </summary>
    public virtual Usuario Usuario { get; set; } = null!;
}
