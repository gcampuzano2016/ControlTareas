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
    ///
    /// Quien puede ver perfiles ajenos NO se decide aca: se decide afuera, con el
    /// permiso de la pantalla PerfilesPersonal.aspx en el portal, y llega como
    /// veAjenos. Esta clase solo aplica esa respuesta.
    /// </summary>
    public class NegPerfilAcceso
    {
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
        public static EntPerfilObjetivo Objetivo(string codSesion, bool veAjenos, string codPedido)
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

            if (veAjenos)
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
