using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CapaEntidad;
using SeguridadAppHelper;
using ReporteTareas.Controles;


namespace ReporteTareas.Formulario
{
    public partial class Principal : System.Web.UI.Page
    {
        #region Variables
        protected NegCRedireccionamientoLogin GenLogin = new NegCRedireccionamientoLogin();
        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            List<EntKpi> listaDetTareas = new List<EntKpi>();
            /*string CodUnico = "";
            CodUnico = Session["Cod_Usuario"].ToString();
            listaDetTareas = NegTareas.ListaDetTareas(CodUnico, fchIni, fchFin, 1); // negDetalleFacturaTemp.listaDetalleTempPresenta(Session["CodUnico"].ToString());
            */
            GenLogin.RedireccionarALogin(this);
            if (Session["UserLogin"] != null)
            {
                if (!IsPostBack)
                {
                    try
                    {
                        List<EntTareas> listaTareas = new List<EntTareas>();
                        string CodUnico = "";
                        CodUnico = Session["Cod_Usuario"].ToString();
                        SeguridadHelper seguridad = new SeguridadHelper();
                        //txtUsuario.Text = seguridad.Encripta(CodUnico.ToString());

                    }
                    catch (Exception ex)
                    { }
                }
            }

        }

    }
}