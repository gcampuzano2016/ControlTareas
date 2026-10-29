using CapaEntidad;
using CapaNegocio;
using System;
using System.Globalization;
using System.Web;

namespace ReporteTareas.clases
{
    /// <summary>
    /// Lo unico que llaman pantallas y handlers para decidir acceso. No decide
    /// nada: saca la identidad de la sesion, mantiene la copia de permisos y le
    /// pregunta a NegPermisos, que es donde estan la regla y las pruebas.
    /// Si aparece un numero de perfil en este archivo, esta en el lugar
    /// equivocado.
    ///
    /// Orden de las comprobaciones (Review Focus 1): primero la sesion
    /// completa. Login.aspx deja UserLogin y Cod_Usuario en "" sin tocar
    /// Id_Perfil, asi que un 18 colgado de una sesion cerrada no debe pasar.
    ///
    /// Ante cualquier error al cargar: log y se niega.
    /// </summary>
    internal static class Permisos
    {
        private const string ClaveSesion = "Permisos.Cache";

        internal static bool Autenticado(HttpContext context)
        {
            return context != null && context.Session != null
                && !string.IsNullOrWhiteSpace(Convert.ToString(context.Session["UserLogin"], CultureInfo.InvariantCulture))
                && !string.IsNullOrWhiteSpace(Convert.ToString(context.Session["Cod_Usuario"], CultureInfo.InvariantCulture));
        }

        internal static bool PuedeEntrar(HttpContext context, string href)
        {
            if (!Autenticado(context)) { return false; }
            int? perfil = Perfil(context);
            if (perfil == NegPermisos.PerfilSuperAdmin) { return true; }
            return NegPermisos.PuedeEntrar(perfil, Obtener(context, perfil), href);
        }

        internal static bool PuedeHacer(HttpContext context, string clave)
        {
            if (!Autenticado(context)) { return false; }
            int? perfil = Perfil(context);
            if (perfil == NegPermisos.PerfilSuperAdmin) { return true; }
            return NegPermisos.PuedeHacer(perfil, Obtener(context, perfil), clave);
        }

        private static int? Perfil(HttpContext context)
        {
            int id;
            return int.TryParse(Convert.ToString(context.Session["Id_Perfil"], CultureInfo.InvariantCulture), out id) ? id : (int?)null;
        }

        private static EntPermisos Obtener(HttpContext context, int? perfil)
        {
            string cod = Convert.ToString(context.Session["Cod_Usuario"], CultureInfo.InvariantCulture);
            try
            {
                EntPermisosCache cache = context.Session[ClaveSesion] as EntPermisosCache;
                DateTime ahora = DateTime.Now;
                if (NegPermisos.DebeRecargar(cache, perfil, cod, ahora))
                {
                    cache = new EntPermisosCache
                    {
                        IdPerfil = perfil,
                        CodUsuario = cod,
                        Cargado = ahora,
                        Permisos = NegPermisos.Cargar(perfil, cod)
                    };
                    context.Session[ClaveSesion] = cache;
                }
                return cache.Permisos;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Permisos: no se pudieron cargar. " + ex);
                context.Session.Remove(ClaveSesion);
                return null;
            }
        }
    }

    /// <summary>Href de las pantallas protegidas, tal como estan en MenuDos.</summary>
    internal static class Pantallas
    {
        internal const string TableroCostos = "TableroCostos.aspx";
        internal const string HorasExtras = "HorasExtras.aspx";
        internal const string ParametrizacionHorasExtras = "ParametrizacionHorasExtras.aspx";
        /// <summary>Da acceso a TODO el modulo Curva (handler y sus 6 pantallas).</summary>
        internal const string CurvaColaboradores = "CurvaColaboradores.aspx";
        internal const string PerfilesPersonal = "PerfilesPersonal.aspx";
        internal const string ParametrizacionPerfiles = "ParametrizacionPerfiles.aspx";
        internal const string ParametrizacionUsuarios = "ParametrizacionUsuarios.aspx";
        internal const string ParametrizacionMenuPerfil = "ParametrizacionMenuPerfil.aspx";
        internal const string ParametrizacionMenuUsuario = "ParametrizacionMenuUsuario.aspx";
        /// <summary>Lista completa de empleados, su descarga y su mantenimiento (ObtenerListaTareas.ashx).</summary>
        internal const string ListaEmpleados = "ListaEmpleados.aspx";
        /// <summary>«Datos Empleado»: solo la ficha propia.</summary>
        internal const string RRHHEmpleados = "RRHHEmpleados.aspx";
        /// <summary>Equipo y agenda: la abre cualquier usuario interno; lo que ve lo decide AdministrarAgenda.</summary>
        internal const string Agenda = "Agenda.aspx";
        internal const string ParametrizacionAreasAgenda = "ParametrizacionAreasAgenda.aspx";
        /// <summary>Procesos entre areas: ver, iniciar y gestionar procesos (AdministrarAgenda).</summary>
        internal const string Procesos = "Procesos.aspx";
        /// <summary>Plantillas de procesos: alta y edicion (menu: perfiles 10 y 18).</summary>
        internal const string ParametrizacionPlantillasProcesos = "ParametrizacionPlantillasProcesos.aspx";
        /// <summary>Bonos: el jefe propone el bono de su gente.</summary>
        internal const string BonosProponer = "BonosProponer.aspx";
        /// <summary>Bonos: Talento Humano revisa, registra, abre y cierra el mes.</summary>
        internal const string BonosRevisar = "BonosRevisar.aspx";
        internal const string BonosCatalogo = "BonosCatalogo.aspx";
        internal const string BonosReporte = "BonosReporte.aspx";
        /// <summary>Saldos de vacaciones de todo el personal (GTH): solo lectura.</summary>
        internal const string VacacionesSaldos = "VacacionesSaldos.aspx";
        /// <summary>Mis vacaciones: cada uno ve solo su saldo (AdministrarMisVacaciones lo saca de la sesion).</summary>
        internal const string MisVacaciones = "MisVacaciones.aspx";
        /// <summary>Nomina y provision de vacaciones: muestra el sueldo de todo el personal.</summary>
        internal const string VacacionesNomina = "VacacionesNomina.aspx";
        /// <summary>Liquidacion de vacaciones para salidas: muestra sueldos.</summary>
        internal const string VacacionesLiquidacion = "VacacionesLiquidacion.aspx";
    }

    /// <summary>Claves de RTA_Accion (docs/sql/2026-10-14-permisos-esquema.sql).</summary>
    internal static class Acciones
    {
        internal const string HorasExtrasReabrir = "HorasExtras.Reabrir";
        internal const string HorasExtrasEditarMaestro = "HorasExtras.EditarMaestro";
        internal const string UsuariosCambiarPerfil = "Usuarios.CambiarPerfil";
        internal const string AgendaVerTodos = "Agenda.VerTodos";
    }
}
