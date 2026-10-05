using System;

namespace ReporteTareas.Formulario
{
    /// <summary>
    /// Evaluacion remota por enlace: la UNICA pagina anonima del modulo Curva de
    /// Aprendizaje. No comprueba sesion a proposito y no carga ningun dato en el
    /// servidor (ni ViewState): el formulario llega por CurvaEvaluacionRemota.ashx,
    /// que solo entrega lo que la pagina publica puede ver.
    /// </summary>
    public partial class CurvaEvaluacionRemota : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
        }
    }
}
