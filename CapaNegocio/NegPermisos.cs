using CapaEntidad;
using System;

namespace CapaNegocio
{
    /// <summary>
    /// La regla de permisos del portal. Clase pura en este archivo: no toca
    /// base, sesion ni HttpContext. El unico archivo que llama a CapaDato es
    /// NegPermisos.Datos.cs.
    ///
    /// - Entrar a una pantalla: perfil 18, o el Href esta en sus permisos
    ///   (PerfilMenu de su perfil mas R_UsuarioMenu).
    /// - Hacer una accion: perfil 18, o puede entrar a la pantalla de la accion
    ///   Y tiene la accion (RTA_PerfilAccion mas RTA_UsuarioAccion).
    /// - Sin permisos cargados, se niega.
    ///
    /// El 18 es la unica regla fija del codigo, a proposito: es el respaldo para
    /// que nadie quede sin poder administrar permisos.
    /// </summary>
    public static partial class NegPermisos
    {
        public const int PerfilSuperAdmin = 18;

        /// <summary>Cuanto vale la copia de la sesion antes de volver a la base.</summary>
        public static readonly TimeSpan Vigencia = TimeSpan.FromMinutes(2);

        public static bool PuedeEntrar(int? idPerfil, EntPermisos permisos, string href)
        {
            if (idPerfil == PerfilSuperAdmin) { return true; }
            string h = (href ?? "").Trim();
            return h != "" && permisos != null && permisos.Pantallas.Contains(h);
        }

        public static bool PuedeHacer(int? idPerfil, EntPermisos permisos, string clave)
        {
            if (idPerfil == PerfilSuperAdmin) { return true; }
            string c = (clave ?? "").Trim();
            if (c == "" || permisos == null) { return false; }
            string pantalla;
            return permisos.Acciones.TryGetValue(c, out pantalla) && PuedeEntrar(idPerfil, permisos, pantalla);
        }

        /// <summary>
        /// Si hay que volver a la base: no hay copia, la copia no tiene permisos,
        /// cambio la identidad, vencio, o el reloj retrocedio.
        /// </summary>
        public static bool DebeRecargar(EntPermisosCache cache, int? idPerfil, string codUsuario, DateTime ahora)
        {
            if (cache == null || cache.Permisos == null) { return true; }
            if (cache.IdPerfil != idPerfil) { return true; }
            if (!string.Equals((cache.CodUsuario ?? "").Trim(), (codUsuario ?? "").Trim(), StringComparison.OrdinalIgnoreCase)) { return true; }
            if (ahora < cache.Cargado) { return true; }
            return ahora - cache.Cargado >= Vigencia;
        }
    }
}
