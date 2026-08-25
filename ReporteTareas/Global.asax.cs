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
            VerificarCadenasDeConexion();
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }

        /// <summary>
        /// Los nombres que la capa de datos busca en la configuración. Si cambia
        /// uno acá, cambia en connections.config y en DESPLIEGUE.md.
        /// </summary>
        private static readonly string[] CadenasRequeridas = { "ReporTarea", "ArandaDb", "Sap" };

        /// <summary>
        /// Falla al arrancar, con el nombre de lo que falta, si la configuración
        /// no trae las cadenas de conexión.
        ///
        /// Existe porque el modo de fallar por defecto es pésimo: las cadenas
        /// viven en connections.config, que no viaja en la publicación y se crea
        /// a mano en cada servidor. Cuando falta, ConfigurationManager devuelve
        /// null, el primer .ConnectionString revienta lejos de la causa, y el
        /// catch de la capa de datos se lo traga y devuelve null. El login lo
        /// interpreta como que el usuario no existe y manda a buscar el problema
        /// a Active Directory. Ya paso: agosto de 2026.
        /// </summary>
        private static void VerificarCadenasDeConexion()
        {
            List<string> faltantes = new List<string>();

            foreach (string nombre in CadenasRequeridas)
            {
                ConnectionStringSettings cadena = ConfigurationManager.ConnectionStrings[nombre];
                if (cadena == null || string.IsNullOrWhiteSpace(cadena.ConnectionString))
                {
                    faltantes.Add(nombre);
                }
            }

            if (faltantes.Count > 0)
            {
                throw new ConfigurationErrorsException(
                    "No se encontraron estas cadenas de conexión: " + string.Join(", ", faltantes) + ". " +
                    "Deben estar en connections.config, en la misma carpeta que el Web.config del sitio. " +
                    "Ese archivo no viaja en la publicación a propósito (tiene las contraseñas): " +
                    "se crea una vez en cada servidor a partir de connections.config.ejemplo. " +
                    "Ver DESPLIEGUE.md, secciones 1 y 2.");
            }
        }
    }
}
