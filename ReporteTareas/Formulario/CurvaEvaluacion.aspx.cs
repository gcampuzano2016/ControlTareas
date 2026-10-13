using System;

namespace ReporteTareas.Formulario
{
    /// <summary>
    /// Completar o editar una evaluacion desde Talento Humano.
    /// Todo el contenido lo pinta el JS contra AdministrarCurvaAprendizaje.ashx;
    /// aqui solo se comprueba sesion y perfil (CurvaAcceso).
    /// </summary>
    public partial class CurvaEvaluacion : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            CurvaAcceso.Verificar(this);
        }
    }
}
