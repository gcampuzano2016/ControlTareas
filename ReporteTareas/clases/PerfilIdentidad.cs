using CapaEntidad;
using CapaNegocio;
using System.Web;

namespace ReporteTareas.clases
{
    /// <summary>
    /// De donde sale la identidad en el modulo de perfil.
    ///
    /// Esta clase NO decide nada: extrae de HttpContext y le pregunta a
    /// NegPerfilAcceso, que es donde vive la regla y donde estan las pruebas.
    /// Si alguna vez aparece un "if" con un numero de perfil en este archivo,
    /// esta en el lugar equivocado.
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
            string idPerfil = "";
            if (context.Session != null && context.Session["Id_Perfil"] != null)
            {
                idPerfil = context.Session["Id_Perfil"].ToString();
            }

            return NegPerfilAcceso.Objetivo(Autor(context), idPerfil, codPedido);
        }

        /// <summary>
        /// Si la sesion es de Talento Humano o Super Admin.
        ///
        /// Delega en NegPerfilAcceso, igual que Objetivo: la lista de perfiles
        /// vive en un solo sitio y esta clase sigue sin decidir nada. Hace falta
        /// aparte porque hay acciones -listar a todo el personal- que no son
        /// "sobre el perfil de alguien" y por lo tanto no pasan por Objetivo.
        /// </summary>
        internal static bool EsRRHH(HttpContext context)
        {
            string idPerfil = "";
            if (context.Session != null && context.Session["Id_Perfil"] != null)
            {
                idPerfil = context.Session["Id_Perfil"].ToString();
            }

            return NegPerfilAcceso.EsRRHH(idPerfil);
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
