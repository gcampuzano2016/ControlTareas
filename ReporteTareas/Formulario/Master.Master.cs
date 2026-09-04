using CapaEntidad;
using CapaNegocio;
using Newtonsoft.Json;
using SeguridadAppHelper;
using System;
using System.Collections.Generic;
using System.Data;


namespace ReporteTareas.Formulario
{
    public partial class Master : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            litVersion.Text = VersionDesplegada();

            int Idperfil = 0;
            Idperfil = Convert.ToInt32(Session["Id_Perfil"]);

            string CodUnico = "";
            CodUnico = Session["Cod_Usuario"].ToString();
            SeguridadHelper seguridad = new SeguridadHelper();
            txtUsuario.Text = seguridad.Encripta(CodUnico.ToString());

            List<EntMenuDos> menuDos = new List<EntMenuDos>();
            menuDos = NegMenuDos.Sp_RTA_ConsultarMenuPerfilUsuario(Idperfil, CodUnico);
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

        /// <summary>
        /// Que version esta corriendo, para el pie de pagina.
        ///
        /// Existe por una confusion concreta de septiembre de 2026: se diagnostico
        /// un problema contra el codigo del repositorio dando por hecho que el
        /// servidor tenia lo mismo, y no lo tenia. Se perdieron dos dias. Con esto,
        /// saber que hay desplegado es mirar el pie de cualquier pantalla.
        ///
        /// La fecha sale del archivo del ensamblado y no de una constante que haya
        /// que acordarse de subir: una constante desactualizada miente, y aqui una
        /// mentira es peor que no tener el dato. Copiar con robocopy conserva la
        /// fecha original, asi que lo que se lee es el momento en que se compilo, no
        /// el de la copia.
        ///
        /// Se lee bin\ReporteTareas.dll y no Assembly.Location a proposito: ASP.NET
        /// hace copia sombra de los ensamblados a Temporary ASP.NET Files, y esa
        /// copia tiene la fecha en que se reciclo la aplicacion, no la de la
        /// compilacion.
        ///
        /// AssemblyVersion no aporta: esta fija en 1.0.0.0 desde siempre.
        /// </summary>
        private string VersionDesplegada()
        {
            try
            {
                string ruta = Server.MapPath("~/bin/ReporteTareas.dll");
                if (!System.IO.File.Exists(ruta)) { return ""; }

                DateTime compilado = System.IO.File.GetLastWriteTime(ruta);
                return "Version del " + compilado.ToString("dd/MM/yyyy HH:mm");
            }
            catch
            {
                /* Un pie de pagina no puede tumbar una pantalla. */
                return "";
            }
        }
    }
}