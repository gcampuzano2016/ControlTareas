using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CapaDato
{
    public class DaoHsCln
    {
        public static EntRespuesta Consulta_Sp_InsUpd_HsCln(EntHsCln obj)
        {
            EntRespuesta Respuesta = new EntRespuesta();
            int respuestaSP = 0;
            string respuestaSP1 = "";
            SqlCommand cmd = null;
            SqlDataReader dr = null;

            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();
                cmd = new SqlCommand("Sp_InsUpdHsCln", cnx);
                cmd.CommandType = CommandType.StoredProcedure;

                //cmd.Parameters.AddWithValue("@idHsCln", obj.idHsCln ?? "");
                cmd.Parameters.AddWithValue("@hsCln_ciEmpleado", obj.hsCln_ciEmpleado ?? "");
                cmd.Parameters.AddWithValue("@hsCln_identificador", obj.hsCln_Identificador ?? "");
                cmd.Parameters.AddWithValue("@hsCln_Fecha", string.IsNullOrEmpty(obj.hsCln_Fecha) ? (object)DBNull.Value : obj.hsCln_Fecha);
                cmd.Parameters.AddWithValue("@hsCln_Hora", obj.hsCln_Hora ?? "");
                cmd.Parameters.AddWithValue("@hsCln_Motivo", obj.hsCln_Motivo ?? "");
                cmd.Parameters.AddWithValue("@hsCln_SigVit", obj.hsCln_SigVit ?? "");
                cmd.Parameters.AddWithValue("@hsCln_Antece", obj.hsCln_Antece ?? "");
                cmd.Parameters.AddWithValue("@hsCln_Interv", obj.hsCln_Interv ?? "");
                cmd.Parameters.AddWithValue("@hsCln_Recom", obj.hsCln_Recom ?? "");
                cmd.Parameters.AddWithValue("@hsCln_Obs", obj.hsCln_Obs ?? "");
                cmd.Parameters.AddWithValue("@hsCln_Seguir", Convert.ToInt32(obj.hsCln_Seguir));
                cmd.Parameters.AddWithValue("@hsVul_Vulnerds", obj.hsVul_Vulnerds ?? "");
                cmd.Parameters.AddWithValue("@hsVul_Rec_Obs", obj.hsVul_Rec_Obs ?? "");



                using (dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        respuestaSP = dr["Respuestas"] != DBNull.Value ? Convert.ToInt32(dr["Respuestas"]) : 0;

                        if (respuestaSP == 1)
                        {
                            Respuesta.estado = "1";
                            Respuesta.mensaje = "Datos guardados con éxito.";
                            Respuesta.tipoMensaje = "success";
                            Respuesta.resultado = respuestaSP.ToString();
                        }
                        else
                        {
                            Respuesta.estado = "0";
                            Respuesta.mensaje = "Ocurrió un error al guardar los datos.";
                            Respuesta.tipoMensaje = "danger";
                        }
                    }
                    else
                    {
                        Respuesta.estado = "0";
                        Respuesta.mensaje = "No se recibió respuesta del procedimiento almacenado.";
                        Respuesta.tipoMensaje = "warning";
                    }
                }

            }
            catch (Exception ex)
            {
                Respuesta.estado = "0";
                Respuesta.mensaje = "Excepción: " + ex.Message;
                Respuesta.tipoMensaje = "danger";
            }
            finally
            {
                if (cmd != null && cmd.Connection != null)
                {
                    cmd.Connection.Close();
                }
            }

            return Respuesta;
        }

        public static EntHsCln Consulta_Sp_DatosPerVulnerable(int op, string IdEmpleado)
        {
            EntHsCln obj = null;
            SqlCommand cmd = null;
            SqlDataReader dr = null;

            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                //DaoArandaDb cn = new DaoArandaDb();
                SqlConnection cnx = cn.conectar();


                cmd = new SqlCommand("sp_DMObtenerVuldsPorEmpleado", cnx);
                cmd.Parameters.AddWithValue("@opcion", op);
                cmd.Parameters.AddWithValue("@ciEmpleado", IdEmpleado);
                cmd.CommandType = CommandType.StoredProcedure;

                cnx.Open();
                dr = cmd.ExecuteReader();
                obj = new EntHsCln();
                dr.Read();

                obj.hsVul_Vulnerds = dr["Vulnerabilidades"] != DBNull.Value ? dr["Vulnerabilidades"].ToString() : "";
                obj.hsCln_ciEmpleado = dr["Cedula"] != DBNull.Value ? dr["Cedula"].ToString() : "";
                obj.hsVul_Rec_Obs = dr["Observaciones"] != DBNull.Value ? dr["Observaciones"].ToString() : "";
                obj.hsCln_Nombre = dr["Nombre"] != DBNull.Value ? dr["Nombre"].ToString() : "";

            }
            catch (Exception ex)
            {
                throw new Exception("Error en sp_DMObtenerVuldsPorEmpleado: " + ex.Message);
            }
            finally
            {
                if (cmd != null && cmd.Connection != null)
                {
                    cmd.Connection.Close();
                }
            }

            return obj;
        }

        public static List<EntHsCln> ConsultaLista_Sp_DatosPerVulnerable(int op, string IdEmpleado)
        {
            List<EntHsCln> listaTareas = null;
            SqlCommand cmd = null;
            SqlDataReader dr = null;

            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                //DaoArandaDb cn = new DaoArandaDb();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("sp_DMObtenerVuldsPorEmpleado", cnx);
                cmd.Parameters.AddWithValue("@opcion", op);
                cmd.Parameters.AddWithValue("@ciEmpleado", IdEmpleado);

                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                listaTareas = new List<EntHsCln>();

                while (dr.Read())
                {
                    EntHsCln obj = new EntHsCln();

                    obj.hsVul_Vulnerds = dr["Vulnerabilidades"] != DBNull.Value ? dr["Vulnerabilidades"].ToString() : "";
                    obj.hsCln_ciEmpleado = dr["Cedula"] != DBNull.Value ? dr["Cedula"].ToString() : "";
                    obj.hsVul_Rec_Obs = dr["Observaciones"] != DBNull.Value ? dr["Observaciones"].ToString() : "";
                    obj.hsCln_Nombre = dr["Nombre"] != DBNull.Value ? dr["Nombre"].ToString() : "";

                    listaTareas.Add(obj);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error en sp_DMObtenerVuldsPorEmpleado: " + ex.Message);
            }
            finally
            {
                if (cmd != null && cmd.Connection != null)
                {
                    cmd.Connection.Close();
                }
            }

            return listaTareas;
        }


        public static List<EntHsCln> ConsultaSp_RTAListaHsCln(string ci, int op, string fecha_ini, string fecha_fin)
        {
            List<EntHsCln> listaTareas = null;

            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("SP_ConsultarHscClnPorCI", cnx);
                cmd.Parameters.AddWithValue("@ciEmpleado", ci);
                cmd.Parameters.AddWithValue("@opcion", op);
                cmd.Parameters.AddWithValue("@fechaInicial", fecha_ini);
                cmd.Parameters.AddWithValue("@fechaFinal", fecha_fin);

                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                listaTareas = new List<EntHsCln>();

                while (dr.Read())
                {
                    EntHsCln Tarea = new EntHsCln();

                    //// Campos básicos con validación de nulos
                    //Tarea.hsCln_ciEmpleado = dr["hsCln_ciEmpleado"] != DBNull.Value ? dr["hsCln_ciEmpleado"].ToString() : "";
                    //Tarea.hsCln_Identificador = dr["hsCln_identificador"] != DBNull.Value ? dr["hsCln_identificador"].ToString() : "";
                    //Tarea.hsCln_Nombre = dr["NombreEmpleado"] != DBNull.Value ? dr["NombreEmpleado"].ToString() : "";

                    //// Fecha con validación especial
                    //Tarea.hsCln_Fecha = dr["hsCln_Fecha"] != DBNull.Value
                    //    ? Convert.ToDateTime(dr["hsCln_Fecha"]).ToString("yyyy-MM-dd")
                    //    : "";

                    //// Campos de texto con validación
                    //Tarea.hsCln_Hora = dr["hsCln_Hora"] != DBNull.Value ? dr["hsCln_Hora"].ToString() : "";
                    //Tarea.hsCln_Motivo = dr["hsCln_Motivo"] != DBNull.Value ? dr["hsCln_Motivo"].ToString() : "";
                    //Tarea.hsCln_Interv = dr["hsCln_Interv"] != DBNull.Value ? dr["hsCln_Interv"].ToString() : "";
                    //Tarea.hsCln_Recom = dr["hsCln_Recom"] != DBNull.Value ? dr["hsCln_Recom"].ToString() : "";
                    //Tarea.hsCln_Obs = dr["hsCln_Obs"] != DBNull.Value ? dr["hsCln_Obs"].ToString() : "";
                    //Tarea.hsCln_Estado = dr["hsCln_Estado"] != DBNull.Value ? dr["hsCln_Estado"].ToString() : "";

                    //// Campo entero con validación
                    //Tarea.hsCln_Seguir = dr["hsCln_Seguir"] != DBNull.Value ? Convert.ToInt32(dr["hsCln_Seguir"]) : 0;

                    //// Los últimos campos que querías validar especialmente
                    //Tarea.hsCln_Antece = dr["hsCln_Antece"] != DBNull.Value ? dr["hsCln_Antece"].ToString() : "";
                    //Tarea.hsCln_SigVit = dr["hsCln_SigVit"] != DBNull.Value ? dr["hsCln_SigVit"].ToString() : "";
                    //Tarea.hsVul_Fecha = "";
                    //Tarea.hsVul_Rec_Obs = "";
                    //Tarea.hsVul_Vulnerds = "";
                    //Tarea.hsVul_ciEmpleado = "";




                    // Campos básicos con validación de nulos
                    Tarea.hsCln_ciEmpleado = dr["hsCln_ciEmpleado"].ToString();
                    Tarea.hsCln_Identificador = dr["hsCln_identificador"].ToString();
                    Tarea.hsCln_Nombre = dr["NombreEmpleado"].ToString();

                    // Fecha con validación especial
                    Tarea.hsCln_Fecha = dr["hsCln_Fecha"].ToString();

                    // Campos de texto con validación
                    Tarea.hsCln_Hora = dr["hsCln_Hora"].ToString();
                    Tarea.hsCln_Motivo = dr["hsCln_Motivo"].ToString();
                    Tarea.hsCln_Interv = dr["hsCln_Interv"].ToString();
                    Tarea.hsCln_Recom = dr["hsCln_Recom"].ToString();
                    Tarea.hsCln_Obs = dr["hsCln_Obs"].ToString();
                    Tarea.hsCln_Estado = dr["hsCln_Estado"].ToString();

                    // Campo entero con validación
                    Tarea.hsCln_Seguir = Convert.ToInt32(dr["hsCln_Seguir"].ToString());

                    // Los últimos campos que querías validar especialmente
                    Tarea.hsCln_Antece = dr["hsCln_Antece"].ToString();
                    Tarea.hsCln_SigVit = dr["hsCln_SigVit"].ToString();
                    Tarea.hsVul_Fecha = "";
                    Tarea.hsVul_Rec_Obs = "";
                    Tarea.hsVul_Vulnerds = "";
                    Tarea.hsVul_ciEmpleado = "";


                    listaTareas.Add(Tarea);
                }

            }
            catch (Exception ex)
            {
                //listaTareas = null;
                throw new Exception("Error en ConsultaSp_RTAListaHsCln: " + ex.Message);

            }
            finally
            {
                if (cmd != null && cmd.Connection != null)
                {
                    cmd.Connection.Close();
                }
            }

            return listaTareas;
        }

    }
}
