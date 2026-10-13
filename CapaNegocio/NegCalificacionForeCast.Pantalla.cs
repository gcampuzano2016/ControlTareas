using CapaDato;
using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CapaNegocio
{
    public static partial class NegCalificacionForeCast
    {
        private static readonly int[] PuntosPresupuesto = { 0, 5, 12, 20 };
        private static readonly int[] PuntosAutoridad = { 0, 5, 12, 20 };
        private static readonly int[] PuntosNecesidad = { 0, 6, 14, 20 };

        public static EntPantallaCalificacion Cargar(string idUsuario, EntFiltroCalificacion filtro)
        {
            // '' significa "sin filtro" para Sp_RTAListaForeCast: nunca se puede
            // llamar al Dao con una sesion vacia o en blanco.
            if (string.IsNullOrWhiteSpace(idUsuario))
            {
                throw new ArgumentException("Sesión no válida. Vuelva a iniciar sesión.");
            }

            EntDatosCalificacion datos = DaoCalificacionForeCast.Cargar(idUsuario);
            return Armar(datos, filtro, DateTime.Today);
        }

        /// <summary>
        /// Evalua todo, arma los combos con el conjunto SIN filtrar (para que un
        /// filtro no se quede sin la opcion de volver atras), filtra, ordena y
        /// calcula los indicadores sobre lo filtrado.
        /// </summary>
        public static EntPantallaCalificacion Armar(EntDatosCalificacion datos, EntFiltroCalificacion filtro, DateTime hoy)
        {
            if (datos == null) { datos = new EntDatosCalificacion(); }
            if (filtro == null) { filtro = new EntFiltroCalificacion(); }

            List<EntCalificacionForeCast> todas = datos.Oportunidades ?? new List<EntCalificacionForeCast>();
            foreach (EntCalificacionForeCast o in todas) { Evaluar(o, datos.Tasas, hoy); }

            EntPantallaCalificacion p = new EntPantallaCalificacion
            {
                Gerentes = Distintos(todas.Select(o => o.GerentedeCuenta)),
                Sucursales = Distintos(todas.Select(o => o.Sucursal)),
                Etapas = Distintos(todas.Select(o => o.Prioridad)),
                Marcas = Distintos(todas.Select(o => o.Marca))
            };

            p.Oportunidades = todas
                .Where(o => Pasa(o, filtro))
                .OrderByDescending(o => o.Puntaje)
                .ThenByDescending(o => o.MontoPonderado)
                .ThenBy(o => o.IdForeCast)
                .ToList();

            p.Total = p.Oportunidades.Count;
            p.MontoTotal = p.Oportunidades.Sum(o => o.PVPEstimado ?? 0m);
            p.MontoPonderado = p.Oportunidades.Sum(o => o.MontoPonderado);
            p.Calientes = p.Oportunidades.Count(o => o.Estado == Caliente);
            p.Tibias = p.Oportunidades.Count(o => o.Estado == Tibia);
            p.Frias = p.Oportunidades.Count(o => o.Estado == Fria);
            p.SinCalificar = p.Oportunidades.Count(o => o.SinCalificar);
            p.PorcentajeSinCalificar = p.Total == 0
                ? 0m
                : Math.Round(100m * p.SinCalificar / p.Total, 1, MidpointRounding.AwayFromZero);

            p.Top5 = p.Oportunidades
                .Where(o => o.Estado == Caliente || o.Estado == Tibia)
                .OrderByDescending(o => o.MontoPonderado)
                .ThenBy(o => o.FechaCierre.HasValue ? 0 : 1)
                .ThenBy(o => o.FechaCierre)
                .Take(5)
                .Select(o => o.IdForeCast)
                .ToList();

            p.EnRiesgo = p.Oportunidades
                .Where(o => o.Estado == Fria
                         && (TieneAlerta(o, AlertaSinActividad)
                          || TieneAlerta(o, AlertaCierreVencido)
                          || TieneAlerta(o, AlertaInconsistente)))
                .Select(o => o.IdForeCast)
                .ToList();

            return p;
        }

        private static bool Pasa(EntCalificacionForeCast o, EntFiltroCalificacion f)
        {
            if (!Vacio(f.GerentedeCuenta) && !Igual(o.GerentedeCuenta, f.GerentedeCuenta)) { return false; }
            if (!Vacio(f.Sucursal) && !Igual(o.Sucursal, f.Sucursal)) { return false; }
            if (!Vacio(f.Prioridad) && !Igual(o.Prioridad, f.Prioridad)) { return false; }
            if (!Vacio(f.Marca) && !Igual(o.Marca, f.Marca)) { return false; }
            if (!Vacio(f.Estado))
            {
                if (Igual(f.Estado, SinCalificarTexto)) { return o.SinCalificar; }
                return Igual(o.Estado, f.Estado);
            }
            return true;
        }

        private static List<string> Distintos(IEnumerable<string> valores)
        {
            return valores
                .Where(v => !Vacio(v))
                .Select(v => v.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(v => v, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public static string ValidarRespuesta(EntCalificacionRespuesta r)
        {
            if (r == null) { return "No llegó la calificación."; }
            if (r.IdForeCast <= 0) { return "No se indicó la oportunidad."; }
            if (!PuntosPresupuesto.Contains(r.Presupuesto)) { return "La respuesta de Presupuesto no es válida."; }
            if (!PuntosAutoridad.Contains(r.Autoridad)) { return "La respuesta de Autoridad no es válida."; }
            if (!PuntosNecesidad.Contains(r.Necesidad)) { return "La respuesta de Necesidad no es válida."; }
            if ((r.DecisorCargo ?? "").Trim().Length > 150) { return "El decisor y su cargo no pueden pasar de 150 caracteres."; }
            if ((r.AccionComprometida ?? "").Trim().Length > 300) { return "La acción comprometida no puede pasar de 300 caracteres."; }
            return "";
        }

        /// <summary>
        /// El IdForeCast llega del navegador: sin esta comprobacion, quien lo
        /// cambie a mano podria calificar una oportunidad fuera de su visibilidad.
        /// </summary>
        public static string ValidarGuardado(EntCalificacionRespuesta r, IEnumerable<long> idsVisibles)
        {
            string error = ValidarRespuesta(r);
            if (error != "") { return error; }
            if (idsVisibles == null || !idsVisibles.Contains(r.IdForeCast)) { return "No tiene acceso a esa oportunidad."; }
            return "";
        }

        public static EntRespuesta Guardar(EntCalificacionRespuesta r, string idUsuario)
        {
            // Mismo motivo que en Cargar: '' es "sin filtro" para el Dao, nunca una
            // sesion valida.
            if (string.IsNullOrWhiteSpace(idUsuario))
            {
                return new EntRespuesta { estado = "0", mensaje = "Sesión no válida. Vuelva a iniciar sesión.", tipoMensaje = "warning" };
            }

            EntDatosCalificacion visibles = DaoCalificacionForeCast.Cargar(idUsuario);
            string error = ValidarGuardado(r, visibles.Oportunidades.Select(o => o.IdForeCast));
            if (error != "")
            {
                return new EntRespuesta { estado = "0", mensaje = error, tipoMensaje = "warning" };
            }

            r.DecisorCargo = Vacio(r.DecisorCargo) ? null : r.DecisorCargo.Trim();
            r.AccionComprometida = Vacio(r.AccionComprometida) ? null : r.AccionComprometida.Trim();
            r.Usuario = idUsuario;
            return DaoCalificacionForeCast.Guardar(r);
        }
    }
}
