using System;
using System.Collections.Generic;

namespace CapaEntidad
{
    /// <summary>
    /// Persona activa de la agenda, con su area ya resuelta por
    /// Sp_RTA_AgendaPersonasActivas: si no tiene area, o la suya esta
    /// inactiva, llega con la fila "Sin area" y SinArea = true.
    /// </summary>
    public class EntAgendaPersona
    {
        public string CodUsuario { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Correo { get; set; } = "";
        public string CodJefe { get; set; } = "";
        public int IdArea { get; set; }
        public string Area { get; set; } = "";
        public string Color { get; set; } = "";
        public bool SinArea { get; set; }
    }

    public class EntAgendaArea
    {
        public int IdArea { get; set; }
        public string Nombre { get; set; } = "";
        public string Color { get; set; } = "";
        public bool Activa { get; set; }
        public bool EsSinArea { get; set; }
        /// <summary>Personas activas en el area. Lo cuenta NegAgendaAreas.ConPersonas.</summary>
        public int Personas { get; set; }
    }

    public class EntAgendaEntregable
    {
        public int IdEntregable { get; set; }
        public int IdActividad { get; set; }
        public string Descripcion { get; set; } = "";
        public int Orden { get; set; }
        public bool Hecho { get; set; }
    }

    public class EntAgendaActividad
    {
        public int IdActividad { get; set; }
        public string Titulo { get; set; } = "";
        public string CodResponsable { get; set; } = "";
        /// <summary>Igual a CodResponsable si la registro la misma persona.</summary>
        public string CodAsignadoPor { get; set; } = "";
        public DateTime FechaEntrega { get; set; }
        /// <summary>Solo cuenta si la actividad no tiene entregables.</summary>
        public bool CompletadaManual { get; set; }
        public DateTime? FechaCompletada { get; set; }
        public bool Anulada { get; set; }
        /// <summary>null si la actividad no es el paso de un proceso.</summary>
        public int? IdProcesoPaso { get; set; }
        public int? IdProceso { get; set; }
        public string NombreProceso { get; set; } = "";
        public string EstadoPaso { get; set; } = "";
        public List<EntAgendaEntregable> Entregables { get; set; } = new List<EntAgendaEntregable>();
    }

    // ------------------------------------------------ lo que va a la pantalla

    public class EntAgendaContador
    {
        public int Actividades { get; set; }
        public int Completas { get; set; }
        /// <summary>De cualquier fecha, no solo de la semana.</summary>
        public int Atrasadas { get; set; }
        public int EntregablesTotal { get; set; }
        public int EntregablesHechos { get; set; }
        /// <summary>null si en la semana no hay entregables.</summary>
        public int? Porcentaje { get; set; }
    }

    public class EntAgendaTarjetaPersona
    {
        public string CodUsuario { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Iniciales { get; set; } = "";
        public EntAgendaContador Contador { get; set; } = new EntAgendaContador();
    }

    public class EntAgendaTarjetaArea
    {
        public int IdArea { get; set; }
        public string Area { get; set; } = "";
        public string Color { get; set; } = "";
        public bool SinArea { get; set; }
        public List<EntAgendaTarjetaPersona> Personas { get; set; } = new List<EntAgendaTarjetaPersona>();
        public EntAgendaContador Contador { get; set; } = new EntAgendaContador();
    }

    public class EntAgendaItemEntregable
    {
        public int IdEntregable { get; set; }
        public string Descripcion { get; set; } = "";
        public bool Hecho { get; set; }
    }

    public class EntAgendaItem
    {
        public int IdActividad { get; set; }
        public string Titulo { get; set; } = "";
        /// <summary>yyyy-MM-dd.</summary>
        public string FechaEntrega { get; set; } = "";
        /// <summary>Nombre de quien la asigno; vacio si es propia.</summary>
        public string AsignadoPor { get; set; } = "";
        public bool Propia { get; set; }
        public bool Completa { get; set; }
        public bool Atrasada { get; set; }
        public bool PuedeEditar { get; set; }
        public bool PuedeMarcar { get; set; }
        public int? IdProceso { get; set; }
        public string NombreProceso { get; set; } = "";
        public bool EsPasoDeProceso { get; set; }
        /// <summary>Aviso a mostrar antes de completar; vacio si no hay.</summary>
        public string AvisoAlCompletar { get; set; } = "";
        public List<EntAgendaItemEntregable> Entregables { get; set; } = new List<EntAgendaItemEntregable>();
    }

    public class EntAgendaDia
    {
        /// <summary>yyyy-MM-dd.</summary>
        public string Fecha { get; set; } = "";
        /// <summary>"lunes", "martes"... en minusculas.</summary>
        public string Nombre { get; set; } = "";
        public bool EsHoy { get; set; }
        public List<EntAgendaItem> Actividades { get; set; } = new List<EntAgendaItem>();
    }

    public class EntAgendaVistaPersona
    {
        public string CodUsuario { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Iniciales { get; set; } = "";
        public string Area { get; set; } = "";
        public string Color { get; set; } = "";
        /// <summary>Es la agenda de quien la mira.</summary>
        public bool EsPropia { get; set; }
        /// <summary>Puede crearle actividades: es la propia, o puede asignarle.</summary>
        public bool PuedeAsignar { get; set; }
        public List<EntAgendaItem> Atrasadas { get; set; } = new List<EntAgendaItem>();
        public List<EntAgendaDia> Dias { get; set; } = new List<EntAgendaDia>();
    }
}
