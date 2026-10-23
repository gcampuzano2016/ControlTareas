using System;
using System.Collections.Generic;

namespace CapaEntidad
{
    /// <summary>Persona activa a la que se le puede proponer un bono.</summary>
    public class EntBonoPersona
    {
        public string CodUsuario { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Correo { get; set; } = "";
        public string CodJefe { get; set; } = "";
        public string Empresa { get; set; } = "";
        public string CodSap { get; set; } = "";
        public string Cedula { get; set; } = "";
        public int IdPerfil { get; set; }
    }

    /// <summary>Tipo de bono del catalogo.</summary>
    public class EntBonoTipo
    {
        public int IdTipo { get; set; }
        public string Nombre { get; set; } = "";
        public string Familia { get; set; } = "";
        public decimal? ValorSugerido { get; set; }
        public bool PorSemana { get; set; }
        public bool Activo { get; set; }
        /// <summary>Empresa -> codigo de concepto SAP.</summary>
        public Dictionary<string, string> Conceptos { get; set; } = new Dictionary<string, string>();
    }

    public class EntBonoPeriodo
    {
        public int IdPeriodo { get; set; }
        public int Anio { get; set; }
        public int Mes { get; set; }
        public string Estado { get; set; } = "";
        public decimal? TotalHistorico { get; set; }
        public DateTime? FechaApertura { get; set; }
        public DateTime? FechaCierre { get; set; }
    }

    public class EntBono
    {
        public int IdBono { get; set; }
        public int? IdPeriodo { get; set; }
        public string CodUsuario { get; set; } = "";
        public string Empresa { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Identificacion { get; set; } = "";
        public string Lugar { get; set; } = "";
        public int IdTipo { get; set; }
        public int Semanas { get; set; }
        public decimal? ValorSugerido { get; set; }
        public decimal Valor { get; set; }
        public string MotivoAjuste { get; set; } = "";
        public string Observacion { get; set; } = "";
        public string Estado { get; set; } = "";
        public string Origen { get; set; } = "";
        public string CodJefe { get; set; } = "";
        public string MotivoRechazo { get; set; } = "";
        public string NombreTipo { get; set; } = "";
        public string CodigoConcepto { get; set; } = "";
        public List<DateTime> Lunes { get; set; } = new List<DateTime>();
    }

    /// <summary>Resultado de una accion. Aviso pide confirmar, no bloquea.</summary>
    public class EntBonoResultado
    {
        public bool Ok { get; set; }
        public string Mensaje { get; set; } = "";
        public string Aviso { get; set; } = "";
    }

    public static class BonoEstados
    {
        public const string Borrador = "Borrador";
        public const string Enviado = "Enviado";
        public const string Aprobado = "Aprobado";
        public const string Rechazado = "Rechazado";
    }

    public static class BonoPeriodoEstados
    {
        public const string Abierto = "Abierto";
        public const string Cerrado = "Cerrado";
        public const string Historico = "Historico";
    }

    public static class BonoOrigenes
    {
        public const string Jefe = "Jefe";
        public const string TalentoHumano = "TalentoHumano";
        public const string Migracion = "Migracion";
    }
}
