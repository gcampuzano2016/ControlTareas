using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CapaDato
{
    public class DaoVacaciones
    {
        /// <summary>
        /// Feriados que caen dentro del rango pedido. Reemplaza al campo que el
        /// colaborador llenaba a mano: un feriado dentro de las vacaciones no
        /// consume días, y ese cálculo no tiene por qué depender de que la
        /// persona se acuerde de los feriados del año.
        /// </summary>
        public static EntFeriadosRango ContarFeriadosRango(DateTime fechaDesde, DateTime fechaHasta)
        {
            EntFeriadosRango resultado = new EntFeriadosRango { Feriados = 0, AniosSinCargar = string.Empty };
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_ContarFeriadosRango", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@FechaDesde", SqlDbType.Date).Value = fechaDesde.Date;
                cmd.Parameters.Add("@FechaHasta", SqlDbType.Date).Value = fechaHasta.Date;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        resultado.Feriados = Convert.ToInt32(dr["Feriados"]);
                        resultado.AniosSinCargar = dr["AniosSinCargar"].ToString();
                    }
                }
            }

            return resultado;
        }

        public static EntRespuesta ConsultarVacaciones(int codSap)
        {
            EntRespuesta Respuesta = new EntRespuesta();
            DataTable dtResultados = new DataTable();
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectarSAP();
                cnx.Open();
                cmd = new SqlCommand("SELECT SUBSTRING(BEGDA,0,5)+'-'+SUBSTRING(ENDDA,0,5) AS PERIODO,SUM(ANZHL) DIASGENERADOS,SUM(KVERB) DIASTOMADOS,SUM(ANZHL)-SUM(KVERB) AS SALDO FROM [dcp].PA2006 WHERE CONVERT(INT, PERNR)=" + codSap + " AND ANZHL<>KVERB GROUP BY BEGDA,ENDDA", cnx);
                cmd.CommandType = CommandType.Text;
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
                if (cmd != null && cmd.Connection != null)
                {
                    cmd.Connection.Close();
                }
            }

            return Respuesta;
        }

        public static List<EntVacaciones> ConsultarSaldoVacaciones(string CodSap, int tipo)
        {
            List<EntVacaciones> listaTareas = null;

            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("Sp_RTAConsultarSaldoVacaciones", cnx);
                cmd.Parameters.AddWithValue("@Cod_Sap", CodSap);
                cmd.Parameters.AddWithValue("@tipo", tipo);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                listaTareas = new List<EntVacaciones>();

                while (dr.Read())
                {
                    EntVacaciones Tarea = new EntVacaciones();
                    if (tipo == 0)
                    {
                        Tarea.IdSaldos = Convert.ToInt32(dr["IdSaldos"].ToString());
                        Tarea.PERIODO = dr["PERIODO"].ToString();
                        Tarea.DIASGENERADOS = Convert.ToDouble(dr["DIASGENERADOS"].ToString());
                        Tarea.DIASTOMADOS = Convert.ToDouble(dr["DIASTOMADOS"].ToString());
                        Tarea.SALDO = Convert.ToDouble(dr["SALDO"].ToString());
                        listaTareas.Add(Tarea);
                    }
                    else if (tipo == 1)
                    {
                        Tarea.TOTAL = Convert.ToDouble(dr["TOTAL"].ToString());
                        listaTareas.Add(Tarea);
                    }

                }

            }
            catch (Exception ex)
            {
                listaTareas = null;
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
