using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CapaEntidad;
using CapaNegocio;
using Newtonsoft.Json;
using System.Data;


namespace ReporteTareas.Formulario
{
    public partial class Master : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            int Idperfil = 0;
            Idperfil = Convert.ToInt32(Session["Id_Perfil"]);
            List<EntMenuDos> menuDos = new List<EntMenuDos>();
            menuDos = NegMenuDos.Sp_RTA_ConsultarMenuPerfilUsuario(Idperfil);
            var json = JsonConvert.SerializeObject(menuDos);
            DataTable dtPadres = new DataTable();
            DataTable dtPrincipal = (DataTable)JsonConvert.DeserializeObject(json, (typeof(DataTable)));
            if (dtPrincipal.Rows.Count > 0)
            {
                DataView view = dtPrincipal.DefaultView;
                view.RowFilter = "Id_MenuPadre=0";
                dtPadres = view.ToTable("UniqueLastNames", true, "id_Menu", "Titulo", "Class_Icon", "Href");

                string plan1 = "";
                plan1 = "<ul class='nav' id='side-menu'>";

                foreach (DataRow data in dtPadres.Rows)
                {
                    plan1 = plan1 + "<li>";
                    plan1 = plan1 + "<a href = '#'><i class='" + data["Class_Icon"].ToString() + "'></i>" + "   " + data["Titulo"].ToString() + "<span class='fa arrow'></span></a>";
                    DataTable dtChild = new DataTable();
                    DataView view2 = dtPrincipal.DefaultView;
                    view2.RowFilter = "Id_MenuPadre=" + data["id_Menu"] + "";
                    dtChild = view.ToTable("UniqueLastNames", true, "id_Menu", "Titulo", "Class_Icon", "Href");
                    plan1 = plan1 + "<ul class='nav nav-second-level'>";
                    foreach (DataRow detalle in dtChild.Rows)
                    {
                        plan1 = plan1 + "<li>";
                        plan1 = plan1 + "<a href ='" + detalle["Href"].ToString() + "'> <i class='" + detalle["Class_Icon"].ToString() + "'></i>" + "   " + detalle["Titulo"].ToString() + "</a>";
                        plan1 = plan1 + "</li>";
                    }
                    plan1 = plan1 + "</ul>";
                    plan1 = plan1 + "</li>";
                }
                plan1 = plan1 + "</ul>";
                lblCargarMenu.Text = plan1;
            }
        }
    }
}