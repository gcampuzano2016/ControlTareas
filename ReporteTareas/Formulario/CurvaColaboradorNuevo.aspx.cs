using System;

namespace ReporteTareas.Formulario
{
    /// <summary>
    /// Alta de colaborador: abre su proceso y las evaluaciones de los 30 dias.
    /// Todo el contenido lo pinta el JS contra AdministrarCurvaAprendizaje.ashx;
    /// aqui solo se comprueba sesion y perfil (CurvaAcceso).
    /// </summary>
    public partial class CurvaColaboradorNuevo : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            CurvaAcceso.Verificar(this);
        }
    }
}
