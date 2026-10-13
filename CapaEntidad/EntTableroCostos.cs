using System;
using System.Collections.Generic;

namespace CapaEntidad
{
    /*  Entidades del Tablero de Costos (TableroCostos.aspx).
        Spec: docs/superpowers/specs/2026-10-12-tablero-costos-design.md

        [Serializable] en lo que el handler guarda en Session: hoy la sesion es
        InProc y no lo necesita, pero si alguien la pasa a StateServer las
        clases sin el atributo revientan al guardar, lejos de este codigo. */

    /// <summary>Una fila de R_DetTareasAranda tal como la trae Sp_RTA_TableroCostos.</summary>
    [Serializable]
    public class EntTcActividad
    {
        public long IdDetalle { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string Tiempo { get; set; } = "";
        public string CodResponsable { get; set; } = "";
        public string NomResponsable { get; set; } = "";
        public string Empresa { get; set; } = "";
        public string Contrato { get; set; } = "";
        public string Ticket { get; set; } = "";
        public string OrdenServicio { get; set; } = "";
        public string Tarea { get; set; } = "";
        public string Estado { get; set; } = "";
        public int? IdEstadoIni { get; set; }
        /// <summary>Det_Horas_Extras_Tipo: 1 = 50%, 2 = 100%; cualquier otro valor, sin recargo.</summary>
        public int TipoHE { get; set; }
        /// <summary>Det_Horas_Extras_Estado: 1 Solicitado, 2 Aprobado, 3 Rechazado; otro, sin solicitud.</summary>
        public int EstadoHE { get; set; }
    }

    /// <summary>Salario vigente y parametros de un Cod_Usuario (Empleados + HE_*).</summary>
    [Serializable]
    public class EntTcTarifa
    {
        public string CodUsuario { get; set; } = "";
        public string Cargo { get; set; } = "";
        public int? JornadaHorasDia { get; set; }
        public bool? AplicaHE { get; set; }
        public decimal? Salario { get; set; }
    }

    /// <summary>Los tres resultados de Sp_RTA_TableroCostos.</summary>
    public class EntTcDatos
    {
        public List<EntTcActividad> Actividades { get; set; } = new List<EntTcActividad>();
        public List<EntTcTarifa> Tarifas { get; set; } = new List<EntTcTarifa>();
        public Dictionary<string, decimal> Parametros { get; set; } =
            new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>Una actividad ya clasificada y valorada. Es la fila del detalle y del Excel.</summary>
    [Serializable]
    public class EntTcFila
    {
        public long IdDetalle { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public int Anio { get; set; }
        public int Mes { get; set; }
        /// <summary>"Ene-2025", "Feb-2025"... igual que la columna Periodo del Excel.</summary>
        public string Periodo { get; set; } = "";
        public string Responsable { get; set; } = "";
        public string Empresa { get; set; } = "";
        public string Contrato { get; set; } = "";
        public string Ticket { get; set; } = "";
        public string OrdenServicio { get; set; } = "";
        public string Tarea { get; set; } = "";
        public string Estado { get; set; } = "";
        /// <summary>Normal, Extra o Traslado.</summary>
        public string Tipo { get; set; } = "";
        /// <summary>"50%", "100%", "Sin recargo" (traslados) o "" (normales).</summary>
        public string Recargo { get; set; } = "";
        /// <summary>Aprobado, Solicitado, Rechazado, Sin solicitud, No aplica o "".</summary>
        public string EstadoHE { get; set; } = "";
        public string Tiempo { get; set; } = "";
        public decimal Horas { get; set; }
        public decimal Factor { get; set; }
        public decimal ValorHora { get; set; }
        public decimal Valor { get; set; }
        /// <summary>El responsable no tiene salario: sus horas se valoran en $0.</summary>
        public bool SinSalario { get; set; }
    }

    /// <summary>Lo que el handler guarda en Session por rango de fechas.</summary>
    [Serializable]
    public class EntTcClasificado
    {
        public List<EntTcFila> Filas { get; set; } = new List<EntTcFila>();
        /// <summary>Faltaba algun parametro vigente y se usaron 1,5 / 2 / 30.</summary>
        public bool ParametrosPorDefecto { get; set; }
    }

    /// <summary>Filtros de la pantalla. "" o "Todos" = sin filtro.</summary>
    public class EntTcFiltro
    {
        public string Mes { get; set; } = "";
        public string Responsable { get; set; } = "";
        public string Empresa { get; set; } = "";
        public string Contrato { get; set; } = "";
        public string Recargo { get; set; } = "";
        public string EstadoHE { get; set; } = "";
    }

    public class EntTcCelda
    {
        public decimal Horas { get; set; }
        public decimal Valor { get; set; }
        public int Actividades { get; set; }
    }

    public class EntTcIndicadores
    {
        public EntTcCelda Normal { get; set; } = new EntTcCelda();
        public EntTcCelda Extra { get; set; } = new EntTcCelda();
        public EntTcCelda Traslado { get; set; } = new EntTcCelda();
        public EntTcCelda Total { get; set; } = new EntTcCelda();
    }

    /// <summary>Una fila de "Resumen por tipo y recargo" (las 11 del Excel).</summary>
    public class EntTcResumenFila
    {
        public string Concepto { get; set; } = "";
        public bool EsSubtotal { get; set; }
        public decimal Horas { get; set; }
        public decimal PctHoras { get; set; }
        public decimal Valor { get; set; }
        public decimal PctValor { get; set; }
        public int Actividades { get; set; }
        public decimal ValorPorHora { get; set; }
    }

    /// <summary>Una fila de "Horas extra por estado y recargo".</summary>
    public class EntTcEstadoExtra
    {
        public string Estado { get; set; } = "";
        public decimal Horas50 { get; set; }
        public decimal Horas100 { get; set; }
        public decimal Valor50 { get; set; }
        public decimal Valor100 { get; set; }
        public decimal Total { get; set; }
    }

    /// <summary>Una fila de "Traslado por estado y recargo".</summary>
    public class EntTcEstadoTraslado
    {
        public string Estado { get; set; } = "";
        public decimal HorasSinRecargo { get; set; }
        public decimal Horas50 { get; set; }
        public decimal Horas100 { get; set; }
        public decimal TotalHoras { get; set; }
        public decimal TotalValor { get; set; }
    }

    /// <summary>Una fila de las tablas por año y por mes. H* = horas, V* = valor.</summary>
    public class EntTcPeriodo
    {
        public string Periodo { get; set; } = "";
        public decimal HNormal { get; set; }
        public decimal HExtra50 { get; set; }
        public decimal HExtra100 { get; set; }
        public decimal HTrasladoSin { get; set; }
        public decimal HTraslado50 { get; set; }
        public decimal HTraslado100 { get; set; }
        public decimal HTotal { get; set; }
        public decimal VNormal { get; set; }
        public decimal VExtra50 { get; set; }
        public decimal VExtra100 { get; set; }
        public decimal VTrasladoSin { get; set; }
        public decimal VTraslado50 { get; set; }
        public decimal VTraslado100 { get; set; }
        public decimal VTotal { get; set; }
    }

    /// <summary>Una fila de los top 15 por tipo de hora.</summary>
    public class EntTcTop
    {
        public string Responsable { get; set; } = "";
        public decimal Horas { get; set; }
        public decimal HorasSinRecargo { get; set; }
        public decimal Horas50 { get; set; }
        public decimal Horas100 { get; set; }
        public decimal Valor { get; set; }
    }

    /// <summary>Todo lo que dibuja TableroCostos.aspx.</summary>
    public class EntTableroCostos
    {
        public EntTcIndicadores Indicadores { get; set; } = new EntTcIndicadores();
        public List<EntTcResumenFila> Resumen { get; set; } = new List<EntTcResumenFila>();
        public List<EntTcEstadoExtra> ExtrasPorEstado { get; set; } = new List<EntTcEstadoExtra>();
        public List<EntTcEstadoTraslado> TrasladosPorEstado { get; set; } = new List<EntTcEstadoTraslado>();
        public List<EntTcPeriodo> PorAnio { get; set; } = new List<EntTcPeriodo>();
        public List<EntTcPeriodo> PorMes { get; set; } = new List<EntTcPeriodo>();
        public List<EntTcTop> TopNormal { get; set; } = new List<EntTcTop>();
        public List<EntTcTop> TopExtra { get; set; } = new List<EntTcTop>();
        public List<EntTcTop> TopTraslado { get; set; } = new List<EntTcTop>();
        public List<EntTcFila> Detalle { get; set; } = new List<EntTcFila>();

        public List<string> Meses { get; set; } = new List<string>();
        public List<string> Responsables { get; set; } = new List<string>();
        public List<string> Empresas { get; set; } = new List<string>();
        public List<string> Contratos { get; set; } = new List<string>();

        public List<string> SinSalario { get; set; } = new List<string>();
        /// <summary>"" si no hay nada que avisar.</summary>
        public string Aviso { get; set; } = "";
        public bool ParametrosPorDefecto { get; set; }
    }
}
