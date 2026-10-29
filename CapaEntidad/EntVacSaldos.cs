using System;
using System.Collections.Generic;

namespace CapaEntidad
{
    /// <summary>Fila de Vac_Colaborador (Sp_RTA_VacColaboradores).</summary>
    public class EntVacColaborador
    {
        public int IdColaborador { get; set; }
        public string Cedula { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Empresa { get; set; } = "";
        public string Area { get; set; } = "";
        public string Cargo { get; set; } = "";
        public DateTime FechaIngreso { get; set; }
        /// <summary>ACTIVO o INACTIVO.</summary>
        public string Estado { get; set; } = "";
        public DateTime? FechaSalida { get; set; }
        public DateTime? FechaCorte { get; set; }
        public decimal? SaldoCorte { get; set; }
        public string Observacion { get; set; } = "";
    }

    /// <summary>Sp_RTA_VacCalcular @Salida = 'SALDOS': una fila por persona.</summary>
    public class EntVacSaldo
    {
        public int IdColaborador { get; set; }
        /// <summary>La pedida, o la de salida si es anterior: un inactivo se calcula a su salida.</summary>
        public DateTime FechaCalculo { get; set; }
        public int Cumplidos { get; set; }
        public decimal DiasPorAnio { get; set; }
        public DateTime InicioPeriodo { get; set; }
        public DateTime ProximoAniversario { get; set; }
        public decimal AjusteCorte { get; set; }
        public decimal Disponible { get; set; }
        public decimal UsadoEnCurso { get; set; }
        public decimal Proporcional { get; set; }
        public decimal Saldo { get; set; }
        public decimal PorVencer { get; set; }
        public DateTime? ProximoVencimiento { get; set; }
        public decimal Vencidos { get; set; }
        public decimal TotalGozado { get; set; }
        /// <summary>ANTICIPO, POR VENCER, ACUMULANDO o AL DIA.</summary>
        public string Estado { get; set; } = "";
    }

    /// <summary>Sp_RTA_VacCalcular @Salida = 'PERIODOS'.</summary>
    public class EntVacPeriodo
    {
        public int N { get; set; }
        public string Periodo { get; set; } = "";
        public DateTime Inicio { get; set; }
        public DateTime Fin { get; set; }
        public decimal DiasGanados { get; set; }
        public decimal DiasTomados { get; set; }
        public decimal DiasVencidos { get; set; }
        public decimal Saldo { get; set; }
        public DateTime Vence { get; set; }
        public string Estado { get; set; } = "";
    }

    /// <summary>
    /// Sp_RTA_VacCalcular @Salida = 'MOVIMIENTOS', completado con lo cargado
    /// (Sp_RTA_VacMovimientosOrigen): el motor dice cuanto descuenta, la tabla
    /// dice que se pidio.
    /// </summary>
    public class EntVacMovimiento
    {
        public int IdMovimiento { get; set; }
        public string Tipo { get; set; } = "";
        public string Estado { get; set; } = "";
        public DateTime? Desde { get; set; }
        public DateTime? Hasta { get; set; }
        /// <summary>SIN FECHA, ANULADO, RELACION ANTERIOR, ANTES DEL CORTE, POSTERIOR o DESCUENTA.</summary>
        public string Situacion { get; set; } = "";
        public int? AnioServicio { get; set; }
        public decimal DiasBase { get; set; }
        public decimal DiasFinDeSemana { get; set; }
        public decimal DiasDescuenta { get; set; }
        public int Cruces { get; set; }

        public decimal? Dias { get; set; }
        public decimal? Horas { get; set; }
        public decimal? Minutos { get; set; }
        public bool BeneficioDos { get; set; }
        public string Observacion { get; set; } = "";
        public string Origen { get; set; } = "";
    }

    /// <summary>Lo que lee DaoVacSaldos.Tablero, antes de unir.</summary>
    public class EntVacDatos
    {
        public List<EntVacColaborador> Colaboradores { get; set; } = new List<EntVacColaborador>();
        public List<EntVacSaldo> Saldos { get; set; } = new List<EntVacSaldo>();
    }

    /// <summary>Una fila de la tabla de saldos: la persona y su saldo.</summary>
    public class EntVacFila
    {
        public EntVacColaborador Colaborador { get; set; }
        public EntVacSaldo Saldo { get; set; }
    }

    /// <summary>Totales de las tarjetas. Se calculan sobre los ACTIVOS, como el DASHBOARD del Excel.</summary>
    public class EntVacResumen
    {
        public int Activos { get; set; }
        public int Inactivos { get; set; }
        public decimal SaldoTotal { get; set; }
        public int PersonasPorVencer { get; set; }
        public decimal DiasPorVencer { get; set; }
        public int PersonasAnticipo { get; set; }
        public decimal DiasAnticipo { get; set; }
        public int PersonasAcumulando { get; set; }
        public decimal DiasVencidos { get; set; }
    }

    public class EntVacTablero
    {
        public DateTime FechaCalculo { get; set; }
        public EntVacResumen Resumen { get; set; } = new EntVacResumen();
        public List<EntVacFila> Filas { get; set; } = new List<EntVacFila>();
        /// <summary>Personas del maestro que el motor no devolvio, o al reves. Deberia quedar vacio.</summary>
        public List<string> Avisos { get; set; } = new List<string>();
    }

    /// <summary>
    /// Un estado individual ya emitido: la ficha que se imprimio, el folio con
    /// el que quedo registrado y quien lo genero.
    /// </summary>
    public class EntVacEstado
    {
        public int Folio { get; set; }
        public DateTime Emitido { get; set; }
        public string Usuario { get; set; } = "";
        public EntVacFicha Ficha { get; set; }
    }

    /// <summary>
    /// Lo que ve un colaborador de si mismo en "Mis vacaciones". Si Ficha es
    /// null, Aviso dice por que.
    /// </summary>
    public class EntVacMias
    {
        public EntVacFicha Ficha { get; set; }
        /// <summary>IdMovimiento -> periodo al que se carga (solo las que descuentan).</summary>
        public Dictionary<int, string> PeriodoDe { get; set; } = new Dictionary<int, string>();
        public string Aviso { get; set; } = "";
    }

    /// <summary>Sp_RTA_VacSueldos: historiales por IdColaborador y el divisor del valor diario.</summary>
    public class EntVacSueldos
    {
        /// <summary>HE_Salario: manda cuando la persona lo tiene.</summary>
        public Dictionary<int, List<EntHeSalario>> HorasExtras { get; set; } = new Dictionary<int, List<EntHeSalario>>();
        /// <summary>Vac_Sueldo: el de los que no estan en HE_Salario.</summary>
        public Dictionary<int, List<EntHeSalario>> Vacaciones { get; set; } = new Dictionary<int, List<EntHeSalario>>();
        public decimal DiasMes { get; set; } = 30m;
    }

    /// <summary>Una fila de la hoja NOMINA: dias pendientes y su valor.</summary>
    public class EntVacNominaFila
    {
        public EntVacColaborador Colaborador { get; set; }
        public EntVacSaldo Saldo { get; set; }
        public decimal? Sueldo { get; set; }
        /// <summary>"HE_Salario", "Excel de GTH", o "" sin sueldo.</summary>
        public string FuenteSueldo { get; set; } = "";
        public decimal? ValorDia { get; set; }
        public decimal? Valor { get; set; }
        public string Observacion { get; set; } = "";
    }

    public class EntVacNominaEmpresa
    {
        public string Empresa { get; set; } = "";
        public int Personas { get; set; }
        public decimal Dias { get; set; }
        public decimal Valor { get; set; }
        public int SinSueldo { get; set; }
    }

    /// <summary>La nomina completa. Los totales son de ACTIVOS, como NOMINA!N4:O4 del Excel.</summary>
    public class EntVacNomina
    {
        public DateTime FechaCalculo { get; set; }
        public List<EntVacNominaFila> Filas { get; set; } = new List<EntVacNominaFila>();
        public List<EntVacNominaEmpresa> PorEmpresa { get; set; } = new List<EntVacNominaEmpresa>();
        public int Activos { get; set; }
        public decimal TotalDias { get; set; }
        public decimal TotalValor { get; set; }
        public int SinSueldo { get; set; }
        public decimal ValorAnticipos { get; set; }
        public List<string> Avisos { get; set; } = new List<string>();
    }

    public class EntVacFicha
    {
        public DateTime FechaCalculo { get; set; }
        public EntVacColaborador Colaborador { get; set; }
        public EntVacSaldo Saldo { get; set; }
        public List<EntVacPeriodo> Periodos { get; set; } = new List<EntVacPeriodo>();
        public List<EntVacMovimiento> Movimientos { get; set; } = new List<EntVacMovimiento>();
    }
}
