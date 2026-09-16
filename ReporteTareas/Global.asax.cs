using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using System.Web.Http;

namespace ReporteTareas
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // Código que se ejecuta al iniciar la aplicación
            VerificarConfiguracion();
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }

        /// <summary>
        /// Cadenas de conexión que la capa de datos busca, en connections.config.
        /// Si cambia un nombre acá, cambia también allá y en DESPLIEGUE.md.
        /// </summary>
        private static readonly string[] CadenasRequeridas = { "ReporTarea", "ArandaDb", "Sap" };

        /// <summary>
        /// Claves de appSettings que FileEncryptionService necesita, en
        /// appsettings.config.
        /// </summary>
        private static readonly string[] ClavesRequeridas = { "EncryptionPassword", "EncryptionSalt" };

        /// <summary>
        /// Falla al arrancar, nombrando lo que falta, si la configuración está
        /// incompleta.
        ///
        /// Existe porque el modo de fallar por defecto es pésimo. Los secretos
        /// viven en connections.config y appsettings.config, que no viajan en la
        /// publicación y se crean a mano en cada servidor. Cuando falta uno,
        /// ConfigurationManager devuelve null, el primer uso revienta lejos de la
        /// causa, y el catch de la capa de datos se lo traga y devuelve null. El
        /// login lo interpreta como que el usuario no existe y manda a buscar el
        /// problema a Active Directory. Ya pasó: agosto de 2026.
        /// </summary>
        private static void VerificarConfiguracion()
        {
            List<string> faltantes = new List<string>();

            foreach (string nombre in CadenasRequeridas)
            {
                ConnectionStringSettings cadena = ConfigurationManager.ConnectionStrings[nombre];
                if (cadena == null || string.IsNullOrWhiteSpace(cadena.ConnectionString))
                {
                    faltantes.Add("cadena de conexión '" + nombre + "' (connections.config)");
                }
            }

            foreach (string nombre in ClavesRequeridas)
            {
                if (string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings[nombre]))
                {
                    faltantes.Add("clave '" + nombre + "' (appsettings.config)");
                }
            }

            if (faltantes.Count > 0)
            {
                throw new ConfigurationErrorsException(
                    "Falta configuración obligatoria: " + string.Join("; ", faltantes) + ". " +
                    "Esos archivos van en la misma carpeta que el Web.config del sitio. " +
                    "No viajan en la publicación a propósito, porque tienen los secretos: " +
                    "se crean una vez en cada servidor a partir de su .ejemplo. " +
                    "Ver DESPLIEGUE.md, secciones 1 y 2.");
            }
        }
    }
}
