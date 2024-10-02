using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CapaDato;
using CapaEntidad;

namespace CapaNegocio
{
    public class NegUsuario
    {
        public static int RTA_InsertaCodigoSeguridad(EntUsuario ObjDatosUsuario)
        {
            var respuesta = 0;


            respuesta = DaoRTAUsuario.RTA_InsertaCodigoSeguridad(ObjDatosUsuario);

            return respuesta;

        }
        public static string RTA_verificaUsuario(string IdUsuario)
        {
            string valida = validaCampo(IdUsuario);
            string respuesta = "";
            if (valida.Equals(IdUsuario))
            {
                respuesta = DaoRTAUsuario.RTA_verificaUsuario(IdUsuario);
            }
            else
            {
                respuesta = valida;
            }
            return respuesta;
        }
        public static string ADB_verificaUsuario(string IdUsuario)
        {



            string valida = validaCampo(IdUsuario);
            string respuesta = "";
            if (valida.Equals(IdUsuario))
            {
                respuesta = DaoADBUsuario.ADB_verificaUsuario(IdUsuario);
            }
            else
            {
                respuesta = valida;
            }
            return respuesta;
        }
        public static EntUsuario ADB_ConsultaUsuario(string IdUsuario)
        {
            EntUsuario ObjRespuesta = new EntUsuario();


            ObjRespuesta = DaoADBUsuario.ADB_ConsultaUsuarioADB(IdUsuario);

            return ObjRespuesta;

        }
        public static EntUsuario RTA_ConsultaUsuarioRTA(string IdUsuario)
        {
            EntUsuario ObjRespuesta = new EntUsuario();


            ObjRespuesta = DaoRTAUsuario.RTA_ConsultaUsuarioRTA(IdUsuario);

            return ObjRespuesta;

        }   
        public static EntUsuario RTAConsultaUsuarioPorCodigo(string IdUsuario)
        {
            EntUsuario ObjRespuesta = new EntUsuario();


            ObjRespuesta = DaoRTAUsuario.RTAConsultaUsuarioPorCodigo(IdUsuario);

            return ObjRespuesta;

        }

        public static EntUsuario RTA_ConsultarCodigoSeguridad(string IdUsuario)
        {
            EntUsuario ObjRespuesta = new EntUsuario();


            ObjRespuesta = DaoRTAUsuario.RTA_ConsultarCodigoSeguridad(IdUsuario);

            return ObjRespuesta;

        }
        public static bool RTA_ConsultaUsuarioEsJefe(string IdUsuario)
        {
            bool respuesta = false;


            respuesta = DaoRTAUsuario.RTA_ConsultaUsuarioEsJefe(IdUsuario);

            return respuesta;

        }
        public static string RTA_CorreoJefeInmediato(string IdUsuario)
        {
            string respuesta = "";


            respuesta = DaoRTAUsuario.RTA_CorreoJefeInmediato(IdUsuario);

            return respuesta;

        }
        public static string RTA_CorreoUsuario(string IdUsuario)
        {
            string respuesta = "";


            respuesta = DaoRTAUsuario.RTA_CorreoUsuario(IdUsuario);

            return respuesta;

        }
        //RTA_IngresaUsuario
        public static int RTA_IngresaUsuario(EntUsuario ObjDatosUsuario)
        {
            var respuesta = 0;


            respuesta = DaoRTAUsuario.RTA_IngresaUsuario(ObjDatosUsuario);

            return respuesta;

        }
        public static string RTA_AutenticaUsuario(string IdUsuario, string Passw)
        {
            var respuesta ="";

            respuesta = DaoRTAUsuario.RTA_AutenticaUsuario(IdUsuario, Passw);

            return respuesta;
        }
        public static List<EntCombo> ListaUsuariosCombo(string Idusuario)
        {
            return DaoRTAUsuario.ListaUsuariosCombo(Idusuario);
        }
        public static List<EntCombo> ConsultarDatosEmpleado(string Cod_Usuario, int Tipo)
        {
            return DaoRTAUsuario.ConsultarDatosEmpleado(Cod_Usuario, Tipo);
        }
        public static List<EntUsuario> ConsultarDatosReemplazo(string Cod_Usuario, int Tipo)
        {
            return DaoRTAUsuario.ConsultarDatosReemplazo(Cod_Usuario, Tipo);
        }
        public static List<EntUsuario> RTA_ConsultaLike(int tipo, string Descripcion)
        {
            return DaoADBUsuario.RTA_ConsultaLike(tipo, Descripcion);
        }

        public static string RTACorreoUsuarioPlanificacionVac(string Cod_Jefe_Inm)
        {
            string respuesta = "";


            respuesta = DaoRTAUsuario.RTACorreoUsuarioPlanificacionVac(Cod_Jefe_Inm);

            return respuesta;

        }

        //Metodo de validacion de caracteres 
        public static string validaCampo(string Campo)
        {
            string respuesta = "";
            string caracter = "";
            int tamanio = 0;
            tamanio = Campo.Length;
            for (int i = 0; i < tamanio; i++)
            {
                caracter = Campo.Substring(i, 1);
                if (caracter.Equals("=") || caracter.Equals("<") || caracter.Equals(">") || caracter.Equals("!") || caracter.Equals("'") || caracter.Equals("/") || caracter.Equals("-"))
                {
                    respuesta = "No es permitido el uso del caracter \"" + caracter + "\" dentro del sistema";
                }
            }

            if (respuesta.Equals(""))
            {
                respuesta = Campo;
            }

            return respuesta;

        }
        //Metodo mensaje informativos 
        public string mensajeInformativo(string mensaje, string tipoMensaje = "success", bool habilitarBotonCerrar = true, string nombreVariableJS = "divMensajesContenido_1")
        {
            string respuesta = "";
            string botonCerrar = "";
            //alert alert-success

            if (habilitarBotonCerrar)
            {
                botonCerrar = "alert-dismissable";
            }

            if (tipoMensaje == "danger")
            {
                respuesta = "<div class='alert alert-danger " + botonCerrar + "'><button type = 'button' class='close' data-dismiss='alert' aria-hidden='true'>×</button>" + mensaje + "</div>";
            }

            if (tipoMensaje == "success")
            {
                respuesta = "<div class='alert alert-success " + botonCerrar + "'><button type = 'button' class='close' data-dismiss='alert' aria-hidden='true'>×</button>" + mensaje + "</div>";
            }

            if (tipoMensaje == "warning")
            {
                respuesta = "<div class='alert alert-warning " + botonCerrar + "'><button type = 'button' class='close' data-dismiss='alert' aria-hidden='true'>×</button>" + mensaje + "</div>";
            }
            
            if (tipoMensaje == "info")
            {
                respuesta = "<div class='alert alert-info " + botonCerrar + "'><button type = 'button' class='close' data-dismiss='alert' aria-hidden='true'>×</button>" + mensaje + "</div>";
            }

            respuesta = "<script>var " + nombreVariableJS + " = \"" + respuesta + "\";</script>";

            return respuesta;

        }

    }
}
