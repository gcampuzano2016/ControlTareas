using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CapaEntidad;
using CapaNegocio;
using SeguridadAppHelper;
using CorreoHelper;


namespace ReporteTareas.Formulario
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Session["Bandera"] = "0";

            if (!IsPostBack)
            {
                if (this.Request.QueryString["strEstadoLogin"] != null)
                {
                    Label1.Text = this.Request.QueryString["strEstadoLogin"].ToString();
                }
                else
                {
                    Label1.Text = "";
                }

                txt_login.Focus();              
                Session["Id_Usuario"] = "";
                Session["Cod_Usuario"] = "";
                Session["Nom_Usuario"] = "";
                Session["Log_Usuario"] = "";
                Session["Pass_Usuario"] = "";
                Session["Cod_Perfil"] = "";
                Session["E_Mail"] = "";
                Session["Rol_Usuario"] = "";
                Session["Fec_Creacion"] = "";
                Session["Usuario_Estado"] = "";
                Session["Cod_Jefe_Inm"] = "";
                Session["MailCodJefeInm"] = "";


                Session["PassIngres"] = "";
                Session["PassConfir"] = "";


                Session["UserLogin"] = "";
                Session["UserPasss"] = "";

                //Session["ObjEmpresa"] = null;
            }

        }

        public string EnviarCodigo()
        {
            string strcodigo="";
            strcodigo = Guid.NewGuid().ToString("N").Substring(1, 5);

            #region EnvioMail
            SeguridadHelper seguridad = new SeguridadHelper();
            List<EntItemValor> listaCamposCorreo = new List<EntItemValor>();
            EnvioCorreoHelper envioCorreo = new EnvioCorreoHelper();
            string correoUsuario = "";
            correoUsuario = NegUsuario.RTA_CorreoUsuario(txt_login.Text.Trim());

            listaCamposCorreo.Add(new EntItemValor() { Item = "tituloNotificacion", Valor = "Comprobar la dirección de correo electrónico" });
            listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta1", Valor = "Gracias por comprobar la cuenta de " });
            listaCamposCorreo.Add(new EntItemValor() { Item = "texto1", Valor = correoUsuario });
            listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta2", Valor = "Su código es: " });
            listaCamposCorreo.Add(new EntItemValor() { Item = "texto2", Valor = strcodigo });
            bool respuestaEnvioCorreo = false;
            string IdUsuarioSession = "";


            respuestaEnvioCorreo = envioCorreo.EnvioCorreoSolicitudJefe(correoUsuario, "Código de verificación del correo electrónico de la cuenta Grupo DOS S.A.", envioCorreo.EstructuraContenidoCorreoSolicitud("contenidoCorreoCodigoActivacion.txt"), listaCamposCorreo, "contenidoCorreoCodigoActivacion.txt");
            #endregion

            return strcodigo;
        }

        protected void btn_ingresar_Click(object sender, EventArgs e)
        {
            NegUsuario negUser = new NegUsuario();
            if (txt_login.Text.Trim() == "")
            {
                Response.Write(negUser.mensajeInformativo("Ingrese su usuario.", "warning", true, "divMensajesContenido_2"));
                txt_login.Focus();
                return;
            }
            if (txt_pass.Text.Trim() == "")
            {
                Response.Write(negUser.mensajeInformativo("Ingrese su contraseña.", "warning", true, "divMensajesContenido_2"));
                txt_pass.Focus();
                return;
            }

            EntUsuario objUsuario = new EntUsuario();
            string AuxClave = txt_pass.Text.Trim();
            SeguridadHelper seguridad = new SeguridadHelper();
            txt_pass.Text=seguridad.GetMd5Hash(AuxClave);
            objUsuario = NegUsuario.RTA_ConsultaUsuarioRTA(txt_login.Text.Trim());
            if (objUsuario != null)
            {
                if (objUsuario.IdCliente == 0)
                {
                    Autentificacion2.AuthenticationSoapClient client = new Autentificacion2.AuthenticationSoapClient();
                    Autentificacion2.UserInfo user = new Autentificacion2.UserInfo();
                    user = client.AutenticateUserAD(txt_login.Text.Trim(), AuxClave, "COMPUEQUIP");
                    if (user != null)
                    {
                        Session["Bandera"] = "1";
                        if (user.ActiveUser.Equals(0))
                        {
                            //ingres a la master
                            objUsuario = NegUsuario.RTA_ConsultaUsuarioRTA(txt_login.Text);
                            if (objUsuario != null)
                            {
                                Session["UserLogin"] = objUsuario.Log_Usuario;
                                Session["Cod_Usuario"] = objUsuario.Cod_Usuario;
                                Session["Nom_Usuario"] = objUsuario.Nom_Usuario;
                                Session["Log_Usuario"] = objUsuario.Log_Usuario;
                                Session["Cod_Perfil"] = objUsuario.Cod_Perfil;
                                Session["E_Mail"] = objUsuario.E_Mail;
                                Session["Rol_Usuario"] = objUsuario.Rol_Usuario;
                                Session["IdCliente"] = objUsuario.IdCliente;
                                Session["Id_Perfil"] = objUsuario.Id_Perfil;

                                if (objUsuario.Id_Perfil == 2 || objUsuario.Id_Perfil == 3)
                                    Session["Id_Usuario"] = 1;
                                else if (objUsuario.Id_Perfil == 4)
                                    Session["Id_Usuario"] = 4;
                                else if (objUsuario.Id_Perfil == 5)
                                    Session["Id_Usuario"] = 5;
                                else if (objUsuario.Id_Perfil == 6) // Reporte Gerencial
                                    Session["Id_Usuario"] = 6;
                                else if (objUsuario.Id_Perfil == 7) // Externos
                                    Session["Id_Usuario"] = 7;
                                else if (objUsuario.Id_Perfil == 8) //Vendedores Gerente de Producto
                                    Session["Id_Usuario"] = 8;
                                else if (objUsuario.Id_Perfil == 9) //Vendedores Gerente de Producto
                                    Session["Id_Usuario"] = 9;
                                else if (objUsuario.Id_Perfil == 10) //gerencia
                                    Session["Id_Usuario"] = 10;
                                else if (objUsuario.Id_Perfil == 11) //subgerencia
                                    Session["Id_Usuario"] = 11;
                                else if (objUsuario.Id_Perfil == 12) //Gerente Ventas
                                    Session["Id_Usuario"] = 12;
                                else if (objUsuario.Id_Perfil == 13) //Gerente Servicio
                                    Session["Id_Usuario"] = 13;
                                else if (objUsuario.Id_Perfil == 14) //Talento Humano
                                    Session["Id_Usuario"] = 14;
                                else if (objUsuario.Id_Perfil == 15) //Tesoreria
                                    Session["Id_Usuario"] = 15;
                                else if (objUsuario.Id_Perfil == 16) //Big&Prices
                                    Session["Id_Usuario"] = 16;
                                else if (objUsuario.Id_Perfil == 17) // Inventario
                                    Session["Id_Usuario"] = 17;
                                else if (objUsuario.Id_Perfil == 18) // ForeCast GD
                                    Session["Id_Usuario"] = 18;
                                else if (objUsuario.Id_Perfil == 19) //Vendedores Gerente de Producto 8
                                    Session["Id_Usuario"] = 19;
                                else if (objUsuario.Id_Perfil == 20) //Vendedores Gerente de Producto 9
                                    Session["Id_Usuario"] = 20;
                                else if (objUsuario.Id_Perfil == 21) //Logistica
                                    Session["Id_Usuario"] = 21;
                                else
                                    Session["Id_Usuario"] = 0;


                                Response.Redirect("Principal.aspx");
                            }
                            else
                            {
                                //Usuario no existe en la base de datos de tareas
                                var ADB_exit_user = "";
                                var ADB_exitEstado = "";
                                var ADB_exite = 0;
                                //EntUsuario objUsuario = new EntUsuario();
                                //se verifica si en ARANDADB existe el usuario
                                ADB_exit_user = NegUsuario.ADB_verificaUsuario(txt_login.Text);
                                if (ADB_exit_user.Length == 2)
                                {
                                    ADB_exitEstado = ADB_exit_user.Substring(1, 1);
                                    ADB_exite = Convert.ToInt16(ADB_exit_user.Substring(0, 1));
                                    if (ADB_exitEstado.Equals("A"))
                                    {
                                        //llenar objeto con los datos del usuario
                                        objUsuario = NegUsuario.ADB_ConsultaUsuario(txt_login.Text);
                                        Session["Id_Usuario"] = objUsuario.Id_Usuario;
                                        Session["Cod_Usuario"] = objUsuario.Cod_Usuario;
                                        Session["Nom_Usuario"] = objUsuario.Nom_Usuario;
                                        Session["Log_Usuario"] = objUsuario.Log_Usuario;
                                        Session["Cod_Perfil"] = objUsuario.Cod_Perfil;
                                        Session["E_Mail"] = objUsuario.E_Mail;
                                        Session["Rol_Usuario"] = objUsuario.Rol_Usuario;
                                        Session["Cod_Jefe_Inm"] = objUsuario.Cod_Jefe_Inm;
                                        Session["MailCodJefeInm"] = objUsuario.MailCodJefeInm;
                                        Session["Id_Perfil"] = objUsuario.Id_Perfil;

                                        EntUsuario objDatUsuarios = new EntUsuario();
                                        objDatUsuarios.Id_Usuario = 0;
                                        objDatUsuarios.Cod_Usuario = Session["Cod_Usuario"].ToString();
                                        objDatUsuarios.Nom_Usuario = Session["Nom_Usuario"].ToString();
                                        objDatUsuarios.Log_Usuario = Session["Log_Usuario"].ToString();
                                        objDatUsuarios.Pass_Usuario = seguridad.GetMd5Hash(AuxClave);
                                        objDatUsuarios.Cod_Perfil = Convert.ToInt32(Session["Cod_Perfil"]);
                                        objDatUsuarios.E_Mail = Session["E_Mail"].ToString();
                                        objDatUsuarios.Rol_Usuario = Convert.ToInt32(Session["Rol_Usuario"]);
                                        objDatUsuarios.Fec_Creacion = System.DateTime.Now.ToString();
                                        objDatUsuarios.Usuario_Estado = "A";

                                        var ResIngreso = NegUsuario.RTA_IngresaUsuario(objDatUsuarios);


                                        if (ResIngreso >= 1)
                                        {
                                            //Mensaje de reintento de acceso
                                            Response.Write(negUser.mensajeInformativo("Su usuario de ARANDA fue replicado. Por favor vuelva a acceder al sistema.", "info", true, "divMensajesContenido_2"));
                                            txt_login.Text = Session["UserLogin"].ToString();
                                            txt_pass.Text = "";
                                            txt_pass.Focus();
                                            return;
                                            Response.Redirect("Login.aspx");
                                        }
                                        else
                                        {
                                            //Mensaje de reintento de acceso
                                            Response.Write(negUser.mensajeInformativo("Su usuario de ARANDA no pudo ser replicado. Por favor comuniquese con el administrador del sistema.", "danger", true, "divMensajesContenido_2"));
                                            txt_pass.Focus();
                                            Response.Redirect("Login.aspx");
                                            return;
                                        }

                                    }
                                    else if (ADB_exitEstado.Equals("I"))
                                    {
                                        Response.Write(negUser.mensajeInformativo("El usuario ingresado se encuentra inactivo en ARANDA. Por favor comuniquese con el administrador del sistema.", "danger", true, "divMensajesContenido_2"));
                                    }
                                    else if (ADB_exitEstado.Equals("X"))
                                    {
                                        Response.Write(negUser.mensajeInformativo("El usuario ingresado no existe dentro de la base de datos de ARANDA", "danger", true, "divMensajesContenido_2"));
                                    }

                                }

                            }
                        }
                        else
                        {
                            //mensaje de no auteticado
                            Response.Write(negUser.mensajeInformativo("Sus credenciales de acceso son incorrectas", "danger", true, "divMensajesContenido_2"));
                            txt_login.Text = Session["UserLogin"].ToString();
                            txt_pass.Text = "";
                            txt_pass.Focus();
                        }
                    }
                    else
                    {
                        // Response.Write("<script>var divMensajesContenido_1 = 'Credencial de Acceso No Autorizadas';</script>");
                        Response.Write(negUser.mensajeInformativo("Credenciales de acceso No Autorizadas", "danger", true, "divMensajesContenido_1"));
                        txt_pass.Focus();
                        //objUsuario.Log_Usuario = txt_login.Text;
                        //objUsuario.CodigoReset = EnviarCodigo();
                        //NegUsuario.RTA_InsertaCodigoSeguridad(objUsuario);
                        //Session["UserLogin"] = txt_login.Text;
                        //Response.Redirect("ResetPassword.aspx");
                    }

                }
                else
                {
                    objUsuario = NegUsuario.RTA_ConsultaUsuarioRTA(txt_login.Text);
                    if (objUsuario != null)
                    {
                        Session["UserLogin"] = objUsuario.Log_Usuario;
                        Session["Cod_Usuario"] = objUsuario.Cod_Usuario;
                        Session["Nom_Usuario"] = objUsuario.Nom_Usuario;
                        Session["Log_Usuario"] = objUsuario.Log_Usuario;
                        Session["Cod_Perfil"] = objUsuario.Cod_Perfil;
                        Session["E_Mail"] = objUsuario.E_Mail;
                        Session["Rol_Usuario"] = objUsuario.Rol_Usuario;
                        Session["IdCliente"] = objUsuario.IdCliente;
                        Session["Id_Perfil"] = objUsuario.Id_Perfil;

                        if (objUsuario.Id_Perfil == 2 || objUsuario.Id_Perfil == 3)
                            Session["Id_Usuario"] = 1;
                        else if (objUsuario.Id_Perfil == 4)
                            Session["Id_Usuario"] = 4;
                        else if (objUsuario.Id_Perfil == 5)
                            Session["Id_Usuario"] = 5;
                        else if (objUsuario.Id_Perfil == 7)
                            Session["Id_Usuario"] = 7;
                        else if (objUsuario.Id_Perfil == 19) //Vendedores Gerente de Producto 8
                            Session["Id_Usuario"] = 19;
                        else if (objUsuario.Id_Perfil == 20) //Vendedores Gerente de Producto 9
                            Session["Id_Usuario"] = 20;
                        else if (objUsuario.Id_Perfil == 21) //Logistica
                            Session["Id_Usuario"] = 21;
                        else if (objUsuario.Id_Perfil == 12) //Gerente Ventas
                            Session["Id_Usuario"] = 12;
                        else
                            Session["Id_Usuario"] = 0;

                        Response.Redirect("Principal.aspx");
                    }
                }
            }
            else
            {
                //Usuario no existe en la base de datos de tareas
                var ADB_exit_user = "";
                var ADB_exitEstado = "";
                var ADB_exite = 0;
                //EntUsuario objUsuario = new EntUsuario();
                //se verifica si en ARANDADB existe el usuario
                ADB_exit_user = NegUsuario.ADB_verificaUsuario(txt_login.Text);
                if (ADB_exit_user.Length == 2)
                {
                    ADB_exitEstado = ADB_exit_user.Substring(1, 1);
                    ADB_exite = Convert.ToInt16(ADB_exit_user.Substring(0, 1));
                    if (ADB_exitEstado.Equals("A"))
                    {
                        //llenar objeto con los datos del usuario
                        objUsuario = NegUsuario.ADB_ConsultaUsuario(txt_login.Text);
                        //Session["Id_Usuario"] = objUsuario.Id_Usuario;
                        Session["Cod_Usuario"] = objUsuario.Cod_Usuario;
                        Session["Nom_Usuario"] = objUsuario.Nom_Usuario;
                        Session["Log_Usuario"] = objUsuario.Log_Usuario;
                        Session["Cod_Perfil"] = objUsuario.Cod_Perfil;
                        Session["E_Mail"] = objUsuario.E_Mail;
                        Session["Rol_Usuario"] = objUsuario.Rol_Usuario;
                        Session["Cod_Jefe_Inm"] = objUsuario.Cod_Jefe_Inm;
                        Session["MailCodJefeInm"] = objUsuario.MailCodJefeInm;
                        Session["Id_Perfil"] = objUsuario.Id_Perfil;

                        EntUsuario objDatUsuarios = new EntUsuario();
                        objDatUsuarios.Id_Usuario = 0;
                        objDatUsuarios.Cod_Usuario = Session["Cod_Usuario"].ToString();
                        objDatUsuarios.Nom_Usuario = Session["Nom_Usuario"].ToString();
                        objDatUsuarios.Log_Usuario = Session["Log_Usuario"].ToString();
                        objDatUsuarios.Pass_Usuario = seguridad.GetMd5Hash(AuxClave);
                        objDatUsuarios.Cod_Perfil = Convert.ToInt32(Session["Cod_Perfil"]);
                        objDatUsuarios.E_Mail = Session["E_Mail"].ToString();
                        objDatUsuarios.Rol_Usuario = Convert.ToInt32(Session["Rol_Usuario"]);
                        objDatUsuarios.Fec_Creacion = System.DateTime.Now.ToString();
                        objDatUsuarios.Usuario_Estado = "A";

                        var ResIngreso = NegUsuario.RTA_IngresaUsuario(objDatUsuarios);


                        if (ResIngreso >= 1)
                        {
                            //Mensaje de reintento de acceso
                            Response.Write(negUser.mensajeInformativo("Su usuario de ARANDA fue replicado. Por favor vuelva a acceder al sistema.", "info", true, "divMensajesContenido_2"));
                            txt_login.Text = Session["UserLogin"].ToString();
                            txt_pass.Text = "";
                            txt_pass.Focus();
                            return;
                            //Response.Redirect("Login.aspx");
                        }
                        else
                        {
                            //Mensaje de reintento de acceso
                            Response.Write(negUser.mensajeInformativo("Su usuario de ARANDA no pudo ser replicado. Por favor comuniquese con el administrador del sistema.", "danger", true, "divMensajesContenido_2"));
                            txt_pass.Focus();
                            //Response.Redirect("Login.aspx");
                            return;
                        }

                    }
                    else if (ADB_exitEstado.Equals("I"))
                    {
                        Response.Write(negUser.mensajeInformativo("El usuario ingresado se encuentra inactivo en ARANDA. Por favor comuniquese con el administrador del sistema.", "danger", true, "divMensajesContenido_2"));
                    }
                    else if (ADB_exitEstado.Equals("X"))
                    {
                        Response.Write(negUser.mensajeInformativo("El usuario ingresado no existe dentro de la base de datos de ARANDA", "danger", true, "divMensajesContenido_2"));
                    }

                }
            }
        }
    }
}