using System;

namespace CapaEntidad
{
    /// <summary>
    /// Colaborador del modulo (CA_empleados). Es una tabla propia: se puede
    /// registrar a alguien que todavia no tiene ficha en dbo.Empleados.
    /// IdEmpleado es el vinculo opcional a esa ficha.
    /// </summary>
    public class EntCurvaEmpleado
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; }
        public string Cargo { get; set; }
        public string Area { get; set; }
        public string Empresa { get; set; }
        public string UnidadNegocio { get; set; }
        public int? JefeId { get; set; }
        public string JefeNombre { get; set; }
        public DateTime FechaIngreso { get; set; }
        /// <summary>"activo" o "inactivo". Informativo: no cierra el proceso.</summary>
        public string Estado { get; set; }
        public DateTime CreatedAt { get; set; }
        public long? IdEmpleado { get; set; }
        /// <summary>Proceso mas reciente (el sistema trabaja con uno por colaborador).</summary>
        public int? ProcesoId { get; set; }
        public string ProcesoEstado { get; set; }
    }

    /// <summary>Una ficha de dbo.Empleados, para enlazar al colaborador nuevo.</summary>
    public class EntCurvaEmpleadoSistema
    {
        public long IdEmpleado { get; set; }
        public string Nombre { get; set; }
        public string Cargo { get; set; }
        public string Area { get; set; }
        public string Empresa { get; set; }
    }
}
