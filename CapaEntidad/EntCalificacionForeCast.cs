using System;
using System.Collections.Generic;

namespace CapaEntidad
{
    /// <summary>Lo que responde el vendedor en el modal. Una fila de ForeCastCalificacion.</summary>
    public class EntCalificacionRespuesta
    {
        public long IdForeCast { get; set; }
        public int Presupuesto { get; set; }
        public int Autoridad { get; set; }
        public int Necesidad { get; set; }
        public string DecisorCargo { get; set; }
        public bool PropuestaEnviada { get; set; }
        public string AccionComprometida { get; set; }
        public DateTime? FechaAccion { get; set; }
        public string Usuario { get; set; }
        public DateTime FechaRegistro { get; set; }
    }

    /// <summary>GANADO y PERDIDO de los ultimos 36 meses por segmento o por tipo de empresa.</summary>
    public class EntTasaExito
    {
        /// <summary>"SEGMENTO" o "TIPO".</summary>
        public string Dimension { get; set; }
        public string Valor { get; set; }
        public int Ganados { get; set; }
        public int Perdidos { get; set; }
    }

    public class EntAlertaCalificacion
    {
        public int Codigo { get; set; }
        public string Texto { get; set; }
    }

    /// <summary>
    /// Una oportunidad abierta: los datos crudos que trae la base y, despues de
    /// NegCalificacionForeCast.Evaluar, el resultado de la calificacion.
    /// </summary>
    public class EntCalificacionForeCast
    {
        /* Crudos */
        public long IdForeCast { get; set; }
        public int IdCliente { get; set; }
        public string Cliente { get; set; }
        public string DetalleProyecto { get; set; }
        public string Marca { get; set; }
        public string Prioridad { get; set; }
        public string GerentedeCuenta { get; set; }
        public string Sucursal { get; set; }
        public string SegmentodeMercado { get; set; }
        public string TipoEmpresa { get; set; }
        public decimal? PVPEstimado { get; set; }
        public decimal? UtilidadBrutaEstimada { get; set; }
        public int? IdNegocio { get; set; }
        public decimal? Porcentaje { get; set; }
        public string DescripcionNegocio { get; set; }
        public DateTime? FechaCierre { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string Contacto { get; set; }
        public string LiderProyectoCliente { get; set; }
        public string Observaciones { get; set; }
        public int FechasCierreDistintas { get; set; }
        public int GanadosCliente { get; set; }
        public int ContactosCuenta { get; set; }
        /// <summary>La calificacion vigente, o null si nunca se califico.</summary>
        public EntCalificacionRespuesta Calificacion { get; set; }

        /* Resultado */
        public int Presupuesto { get; set; }
        public int Autoridad { get; set; }
        public int Necesidad { get; set; }
        public int Tiempo { get; set; }
        public int Actividad { get; set; }
        public int Encaje { get; set; }
        public int Puntaje { get; set; }
        public string Estado { get; set; }
        public bool SinCalificar { get; set; }
        public decimal MontoPonderado { get; set; }
        public DateTime? UltimaActividad { get; set; }
        public List<EntAlertaCalificacion> Alertas { get; set; } = new List<EntAlertaCalificacion>();
        public List<string> Faltantes { get; set; } = new List<string>();
        public string AccionSugerida { get; set; }

        /* Textos para la pantalla (dd/MM/yyyy, cultura invariante) */
        public string FechaCierreTexto { get; set; }
        public string UltimaActividadTexto { get; set; }
        public string FechaCalificacionTexto { get; set; }
        public string FechaAccionTexto { get; set; }
    }

    /// <summary>Lo que devuelve el DAO: los dos resultados del procedimiento.</summary>
    public class EntDatosCalificacion
    {
        public List<EntCalificacionForeCast> Oportunidades { get; set; } = new List<EntCalificacionForeCast>();
        public List<EntTasaExito> Tasas { get; set; } = new List<EntTasaExito>();
    }

    /// <summary>Filtros de la pantalla. Vacio = todos.</summary>
    public class EntFiltroCalificacion
    {
        public string GerentedeCuenta { get; set; } = "";
        public string Sucursal { get; set; } = "";
        public string Prioridad { get; set; } = "";
        /// <summary>"Caliente", "Tibia", "Fría" o "Sin calificar".</summary>
        public string Estado { get; set; } = "";
        public string Marca { get; set; } = "";
    }

    public class EntPantallaCalificacion
    {
        public List<EntCalificacionForeCast> Oportunidades { get; set; } = new List<EntCalificacionForeCast>();
        public int Total { get; set; }
        public decimal MontoTotal { get; set; }
        public decimal MontoPonderado { get; set; }
        public int Calientes { get; set; }
        public int Tibias { get; set; }
        public int Frias { get; set; }
        public int SinCalificar { get; set; }
        /// <summary>Entre 0 y 100, con un decimal.</summary>
        public decimal PorcentajeSinCalificar { get; set; }
        public List<long> Top5 { get; set; } = new List<long>();
        public List<long> EnRiesgo { get; set; } = new List<long>();
        /* Valores de los combos, tomados del conjunto SIN filtrar */
        public List<string> Gerentes { get; set; } = new List<string>();
        public List<string> Sucursales { get; set; } = new List<string>();
        public List<string> Etapas { get; set; } = new List<string>();
        public List<string> Marcas { get; set; } = new List<string>();
    }
}
