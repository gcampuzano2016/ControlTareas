using System;
using System.Collections.Generic;

namespace CapaEntidad
{
    /// <summary>
    /// Un certificado laboral: lo que dice el papel. Sirve igual para el que se
    /// esta por generar -datos en vivo- y para uno del historial -la foto que se
    /// guardo al generarlo-, porque los dos tienen que imprimirse igual.
    /// </summary>
    public class EntCertificadoLaboral
    {
        public int IdCertificado { get; set; }
        public string CodUsuario { get; set; } = "";

        /// <summary>"SIN" o "CON" salario.</summary>
        public string Tipo { get; set; } = "SIN";

        /// <summary>"DOS" o "AGILITY": decide membrete, razon social y firmante.</summary>
        public string Empresa { get; set; } = "";

        public string Nombre { get; set; } = "";
        public string Cedula { get; set; } = "";
        public string Cargo { get; set; } = "";
        public DateTime? FechaIngreso { get; set; }

        /// <summary>Solo con valor en los "CON".</summary>
        public decimal? Salario { get; set; }

        public string Ciudad { get; set; } = "";

        /// <summary>La fecha que va impresa en el certificado.</summary>
        public DateTime FechaSolicitud { get; set; }

        /// <summary>Quien lo genero: el dueno o alguien de Talento Humano.</summary>
        public string UsuSolicita { get; set; } = "";
    }

    /// <summary>
    /// Los datos en vivo de una persona, antes de elegir el tipo. Trae el
    /// historial de sueldos crudo: cual rige lo decide NegHorasExtras.SalarioVigente.
    /// </summary>
    public class EntCertificadoDatos
    {
        /// <summary>Falso si el codigo no existe, esta inactivo o esta repetido.</summary>
        public bool Encontrado { get; set; }

        /// <summary>Si la persona esta en Vac_Colaborador, el maestro de GTH.</summary>
        public bool EnMaestro { get; set; }

        public string CodUsuario { get; set; } = "";
        public string Cedula { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Cargo { get; set; } = "";
        public string Empresa { get; set; } = "";
        public DateTime? FechaIngreso { get; set; }
        public string Ciudad { get; set; } = "";

        public List<EntHeSalario> Salarios { get; set; } = new List<EntHeSalario>();
    }
}
