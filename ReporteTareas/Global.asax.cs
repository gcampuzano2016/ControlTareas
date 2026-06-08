using System;
using System.Web;
using System.Web.Http;

namespace ReporteTareas
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // Código que se ejecuta al iniciar la aplicación
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}