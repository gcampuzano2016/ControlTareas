using CapaEntidad;
using CapaNegocio;
using System.Web;

namespace ReporteTareas.clases
{
    /// <summary>
    /// De donde sale la identidad en el modulo de perfil.
    ///
    /// Esta clase NO decide nada: extrae de HttpContext, le pregunta a
    /// Permisos si la sesion ve perfiles ajenos y se lo pasa a NegPerfilAcceso,
    /// que es donde vive la regla y donde estan las pruebas. Si alguna vez
    /// aparece un "if" con un numero de perfil en este archivo, esta en el
    /// lugar equivocado.
    ///
    /// Existe porque el modulo habla por tres transportes distintos -JSON,
    /// multipart y query string- y el criterio tiene que ser uno solo. Cada
    /// transporte aporta su extractor; la decision es compartida.
    /// </summary>
    internal static class PerfilIdentidad
    {
        /// <summary>
        /// QUIEN esta actuando. Sale de la sesion y nunca de otro lado: no hay
        /// parametro, no hay rama, el cliente no puede influir. Es lo que hace
        /// que Usu_Modificacion signifique algo.
        /// </summary>
        internal static string Autor(HttpContext context)
        {
            if (context.Session != null && context.Session["Cod_Usuario"] != null)
            {
                return context.Session["Cod_Usuario"].ToString().Trim();
            }
            return "";
        }

        /// <summary>DE QUIEN es el perfil sobre el que se actua.</summary>
        internal static EntPerfilObjetivo Objetivo(HttpContext context, string codPedido)
        {
            return NegPerfilAcceso.Objetivo(Autor(context), VeAjenos(context), codPedido);
        }

        /// <summary>
        /// Si la sesion puede ver y corregir perfiles ajenos: quien tiene la
        /// pantalla PerfilesPersonal.aspx (por perfil o como adicional).
        ///
        /// Hace falta aparte porque hay acciones -listar a todo el personal- que
        /// no son "sobre el perfil de alguien" y por lo tanto no pasan por Objetivo.
        /// </summary>
        internal static bool VeAjenos(HttpContext context)
        {
            return Permisos.PuedeEntrar(context, Pantallas.PerfilesPersonal);
        }

        /// <summary>El codigo pedido en el payload JSON, o cadena vacia.</summary>
        internal static string CodigoPedidoJson(dynamic campos)
        {
            var d = campos as System.Collections.Generic.IDictionary<string, object>;
            if (d == null) { return ""; }

            object valor;
            if (!d.TryGetValue("codUsuario", out valor) || valor == null) { return ""; }

            return valor.ToString().Trim();
        }

        /// <summary>El codigo pedido en el formulario multipart, o cadena vacia.</summary>
        internal static string CodigoPedidoFormulario(HttpContext context)
        {
            return (context.Request.Form.Get("codUsuario") ?? "").Trim();
        }

        /// <summary>El codigo pedido en la query string, o cadena vacia.</summary>
        internal static string CodigoPedidoQuery(HttpContext context)
        {
            return (context.Request.QueryString["u"] ?? "").Trim();
        }
    }
}
