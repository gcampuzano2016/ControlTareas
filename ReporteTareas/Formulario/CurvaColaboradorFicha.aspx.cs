using System;

namespace ReporteTareas.Formulario
{
    /// <summary>
    /// Ficha del colaborador: curva, semaforo, evaluaciones, reportes y compromisos.
    /// Todo el contenido lo pinta el JS contra AdministrarCurvaAprendizaje.ashx;
    /// aqui solo se comprueba sesion y perfil (CurvaAcceso).
    /// </summary>
    public partial class CurvaColaboradorFicha : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            CurvaAcceso.Verificar(this);
        }
    }
}
