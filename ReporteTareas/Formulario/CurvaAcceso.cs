using ReporteTareas.Controles;
using System;
using System.Web.UI;

namespace ReporteTareas.Formulario
{
    /// <summary>
    /// Puerta de las pantallas de Curva de Aprendizaje: sesion iniciada y
    /// perfil autorizado, con la MISMA lista que usa el handler
    /// (AdministrarCurvaAprendizaje.PerfilesAutorizados). Es cortesia: la
    /// barrera de verdad es la del handler, pero sin esto quien teclea la URL
    /// ve la pantalla vacia y recien le saltan los "no tiene permisos".
    ///
    /// CurvaEvaluacionRemota.aspx NO la usa: es la unica pagina anonima.
    /// </summary>
    public static class CurvaAcceso
    {
        public static void Verificar(Page pagina)
        {
            new NegCRedireccionamientoLogin().RedireccionarALogin(pagina);
            if (!JsonJQueryNetCurvaAprendizaje.AdministrarCurvaAprendizaje.Autorizado(System.Web.HttpContext.Current))
                pagina.Response.Redirect("~/Formulario/Principal.aspx", true);
        }
    }
}
