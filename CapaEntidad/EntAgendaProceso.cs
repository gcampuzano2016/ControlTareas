using System;
using System.Collections.Generic;

namespace CapaEntidad
{
    public static class AgendaProcesoEstados
    {
        public const string EnCurso = "EnCurso";
        public const string Terminado = "Terminado";
        public const string Cancelado = "Cancelado";
    }

    public static class AgendaPasoEstados
    {
        public const string Pendiente = "Pendiente";
        public const string EnCurso = "EnCurso";
        public const string Completo = "Completo";
        public const string Cancelado = "Cancelado";
    }

    public class EntAgendaPlantillaPaso
    {
        public int IdPlantillaPaso { get; set; }
        public int Orden { get; set; }
        public int IdArea { get; set; }
        public string Area { get; set; } = "";
        public string Titulo { get; set; } = "";
        public int PlazoDias { get; set; }
        /// <summary>Uno por linea.</summary>
        public string Entregables { get; set; } = "";
    }

    public class EntAgendaPlantilla
    {
        public int IdPlantilla { get; set; }
        public string Nombre { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public bool Activa { get; set; }
        public bool TieneProcesos { get; set; }
        public List<EntAgendaPlantillaPaso> Pasos { get; set; } = new List<EntAgendaPlantillaPaso>();
    }

    public class EntAgendaProcesoPaso
    {
        public int IdProcesoPaso { get; set; }
        public int IdProceso { get; set; }
        public int Orden { get; set; }
        public int IdArea { get; set; }
        public string Area { get; set; } = "";
        public string Color { get; set; } = "";
        public string Titulo { get; set; } = "";
        public int PlazoDias { get; set; }
        public string Entregables { get; set; } = "";
        public string CodResponsable { get; set; } = "";
        public string Estado { get; set; } = "";
        public DateTime? FechaActivacion { get; set; }
        public DateTime? FechaCompletado { get; set; }
        public int? IdActividad { get; set; }
        public DateTime? FechaEntrega { get; set; }
        public bool ActividadCompleta { get; set; }
    }

    public class EntAgendaProceso
    {
        public int IdProceso { get; set; }
        public string Nombre { get; set; } = "";
        public int IdPlantilla { get; set; }
        public string Plantilla { get; set; } = "";
        public string CodIniciadoPor { get; set; } = "";
        public string Estado { get; set; } = "";
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string MotivoCancelacion { get; set; } = "";
        public List<EntAgendaProcesoPaso> Pasos { get; set; } = new List<EntAgendaProcesoPaso>();
    }

    /// <summary>Lo que devuelve un marcado que hizo avanzar un proceso.</summary>
    public class EntAgendaAvance
    {
        public int? IdProcesoPasoActivado { get; set; }
        public int? IdProcesoTerminado { get; set; }
    }
}
