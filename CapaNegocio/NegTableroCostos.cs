using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CapaNegocio
{
    /// <summary>
    /// Tablero de Costos: la logica de C:\ReporteExcel\generar_dashboard.py
    /// llevada a la aplicacion. El Excel que genera ese script es la referencia:
    /// si el tablero y el Excel no cuadran con los mismos datos, el que esta mal
    /// es el tablero.
    ///
    /// Clase pura en este archivo y en NegTableroCostos.Armar.cs: no toca base,
    /// sesion ni HttpContext. El unico archivo que llama a CapaDato es
    /// NegTableroCostos.Datos.cs.
    /// </summary>
    public static partial class NegTableroCostos
    {
        public const string Normal = "Normal";
        public const string Extra = "Extra";
        public const string Traslado = "Traslado";

        public const string SinRecargo = "Sin recargo";
        public const string Recargo50 = "50%";
        public const string Recargo100 = "100%";

        public const string Todos = "Todos";

        /// <summary>Las seis categorias de los desgloses y los graficos, en el orden del Excel.</summary>
        public const string CatNormal = "Normal";
        public const string CatExtra50 = "Extra 50%";
        public const string CatExtra100 = "Extra 100%";
        public const string CatTrasladoSin = "Traslado sin recargo";
        public const string CatTraslado50 = "Traslado 50%";
        public const string CatTraslado100 = "Traslado 100%";

        /// <summary>
        /// Perfiles que ven el tablero completo. Quien agregue uno aqui tiene que
        /// agregarlo tambien en PerfilMenu (docs/sql/2026-10-12-tablero-costos.sql).
        /// </summary>
        public static readonly int[] PerfilesAutorizados = { 14, 18 };

        /// <summary>
        /// Regla de acceso al tablero: un perfil autorizado, o el modulo extra
        /// TableroCostos.aspx asignado a la persona en R_UsuarioMenu (pantalla
        /// ParametrizacionMenuUsuario.aspx). Lo segundo da el tablero a una
        /// persona sin abrirlo a todo su perfil.
        ///
        /// Los perfiles autorizados no consultan la base. Si la consulta por
        /// usuario falla, se niega: el tablero muestra montos calculados con
        /// salarios.
        /// </summary>
        public static bool PuedeVer(int? idPerfil, Func<bool> accesoPorUsuario)
        {
            if (idPerfil.HasValue && Array.IndexOf(PerfilesAutorizados, idPerfil.Value) >= 0)
            {
                return true;
            }
            if (accesoPorUsuario == null)
            {
                return false;
            }
            try
            {
                return accesoPorUsuario();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Tablero de Costos: no se pudo consultar el acceso por usuario. " + ex);
                return false;
            }
        }

        public static readonly string[] Recargos = { SinRecargo, Recargo50, Recargo100 };
        public static readonly string[] EstadosHE = { "Aprobado", "Solicitado", "Rechazado", "Sin solicitud", "No aplica" };

        /// <summary>IdDet_EstadoIni que el Excel cuenta como traslado aunque el estado no diga "Movilizacion".</summary>
        private static readonly int[] EstadosIniTraslado = { 14, 28, 62 };

        private static readonly string[] NombresMes =
            { "Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic" };

        /// <summary>
        /// Una actividad cae en UN solo tipo, en este orden: Traslado, Extra, Normal.
        /// El prefijo "movilizaci" cubre "Movilizacion" con y sin tilde.
        /// </summary>
        public static string Tipo(EntTcActividad a)
        {
            string estado = (a.Estado ?? "").Trim().ToLowerInvariant();
            if (estado.StartsWith("movilizaci", StringComparison.Ordinal)
                || (a.IdEstadoIni.HasValue && Array.IndexOf(EstadosIniTraslado, a.IdEstadoIni.Value) >= 0))
            {
                return Traslado;
            }
            if (a.TipoHE == 1 || a.TipoHE == 2)
            {
                return Extra;
            }
            return Normal;
        }

        public static string Recargo(EntTcActividad a, string tipo)
        {
            if (tipo != Normal && a.TipoHE == 1) { return Recargo50; }
            if (tipo != Normal && a.TipoHE == 2) { return Recargo100; }
            return tipo == Traslado ? SinRecargo : "";
        }

        /// <summary>Catalogo IdTipoCatalogo = 4: 1 Solicitado, 2 Aprobado, 3 Rechazado.</summary>
        public static string EstadoHe(EntTcActividad a, string tipo, string recargo)
        {
            if (recargo == Recargo50 || recargo == Recargo100)
            {
                switch (a.EstadoHE)
                {
                    case 1: return "Solicitado";
                    case 2: return "Aprobado";
                    case 3: return "Rechazado";
                    default: return "Sin solicitud";
                }
            }
            return tipo == Traslado ? "No aplica" : "";
        }

        /// <summary>Det_Tiempo "hh:mm:ss" a horas decimales, a 4 decimales. Formato invalido = 0.</summary>
        public static decimal Horas(string tiempo)
        {
            string[] p = (tiempo ?? "").Trim().Split(':');
            int h, m, s;
            if (p.Length != 3
                || !int.TryParse(p[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out h)
                || !int.TryParse(p[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out m)
                || !int.TryParse(p[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out s))
            {
                return 0m;
            }
            return Math.Round(h + m / 60m + s / 3600m, 4, MidpointRounding.ToEven);
        }

        public static string Categoria(EntTcFila r)
        {
            if (r.Tipo == Extra)
            {
                return r.Recargo == Recargo100 ? CatExtra100 : CatExtra50;
            }
            if (r.Tipo == Traslado)
            {
                if (r.Recargo == Recargo50) { return CatTraslado50; }
                if (r.Recargo == Recargo100) { return CatTraslado100; }
                return CatTrasladoSin;
            }
            return CatNormal;
        }

        /// <summary>
        /// Clasifica y valora cada actividad.
        ///   Valor hora = Salario / (JornadaHorasDia x DiasMes); sin jornada, 8.
        ///   Factor     = Factor50 / Factor100 segun el recargo; 1 si no hay
        ///                recargo o si el colaborador tiene AplicaHE = No.
        ///   Valor $    = Horas x Valor hora x Factor.
        /// </summary>
        public static EntTcClasificado Clasificar(EntTcDatos datos)
        {
            EntTcClasificado c = new EntTcClasificado();
            bool faltan = false;
            decimal factor50 = Parametro(datos.Parametros, "Factor50", 1.5m, ref faltan);
            decimal factor100 = Parametro(datos.Parametros, "Factor100", 2m, ref faltan);
            decimal diasMes = Parametro(datos.Parametros, "DiasMes", 30m, ref faltan);
            c.ParametrosPorDefecto = faltan;

            Dictionary<string, EntTcTarifa> porCodigo = new Dictionary<string, EntTcTarifa>(StringComparer.OrdinalIgnoreCase);
            foreach (EntTcTarifa t in datos.Tarifas)
            {
                string clave = Limpio(t.CodUsuario);
                if (clave != "" && !porCodigo.ContainsKey(clave)) { porCodigo[clave] = t; }
            }
            Dictionary<string, EntTcTarifa> porNombre = TarifasPorNombre(datos.Actividades, porCodigo);

            foreach (EntTcActividad a in datos.Actividades)
            {
                string tipo = Tipo(a);
                string recargo = Recargo(a, tipo);
                string responsable = NombreResponsable(a);

                EntTcTarifa tarifa;
                porNombre.TryGetValue(responsable, out tarifa);

                decimal valorHora = ValorHora(tarifa, diasMes);
                bool aplicaHe = tarifa == null || tarifa.AplicaHE != false;
                decimal factor = !aplicaHe ? 1m
                               : recargo == Recargo50 ? factor50
                               : recargo == Recargo100 ? factor100
                               : 1m;
                decimal horas = Horas(a.Tiempo);

                c.Filas.Add(new EntTcFila
                {
                    IdDetalle = a.IdDetalle,
                    FechaInicio = a.FechaInicio,
                    FechaFin = a.FechaFin,
                    Anio = a.FechaInicio.Year,
                    Mes = a.FechaInicio.Month,
                    Periodo = NombresMes[a.FechaInicio.Month - 1] + "-"
                              + a.FechaInicio.Year.ToString(CultureInfo.InvariantCulture),
                    Responsable = responsable,
                    Empresa = OPorDefecto(a.Empresa, "(Sin empresa)"),
                    Contrato = OPorDefecto(a.Contrato, "(Sin contrato)"),
                    Ticket = Limpio(a.Ticket),
                    OrdenServicio = Limpio(a.OrdenServicio),
                    Tarea = Limpio(a.Tarea),
                    Estado = Limpio(a.Estado),
                    Tipo = tipo,
                    Recargo = recargo,
                    EstadoHE = EstadoHe(a, tipo, recargo),
                    Tiempo = Limpio(a.Tiempo),
                    Horas = horas,
                    Factor = factor,
                    ValorHora = valorHora,
                    Valor = horas * valorHora * factor,
                    SinSalario = valorHora == 0m
                });
            }
            return c;
        }

        /// <summary>
        /// Desviacion deliberada respecto al Excel (ver el plan): el script toma
        /// el Cod_Usuario de la PRIMERA actividad de cada nombre, asi que una
        /// persona con dos cuentas se valora segun el orden de las fechas. Aqui,
        /// entre todos los codigos de ese nombre, gana el primero (alfabetico)
        /// con salario; si ninguno tiene, el primero con tarifa.
        /// </summary>
        private static Dictionary<string, EntTcTarifa> TarifasPorNombre(
            IEnumerable<EntTcActividad> actividades, Dictionary<string, EntTcTarifa> porCodigo)
        {
            Dictionary<string, EntTcTarifa> resultado = new Dictionary<string, EntTcTarifa>();
            foreach (IGrouping<string, EntTcActividad> g in actividades.GroupBy(NombreResponsable))
            {
                List<EntTcTarifa> candidatas = g
                    .Select(a => Limpio(a.CodResponsable))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(cod => cod, StringComparer.Ordinal)
                    .Select(cod =>
                    {
                        EntTcTarifa t;
                        return porCodigo.TryGetValue(cod, out t) ? t : null;
                    })
                    .Where(t => t != null)
                    .ToList();

                resultado[g.Key] = candidatas.FirstOrDefault(t => t.Salario.HasValue && t.Salario.Value > 0m)
                                   ?? candidatas.FirstOrDefault();
            }
            return resultado;
        }

        private static decimal ValorHora(EntTcTarifa t, decimal diasMes)
        {
            if (t == null || !t.Salario.HasValue || t.Salario.Value <= 0m) { return 0m; }
            int jornada = t.JornadaHorasDia.HasValue && t.JornadaHorasDia.Value > 0 ? t.JornadaHorasDia.Value : 8;
            return t.Salario.Value / (jornada * diasMes);
        }

        private static decimal Parametro(Dictionary<string, decimal> p, string clave, decimal porDefecto, ref bool faltan)
        {
            decimal v;
            if (p != null && p.TryGetValue(clave, out v) && v > 0m) { return v; }
            faltan = true;
            return porDefecto;
        }

        private static string NombreResponsable(EntTcActividad a)
        {
            string nombre = Limpio(a.NomResponsable);
            return nombre != "" ? nombre : "Cod " + Limpio(a.CodResponsable);
        }

        private static string Limpio(string s)
        {
            return (s ?? "").Trim();
        }

        private static string OPorDefecto(string s, string porDefecto)
        {
            string v = Limpio(s);
            return v == "" ? porDefecto : v;
        }
    }
}
