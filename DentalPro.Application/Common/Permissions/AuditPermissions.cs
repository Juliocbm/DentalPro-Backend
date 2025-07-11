namespace DentalPro.Application.Common.Permissions
{
    /// <summary>
    /// Define los permisos para el módulo de auditoría
    /// </summary>
    public static class AuditPermissions
    {
        private const string Base = "audit";

        /// <summary>
        /// Permiso para ver todos los registros de auditoría
        /// </summary>
        public const string ViewAll = Base + ".view.all";

        /// <summary>
        /// Permiso para ver registros de auditoría por entidad
        /// </summary>
        public const string ViewEntity = Base + ".view.entity";

        /// <summary>
        /// Permiso para ver registros de auditoría por usuario
        /// </summary>
        public const string ViewByUser = Base + ".view.user";
    }
}
