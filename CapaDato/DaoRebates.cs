using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using CapaEntidad;
using System.Globalization;

namespace CapaDato
{
    public class DaoRebates
    {
        public static EntRespuesta RTA_InsertaNuevoRebates(EntRebates objEntRebates)
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
                cmd = new SqlCommand("Sp_RTAInsertaNuevoRebates", cnx);

                cmd.Parameters.AddWithValue("@FECHA", Convert.ToDateTime(objEntRebates.FECHA));
                cmd.Parameters.AddWithValue("@ID_TRANSACCION", objEntRebates.ID_TRANSACCION);
                cmd.Parameters.AddWithValue("@PROGRAMA", objEntRebates.PROGRAMA);
                cmd.Parameters.AddWithValue("@TIPO_DE_INGRESO", objEntRebates.TIPO_DE_INGRESO);
                cmd.Parameters.AddWithValue("@PROCESO", objEntRebates.PROCESO);
                cmd.Parameters.AddWithValue("@MARCA", objEntRebates.MARCA);
                cmd.Parameters.AddWithValue("@DESCRIPCION", objEntRebates.DESCRIPCION);
                cmd.Parameters.AddWithValue("@VALOR", objEntRebates.VALOR);
                cmd.Parameters.AddWithValue("@Q_FABRICANTE", objEntRebates.Q_FABRICANTE);
                cmd.Parameters.AddWithValue("@TIPO_DE_PAGO", objEntRebates.TIPO_DE_PAGO);
                cmd.Parameters.AddWithValue("@ESTADO", objEntRebates.ESTADO);
                cmd.Parameters.AddWithValue("@ID_TRANSACCION1", objEntRebates.ID_TRANSACCION1);
                cmd.Parameters.AddWithValue("@ID_PAGO", objEntRebates.ID_PAGO);

                if (objEntRebates.FECHA_PAGO == "")
                {
                    cmd.Parameters.AddWithValue("@FECHA_PAGO", Convert.ToDateTime("1900-01-01"));
                }
                else
                {
                    cmd.Parameters.AddWithValue("@FECHA_PAGO", Convert.ToDateTime(objEntRebates.FECHA_PAGO));
                }
                if (objEntRebates.FECHA_ESTIMADA_PAGO == "")
                {
                    cmd.Parameters.AddWithValue("@FECHA_ESTIMADA_PAGO", Convert.ToDateTime("1900-01-01"));
                }
                else
                {
                    cmd.Parameters.AddWithValue("@FECHA_ESTIMADA_PAGO", Convert.ToDateTime(objEntRebates.FECHA_ESTIMADA_PAGO));
                }
                cmd.Parameters.AddWithValue("@OBSERVACIONES", objEntRebates.OBSERVACIONES);
                cmd.Parameters.AddWithValue("@RESPONSABLE", objEntRebates.RESPONSABLE);
                cmd.Parameters.AddWithValue("@IdRebates", objEntRebates.IdRebates);
                cmd.Parameters.AddWithValue("@IdBanco", objEntRebates.IdBanco);
                cmd.Parameters.AddWithValue("@Tipo", objEntRebates.Tipo);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                dr.Read();

                respuestaSP1 = dr["Respuestas"].ToString();
                if(respuestaSP1 == "")
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

        public static List<EntRebates> ConsultaSp_RTAListaRebetes(string FchIni, string FchFin, int IdTipoIngreso, int IdMarca, int IdPago,string estado, string  Anio, string meses, int idFecha)
        {
            List<EntRebates> listaTareas = null;

            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("Sp_RTAListaRebates", cnx);
                cmd.Parameters.AddWithValue("@FchIni", FchIni);
                cmd.Parameters.AddWithValue("@FchFin", FchFin);
                cmd.Parameters.AddWithValue("@IdTipoIngreso", IdTipoIngreso);
                cmd.Parameters.AddWithValue("@IdMarca", IdMarca);
                cmd.Parameters.AddWithValue("@IdPago", IdPago);
                cmd.Parameters.AddWithValue("@estado", estado);
                cmd.Parameters.AddWithValue("@Anio", Anio);
                cmd.Parameters.AddWithValue("@meses", meses);
                cmd.Parameters.AddWithValue("@idFecha", idFecha);

                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                listaTareas = new List<EntRebates>();

                while (dr.Read())
                {
                    EntRebates Tarea = new EntRebates();

                    Tarea.IdRebates = Convert.ToInt32(dr["IdRebates"].ToString());
                    //Tarea.IdTipoIngreso = Convert.ToInt32(dr["IdTipoIngreso"].ToString());
                    //Tarea.IdMarca = Convert.ToInt32(dr["IdMarca"].ToString());
                    //Tarea.IdPago = Convert.ToInt32(dr["IdPago"].ToString());
                    Tarea.FECHA = dr["FECHA"].ToString();
                    Tarea.ID_TRANSACCION = dr["ID_TRANSACCION"].ToString();
                    Tarea.PROGRAMA = dr["PROGRAMA"].ToString();
                    Tarea.TIPO_DE_INGRESO = dr["TIPO_DE_INGRESO"].ToString();
                    Tarea.PROCESO = dr["PROCESO"].ToString();
                    Tarea.MARCA = dr["MARCA"].ToString();
                    Tarea.DESCRIPCION = dr["DESCRIPCION"].ToString();
                    Tarea.VALOR = dr["VALOR"].ToString();
                    Tarea.Q_FABRICANTE = dr["Q_FABRICANTE"].ToString();
                    Tarea.TIPO_DE_PAGO = dr["TIPO_DE_PAGO"].ToString();
                    Tarea.ESTADO = dr["ESTADO"].ToString();
                    Tarea.ID_TRANSACCION1 = dr["ID_TRANSACCION1"].ToString();
                    Tarea.ID_PAGO = dr["ID_PAGO"].ToString();
                    Tarea.FECHA_PAGO = dr["FECHA_PAGO"].ToString();
                    Tarea.FECHA_ESTIMADA_PAGO = dr["FECHA_ESTIMADA_PAGO"].ToString();
                    Tarea.OBSERVACIONES = dr["OBSERVACIONES"].ToString();
                    Tarea.RESPONSABLE = dr["RESPONSABLE"].ToString();
                    Tarea.conteoArchivosAdjuntos = Convert.ToInt32(dr["conteoArchivosAdjuntos"].ToString());
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

        public static EntRespuesta ConsultaSp_RTAListaRebetesDescargar(string FchIni, string FchFin, int IdTipoIngreso, int IdMarca, int IdPago, string estado, string  Anio, string meses, int idFecha)
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

                cmd = new SqlCommand("Sp_RTAListaRebates", cnx);
                cmd.Parameters.AddWithValue("@FchIni", FchIni);
                cmd.Parameters.AddWithValue("@FchFin", FchFin);
                cmd.Parameters.AddWithValue("@IdTipoIngreso", IdTipoIngreso);
                cmd.Parameters.AddWithValue("@IdMarca", IdMarca);
                cmd.Parameters.AddWithValue("@IdPago", IdPago);
                cmd.Parameters.AddWithValue("@estado", estado);
                cmd.Parameters.AddWithValue("@Anio", Anio);
                cmd.Parameters.AddWithValue("@meses", meses);
                cmd.Parameters.AddWithValue("@idFecha", idFecha);

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

        public static List<EntRebates> ConsultaSp_RTAConsultarRebetes(int IdRebates, string orden, int tipo)
        {
            List<EntRebates> listaTareas = null;

            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("Sp_RTAConsultarRebates", cnx);
                cmd.Parameters.AddWithValue("@IdRebates", IdRebates);
                cmd.Parameters.AddWithValue("@orden", orden);
                cmd.Parameters.AddWithValue("@tipo", tipo);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                listaTareas = new List<EntRebates>();

                while (dr.Read())
                {
                    EntRebates Tarea = new EntRebates();
                    Tarea.IdRebates = Convert.ToInt32(dr["IdRebates"].ToString());
                    Tarea.IdTipoIngreso = Convert.ToInt32(dr["IdTipoIngreso"].ToString());
                    Tarea.IdMarca = Convert.ToInt32(dr["IdMarca"].ToString());
                    Tarea.IdPago = Convert.ToInt32(dr["IdPago"].ToString());
                    Tarea.FECHA = dr["FECHA"].ToString();
                    Tarea.ID_TRANSACCION = dr["ID_TRANSACCION"].ToString();
                    Tarea.PROGRAMA = dr["PROGRAMA"].ToString();
                    Tarea.TIPO_DE_INGRESO = dr["TIPO_DE_INGRESO"].ToString();
                    Tarea.PROCESO = dr["PROCESO"].ToString();
                    Tarea.MARCA = dr["MARCA"].ToString();
                    Tarea.DESCRIPCION = dr["DESCRIPCION"].ToString();
                    Tarea.VALOR = dr["VALOR"].ToString();
                    Tarea.Q_FABRICANTE = dr["Q_FABRICANTE"].ToString();
                    Tarea.TIPO_DE_PAGO = dr["TIPO_DE_PAGO"].ToString();
                    Tarea.ESTADO = dr["ESTADO"].ToString();
                    Tarea.ID_TRANSACCION1 = dr["ID_TRANSACCION1"].ToString();
                    Tarea.ID_PAGO = dr["ID_PAGO"].ToString();
                    Tarea.FECHA_PAGO = dr["FECHA_PAGO"].ToString();
                    Tarea.FECHA_ESTIMADA_PAGO = dr["FECHA_ESTIMADA_PAGO"].ToString();
                    Tarea.OBSERVACIONES = dr["OBSERVACIONES"].ToString();
                    Tarea.RESPONSABLE = dr["RESPONSABLE"].ToString();
                    Tarea.IdBanco = Convert.ToInt32(dr["IdBanco"].ToString());
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

        //BANCOS
        public static EntRespuesta RTA_InsertaNuevoBanco(EntRebates objEntRebates)
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
                cmd = new SqlCommand("Sp_RTAInsertaNuevoBanco", cnx);

                cmd.Parameters.AddWithValue("@IdBanco", Convert.ToInt32(objEntRebates.IdBanco));
                cmd.Parameters.AddWithValue("@Descripcion", objEntRebates.DescripcionB);
                cmd.Parameters.AddWithValue("@Valor", objEntRebates.ValorB);
                cmd.Parameters.AddWithValue("@FechaIngreso", objEntRebates.FechaIngreso);
                cmd.Parameters.AddWithValue("@Estado", objEntRebates.Estado);
                cmd.Parameters.AddWithValue("@Tipo", objEntRebates.Tipo);
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

        //BANCOS

        //PERIODO FISCAL
        public static EntRespuesta RTA_InsertaNuevoPeriodoFiscal(EntRebates objEntRebates)
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
                cmd = new SqlCommand("Sp_RTAInsertaNuevoPeriodoFiscal", cnx);

                cmd.Parameters.AddWithValue("@IdAnioFiscal", Convert.ToInt32(objEntRebates.IdAnioFiscal));
                cmd.Parameters.AddWithValue("@IdMarca", objEntRebates.IdMarca);
                cmd.Parameters.AddWithValue("@Descripcion", objEntRebates.DescripcionP);
                cmd.Parameters.AddWithValue("@FechaInicio", Convert.ToDateTime(objEntRebates.FechaInicio));
                cmd.Parameters.AddWithValue("@FechaFinal", Convert.ToDateTime(objEntRebates.FechaFinal));
                cmd.Parameters.AddWithValue("@FechaInicioDOS", Convert.ToDateTime(objEntRebates.FechaInicioDOS));
                cmd.Parameters.AddWithValue("@FechaFinalDOS", Convert.ToDateTime(objEntRebates.FechaFinalDOS));
                cmd.Parameters.AddWithValue("@Estado", objEntRebates.Estado);
                cmd.Parameters.AddWithValue("@Tipo", objEntRebates.Tipo);
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

        public static List<EntRebates> ConsultaSp_RTAConsultarPeriodoFiscal(int IdMarca, string FchIni, string FchFin, int IdAnioFiscal, int tipo)
        {
            List<EntRebates> listaTareas = null;

            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try 
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("Sp_RTAConsultarPeriodoFiscal", cnx);
                cmd.Parameters.AddWithValue("@IdMarca", IdMarca);
                cmd.Parameters.AddWithValue("@FchIni", FchIni);
                cmd.Parameters.AddWithValue("@FchFin", FchFin);
                cmd.Parameters.AddWithValue("@IdAnioFiscal", IdAnioFiscal);
                cmd.Parameters.AddWithValue("@tipo", tipo);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                listaTareas = new List<EntRebates>();

                while (dr.Read())
                {
                    EntRebates Tarea = new EntRebates();
                    Tarea.IdAnioFiscal = Convert.ToInt32(dr["IdAnioFiscal"].ToString());
                    Tarea.IdMarca = Convert.ToInt32(dr["IdMarca"].ToString());
                    Tarea.DESCRIPCION = dr["Descripcion"].ToString();
                    Tarea.FechaInicio = dr["FechaInicio"].ToString();
                    Tarea.FechaFinal = dr["FechaFinal"].ToString();
                    Tarea.FechaInicioDOS = dr["FechaInicioDOS"].ToString();
                    Tarea.FechaFinalDOS = dr["FechaFinalDOS"].ToString();
                    Tarea.MARCA = dr["Marca"].ToString();
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

        //PERIODO FISCAL

    }
}
