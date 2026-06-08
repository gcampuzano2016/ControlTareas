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
    public class DaoSolicitud
    {

        public static List<EntSolicitud> ConsultaSp_RTAListaSolicitud(int Tipo, string Cod_Jefe_Inm,int Pagina,string EstadoSolicitud,string FchIni,string FchFin, int TipoSolicitud,string usuario)
        {
            List<EntSolicitud> listaTareas = null;

            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("Sp_RTAListaSolicitud", cnx);
                cmd.Parameters.AddWithValue("@Tipo", Tipo);
                cmd.Parameters.AddWithValue("@Cod_Jefe_Inm", Cod_Jefe_Inm);
                cmd.Parameters.AddWithValue("@Pagina", Pagina);
                cmd.Parameters.AddWithValue("@EstadoSolicitud", EstadoSolicitud);
                cmd.Parameters.AddWithValue("@FchIni", FchIni);
                cmd.Parameters.AddWithValue("@FchFin", FchFin);
                cmd.Parameters.AddWithValue("@TipoSolicitud", TipoSolicitud);
                cmd.Parameters.AddWithValue("@Cod_Usuario", usuario);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                listaTareas = new List<EntSolicitud>();

                while (dr.Read())
                {
                    EntSolicitud Tarea = new EntSolicitud();

                    Tarea.IdVacaciones = Convert.ToInt32(dr["IdVacaciones"].ToString());
                    Tarea.Descripcion = dr["Descripcion"].ToString();
                    Tarea.FechaRegistro = dr["FechaRegistro"].ToString();
                    Tarea.Cedula = dr["Cedula"].ToString();
                    Tarea.Colaborador = dr["Colaborador"].ToString();
                    Tarea.Departamento = dr["Departamento"].ToString();
                    Tarea.JefeInmediato = dr["JefeInmediato"].ToString();
                    Tarea.EstadoSolicitud = dr["EstadoSolicitud"].ToString();
                    Tarea.FechaDesde = dr["FechaDesde"].ToString();
                    Tarea.FechaHasta = dr["FechaHasta"].ToString();
                    Tarea.TotalDias = Convert.ToDouble(dr["TotalDias"].ToString());
                    Tarea.Feriado = Convert.ToDouble(dr["Feriado"].ToString());
                    Tarea.Horas = dr["Horas"].ToString();
                    Tarea.Actividad = dr["Actividad"].ToString();
                    Tarea.FechaHasta = dr["FechaHasta"].ToString();
                    Tarea.FechaHasta = dr["FechaHasta"].ToString();
                    Tarea.SaldoDias = Convert.ToDouble(dr["SaldoDias"].ToString());
                    Tarea.MotivoAnulacion = dr["MotivoAnulacion"].ToString();
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
        public static EntSolicitud ConsultaSp_RTANotificarSolicitud(int Tipo, int IdVacaciones)
        {
            EntSolicitud Tarea = null;

            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("Sp_RTANotificarSolicitud", cnx);
                cmd.Parameters.AddWithValue("@Tipo", Tipo);
                cmd.Parameters.AddWithValue("@IdVacaciones", IdVacaciones);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();

                Tarea = new EntSolicitud();

                while (dr.Read())
                {
                    Tarea.IdVacaciones = Convert.ToInt32(dr["IdVacaciones"].ToString());
                    Tarea.FechaRegistro = dr["FechaRegistro"].ToString();
                    Tarea.Cedula = dr["Cedula"].ToString();
                    Tarea.Colaborador = dr["Colaborador"].ToString();
                    Tarea.Departamento = dr["Departamento"].ToString();
                    Tarea.JefeInmediato = dr["JefeInmediato"].ToString();
                    Tarea.EstadoSolicitud = dr["EstadoSolicitud"].ToString();
                    Tarea.FechaDesde = dr["FechaDesde"].ToString();
                    Tarea.FechaHasta = dr["FechaHasta"].ToString();
                    Tarea.TotalDias = Convert.ToDouble(dr["TotalDias"].ToString());
                    Tarea.Feriado = Convert.ToDouble(dr["Feriado"].ToString());
                    Tarea.Horas = dr["Horas"].ToString();
                    Tarea.Actividad = dr["Actividad"].ToString();
                    Tarea.FechaHasta = dr["FechaHasta"].ToString();
                    Tarea.FechaHasta = dr["FechaHasta"].ToString();
                    Tarea.SaldoDias = Convert.ToDouble(dr["SaldoDias"].ToString());
                    Tarea.Remplazo = dr["Remplazo"].ToString();
                    Tarea.StrCargoVacaciones = dr["CargoVacaciones"].ToString();
                    Tarea.Observacion = dr["Observacion"].ToString();
                    Tarea.Descripcion = dr["Descripcion"].ToString();
                    Tarea.IdTipoSolicitud = Convert.ToInt32(dr["IdTipoSolicitud"].ToString());
                    Tarea.MotivoAnulacion = dr["MotivoAnulacion"].ToString();
                    Tarea.Cod_Usuario = dr["Cod_Usuario"].ToString();
                }

            }
            catch (Exception ex)
            {
                Tarea = null;
            }
            finally
            {
                cmd.Connection.Close();
            }

            return Tarea;
        }
        public static EntRespuesta ConsultaSp_RTAListaSolicitudDescargar(int Tipo, string Cod_Jefe_Inm, int Pagina, string EstadoSolicitud, string FchIni, string FchFin, int TipoSolicitud)
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

                cmd = new SqlCommand("Sp_RTAListaSolicitud", cnx);
                cmd.Parameters.AddWithValue("@Tipo", Tipo);
                cmd.Parameters.AddWithValue("@Cod_Jefe_Inm", Cod_Jefe_Inm);
                cmd.Parameters.AddWithValue("@Pagina", Pagina);
                cmd.Parameters.AddWithValue("@EstadoSolicitud", EstadoSolicitud);
                cmd.Parameters.AddWithValue("@FchIni", FchIni);
                cmd.Parameters.AddWithValue("@FchFin", FchFin);
                cmd.Parameters.AddWithValue("@TipoSolicitud", TipoSolicitud);
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
        public static EntRespuesta RTA_InsertaNuevaSolicitud(EntSolicitud objEntSolicitud)
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
                cmd = new SqlCommand("Sp_RTAInsertaNuevaSolicitud", cnx);

                cmd.Parameters.AddWithValue("@IdVacaciones", objEntSolicitud.IdVacaciones);
                cmd.Parameters.AddWithValue("@IdTipoSolicitud", objEntSolicitud.IdTipoSolicitud);
                cmd.Parameters.AddWithValue("@FechaRegistro", Convert.ToDateTime(objEntSolicitud.FechaRegistro));
                cmd.Parameters.AddWithValue("@CodSap", objEntSolicitud.CodSap);
                cmd.Parameters.AddWithValue("@Cedula", objEntSolicitud.Cedula);
                cmd.Parameters.AddWithValue("@Colaborador", objEntSolicitud.Colaborador);
                cmd.Parameters.AddWithValue("@Departamento", objEntSolicitud.Departamento);
                cmd.Parameters.AddWithValue("@JefeInmediato", objEntSolicitud.JefeInmediato);
                cmd.Parameters.AddWithValue("@Remplazo", objEntSolicitud.Remplazo);

                if (objEntSolicitud.FechaDesde == "")
                {
                    cmd.Parameters.AddWithValue("@FechaDesde", Convert.ToDateTime("1900-01-01"));
                }
                else
                {
                    cmd.Parameters.AddWithValue("@FechaDesde", Convert.ToDateTime(objEntSolicitud.FechaDesde));
                }

                if (objEntSolicitud.FechaHasta == "")
                {
                    cmd.Parameters.AddWithValue("@FechaHasta", Convert.ToDateTime("1900-01-01"));
                }
                else
                {
                    cmd.Parameters.AddWithValue("@FechaHasta", Convert.ToDateTime(objEntSolicitud.FechaHasta));
                }

                cmd.Parameters.AddWithValue("@TotalDias", objEntSolicitud.TotalDias);
                cmd.Parameters.AddWithValue("@Feriado", objEntSolicitud.Feriado);
                cmd.Parameters.AddWithValue("@SaldoDias", objEntSolicitud.SaldoDias);
                cmd.Parameters.AddWithValue("@CargoVacaciones", objEntSolicitud.CargoVacaciones);
                cmd.Parameters.AddWithValue("@Horas", objEntSolicitud.Horas);
                cmd.Parameters.AddWithValue("@Actividad", objEntSolicitud.Actividad);
                cmd.Parameters.AddWithValue("@Observacion", objEntSolicitud.Observacion);
                cmd.Parameters.AddWithValue("@EstadoSolicitud", objEntSolicitud.EstadoSolicitud);
                if (objEntSolicitud.FechaAprobacion == "")
                {
                    cmd.Parameters.AddWithValue("@FechaAprobacion", Convert.ToDateTime("1900-01-01"));
                }
                else
                {
                    cmd.Parameters.AddWithValue("@FechaAprobacion", Convert.ToDateTime(objEntSolicitud.FechaAprobacion));
                }
                if (objEntSolicitud.FechaRechazo == "")
                {
                    cmd.Parameters.AddWithValue("@FechaRechazo", Convert.ToDateTime("1900-01-01"));
                }
                else
                {
                    cmd.Parameters.AddWithValue("@FechaRechazo", Convert.ToDateTime(objEntSolicitud.FechaRechazo));
                }
                cmd.Parameters.AddWithValue("@UsuarioAprobo", objEntSolicitud.UsuarioAprobo);
                cmd.Parameters.AddWithValue("@UsuarioRechazo", objEntSolicitud.UsuarioRechazo);
                cmd.Parameters.AddWithValue("@Cod_Usuario", objEntSolicitud.Cod_Usuario);
                cmd.Parameters.AddWithValue("@Tipo", objEntSolicitud.Tipo);
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
        public static EntRespuesta RTA_ActualizarSolicitud(EntSolicitud objEntSolicitud)
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
                cmd = new SqlCommand("Sp_RTActualizarSolicitud", cnx);
                cmd.Parameters.AddWithValue("@IdVacaciones", objEntSolicitud.IdVacaciones);
                cmd.Parameters.AddWithValue("@EstadoSolicitud", objEntSolicitud.EstadoSolicitud);
                cmd.Parameters.AddWithValue("@UsuarioAprobo", objEntSolicitud.UsuarioAprobo);
                cmd.Parameters.AddWithValue("@UsuarioRechazo", objEntSolicitud.UsuarioRechazo);
                cmd.Parameters.AddWithValue("@Cod_Usuario", objEntSolicitud.Cod_Usuario);
                cmd.Parameters.AddWithValue("@MotivoAnulacion", objEntSolicitud.MotivoAnulacion);
                cmd.Parameters.AddWithValue("@Tipo", objEntSolicitud.Tipo);
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
        public static int RTA_ActualizarRutaRide(EntSolicitud objEntSolicitud)
        {
            int Respuesta = 0;
            SqlCommand cmd = null;
            SqlDataReader dr = null;

            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cmd = new SqlCommand("Sp_RTAActualizarRutaRide", cnx);
                cmd.Parameters.AddWithValue("@IdVacaciones", objEntSolicitud.IdVacaciones);
                cmd.Parameters.AddWithValue("@Ruta_Archivo", objEntSolicitud.Ruta_Archivo);
                cmd.Parameters.AddWithValue("@Descripcion_Archivo", objEntSolicitud.Descripcion_Archivo);

                cmd.CommandType = CommandType.StoredProcedure;
                cnx.Open();
                dr = cmd.ExecuteReader();
                dr.Read();
                Respuesta = Convert.ToInt16(dr["Respuestas"].ToString());
            }
            catch (Exception ex)
            {
                Respuesta = 0;
            }
            finally
            {
                cmd.Connection.Close();
            }

            return Respuesta;

        }
        public static List<EntArchivoTarea> ListaArchivosSolicitudes(Int32 IdTarea)
        {

            List<EntArchivoTarea> listaArchivosTarea = null;
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();

                SqlConnection cnx = cn.conectar();
                cnx.Open();
                cmd = new SqlCommand("Sp_RTAListaArchivosSolicitud", cnx);
                cmd.Parameters.AddWithValue("@IdVacaciones", IdTarea);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                listaArchivosTarea = new List<EntArchivoTarea>();
                while (dr.Read())
                {
                    EntArchivoTarea archivoTarea = new EntArchivoTarea();

                    archivoTarea.Id_ArchivosTarea = Convert.ToInt32(dr["Id_ArchivosTarea"].ToString());
                    archivoTarea.Nombre_ArchivoCodigo = dr["Nombre_ArchivoCodigo"].ToString();
                    archivoTarea.Icon_Nombre = dr["Icon_Nombre"].ToString();
                    archivoTarea.Nombre_Archivo = dr["Nombre_Archivo"].ToString();
                    listaArchivosTarea.Add(archivoTarea);
                }
                //int prueba = 1;
            }
            catch (Exception ex)
            {
                listaArchivosTarea = null;
            }
            finally
            {
                cmd.Connection.Close();
            }
            return listaArchivosTarea;
        }

        public static EntRespuesta ConsultaSp_RTAListaSolicitudAct(int Tipo, string Cod_Jefe_Inm, int Pagina, string EstadoSolicitud, string FchIni, string FchFin, int TipoSolicitud)
        {
            EntRespuesta Respuesta = new EntRespuesta();
            Int32 respuestaSP = 0;
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            string respuestaSP1 = "";
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("Sp_RTAListaSolicitud", cnx);
                cmd.Parameters.AddWithValue("@Tipo", Tipo);
                cmd.Parameters.AddWithValue("@Cod_Jefe_Inm", Cod_Jefe_Inm);
                cmd.Parameters.AddWithValue("@Pagina", Pagina);
                cmd.Parameters.AddWithValue("@EstadoSolicitud", EstadoSolicitud);
                cmd.Parameters.AddWithValue("@FchIni", FchIni);
                cmd.Parameters.AddWithValue("@FchFin", FchFin);
                cmd.Parameters.AddWithValue("@TipoSolicitud", TipoSolicitud);
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
                else if(respuestaSP == 0)
                {
                    Respuesta.estado = "0";
                    Respuesta.mensaje = "No hay información en estado APROBADO para actualizar.";
                    Respuesta.tipoMensaje = "success";
                    Respuesta.resultado = respuestaSP.ToString();
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
