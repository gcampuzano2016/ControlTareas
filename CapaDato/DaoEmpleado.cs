using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDato
{
    public class DaoEmpleado
    {
        public static EntRespuesta Consulta_Sp_InsertarActualizarEmpleado(EntEmpleado objEmpleado)
        {
            EntRespuesta Respuesta = new EntRespuesta();
            Int32 respuestaSP = 0;
            string respuestaSP1 = "";
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();
                cmd = new SqlCommand("Sp_RTAInsUpdEmpleado", cnx);

                cmd.Parameters.AddWithValue("@IdEmpleado", objEmpleado.IdEmpleado);
                cmd.Parameters.AddWithValue("@Cedula", objEmpleado.Cedula);
                cmd.Parameters.AddWithValue("@Nombre", objEmpleado.Nombre);
                cmd.Parameters.AddWithValue("@Sociedad", objEmpleado.Sociedad);
                cmd.Parameters.AddWithValue("@Ciudad", objEmpleado.Ciudad);
                cmd.Parameters.AddWithValue("@AreaTrabajo", objEmpleado.AreaTrabajo);
                cmd.Parameters.AddWithValue("@Direccion", objEmpleado.Direccion);
                cmd.Parameters.AddWithValue("@Telefono", objEmpleado.Telefono);
                cmd.Parameters.AddWithValue("@Sexo", objEmpleado.Sexo);
                cmd.Parameters.AddWithValue("@Fecha_nacimiento", objEmpleado.Fecha_Nacimiento);
                cmd.Parameters.AddWithValue("@Provincia", objEmpleado.Provincia);
                cmd.Parameters.AddWithValue("@EstadoCivil", objEmpleado.EstadoCivil);
                cmd.Parameters.AddWithValue("@PuestoTrabajo", objEmpleado.PuestoTrabajo);
                cmd.Parameters.AddWithValue("@Correo", objEmpleado.Correo);

                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                dr.Read();

                respuestaSP1 = dr["Respuestas"].ToString();
                if (respuestaSP1 == "")
                {
                    respuestaSP = 1;
                }
                else
                {
                    respuestaSP = Convert.ToInt32(respuestaSP1);
                }

                if (respuestaSP >= 1)
                {
                    //Respuesta.estado = respuestaSP.ToString();
                    Respuesta.estado = "1";
                    Respuesta.mensaje = "Datos Guardados con Exito.";
                    Respuesta.tipoMensaje = "success";
                    Respuesta.resultado = respuestaSP.ToString();
                }
                else if (respuestaSP == -1)
                {
                    //Respuesta.estado = respuestaSP.ToString();
                    Respuesta.estado = "-1";
                    Respuesta.mensaje = "Por favor validar que la informacion este correcto";
                    Respuesta.tipoMensaje = "danger";
                    Respuesta.resultado = respuestaSP.ToString();
                }
                else
                {
                    Respuesta.estado = respuestaSP.ToString();
                    Respuesta.mensaje = "Ocurrio un error al guardar los datos.";
                    Respuesta.tipoMensaje = "danger";
                }
            }
            catch (Exception ex)
            {
                Respuesta.estado = "0";
                Respuesta.mensaje = ex.Message.ToString();
                Respuesta.tipoMensaje = "danger";
            }
            finally
            {
                cmd.Connection.Close();
            }

            return Respuesta;

        }



        /*public static EntRespuesta Sp_RTActualizarEmpleado(EntEmpleado objTarea)
                {
                    EntRespuesta Respuesta = new EntRespuesta();
                    int respuestaSP = 0;
                    SqlCommand cmd = null;
                    SqlDataReader dr = null;

                    try
                    {
                        DaoReporTareaAranda cn = new DaoReporTareaAranda();
                        SqlConnection cnx = cn.conectar();

                        cnx.Open();

                        cmd = new SqlCommand("Sp_RTActualizarEmpleado", cnx);

                        cmd.Parameters.AddWithValue("@IdEmpleado",  objTarea.IdEmpleado));
                        cmd.Parameters.AddWithValue("@Cedula",  objTarea.Cedula));
                        cmd.Parameters.AddWithValue("@Nombre",  objTarea.Nombre));
                        cmd.Parameters.AddWithValue("@Sociedad",  objTarea.Sociedad));
                        cmd.Parameters.AddWithValue("@Ciudad",  objTarea.Ciudad));
                        cmd.Parameters.AddWithValue("@AreaTrabajo",  objTarea.AreaTrabajo));
                        cmd.Parameters.AddWithValue("@Direccion",  objTarea.Direccion));
                        cmd.Parameters.AddWithValue("@Telefono",  objTarea.Telefono));
                        cmd.Parameters.AddWithValue("@Sexo",  objTarea.Sexo));
                        cmd.Parameters.AddWithValue("@Fecha_nacimiento",  objTarea.Fecha_nacimiento));
                        cmd.Parameters.AddWithValue("@Provincia",  objTarea.Provincia));

                        cmd.CommandType = CommandType.StoredProcedure;
                        dr = cmd.ExecuteReader();
                        dr.Read();
                        respuestaSP = Convert.ToInt32(dr["Respuestas"].ToString());

                        if (respuestaSP == 1)
                        {
                            Respuesta.estado = respuestaSP.ToString();
                            Respuesta.mensaje = "Datos Guardados con Exito.";
                            Respuesta.tipoMensaje = "success";
                        }
                        else
                        {
                            Respuesta.estado = respuestaSP.ToString();
                            Respuesta.mensaje = "Ocurrio un error al guardar los datos.";
                            Respuesta.tipoMensaje = "danger";
                        }

                    }
                    catch (Exception ex)
                    {
                        Respuesta.estado = "0";
                        Respuesta.mensaje = ex.Message.ToString();
                        Respuesta.tipoMensaje = "danger";
                    }
                    finally
                    {
                        cmd.Connection.Close();
                    }


                    return Respuesta;

                }*/



        public static List<EntEmpleado> Consulta_Sp_RTAConsultarListaEmpleados(string Cod_Usuario, int tipo)
        {
            List<EntEmpleado> listaTareas = null;

            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("Sp_RTAConsultaEmpleados", cnx);
                cmd.Parameters.AddWithValue("@Cod_Usuario", Cod_Usuario);
                cmd.Parameters.AddWithValue("@tipo", tipo);

                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                listaTareas = new List<EntEmpleado>();

                while (dr.Read())
                {
                    EntEmpleado Tarea = new EntEmpleado();
                    Tarea.IdEmpleado = Convert.ToInt32(dr["IdEmpleado"].ToString());
                    Tarea.Cedula = dr["CEDULA"].ToString();
                    Tarea.Nombre = dr["NOMBRE"].ToString();
                    Tarea.Sociedad = dr["SOCIEDAD"].ToString();
                    Tarea.Ciudad = dr["CIUDAD"].ToString();
                    Tarea.AreaTrabajo = dr["AREATRABAJO"].ToString();
                    Tarea.PuestoTrabajo = dr["PUESTOTRABAJO"].ToString();
                    Tarea.Direccion = dr["DIRECCION"].ToString();
                    Tarea.Telefono = dr["TELEFONO"].ToString();
                    Tarea.Sexo = dr["SEXO"].ToString();
                    Tarea.Fecha_Nacimiento = dr["FECHA_NACIMIENTO"].ToString(); //date
                    Tarea.Provincia = dr["PROVINCIA"].ToString();
                    Tarea.Correo = dr["CORREO"].ToString();
                    Tarea.EstadoCivil = dr["ESTADOCIVIL"].ToString();
                    Tarea.Estado = dr["ESTADO"].ToString();
                    Tarea.Notificacion = Convert.ToInt32(dr["NOTIFICACION"].ToString());
                    listaTareas.Add(Tarea);
                }

            }
            catch (Exception ex)
            {
                listaTareas = null;
            }
            finally
            {
                cmd.Connection.Close();
            }

            return listaTareas;
        }



        #region Consulta_Sp_RTAConsultarListaEmpleadosDescargar

        public static EntRespuesta Consulta_Sp_RTAConsultarListaEmpleadosDescargar()
        {
            EntRespuesta Respuesta = new EntRespuesta();
            DataTable dtResultados = new DataTable();

            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("Sp_RTAConsultaEmpleados", cnx);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();

                dtResultados.Load(dr);

                Respuesta.estado = "1";
                Respuesta.mensaje = "OK";
                Respuesta.tipoMensaje = "success";
                Respuesta.resultadoTabla = dtResultados;

            }
            catch (Exception ex)
            {
                Respuesta.estado = "0";
                Respuesta.mensaje = ex.Message.ToString();
                Respuesta.tipoMensaje = "danger";
            }
            finally
            {
                cmd.Connection.Close();
            }

            return Respuesta;
        }
        #endregion



        public static EntEmpleado Consulta_Sp_RTAConsultarEmpleadoPorId(string IdEmpleado)
        {

            EntEmpleado objEmpleado = null;// new entUsuario();
            SqlCommand cmd = null;
            SqlDataReader dr = null;

            try
            {
                DaoArandaDb cn = new DaoArandaDb();
                SqlConnection cnx = cn.conectar();

                cmd = new SqlCommand("Sp_RTAConsultarEmpleadoPorId", cnx);
                cmd.Parameters.AddWithValue("@IdEmpleado", IdEmpleado);
                cmd.CommandType = CommandType.StoredProcedure;

                cnx.Open();
                dr = cmd.ExecuteReader();
                objEmpleado = new EntEmpleado();
                dr.Read();

                objEmpleado.IdEmpleado = Convert.ToInt32(dr["IdEmpleado"].ToString());
                objEmpleado.Cedula = dr["CEDULA"].ToString();
                objEmpleado.Nombre = dr["NOMBRE"].ToString();
                objEmpleado.Sociedad = dr["SOCIEDAD"].ToString();
                objEmpleado.Ciudad = dr["CIUDAD"].ToString();
                objEmpleado.AreaTrabajo = dr["AREATRABAJO"].ToString();
                objEmpleado.Direccion = dr["DIRECCION"].ToString();
                objEmpleado.Telefono = dr["TELEFONO"].ToString();
                objEmpleado.Sexo = dr["SEXO"].ToString();
                objEmpleado.Fecha_Nacimiento = dr["FECHA_NACIMIENTO"].ToString(); //date
                objEmpleado.Provincia = dr["PROVINCIA"].ToString();
                objEmpleado.Correo = dr["CORREO"].ToString();
                objEmpleado.EstadoCivil = dr["ESTADOCIVIL"].ToString();
                objEmpleado.Provincia = dr["ESTADO"].ToString();

            }
            catch (Exception ex)
            {
                objEmpleado = null;

            }
            finally
            {
                cmd.Connection.Close();
            }
            return objEmpleado;
        }


        public static List<EntEmpleado> Consulta_Sp_RTAConsultarEmpleadoPorCedula(string IdEmpleado)
        {
            List<EntEmpleado> listaTareas = null;

            SqlCommand cmd = null;
            SqlDataReader dr = null;

            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();
                cmd = new SqlCommand("Sp_RTAConsultarEmpleadoPorId", cnx);
                cmd.Parameters.AddWithValue("@CedulaEmp", IdEmpleado);

                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                listaTareas = new List<EntEmpleado>();
                while (dr.Read())
                {
                    EntEmpleado objEmpleado = new EntEmpleado();
                    objEmpleado.IdEmpleado = Convert.ToInt32(dr["IdEmpleado"].ToString());
                    objEmpleado.Cedula = dr["CEDULA"].ToString();
                    objEmpleado.Nombre = dr["NOMBRE"].ToString();
                    objEmpleado.Sociedad = dr["SOCIEDAD"].ToString();
                    objEmpleado.Ciudad = dr["CIUDAD"].ToString();
                    objEmpleado.AreaTrabajo = dr["AREATRABAJO"].ToString();
                    objEmpleado.Direccion = dr["DIRECCION"].ToString();
                    objEmpleado.Telefono = dr["TELEFONO"].ToString();
                    objEmpleado.Sexo = dr["SEXO"].ToString();
                    objEmpleado.Fecha_Nacimiento = dr["FECHA_NACIMIENTO"].ToString(); //date
                    objEmpleado.Provincia = dr["PROVINCIA"].ToString();
                    objEmpleado.Estado = dr["ESTADO"].ToString();
                    objEmpleado.EstadoCivil = dr["ESTADOCIVIL"].ToString();
                    objEmpleado.PuestoTrabajo = dr["PUESTOTRABAJO"].ToString();
                    objEmpleado.Correo = dr["CORREO"].ToString();
                    listaTareas.Add(objEmpleado);
                }

            }
            catch (Exception ex)
            {
                listaTareas = null;
            }
            finally
            {
                cmd.Connection.Close();
            }

            return listaTareas;
        }


        public static EntRespuesta RTA_EliminarEmpleado(int IdEmpleado, string estado)
        {
            EntRespuesta Respuesta = new EntRespuesta();
            int respuestaSP = 0;
            SqlCommand cmd = null;
            SqlDataReader dr = null;

            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();
                cmd = new SqlCommand("Sp_RTACambiarEstadoEmpleado", cnx);


                cmd.Parameters.AddWithValue("@IDEMPLEADO", IdEmpleado);
                cmd.Parameters.AddWithValue("@ESTADO", estado);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                dr.Read();

                respuestaSP = Convert.ToInt32(dr["Respuestas"].ToString());

                if (respuestaSP >= 1)
                {
                    //Respuesta.estado = respuestaSP.ToString();
                    Respuesta.estado = "1";
                    Respuesta.mensaje = "Datos Guardados con Exito.";
                    Respuesta.tipoMensaje = "success";
                    Respuesta.resultado = respuestaSP.ToString();
                }
                else if (respuestaSP == -1)
                {
                    //Respuesta.estado = respuestaSP.ToString();
                    Respuesta.estado = "-1";
                    Respuesta.mensaje = "Por favor validar que la informacion este correcto";
                    Respuesta.tipoMensaje = "danger";
                    Respuesta.resultado = respuestaSP.ToString();
                }
                else
                {
                    Respuesta.estado = respuestaSP.ToString();
                    Respuesta.mensaje = "Ocurrio un error al guardar los datos.";
                    Respuesta.tipoMensaje = "danger";
                }
            }
            catch (Exception ex)
            {
                Respuesta.estado = "0";
                Respuesta.mensaje = ex.Message.ToString();
                Respuesta.tipoMensaje = "danger";
            }
            finally
            {
                cmd.Connection.Close();
            }

            return Respuesta;

        }

    }
}
