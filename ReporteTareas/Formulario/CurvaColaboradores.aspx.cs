using System;

namespace ReporteTareas.Formulario
{
    /// <summary>
    /// Lista de colaboradores del modulo Curva de Aprendizaje.
    /// Todo el contenido lo pinta el JS contra AdministrarCurvaAprendizaje.ashx;
    /// aqui solo se comprueba sesion y perfil (CurvaAcceso).
    /// </summary>
    public partial class CurvaColaboradores : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            CurvaAcceso.Verificar(this);
        }
    }
}
