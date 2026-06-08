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
    public class DaoCargaFamiliar
    {

        public static EntRespuesta RTAInsertarActualizarCargaFam(EntCargaFamiliar objCargar)
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
                cmd = new SqlCommand("Sp_RTAInsUpdCargaFam", cnx);

                cmd.Parameters.AddWithValue("@IdCargaFam", objCargar.IdCargaFam);
                cmd.Parameters.AddWithValue("@IdEmpleado", objCargar.IdEmpleado);
                cmd.Parameters.AddWithValue("@Parentesco", objCargar.Parentesco);
                cmd.Parameters.AddWithValue("@Nombre", objCargar.Nombre);
                cmd.Parameters.AddWithValue("@Fecha_nacimiento", Convert.ToDateTime ( objCargar.fecha_nacimiento));
                cmd.Parameters.AddWithValue("@Operacion", objCargar.Operacion);

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

                if (respuestaSP == 0 || respuestaSP == 2)
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
                    Respuesta.estado = "1";
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
                Respuesta.estado = "2";
                Respuesta.mensaje = ex.Message.ToString();
                Respuesta.tipoMensaje = "danger";
            }
            finally
            {
                cmd.Connection.Close();
            }

            return Respuesta;

        }

        public static List<EntCargaFamiliar> Consulta_Sp_RTAConsultarListaCargasFam()
        {
            List<EntCargaFamiliar> listaTareas = null;

            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("Sp_RTAConsultaCargaFam", cnx);
                //cmd.Parameters.AddWithValue("@IdEmpleado", IdEmpleado);
                //cmd.Parameters.AddWithValue("@tipo", tipo);

                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                listaTareas = new List<EntCargaFamiliar>();

                while (dr.Read())
                {
                    EntCargaFamiliar Tarea = new EntCargaFamiliar();
                    Tarea.IdCargaFam = Convert.ToInt32(dr["IdCargaFam"].ToString());
                    Tarea.IdEmpleado = Convert.ToInt32(dr["IdEmpleado"].ToString());
                    Tarea.Parentesco = dr["PARENTESCO"].ToString();
                    Tarea.Nombre = dr["NOMBRE"].ToString();
                    Tarea.fecha_nacimiento = dr["FECHA_NACIMIENTO"].ToString(); //date
                    Tarea.Estado = dr["ESTADO"].ToString();

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

        public static List<EntCargaFamiliar> ConsultaSp_RTAConsultarCargaFamPorId(int IdEmpleado)
        {      

                List<EntCargaFamiliar> listaTareas = null;

                SqlCommand cmd = null;
                SqlDataReader dr = null;
                try
                {
                    DaoReporTareaAranda cn = new DaoReporTareaAranda();
                    SqlConnection cnx = cn.conectar();
                    cnx.Open();

                    cmd = new SqlCommand("Sp_RTAConsultarCargaFamPorId", cnx);
                    cmd.Parameters.AddWithValue("@IdEmpleado", IdEmpleado);
                    //cmd.Parameters.AddWithValue("@tipo", tipo);

                    cmd.CommandType = CommandType.StoredProcedure;
                    dr = cmd.ExecuteReader();
                    listaTareas = new List<EntCargaFamiliar>();

                    while (dr.Read())
                    {
                        EntCargaFamiliar Tarea = new EntCargaFamiliar();
                        Tarea.IdCargaFam = Convert.ToInt32(dr["IdCargaFam"].ToString());
                        Tarea.IdEmpleado = Convert.ToInt32(dr["IdEmpleado"].ToString());
                        Tarea.Parentesco = dr["PARENTESCO"].ToString();
                        Tarea.Nombre = dr["NOMBRE"].ToString();
                        Tarea.fecha_nacimiento = dr["FECHA_NACIMIENTO"].ToString(); //date
                        Tarea.Estado = dr["ESTADO"].ToString();

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

        public static EntRespuesta RTA_EliminarCargaFamiliar(string Nombre)
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
                cmd = new SqlCommand("Sp_RTACambiarEstadoCargaFam", cnx);


                //cmd.Parameters.AddWithValue("@IDEMPLEADO", IdEmpleado);
                cmd.Parameters.AddWithValue("@Nombre", Nombre);
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
