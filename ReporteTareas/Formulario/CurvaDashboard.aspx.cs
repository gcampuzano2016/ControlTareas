using System;

namespace ReporteTareas.Formulario
{
    /// <summary>
    /// Vista agregada del programa de curva de aprendizaje.
    /// Todo el contenido lo pinta el JS contra AdministrarCurvaAprendizaje.ashx;
    /// aqui solo se comprueba sesion y perfil (CurvaAcceso).
    /// </summary>
    public partial class CurvaDashboard : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            CurvaAcceso.Verificar(this);
        }
    }
}
