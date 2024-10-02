using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CapaEntidad;

namespace CapaNegocio
{
    public class NegBoolean
    {


        public static List<EntBoolean> listaEstado()
        {
            List<EntBoolean> listaBoolean = new List<EntBoolean>();
            EntBoolean objBoolean = null;

            objBoolean = new EntBoolean();
            objBoolean.IdBoolean = 0;
            objBoolean.NomBoolean = "--Seleccione Estado--";
            listaBoolean.Add(objBoolean);

            //objBoolean = new EntBoolean();
            //objBoolean.IdBoolean = 1;
            //objBoolean.NomBoolean = "En Progreso";
            //listaBoolean.Add(objBoolean);

            //objBoolean = new EntBoolean();
            //objBoolean.IdBoolean = 2;
            //objBoolean.NomBoolean = "Pausado";
            //listaBoolean.Add(objBoolean);

            //objBoolean = new EntBoolean();
            //objBoolean.IdBoolean = 3;
            //objBoolean.NomBoolean = "Finalizado";
            //listaBoolean.Add(objBoolean);


            //objBoolean = new EntBoolean();
            //objBoolean.IdBoolean = 1;
            //objBoolean.NomBoolean = "Asignado";
            //listaBoolean.Add(objBoolean);

            objBoolean = new EntBoolean();
            objBoolean.IdBoolean = 132;
            objBoolean.NomBoolean = "En Progreso";
            listaBoolean.Add(objBoolean);

            objBoolean = new EntBoolean();
            objBoolean.IdBoolean = 2;
            objBoolean.NomBoolean = "Solucionado";
            listaBoolean.Add(objBoolean);

            objBoolean = new EntBoolean();
            objBoolean.IdBoolean = 3;
            objBoolean.NomBoolean = "Movilización";//"En Proceso";
            listaBoolean.Add(objBoolean);

            objBoolean = new EntBoolean();
            objBoolean.IdBoolean = 4;
            objBoolean.NomBoolean = "Pendiente Cliente";
            listaBoolean.Add(objBoolean);

            objBoolean = new EntBoolean();
            objBoolean.IdBoolean = 5;
            objBoolean.NomBoolean = "Pendiente Fabrica";
            listaBoolean.Add(objBoolean);

            objBoolean = new EntBoolean();
            objBoolean.IdBoolean = 59;
            objBoolean.NomBoolean = "Cerrado";
            listaBoolean.Add(objBoolean);

            //objBoolean = new EntBoolean();
            //objBoolean.IdBoolean = 8;
            //objBoolean.NomBoolean = "En Aprobación";
            //listaBoolean.Add(objBoolean);

            //objBoolean = new EntBoolean();
            //objBoolean.IdBoolean = 9;
            //objBoolean.NomBoolean = "Aprobado";
            //listaBoolean.Add(objBoolean);

            //objBoolean = new EntBoolean();
            //objBoolean.IdBoolean = 10;
            //objBoolean.NomBoolean = "Rechazado";
            //listaBoolean.Add(objBoolean);


            return listaBoolean;

        }



        public static List<EntBoolean> listaOsAdministrativas()
        {
            List<EntBoolean> listaBoolean = new List<EntBoolean>();
            EntBoolean objBoolean = null;

            objBoolean = new EntBoolean();
            objBoolean.IdBoolean = 0;
            objBoolean.NomBoolean = "--Seleccione OS--";
            listaBoolean.Add(objBoolean);

            //objBoolean = new EntBoolean();
            //objBoolean.IdBoolean = 5;
            //objBoolean.NomBoolean = "600000260 - SEVRV. INTERNO TEAM CAS";
            //listaBoolean.Add(objBoolean);

            //objBoolean = new EntBoolean();
            //objBoolean.IdBoolean = 6;
            //objBoolean.NomBoolean = "600000262 - SEVRV. INTERNO TEAM HP Hardware";
            //listaBoolean.Add(objBoolean);

            //objBoolean = new EntBoolean();
            //objBoolean.IdBoolean = 9;
            //objBoolean.NomBoolean = "600000263 - SEVRV. INTERNO TEAM CISCO";
            //listaBoolean.Add(objBoolean);

            //objBoolean = new EntBoolean();
            //objBoolean.IdBoolean = 7;
            //objBoolean.NomBoolean = "600000264 - SEVRV. INTERNO TEAM F5";
            //listaBoolean.Add(objBoolean);

            //objBoolean = new EntBoolean();
            //objBoolean.IdBoolean = 8;
            //objBoolean.NomBoolean = "600000265 - SEVRV. INTERNO TEAM Microsoft";
            //listaBoolean.Add(objBoolean);

            objBoolean = new EntBoolean();
            objBoolean.IdBoolean = 1;
            objBoolean.NomBoolean = "Servicio técnico interno DOS";
            listaBoolean.Add(objBoolean);

            objBoolean = new EntBoolean();
            objBoolean.IdBoolean = 2;
            objBoolean.NomBoolean = "Certificación / Capacitación";
            listaBoolean.Add(objBoolean);

            //objBoolean = new EntBoolean();
            //objBoolean.IdBoolean = 3;
            //objBoolean.NomBoolean = "600000276 - SEVRV. CLIENTE F5";
            //listaBoolean.Add(objBoolean);

            //objBoolean = new EntBoolean();
            //objBoolean.IdBoolean = 4;
            //objBoolean.NomBoolean = "600000277 - SEVRV. CLIENTE Microsoft";
            //listaBoolean.Add(objBoolean);


            return listaBoolean;

        }




        public static List<EntBoolean> listaEstadoIni()
        {
            List<EntBoolean> listaBoolean = new List<EntBoolean>();
            EntBoolean objBoolean = null;

            objBoolean = new EntBoolean();
            objBoolean.IdBoolean = 0;
            objBoolean.NomBoolean = "--Seleccione Estado--";
            listaBoolean.Add(objBoolean);

            objBoolean = new EntBoolean();
            objBoolean.IdBoolean = 1;
            objBoolean.NomBoolean = "En Progreso";
            listaBoolean.Add(objBoolean);

            return listaBoolean;

        }


        public static List<EntBoolean> listaDetEstado()
        {
            List<EntBoolean> listaBoolean = new List<EntBoolean>();
            EntBoolean objBoolean = null;

            objBoolean = new EntBoolean();
            objBoolean.IdBoolean = 0;
            objBoolean.NomBoolean = "--Seleccione Estado--";
            listaBoolean.Add(objBoolean);

            objBoolean = new EntBoolean();
            objBoolean.IdBoolean = 1;
            objBoolean.NomBoolean = "Movilización";
            listaBoolean.Add(objBoolean);

            objBoolean = new EntBoolean();
            objBoolean.IdBoolean = 2;
            objBoolean.NomBoolean = "En espera del cliente";
            listaBoolean.Add(objBoolean);

            objBoolean = new EntBoolean();
            objBoolean.IdBoolean = 3;
            objBoolean.NomBoolean = "Alimentación";
            listaBoolean.Add(objBoolean);

            objBoolean = new EntBoolean();
            objBoolean.IdBoolean = 4;
            objBoolean.NomBoolean = "Cambio de cliente";
            listaBoolean.Add(objBoolean);

            objBoolean = new EntBoolean();
            objBoolean.IdBoolean = 5;
            objBoolean.NomBoolean = "Otros";
            listaBoolean.Add(objBoolean);


            return listaBoolean;

        }




        public static List<EntBoolean> listaCategoria()
        {
            List<EntBoolean> listaBoolean = new List<EntBoolean>();
            EntBoolean objBoolean = null;



            objBoolean = new EntBoolean();
            objBoolean.IdBoolean = 0;
            objBoolean.NomBoolean = "--Seleccione SubCategoria--";
            listaBoolean.Add(objBoolean);

            objBoolean = new EntBoolean();
            objBoolean.IdBoolean = 1;
            objBoolean.NomBoolean = "Reuniones";
            listaBoolean.Add(objBoolean);

            objBoolean = new EntBoolean();
            objBoolean.IdBoolean = 2;
            objBoolean.NomBoolean = "Capacitaciones";
            listaBoolean.Add(objBoolean);

            objBoolean = new EntBoolean();
            objBoolean.IdBoolean = 3;
            objBoolean.NomBoolean = "Acompañamiento";
            listaBoolean.Add(objBoolean);

            objBoolean = new EntBoolean();
            objBoolean.IdBoolean = 4;
            objBoolean.NomBoolean = "Informes";
            listaBoolean.Add(objBoolean);

            //objBoolean = new EntBoolean();
            //objBoolean.IdBoolean = 0;
            //objBoolean.NomBoolean = "--Seleccione Estado--";
            //listaBoolean.Add(objBoolean);

            //objBoolean = new EntBoolean();
            //objBoolean.IdBoolean = 1;
            //objBoolean.NomBoolean = "Solicitud de Ofertas Técnicas y Costeos";
            //listaBoolean.Add(objBoolean);

            //objBoolean = new EntBoolean();
            //objBoolean.IdBoolean = 2;
            //objBoolean.NomBoolean = "Solicitud de POC";
            //listaBoolean.Add(objBoolean);

            //objBoolean = new EntBoolean();
            //objBoolean.IdBoolean = 3;
            //objBoolean.NomBoolean = "Requerimientos de Preventa y Reuniones para Especialistas de Servicios";
            //listaBoolean.Add(objBoolean);

            //objBoolean = new EntBoolean();
            //objBoolean.IdBoolean = 4;
            //objBoolean.NomBoolean = "Reuniones, Viáticos y Capacitaciones";
            //listaBoolean.Add(objBoolean);

            //objBoolean = new EntBoolean();
            //objBoolean.IdBoolean = 5;
            //objBoolean.NomBoolean = "Requerimiento de Cliente";
            //listaBoolean.Add(objBoolean);

            //objBoolean = new EntBoolean();
            //objBoolean.IdBoolean = 6;
            //objBoolean.NomBoolean = "Soporte Técnico";
            //listaBoolean.Add(objBoolean);

            //objBoolean = new EntBoolean();
            //objBoolean.IdBoolean = 7;
            //objBoolean.NomBoolean = "Tareas de Proyectos";
            //listaBoolean.Add(objBoolean);


            return listaBoolean;

        }






    }
}
