using System;
using System.Collections.Generic;

namespace DentalPro.Domain.Entities;

/// <summary>
/// Representa información detallada específica para usuarios con rol de Doctor
/// Esta entidad se implementará como Owned Entity Type en EF Core
/// </summary>
public class DoctorDetail
{
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
}
