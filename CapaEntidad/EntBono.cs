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
        /// <summary>Suma de lo Aprobado; en un Historico, TotalHistorico (lo calcula Sp_RTA_BonoPeriodos).</summary>
        public decimal TotalAprobado { get; set; }
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
        /// <summary>Ok, pero una parte no se hizo (p. ej. Enviar salto algun bono): la pantalla lo muestra como advertencia.</summary>
        public bool Incompleto { get; set; }
    }

    /// <summary>Lo que Talento Humano ve antes de cerrar el mes.</summary>
    public class EntBonoCierre
    {
        public List<EntBono> QuedanFuera { get; set; } = new List<EntBono>();
        /// <summary>"tipo (empresa)" de los aprobados cuyo concepto SAP falta.</summary>
        public List<string> SinConcepto { get; set; } = new List<string>();
        public bool PuedeCerrar { get; set; }
        public decimal Total { get; set; }
    }

    /// <summary>Persona a cargo del jefe, sin cedula ni codigo SAP: solo lo que la pantalla Proponer necesita.</summary>
    public class EntBonoGente
    {
        public string CodUsuario { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Empresa { get; set; } = "";
        /// <summary>El lugar del ultimo bono no rechazado de la persona ("" si no hay).</summary>
        public string LugarSugerido { get; set; } = "";
        /// <summary>"" si la identificacion esta completa; si no, que dato falta (el bono se guarda pero no se envia).</summary>
        public string AvisoIdentificacion { get; set; } = "";
    }

    /// <summary>Semanas ya tomadas por un bono no rechazado (se tachan en el calendario salvo las del bono que se edita).</summary>
    public class EntBonoSemanasTomadas
    {
        public int IdBono { get; set; }
        public string CodUsuario { get; set; } = "";
        public int IdTipo { get; set; }
        public List<DateTime> Lunes { get; set; } = new List<DateTime>();
    }

    public class EntBonoPantallaProponer
    {
        /// <summary>null si no hay mes abierto.</summary>
        public EntBonoPeriodo Periodo { get; set; }
        public List<EntBonoGente> Gente { get; set; } = new List<EntBonoGente>();
        /// <summary>Los bonos del mes abierto y los pendientes cuyo jefe soy yo.</summary>
        public List<EntBono> Bonos { get; set; } = new List<EntBono>();
        /// <summary>Solo los tipos activos.</summary>
        public List<EntBonoTipo> Tipos { get; set; } = new List<EntBonoTipo>();
        public List<EntBonoSemanasTomadas> SemanasTomadas { get; set; } = new List<EntBonoSemanasTomadas>();
        /// <summary>Lunes marcables hoy: desde el 2026-08-03 hasta la ultima semana terminada.</summary>
        public List<DateTime> SemanasMarcables { get; set; } = new List<DateTime>();
    }

    public class EntBonoResumen
    {
        public int Borradores { get; set; }
        public int Enviados { get; set; }
        public int Aprobados { get; set; }
        public int Rechazados { get; set; }
        public decimal TotalAprobado { get; set; }
    }

    public class EntBonoPantallaRevisar
    {
        /// <summary>null si no hay mes abierto.</summary>
        public EntBonoPeriodo Periodo { get; set; }
        /// <summary>Los del mes abierto y los pendientes, sin los Borrador (son del jefe hasta que los envia).</summary>
        public List<EntBono> Bonos { get; set; } = new List<EntBono>();
        public EntBonoResumen Resumen { get; set; } = new EntBonoResumen();
        /// <summary>Personas activas: registro directo y nombre del jefe de cada fila.</summary>
        public List<EntBonoPersona> Personas { get; set; } = new List<EntBonoPersona>();
        /// <summary>Todo el catalogo (activos e inactivos).</summary>
        public List<EntBonoTipo> Tipos { get; set; } = new List<EntBonoTipo>();
    }

    /// <summary>Una fila del reporte mensual.</summary>
    public class EntBonoMes
    {
        public int IdPeriodo { get; set; }
        public int Anio { get; set; }
        public int Mes { get; set; }
        public string Estado { get; set; } = "";
        public decimal Total { get; set; }
        /// <summary>Fraccion contra el mes calendario anterior; null si no existe o su total es cero.</summary>
        public decimal? Variacion { get; set; }
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
