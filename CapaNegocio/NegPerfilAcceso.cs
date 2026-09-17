using CapaEntidad;
using System;

namespace CapaNegocio
{
    /// <summary>
    /// Quien puede actuar sobre el perfil de quien.
    ///
    /// Esta clase es la unica barrera real del modulo. El menu solo controla que
    /// la pantalla se VEA; los handlers son alcanzables por HTTP directo por
    /// cualquiera con sesion iniciada. Mismo razonamiento que
    /// AdministrarHorasExtras.ashx.
    ///
    /// Vive en CapaNegocio y no en el proyecto web a proposito: CapaPruebas solo
    /// referencia CapaEntidad y CapaNegocio, asi que una regla escrita dentro del
    /// handler seria una regla sin pruebas. Es el mismo motivo por el que la
    /// fase 3b saco su guarda a Fn_RTA_EsSubordinado en vez de escribirla a mano
    /// dentro del procedimiento.
    ///
    /// No conoce HttpContext. Quien la llama extrae los tres datos de donde sea
    /// que vengan -sesion, JSON, formulario multipart, query string- y los pasa.
    /// </summary>
    public class NegPerfilAcceso
    {
        /// <summary>
        /// Talento Humano (14) y Super Admin (18): los mismos dos perfiles a los
        /// que el menu le muestra las pantallas de nomina, y los mismos que
        /// AdministrarHorasExtras.ashx ya autoriza.
        /// </summary>
        public static readonly int[] PerfilesRRHH = { 14, 18 };

        /// <summary>
        /// Si ese perfil de sesion es uno de los dos que pueden ver y editar
        /// perfiles ajenos. Cualquier cosa que no sea exactamente 14 o 18
        /// -vacio, nulo, texto, un numero cualquiera- es que no.
        /// </summary>
        public static bool EsRRHH(string idPerfilSesion)
        {
            int idPerfil;
            if (!int.TryParse((idPerfilSesion ?? "").Trim(), out idPerfil)) { return false; }

            return Array.IndexOf(PerfilesRRHH, idPerfil) >= 0;
        }

        /// <summary>
        /// Sobre que perfil se esta actuando.
        ///
        /// El orden de las ramas importa: primero se descarta la sesion sin
        /// identidad, porque sin ella no hay ni perfil propio que devolver.
        /// Despues el caso normal -no se pidio ninguno-, que es el de
        /// MiPerfil.aspx y el 99% del trafico. Recien al final se evalua el
        /// permiso, que es el caso raro.
        ///
        /// Pedir el propio codigo NO es pedir uno ajeno: se compara recortando,
        /// igual que compara todo el resto del modulo.
        /// </summary>
        public static EntPerfilObjetivo Objetivo(string codSesion, string idPerfilSesion, string codPedido)
        {
            string propio = (codSesion ?? "").Trim();
            string pedido = (codPedido ?? "").Trim();

            if (propio == "")
            {
                return Rechazo("No se pudo identificar al usuario de la sesión.");
            }

            if (pedido == "" || string.Equals(pedido, propio, StringComparison.OrdinalIgnoreCase))
            {
                return Permitido(propio);
            }

            if (EsRRHH(idPerfilSesion))
            {
                return Permitido(pedido);
            }

            return Rechazo("No tiene permiso para ver ni editar el perfil de otra persona.");
        }

        private static EntPerfilObjetivo Permitido(string codUsuario)
        {
            return new EntPerfilObjetivo
            {
                Permitido  = true,
                CodUsuario = codUsuario,
                Mensaje    = ""
            };
        }

        /// <summary>
        /// Un rechazo devuelve el codigo VACIO, no el propio. Si devolviera el
        /// propio, un sitio que se olvidara de mirar Permitido escribiria sobre el
        /// perfil de quien llama en vez de fallar, y eso es un guardado silencioso
        /// en la fila equivocada: peor que un error.
        /// </summary>
        private static EntPerfilObjetivo Rechazo(string mensaje)
        {
            return new EntPerfilObjetivo
            {
                Permitido  = false,
                CodUsuario = "",
                Mensaje    = mensaje
            };
        }
    }
}
