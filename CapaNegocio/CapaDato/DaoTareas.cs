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
    public class DaoTareas
    {
    
        public static List<EntTareas> ListaTareas(string Idusuario)
        {
            List<EntTareas> listaTareas = null;
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();

                SqlConnection cnx = cn.conectar();
                cnx.Open();
                cmd = new SqlCommand("Sp_RTAListaTareas", cnx);
                cmd.Parameters.AddWithValue("@Idusuario", Idusuario);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                listaTareas = new List<EntTareas>();
                while (dr.Read())
                {
                    EntTareas Tarea = new EntTareas();
                    

                    Tarea.Id_RegTareas = Convert.ToInt32(dr["Id_RegTareas"].ToString());
                    Tarea.Num_OrdenServicio = dr["Num_OrdenServicio"].ToString();
                    Tarea.Id_CompAranda = dr["Id_CompAranda"].ToString();
                    Tarea.Fch_Registro = dr["Fch_Registro"].ToString();
                    Tarea.Fch_EstAtencion = dr["Fch_EstAtencion"].ToString();
                    Tarea.Fch_EstSolucion = dr["Fch_EstSolucion"].ToString();
                    Tarea.Id_Responsable = dr["Id_Responsable"].ToString();
                    Tarea.Nom_Responsable = dr["Nom_Responsable"].ToString();
                    Tarea.Nom_Empresa = dr["Nom_Empresa"].ToString();
                    Tarea.Nom_SlaAranda = dr["Nom_SlaAranda"].ToString();
                    Tarea.Det_Tarea = dr["Det_Tarea"].ToString();
                    Tarea.Estado = dr["Estado"].ToString();
                    Tarea.Categoria = dr["Categoria"].ToString();
                    Tarea.SubCategoria = dr["SubCategoria"].ToString();
                    Tarea.EstadoApro = dr["EstadoApro"].ToString();
                    Tarea.FechaAprobacion = dr["FechaAprobacion"].ToString();
                    Tarea.IdEstadoTarea = Convert.ToInt32(dr["IdEstadoTarea"].ToString());
                    Tarea.EstadoTarea = dr["EstadoTarea"].ToString();
                    Tarea.CatalogoTareaSap = dr["CatalogoTareaSap"].ToString(); 
                    Tarea.TareaEnEjecucion = Convert.ToInt32(dr["TareaEnEjecucion"].ToString());
                    Tarea.conteoArchivosAdjuntos = Convert.ToInt32(dr["conteoArchivosAdjuntos"].ToString());

                    Tarea.IdCategoria = Convert.ToInt32(dr["IdCategoria"].ToString());
                    Tarea.DetCategoria = dr["DetCategoria"].ToString();

                    listaTareas.Add(Tarea);
                }
                //int prueba = 1;
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

        public static List<EntArchivoTarea> ListaArchivosContrato(Int32 IdTarea)
        {

            List<EntArchivoTarea> listaArchivosTarea = null;
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();

                SqlConnection cnx = cn.conectar();
                cnx.Open();
                cmd = new SqlCommand("Sp_RTAListaArchivosContrato", cnx);
                cmd.Parameters.AddWithValue("@Id_RegTareas", IdTarea);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                listaArchivosTarea = new List<EntArchivoTarea>();
                while (dr.Read())
                {
                    EntArchivoTarea archivoTarea = new EntArchivoTarea();

                    archivoTarea.Id_ArchivosTarea = Convert.ToInt32(dr["Id_ArchivosTarea"].ToString());
                    archivoTarea.Nombre_Archivo = dr["Nombre_Archivo"].ToString();
                    archivoTarea.Extension_Archivo = dr["Extension_Archivo"].ToString();
                    archivoTarea.Codigo_Archivo = dr["Codigo_Archivo"].ToString();
                    archivoTarea.Nombre_ArchivoCodigo = dr["Nombre_ArchivoCodigo"].ToString();
                    archivoTarea.Ruta_Archivo = dr["Ruta_Archivo"].ToString();
                    archivoTarea.Descripcion_Archivo = dr["Descripcion_Archivo"].ToString();
                    archivoTarea.Id_RegDetTareas = Convert.ToInt32(dr["Id_RegDetTareas"].ToString());
                    archivoTarea.Id_RegTareas = Convert.ToInt32(dr["Id_RegTareas"].ToString());
                    archivoTarea.Orden_Archivo = Convert.ToInt32(dr["Orden_Archivo"].ToString());
                    archivoTarea.Icon_Nombre = dr["Icon_Nombre"].ToString();
                    archivoTarea.Fec_Modificacion = dr["Fec_Modificacion"].ToString();
                    archivoTarea.Usu_Modificacion = Convert.ToInt32(dr["Usu_Modificacion"].ToString());
                    archivoTarea.Ip_Modificacion = dr["Ip_Modificacion"].ToString();

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

        public static List<EntArchivoTarea> ListaArchivosTareas(Int32 IdTarea, Int32 idServicio)
        {
            
            List<EntArchivoTarea> listaArchivosTarea = null;
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();

                SqlConnection cnx = cn.conectar();
                cnx.Open();
                cmd = new SqlCommand("Sp_RTAListaArchivosTarea", cnx);
                cmd.Parameters.AddWithValue("@Id_RegTareas", IdTarea);
                cmd.Parameters.AddWithValue("@idServicio", idServicio);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                listaArchivosTarea = new List<EntArchivoTarea>();
                while (dr.Read())
                {
                    EntArchivoTarea archivoTarea = new EntArchivoTarea();

                    archivoTarea.Id_ArchivosTarea = Convert.ToInt32(dr["Id_ArchivosTarea"].ToString()); 
                    archivoTarea.Nombre_Archivo = dr["Nombre_Archivo"].ToString();
                    archivoTarea.Extension_Archivo = dr["Extension_Archivo"].ToString();
                    archivoTarea.Codigo_Archivo = dr["Codigo_Archivo"].ToString();
                    archivoTarea.Nombre_ArchivoCodigo = dr["Nombre_ArchivoCodigo"].ToString();
                    archivoTarea.Ruta_Archivo = dr["Ruta_Archivo"].ToString();
                    archivoTarea.Descripcion_Archivo = dr["Descripcion_Archivo"].ToString();
                    archivoTarea.Id_RegDetTareas = Convert.ToInt32(dr["Id_RegDetTareas"].ToString());
                    archivoTarea.Id_RegTareas = Convert.ToInt32(dr["Id_RegTareas"].ToString());
                    archivoTarea.Orden_Archivo = Convert.ToInt32(dr["Orden_Archivo"].ToString());
                    archivoTarea.Icon_Nombre = dr["Icon_Nombre"].ToString();
                    archivoTarea.Fec_Modificacion = dr["Fec_Modificacion"].ToString();
                    archivoTarea.Usu_Modificacion = Convert.ToInt32(dr["Usu_Modificacion"].ToString());
                    archivoTarea.Ip_Modificacion = dr["Ip_Modificacion"].ToString();

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


        public static List<EntTareas> ListaTareasFechas(string Idusuario, string FchIni, string FchFin, int Tipo)
        {
            List<EntTareas> listaTareas = null;

            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("Sp_RTAListaTareasFechas", cnx);
                cmd.Parameters.AddWithValue("@Idusuario", Idusuario);
                cmd.Parameters.AddWithValue("@FchIni", FchIni);
                cmd.Parameters.AddWithValue("@FchFin", FchFin);
                cmd.Parameters.AddWithValue("@Tipo", Tipo);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                listaTareas = new List<EntTareas>();

                while (dr.Read())
                {
                    EntTareas Tarea = new EntTareas();

                    Tarea.Id_RegTareas = Convert.ToInt32(dr["Id_RegTareas"].ToString());
                    Tarea.Num_OrdenServicio = dr["Num_OrdenServicio"].ToString();
                    Tarea.Id_CompAranda = dr["Id_CompAranda"].ToString();
                    Tarea.Fch_Registro = dr["Fch_Registro"].ToString();
                    Tarea.Fch_EstAtencion = dr["Fch_EstAtencion"].ToString();
                    Tarea.Fch_EstSolucion = dr["Fch_EstSolucion"].ToString();
                    Tarea.Id_Responsable = dr["Id_Responsable"].ToString();
                    Tarea.Nom_Responsable = dr["Nom_Responsable"].ToString();
                    Tarea.Nom_Empresa = dr["Nom_Empresa"].ToString();
                    Tarea.Nom_SlaAranda = dr["Nom_SlaAranda"].ToString();
                    Tarea.Det_Tarea = dr["Det_Tarea"].ToString();
                    Tarea.Estado = dr["Estado"].ToString();
                    Tarea.Categoria = dr["Categoria"].ToString();
                    Tarea.SubCategoria = dr["SubCategoria"].ToString();
                    Tarea.EstadoApro = dr["EstadoApro"].ToString();
                    Tarea.FechaAprobacion = dr["FechaAprobacion"].ToString();
                    Tarea.IdEstadoTarea = Convert.ToInt32(dr["IdEstadoTarea"].ToString());
                    Tarea.EstadoTarea = dr["EstadoTarea"].ToString();
                    Tarea.CatalogoTareaSap = dr["CatalogoTareaSap"].ToString();


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


        public static List<EntTareas> ConsultaHorasExtrasPorAutorizar(string FchIni, string FchFin, string IdResponsable,int Det_Horas_Extras_Estado,string IdUsuarioJefe)
        {
            List<EntTareas> listaTareas = null;

            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("Sp_RTAConsultaHorasExtrasPorAutorizar", cnx);
                cmd.Parameters.AddWithValue("@FchIni", FchIni);
                cmd.Parameters.AddWithValue("@FchFin", FchFin);
                cmd.Parameters.AddWithValue("@IdResponsable", IdResponsable);
                cmd.Parameters.AddWithValue("@Det_Horas_Extras_Estado", Det_Horas_Extras_Estado);
                cmd.Parameters.AddWithValue("@IdUsuarioJefe", IdUsuarioJefe);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                listaTareas = new List<EntTareas>();

                while (dr.Read())
                {
                    EntTareas Tarea = new EntTareas();

                    Tarea.Id_RegTareas = Convert.ToInt32(dr["Id_RegDetTareas"].ToString());
                    Tarea.Num_OrdenServicio = dr["Det_Num_OrdenServicio"].ToString();
                    Tarea.Id_CompAranda = dr["Det_Id_CompAranda"].ToString();
                    Tarea.Nom_Empresa = dr["Det_Nom_Empresa"].ToString();
                    Tarea.Det_Tarea = dr["Det_Det_Tarea"].ToString();
                    Tarea.Nom_Responsable = dr["Nom_Usuario"].ToString();
                    Tarea.Det_Tiempo = dr["Det_Tiempo"].ToString();
                    Tarea.TipoHoras = dr["TipoHoras"].ToString();
                    Tarea.Det_Fch_RegDetalleIni = dr["Det_Fch_RegDetalleIni"].ToString();
                    Tarea.Det_Fch_RegDetalleFin = dr["Det_Fch_RegDetalleFin"].ToString();
                    Tarea.FechaRegistro = dr["FechaRegistro"].ToString();
                    Tarea.EstadoAprobacion = dr["EstadoAprobacion"].ToString();
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
        public static EntRespuesta ConsultaHorasExtrasPorAutorizarDescargar(string FchIni, string FchFin, string IdResponsable, int Det_Horas_Extras_Estado, string IdUsuarioJefe)
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

                cmd = new SqlCommand("Sp_RTAConsultaHorasExtrasPorAutorizar", cnx);
                cmd.Parameters.AddWithValue("@FchIni", FchIni);
                cmd.Parameters.AddWithValue("@FchFin", FchFin);
                cmd.Parameters.AddWithValue("@IdResponsable", IdResponsable);
                cmd.Parameters.AddWithValue("@Det_Horas_Extras_Estado", Det_Horas_Extras_Estado);
                cmd.Parameters.AddWithValue("@IdUsuarioJefe", IdUsuarioJefe);
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


        public static List<EntTareas> ConsultaTareasGeneradas(string FchIni, string FchFin, int Tipo, int Id_RegTareas,string registro,string busqueda)
        {
            List<EntTareas> listaTareas = null;

            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("Sp_RTAConsultaTareasGeneradas", cnx);
                cmd.Parameters.AddWithValue("@FchIni", FchIni);
                cmd.Parameters.AddWithValue("@FchFin", FchFin);
                cmd.Parameters.AddWithValue("@Tipo", Tipo);
                cmd.Parameters.AddWithValue("@Id_RegTareas", Id_RegTareas);
                cmd.Parameters.AddWithValue("@registro", registro);
                cmd.Parameters.AddWithValue("@busqueda", busqueda);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                listaTareas = new List<EntTareas>();

                while (dr.Read())
                {
                    EntTareas Tarea = new EntTareas();

                    Tarea.Id_RegTareas = Convert.ToInt32(dr["Id_RegTareas"].ToString());
                    Tarea.Num_OrdenServicio = dr["Num_OrdenServicio"].ToString();
                    Tarea.Id_CompAranda = dr["Id_CompAranda"].ToString();
                    Tarea.Det_Fch_RegDetalleIni = dr["Det_Fch_RegDetalleIni"].ToString();
                    Tarea.Det_Fch_RegDetalleFin = dr["Det_Fch_RegDetalleFin"].ToString();
                    Tarea.Nom_Responsable = dr["Nom_Responsable"].ToString();
                    Tarea.Nom_Empresa = dr["Nom_Empresa"].ToString();
                    Tarea.Nom_SlaAranda = dr["Nom_SlaAranda"].ToString();
                    Tarea.Det_Tarea = dr["Det_Tarea"].ToString();
                    Tarea.EstadoTarea = dr["EstadoTarea"].ToString();
                    Tarea.Det_Tiempo = dr["Det_Tiempo"].ToString();
                    Tarea.Tipos = dr["Tipo"].ToString();
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

        public static EntRespuesta ConsultaTareasGeneradasDescargar(string FchIni, string FchFin, int Tipo, int Id_RegTareas, string registro, string busqueda)
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

                cmd = new SqlCommand("Sp_RTAConsultaTareasGeneradas", cnx);
                cmd.Parameters.AddWithValue("@FchIni", FchIni);
                cmd.Parameters.AddWithValue("@FchFin", FchFin);
                cmd.Parameters.AddWithValue("@Tipo", Tipo);
                cmd.Parameters.AddWithValue("@Id_RegTareas", Id_RegTareas);
                cmd.Parameters.AddWithValue("@registro", registro);
                cmd.Parameters.AddWithValue("@busqueda", busqueda);
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


        public static List<EntTareas> ConsultaTareasGeneradasContrato(string FchIni, string FchFin, int Tipo, int Id_RegTareas, string registro, string numeroOS,int idCliente,int idProceso)
        {
            List<EntTareas> listaTareas = null;

            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("Sp_RTAConsultaTareasGeneradasContrato", cnx);
                cmd.Parameters.AddWithValue("@FchIni", FchIni);
                cmd.Parameters.AddWithValue("@FchFin", FchFin);
                cmd.Parameters.AddWithValue("@Tipo", Tipo);
                cmd.Parameters.AddWithValue("@Id_RegTareas", Id_RegTareas);
                cmd.Parameters.AddWithValue("@registro", registro);
                cmd.Parameters.AddWithValue("@numeroOS", numeroOS);
                cmd.Parameters.AddWithValue("@idCliente", idCliente);
                cmd.Parameters.AddWithValue("@idProceso", idProceso);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                listaTareas = new List<EntTareas>();

                while (dr.Read())
                {
                    EntTareas Tarea = new EntTareas();

                    Tarea.Id_RegTareas = Convert.ToInt32(dr["Id_RegTareas"].ToString());
                    Tarea.Num_OrdenServicio = dr["Num_OrdenServicio"].ToString();
                    Tarea.Id_CompAranda = dr["Id_CompAranda"].ToString();
                    Tarea.Det_Fch_RegDetalleIni = dr["Det_Fch_RegDetalleIni"].ToString();
                    Tarea.Det_Fch_RegDetalleFin = dr["Det_Fch_RegDetalleFin"].ToString();
                    Tarea.Nom_Responsable = dr["Nom_Responsable"].ToString();
                    Tarea.Nom_Empresa = dr["Nom_Empresa"].ToString();
                    Tarea.Nom_SlaAranda = dr["Nom_SlaAranda"].ToString();
                    Tarea.Det_Tarea = dr["Det_Tarea"].ToString();
                    Tarea.EstadoTarea = dr["EstadoTarea"].ToString();
                    Tarea.Det_Tiempo = dr["Det_Tiempo"].ToString();
                    Tarea.Tipos = dr["Tipo"].ToString();
                    Tarea.conteoArchivosAdjuntos = Convert.ToInt32(dr["conteoArchivosAdjuntos"].ToString());
                    Tarea.Proceso = dr["Proceso"].ToString();
                    Tarea.NombreCliente = dr["Servicio"].ToString();
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

        public static List<EntTareas> ConsultaHorasContratadas(string FchIni, string FchFin, int Tipo, int Id_RegTareas, string registro, string numeroOS, int idCliente, int idProceso)
        {
            List<EntTareas> listaTareas = null;

            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("Sp_RTAConsultaHorasContratadas", cnx);
                cmd.Parameters.AddWithValue("@FchIni", FchIni);
                cmd.Parameters.AddWithValue("@FchFin", FchFin);
                cmd.Parameters.AddWithValue("@Tipo", Tipo);
                cmd.Parameters.AddWithValue("@Id_RegTareas", Id_RegTareas);
                cmd.Parameters.AddWithValue("@registro", registro);
                cmd.Parameters.AddWithValue("@numeroOS", numeroOS);
                cmd.Parameters.AddWithValue("@idCliente", idCliente);
                cmd.Parameters.AddWithValue("@idProceso", idProceso);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                listaTareas = new List<EntTareas>();

                while (dr.Read())
                {
                    EntTareas Tarea = new EntTareas();

                    Tarea.HORAS_CONTRATADAS = dr["HORAS_CONTRATADAS"].ToString();
                    Tarea.HORAS_ENTREGADAS = dr["HORAS_ENTREGADAS"].ToString();
                    Tarea.HORAS_DISPONIBLES = dr["HORAS_DISPONIBLES"].ToString();
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


        public static EntRespuesta ConsultaTareasGeneradasContratoDescargar(string FchIni, string FchFin, int Tipo, int Id_RegTareas, string registro, string numeroOS,int idCliente, int idProceso)
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

                cmd = new SqlCommand("Sp_RTAConsultaTareasGeneradasContrato", cnx);
                cmd.Parameters.AddWithValue("@FchIni", FchIni);
                cmd.Parameters.AddWithValue("@FchFin", FchFin);
                cmd.Parameters.AddWithValue("@Tipo", Tipo);
                cmd.Parameters.AddWithValue("@Id_RegTareas", Id_RegTareas);
                cmd.Parameters.AddWithValue("@registro", registro);
                cmd.Parameters.AddWithValue("@numeroOS", numeroOS);
                cmd.Parameters.AddWithValue("@idCliente", idCliente);
                cmd.Parameters.AddWithValue("@idProceso", idProceso);
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

        public static EntRespuesta ObtenerReporteGPFGeneradasDescargar(string FchIni, string FchFin,int idtipo)
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

                cmd = new SqlCommand("Sp_RTAReporteGPF", cnx);
                cmd.Parameters.AddWithValue("@FchIni", FchIni);
                cmd.Parameters.AddWithValue("@FchFin", FchFin);
                cmd.Parameters.AddWithValue("@idtipo", idtipo);
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
        public static EntTareas RTA_ConsultaTareaRTA(Int32 IdTarea)
        {

            EntTareas objTarea = null;// new entUsuario();
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();
                cmd = new SqlCommand("Sp_RTAConsultaTarea", cnx);
                cmd.Parameters.AddWithValue("@IdTarea", IdTarea);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                objTarea = new EntTareas();
                dr.Read();

                objTarea.Id_RegTareas = Convert.ToInt32(dr["Id_RegTareas"].ToString());
                objTarea.Num_OrdenServicio = dr["Num_OrdenServicio"].ToString();
                objTarea.Id_CompAranda = dr["Id_CompAranda"].ToString();
                objTarea.Fch_Registro = dr["Fch_Registro"].ToString();
                objTarea.Fch_EstAtencion = dr["Fch_EstAtencion"].ToString();
                objTarea.Fch_EstSolucion = dr["Fch_EstSolucion"].ToString();
                objTarea.Id_Responsable = dr["Id_Responsable"].ToString();
                objTarea.Nom_Responsable = dr["Nom_Responsable"].ToString();
                objTarea.Nom_Empresa = dr["Nom_Empresa"].ToString();
                objTarea.Nom_SlaAranda = dr["Nom_SlaAranda"].ToString();
                objTarea.Det_Tarea = dr["Det_Tarea"].ToString();
                objTarea.Estado = dr["Estado"].ToString();
                objTarea.EstadoApro = dr["EstadoApro"].ToString();
                objTarea.IdEstado = Convert.ToInt32(dr["IdEstado"].ToString());
                objTarea.IdEstadoAprobacion = Convert.ToInt32(dr["IdEstadoAprobacion"].ToString());
                objTarea.EstadoTarea = dr["EstadoTarea"].ToString();
                objTarea.NombreCliente = dr["NombreCliente"].ToString();

                objTarea.IdProyecto = Convert.ToInt32(dr["IdProyecto"].ToString()); //reversar

                //resultado = dr["Respuesta"].ToString();
            }
            catch (Exception ex)
            {
                objTarea = null;

            }
            finally
            {
                cmd.Connection.Close();
            }
            return objTarea;
        }

        public static EntTareas RTAConsultaNumeroOrden(string Num_OrdenServicio)
        {

            EntTareas objTarea = null;// new entUsuario();
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();
                cmd = new SqlCommand("Sp_RTAConsultaNumeroOrden", cnx);
                cmd.Parameters.AddWithValue("@Num_OrdenServicio", Num_OrdenServicio);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                objTarea = new EntTareas();
                dr.Read();
                objTarea.Valor = dr["Valor"].ToString();
            }
            catch (Exception ex)
            {
                objTarea = null;

            }
            finally
            {
                cmd.Connection.Close();
            }
            return objTarea;
        }


        public static EntDetalleTarea RTA_ConsultaDetalleTareaRTA(int IdDetalleTarea)
        {

            EntDetalleTarea objDetalleTarea = null;
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();
                cmd = new SqlCommand("Sp_RTAConsultaDetalleTarea", cnx);
                cmd.Parameters.AddWithValue("@IdDetalleTarea", IdDetalleTarea);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                objDetalleTarea = new EntDetalleTarea();

                while (dr.Read())
                {
                    objDetalleTarea.Id_RegDetTareas = Convert.ToInt32(dr["Id_RegDetTareas"].ToString());
                    objDetalleTarea.Id_RegTareas = Convert.ToInt32(dr["Id_RegTareas"].ToString());
                    objDetalleTarea.Det_Num_OrdenServicio = dr["Det_Num_OrdenServicio"].ToString();
                    objDetalleTarea.Det_Id_CompAranda = dr["Det_Id_CompAranda"].ToString();
                    objDetalleTarea.Det_Fch_RegDetalleIni = dr["Det_Fch_RegDetalleIni"].ToString();
                    objDetalleTarea.Det_Fch_RegDetalleFin = dr["Det_Fch_RegDetalleFin"].ToString();
                    objDetalleTarea.Det_EstadoIni = dr["Det_EstadoIni"].ToString();
                    objDetalleTarea.Det_EstadoFin = dr["Det_EstadoFin"].ToString();
                    objDetalleTarea.Det_Tiempo = dr["Det_Tiempo"].ToString();
                    objDetalleTarea.Det_Nom_Empresa = dr["Det_Nom_Empresa"].ToString();
                    objDetalleTarea.Det_Det_Tarea = dr["Det_Det_Tarea"].ToString();
                    objDetalleTarea.Det_Tarea = dr["Det_Tarea"].ToString(); 
                    objDetalleTarea.Det_Estado = dr["Det_Estado"].ToString();
                    objDetalleTarea.Id_Responsable = dr["Id_Responsable"].ToString();
                    objDetalleTarea.Det_Det_TareaFin = dr["Det_Det_TareaFin"].ToString();
                    objDetalleTarea.Det_Motivo_Cambio_Estado = Convert.ToString(dr["Det_Motivo_Cambio_Estado"].ToString());
                    objDetalleTarea.IdDet_EstadoFin = Convert.ToInt32(dr["IdDet_EstadoFin"].ToString());
                    objDetalleTarea.IdDet_EstadoIni = Convert.ToInt32(dr["IdDet_EstadoIni"].ToString());
                    objDetalleTarea.Det_Observaciones = dr["Det_Observaciones"].ToString();
                    objDetalleTarea.Det_Horas_Extras_Estado = Convert.ToInt32(dr["Det_Horas_Extras_Estado"].ToString());
                    objDetalleTarea.Det_Horas_Extras_Tipo = Convert.ToInt32(dr["Det_Horas_Extras_Tipo"].ToString());
                    objDetalleTarea.Det_Horas_Extras_Descripcion = dr["Det_Horas_Extras_Descripcion"].ToString();
                    objDetalleTarea.Det_Horas_Extras_Envio_Correo = Convert.ToInt32(dr["Det_Horas_Extras_Envio_Correo"].ToString());
                    objDetalleTarea.Cod_CatalogoTareaSap = Convert.ToInt32(dr["Cod_CatalogoTareaSap"].ToString());
                    objDetalleTarea.Det_Aprobacion_Tarea_Estado = Convert.ToInt32(dr["Det_Aprobacion_Tarea_Estado"].ToString());
                    objDetalleTarea.Det_Fecha_Aprobacion_Tarea = dr["Det_Fecha_Aprobacion_Tarea"].ToString();
                    objDetalleTarea.Det_Aprobacion_Tarea_Estado_Descripcion = dr["Det_Aprobacion_Tarea_Estado_Descripcion"].ToString(); 
                    objDetalleTarea.Det_Horas_Extras_Fecha_Solicitud = dr["Det_Horas_Extras_Fecha_Solicitud"].ToString();
                    objDetalleTarea.Det_Horas_Extras_Fecha_Aprobacion = dr["Det_Horas_Extras_Fecha_Aprobacion"].ToString();
                    objDetalleTarea.Det_Aprobacion_Tarea_Estado_QA = Convert.ToInt32(dr["Det_Aprobacion_Tarea_Estado_QA"].ToString());
                    objDetalleTarea.Det_Aprobacion_Tarea_Estado_QA_Descripcion = dr["Det_Aprobacion_Tarea_Estado_QA_Descripcion"].ToString();
                    objDetalleTarea.Det_Fecha_Aprobacion_Tarea_QA = dr["Det_Fecha_Aprobacion_Tarea_QA"].ToString();
                    objDetalleTarea.IdTipoGasto = Convert.ToInt32(dr["IdTipoGasto"].ToString());
                }
            }
            catch (Exception ex)
            {
                objDetalleTarea = null;

            }
            finally
            {
                cmd.Connection.Close();
            }
            return objDetalleTarea;
        }

        

        public static int RTA_IngresaHisTarea(EntHisTarea objHisTarea)
        {
            int Respuesta = 0;
            SqlCommand cmd = null;
            SqlDataReader dr = null;

            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cmd = new SqlCommand("Sp_RTAInsertaHisTarea", cnx);

                cmd.Parameters.AddWithValue("@Id_RegTareas", objHisTarea.Id_RegTareas);
                cmd.Parameters.AddWithValue("@Det_Num_OrdenServicio", objHisTarea.Det_Num_OrdenServicio);
                cmd.Parameters.AddWithValue("@Det_Id_CompAranda", objHisTarea.Det_Id_CompAranda);
                cmd.Parameters.AddWithValue("@Det_Fch_RegDetalleIni", objHisTarea.Det_Fch_RegDetalleIni);
                cmd.Parameters.AddWithValue("@Det_Fch_RegDetalleFin", objHisTarea.Det_Fch_RegDetalleFin);
                cmd.Parameters.AddWithValue("@Det_EstadoIni", objHisTarea.Det_EstadoIni);
                cmd.Parameters.AddWithValue("@Det_EstadoFin", objHisTarea.Det_EstadoFin);
                cmd.Parameters.AddWithValue("@Det_Nom_Empresa", objHisTarea.Det_Nom_Empresa);
                cmd.Parameters.AddWithValue("@Det_Det_Tarea", objHisTarea.Det_Det_Tarea);
                cmd.Parameters.AddWithValue("@Det_Estado", objHisTarea.Det_Estado);
                cmd.Parameters.AddWithValue("@Det_Motivo_Cambio_Estado", objHisTarea.Det_Motivo_Cambio_Estado );
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

        public static EntRespuesta RTA_ActualizarRequerimiento(EntTareas objTarea)
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

                cmd = new SqlCommand("Sp_RTA_ActualizarRequerimiento", cnx);

                cmd.Parameters.AddWithValue("@Solucion_Tarea", objTarea.EstadoTarea);
                cmd.Parameters.AddWithValue("@Id_RegTareas", objTarea.Id_RegTareas);
                cmd.Parameters.AddWithValue("@Tipo", objTarea.Tipo);
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

        }


        public static EntRespuesta RTA_ActualizaEstadoTarea(EntTareas objTarea)
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

                cmd = new SqlCommand("Sp_RTAActualizaEstadoTarea", cnx);

                cmd.Parameters.AddWithValue("@Id_RegTareas", objTarea.Id_RegTareas);
                cmd.Parameters.AddWithValue("@Estado", objTarea.Estado);
                cmd.Parameters.AddWithValue("@IdEstadoTarea", objTarea.IdEstadoTarea);
                cmd.Parameters.AddWithValue("@EstadoTarea", objTarea.EstadoTarea);
                cmd.Parameters.AddWithValue("@Id_Responsable", objTarea.Id_Responsable);
                cmd.Parameters.AddWithValue("@Det_Id_Usuario_Modificacion", objTarea.Det_Id_Usuario_Modificacion);
                cmd.Parameters.AddWithValue("@Det_Ip_Modificacion", objTarea.Det_Ip_Modificacion);
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

        }



        public static int RTA_IngresaHisTareaHorasExtra(EntHisTarea objHisTarea)
        {
            int Respuesta = 0;
            SqlCommand cmd = null;
            SqlDataReader dr = null;

            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cmd = new SqlCommand("Sp_RTAInsertaHisTareaHorasExtra", cnx);

                cmd.Parameters.AddWithValue("@Id_RegTareas", objHisTarea.Id_RegTareas);
                cmd.Parameters.AddWithValue("@Det_Num_OrdenServicio", objHisTarea.Det_Num_OrdenServicio);
                cmd.Parameters.AddWithValue("@Det_Id_CompAranda", objHisTarea.Det_Id_CompAranda);
                cmd.Parameters.AddWithValue("@Det_Fch_RegDetalleIni", objHisTarea.Det_Fch_RegDetalleIni);
                cmd.Parameters.AddWithValue("@Det_Fch_RegDetalleFin", objHisTarea.Det_Fch_RegDetalleFin);
                cmd.Parameters.AddWithValue("@Det_EstadoIni", objHisTarea.Det_EstadoIni);
                cmd.Parameters.AddWithValue("@Det_EstadoFin", objHisTarea.Det_EstadoFin);
                cmd.Parameters.AddWithValue("@Det_Nom_Empresa", objHisTarea.Det_Nom_Empresa);
                cmd.Parameters.AddWithValue("@Det_Det_Tarea", objHisTarea.Det_Det_Tarea);
                cmd.Parameters.AddWithValue("@Det_Estado", objHisTarea.Det_Estado);
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


        public static int RTA_ActualizaHisTarea(EntHisTarea objHisTarea)
        {
            int Respuesta = 0;
            SqlCommand cmd = null;
            SqlDataReader dr = null;

            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cmd = new SqlCommand("Sp_RTAActualizaHisTarea", cnx);

                cmd.Parameters.AddWithValue("@Id_RegTareas", objHisTarea.Id_RegTareas);
                cmd.Parameters.AddWithValue("@Det_Fch_RegDetalleFin", objHisTarea.Det_Fch_RegDetalleFin);
                cmd.Parameters.AddWithValue("@Det_EstadoFin", objHisTarea.Det_EstadoFin);
                cmd.Parameters.AddWithValue("@Det_Det_Tarea", objHisTarea.Det_Det_Tarea);
                cmd.Parameters.AddWithValue("@Det_Estado", objHisTarea.Det_Estado);
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

        public static EntRespuesta RTA_ActualizaDetalleTarea(EntDetalleTarea objDetalleTarea, string IdUsuarioSession, string IpCliente)
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

                cmd = new SqlCommand("Sp_RTAActualizaDetalleTarea", cnx);

                cmd.Parameters.AddWithValue("@Id_RegDetTareas", objDetalleTarea.Id_RegDetTareas);
                cmd.Parameters.AddWithValue("@Id_RegTareas", objDetalleTarea.Id_RegTareas);
                cmd.Parameters.AddWithValue("@Det_Num_OrdenServicio", objDetalleTarea.Det_Num_OrdenServicio);
				cmd.Parameters.AddWithValue("@Det_Id_CompAranda", objDetalleTarea.Det_Id_CompAranda);
                cmd.Parameters.AddWithValue("@Det_Det_Tarea", objDetalleTarea.Det_Det_Tarea); 
                cmd.Parameters.AddWithValue("@Det_Fch_RegDetalleIni", objDetalleTarea.Det_Fch_RegDetalleIni);
                cmd.Parameters.AddWithValue("@Det_Fch_RegDetalleFin", objDetalleTarea.Det_Fch_RegDetalleFin);
                cmd.Parameters.AddWithValue("@Det_Tiempo", objDetalleTarea.Det_Tiempo);
                cmd.Parameters.AddWithValue("@Det_Observaciones", objDetalleTarea.Det_Observaciones);
                cmd.Parameters.AddWithValue("@Det_Horas_Extras_Tipo", objDetalleTarea.Det_Horas_Extras_Tipo);
                cmd.Parameters.AddWithValue("@IdUsuarioSession", IdUsuarioSession);
                cmd.Parameters.AddWithValue("@Det_Ip_Modificacion", IpCliente);
                cmd.Parameters.AddWithValue("@Cod_CatalogoTareaSap", objDetalleTarea.Cod_CatalogoTareaSap);
                cmd.Parameters.AddWithValue("@IdTipoGasto", objDetalleTarea.IdTipoGasto);
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

        }


        public static EntRespuesta RTA_BorrarDetalleTarea(Int32 idRegDetTareas)
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

                cmd = new SqlCommand("Sp_RTABorrarDetalleTarea", cnx);

                cmd.Parameters.AddWithValue("@Id_RegDetTareas", idRegDetTareas);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                dr.Read();
                respuestaSP = Convert.ToInt32(dr["Respuestas"].ToString());

                if (respuestaSP == 1)
                {
                    Respuesta.estado = respuestaSP.ToString();
                    Respuesta.mensaje = "Tarea borrada con Exito.";
                    Respuesta.tipoMensaje = "success";
                }
                else
                {
                    Respuesta.estado = respuestaSP.ToString();
                    Respuesta.mensaje = "Ocurrio un error al borrar los datos.";
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


        public static EntRespuesta RTA_CambiarEstadoDetalleTarea(Int32 idRegDetTareas)
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

                cmd = new SqlCommand("Sp_RTACambiarEstadoDetalleTarea", cnx);

                cmd.Parameters.AddWithValue("@Id_RegDetTareas", idRegDetTareas);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                dr.Read();
                respuestaSP = Convert.ToInt32(dr["Respuestas"].ToString());

                if (respuestaSP == 1)
                {
                    Respuesta.estado = respuestaSP.ToString();
                    Respuesta.mensaje = "Tarea actualizada con Exito.";
                    Respuesta.tipoMensaje = "success";
                }
                else
                {
                    Respuesta.estado = respuestaSP.ToString();
                    Respuesta.mensaje = "Ocurrio un error al borrar los datos.";
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


        public static EntRespuesta RTA_BorrarArchivosTarea(Int32 IdArchivo)
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

                cmd = new SqlCommand("Sp_RTA_BorrarArchivosTarea", cnx);

                cmd.Parameters.AddWithValue("@Id_ArchivosTarea", IdArchivo);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                dr.Read();
                respuestaSP = Convert.ToInt32(dr["Respuestas"].ToString());

                if (respuestaSP == 1)
                {
                    Respuesta.estado = respuestaSP.ToString();
                    Respuesta.mensaje = "Archivo borrado con Exito.";
                    Respuesta.tipoMensaje = "success";
                }
                else
                {
                    Respuesta.estado = respuestaSP.ToString();
                    Respuesta.mensaje = "Ocurrio un error al borrar el archivo.";
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


        public static EntRespuesta RTA_BorrarArchivosContrato(Int32 IdArchivo)
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

                cmd = new SqlCommand("Sp_RTA_BorrarArchivosContrato", cnx);

                cmd.Parameters.AddWithValue("@Id_ArchivosTarea", IdArchivo);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                dr.Read();
                respuestaSP = Convert.ToInt32(dr["Respuestas"].ToString());

                if (respuestaSP == 1)
                {
                    Respuesta.estado = respuestaSP.ToString();
                    Respuesta.mensaje = "Archivo borrado con Exito.";
                    Respuesta.tipoMensaje = "success";
                }
                else
                {
                    Respuesta.estado = respuestaSP.ToString();
                    Respuesta.mensaje = "Ocurrio un error al borrar el archivo.";
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


        public static EntRespuesta RTAActualizarEstadoHorasExtras(int Id_RegDetTareas,  int Det_HorasExtrasEstado)
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

                cmd = new SqlCommand("Sp_RTAActualizarEstadoHorasExtras", cnx);

                cmd.Parameters.AddWithValue("@Id_RegDetTareas", Id_RegDetTareas);
                cmd.Parameters.AddWithValue("@Det_Horas_Extras_Estado", Det_HorasExtrasEstado);
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

        }

        public static EntRespuesta RTA_InsertaDetalleTarea(EntDetalleTarea objDetalleTarea)
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
                                       
                cmd = new SqlCommand("Sp_RTAInsertaDetalleTarea", cnx);

                cmd.Parameters.AddWithValue("@Id_RegTareas", objDetalleTarea.Id_RegTareas);
                cmd.Parameters.AddWithValue("@Det_Num_OrdenServicio", objDetalleTarea.Det_Num_OrdenServicio);
                cmd.Parameters.AddWithValue("@Det_Id_CompAranda", objDetalleTarea.Det_Id_CompAranda);
                cmd.Parameters.AddWithValue("@Det_Fch_RegDetalleIni", objDetalleTarea.Det_Fch_RegDetalleIni);
                cmd.Parameters.AddWithValue("@Det_Fch_RegDetalleFin", objDetalleTarea.Det_Fch_RegDetalleFin);
                cmd.Parameters.AddWithValue("@Det_EstadoIni", objDetalleTarea.Det_EstadoIni);
                cmd.Parameters.AddWithValue("@Det_EstadoFin", objDetalleTarea.Det_EstadoFin);
                cmd.Parameters.AddWithValue("@Det_Nom_Empresa", objDetalleTarea.Det_Nom_Empresa);
                cmd.Parameters.AddWithValue("@Det_Det_Tarea", objDetalleTarea.Det_Det_Tarea);
                cmd.Parameters.AddWithValue("@Det_Estado", objDetalleTarea.Det_Estado);
                cmd.Parameters.AddWithValue("@IdDet_EstadoIni", objDetalleTarea.IdDet_EstadoIni); 
                cmd.Parameters.AddWithValue("@Det_Motivo_Cambio_Estado", objDetalleTarea.Det_Motivo_Cambio_Estado);
                cmd.Parameters.AddWithValue("@Det_Observaciones", objDetalleTarea.Det_Observaciones);
                cmd.Parameters.AddWithValue("@Det_Horas_Extras_Tipo", objDetalleTarea.Det_Horas_Extras_Tipo);
                cmd.Parameters.AddWithValue("@Id_Responsable", objDetalleTarea.Id_Responsable);
                cmd.Parameters.AddWithValue("@Det_Tiempo", objDetalleTarea.Det_Tiempo);
                cmd.Parameters.AddWithValue("@Cod_CatalogoTareaSap", objDetalleTarea.Cod_CatalogoTareaSap);
                cmd.Parameters.AddWithValue("@IdTipoGasto", objDetalleTarea.IdTipoGasto);
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
                else if(respuestaSP==-1)
                {
                    //Respuesta.estado = respuestaSP.ToString();
                    Respuesta.estado = "-1";
                    Respuesta.mensaje = "No existe el número de orden por favor validar";
                    Respuesta.tipoMensaje = "danger";
                    Respuesta.resultado = respuestaSP.ToString();
                }
                else if (respuestaSP == -3)
                {
                    //Respuesta.estado = respuestaSP.ToString();
                    Respuesta.estado = "-3";
                    Respuesta.mensaje = "Estas horas ya fuerón registradas en otra actividad. Validar que no se duplique ";
                    Respuesta.tipoMensaje = "danger";
                    Respuesta.resultado = respuestaSP.ToString();
                }
                else if (respuestaSP == -4)
                {
                    //Respuesta.estado = respuestaSP.ToString();
                    Respuesta.estado = "-4";
                    Respuesta.mensaje = "La OS se encuentra CERRADA no puede registrar mas tiempo comuniquese con Planificador. ";
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

        public static EntRespuesta RTA_InsertaDetalleTareaActualizar(EntDetalleTarea objDetalleTarea)
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

                cmd = new SqlCommand("Sp_RTAInsertaDetalleTareaActualizar", cnx);

                cmd.Parameters.AddWithValue("@Id_RegTareas", objDetalleTarea.Id_RegTareas);
                cmd.Parameters.AddWithValue("@Det_Num_OrdenServicio", objDetalleTarea.Det_Num_OrdenServicio);
                cmd.Parameters.AddWithValue("@Det_Id_CompAranda", objDetalleTarea.Det_Id_CompAranda);
                cmd.Parameters.AddWithValue("@Det_Fch_RegDetalleIni", objDetalleTarea.Det_Fch_RegDetalleIni);
                cmd.Parameters.AddWithValue("@Det_Fch_RegDetalleFin", objDetalleTarea.Det_Fch_RegDetalleFin);
                cmd.Parameters.AddWithValue("@Det_EstadoIni", objDetalleTarea.Det_EstadoIni);
                cmd.Parameters.AddWithValue("@Det_EstadoFin", objDetalleTarea.Det_EstadoFin);
                cmd.Parameters.AddWithValue("@Det_Nom_Empresa", objDetalleTarea.Det_Nom_Empresa);
                cmd.Parameters.AddWithValue("@Det_Det_Tarea", objDetalleTarea.Det_Det_Tarea);
                cmd.Parameters.AddWithValue("@Det_Estado", objDetalleTarea.Det_Estado);
                cmd.Parameters.AddWithValue("@IdDet_EstadoIni", objDetalleTarea.IdDet_EstadoIni);
                cmd.Parameters.AddWithValue("@Det_Motivo_Cambio_Estado", objDetalleTarea.Det_Motivo_Cambio_Estado);
                cmd.Parameters.AddWithValue("@Det_Observaciones", objDetalleTarea.Det_Observaciones);
                cmd.Parameters.AddWithValue("@Det_Horas_Extras_Tipo", objDetalleTarea.Det_Horas_Extras_Tipo);
                cmd.Parameters.AddWithValue("@Id_Responsable", objDetalleTarea.Id_Responsable);
                cmd.Parameters.AddWithValue("@Det_Tiempo", objDetalleTarea.Det_Tiempo);
                cmd.Parameters.AddWithValue("@Cod_CatalogoTareaSap", objDetalleTarea.Cod_CatalogoTareaSap);
                cmd.Parameters.AddWithValue("@IdTipoGasto", objDetalleTarea.IdTipoGasto);
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
                    Respuesta.mensaje = "No existe el número de orden por favor validar";
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


        public static EntRespuesta RTAInsertaArchivoTarea(EntArchivoTarea registro)
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

                cmd = new SqlCommand("Sp_RTAInsertaArchivoTarea", cnx);

                cmd.Parameters.AddWithValue("@Nombre_Archivo", registro.Nombre_Archivo);
                cmd.Parameters.AddWithValue("@Extension_Archivo", registro.Extension_Archivo);
                cmd.Parameters.AddWithValue("@Codigo_Archivo", registro.Codigo_Archivo);
                cmd.Parameters.AddWithValue("@Nombre_ArchivoCodigo", registro.Nombre_ArchivoCodigo);
                cmd.Parameters.AddWithValue("@Ruta_Archivo", registro.Ruta_Archivo);
                cmd.Parameters.AddWithValue("@Descripcion_Archivo", registro.Descripcion_Archivo);
                cmd.Parameters.AddWithValue("@Id_RegDetTareas", registro.Id_RegDetTareas);
                cmd.Parameters.AddWithValue("@Id_RegTareas", registro.Id_RegTareas);
                cmd.Parameters.AddWithValue("@Orden_Archivo", registro.Orden_Archivo);
                cmd.Parameters.AddWithValue("@Icon_Nombre", registro.Icon_Nombre); 
                cmd.Parameters.AddWithValue("@Usu_Modificacion", registro.Usu_Modificacion);
                cmd.Parameters.AddWithValue("@Ip_Modificacion", registro.Ip_Modificacion);
                cmd.Parameters.AddWithValue("@idServicio", registro.idServicio);

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


        public static int RTA_ActuaProgHisTarea(string IdUsuario)
        {
            int Respuesta = 0;
            SqlCommand cmd = null;
            SqlDataReader dr = null;

            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cmd = new SqlCommand("Sp_RTAActuaProgHisTarea", cnx);

                cmd.Parameters.AddWithValue("@Id_Responsable", IdUsuario);
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


        public static int RTA_CreaTareas(EntTareas objTarea)
        {
            int Respuesta = 0;
            SqlCommand cmd = null;
            SqlDataReader dr = null;

            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cmd = new SqlCommand("Sp_RTAInsertaTarea", cnx);

                cmd.Parameters.AddWithValue("@Num_OrdenServicio", objTarea.Num_OrdenServicio);
                cmd.Parameters.AddWithValue("@Fch_Registro", objTarea.Fch_Registro);
                cmd.Parameters.AddWithValue("@Id_Responsable", objTarea.Id_Responsable);
                cmd.Parameters.AddWithValue("@Nom_Responsable", objTarea.Nom_Responsable);
                cmd.Parameters.AddWithValue("@Nom_Empresa", objTarea.Nom_Empresa);
                cmd.Parameters.AddWithValue("@Det_Tarea", objTarea.Det_Tarea);
                cmd.Parameters.AddWithValue("@Estado", objTarea.Estado);
                cmd.Parameters.AddWithValue("@Categoria", objTarea.Categoria);
                cmd.Parameters.AddWithValue("@SubCategoria", objTarea.SubCategoria);
                cmd.Parameters.AddWithValue("@EstadoApro", objTarea.EstadoApro);
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


        public static EntRespuesta RTA_CreaTareas2(EntTareas objTarea)
        {
            EntRespuesta Respuesta = new EntRespuesta();
            int respuestaSP = 0;
            SqlCommand cmd = null;
            SqlDataReader dr = null;

            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cmd = new SqlCommand("Sp_RTAInsertaTarea", cnx);

                cmd.Parameters.AddWithValue("@Num_OrdenServicio", objTarea.Num_OrdenServicio);
                cmd.Parameters.AddWithValue("@Fch_Registro", objTarea.Fch_Registro);
                cmd.Parameters.AddWithValue("@Id_Responsable", objTarea.Id_Responsable);
                cmd.Parameters.AddWithValue("@Nom_Responsable", objTarea.Nom_Responsable);
                cmd.Parameters.AddWithValue("@Nom_Empresa", objTarea.Nom_Empresa);
                cmd.Parameters.AddWithValue("@Det_Tarea", objTarea.Det_Tarea);
                cmd.Parameters.AddWithValue("@Estado", objTarea.Estado);
                cmd.Parameters.AddWithValue("@Categoria", objTarea.Categoria);
                cmd.Parameters.AddWithValue("@SubCategoria", objTarea.SubCategoria);
                cmd.Parameters.AddWithValue("@EstadoApro", objTarea.EstadoApro);
                cmd.CommandType = CommandType.StoredProcedure;
                cnx.Open();
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

        public static EntRespuesta RTAMaximaOrdenArchivosTarea(Int32 IdTarea, Int32 idServicio)
        {
            EntRespuesta Respuesta = new EntRespuesta();
            int maximoValorItem = 0;
            SqlCommand cmd = null;
            SqlDataReader dr = null;

            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("Sp_RTAMaximaOrdenArchivosTarea", cnx);
                cmd.Parameters.AddWithValue("@Id_RegTareas", IdTarea);
                cmd.Parameters.AddWithValue("@idServicio", idServicio);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();

                while (dr.Read())
                {

                    maximoValorItem = Convert.ToInt32(dr["Orden_Archivo_Maximo"].ToString());

                }

                Respuesta.estado = "1";
                Respuesta.mensaje = "OK";
                Respuesta.tipoMensaje = "success";
                Respuesta.resultado = maximoValorItem;

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


        public static EntRespuesta RTA_ConsultaCatalogo(Int32 IdCatalogo)
        {
            EntRespuesta Respuesta = new EntRespuesta();
            int respuestaSP = 0;
            EntCatalogo itemCatalogo = new EntCatalogo();
            SqlCommand cmd = null;
            SqlDataReader dr = null;

            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("Sp_RTAConsultaCatalogo", cnx);
                cmd.Parameters.AddWithValue("@IdCatalogo", IdCatalogo);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();

                while (dr.Read())
                {

                    itemCatalogo.IdCatalogo = Convert.ToInt32(dr["IdCatalogo"].ToString());
                    itemCatalogo.IdTipoCatalogo = Convert.ToInt32(dr["IdTipoCatalogo"].ToString());
                    itemCatalogo.Descripcion = dr["Descripcion"].ToString();
                    itemCatalogo.DescripcionLarga = dr["DescripcionLarga"].ToString();
                    itemCatalogo.IdExterno = dr["IdExterno"].ToString();
                    itemCatalogo.IdAranda = dr["IdAranda"].ToString();
                    itemCatalogo.IdProyecto = Convert.ToInt32(dr["IdProyecto"].ToString());
                    itemCatalogo.RequiereAprobacion = Convert.ToInt32(dr["RequiereAprobacion"].ToString());
                    itemCatalogo.IdCatalogoPadre = Convert.ToInt32(dr["IdCatalogoPadre"].ToString());
                    itemCatalogo.RequiereCrearTareaDetalle = Convert.ToInt32(dr["RequiereCrearTareaDetalle"].ToString());
                    itemCatalogo.EsEstadoFinal = Convert.ToInt32(dr["EsEstadoFinal"].ToString());
                    itemCatalogo.RequiereTextoSolucion = Convert.ToInt32(dr["RequiereTextoSolucion"].ToString());
                }

                Respuesta.estado = "1";
                Respuesta.mensaje = "OK";
                Respuesta.tipoMensaje = "success";
                Respuesta.resultado = itemCatalogo;

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


        public static List<EntDetTarea> ListaDetTareas(string Idusuario, string FchIni, string FchFin,string Det_Nom_Empresa,string Det_Num_OrdenServicio,int IdTipoGasto, int idTareaAprobada, int idTareaAprobadaQA,int idFecha, int Tipo)
        {
            List<EntDetTarea> listaDetTareas = null;
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("Sp_RTAListaDetTareas", cnx);
                cmd.Parameters.AddWithValue("@Idusuario", Idusuario); 
                cmd.Parameters.AddWithValue("@FchIni", FchIni);
                cmd.Parameters.AddWithValue("@FchFin", FchFin);
                cmd.Parameters.AddWithValue("@Det_Nom_Empresa", Det_Nom_Empresa);
                cmd.Parameters.AddWithValue("@Det_Num_OrdenServicio", Det_Num_OrdenServicio);
                cmd.Parameters.AddWithValue("@IdTipoGasto", IdTipoGasto);
                cmd.Parameters.AddWithValue("@idTareaAprobada", idTareaAprobada);
                cmd.Parameters.AddWithValue("@idTareaAprobadaQA", idTareaAprobadaQA);
                cmd.Parameters.AddWithValue("@idFecha", idFecha);
                cmd.Parameters.AddWithValue("@Tipo", Tipo);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                listaDetTareas = new List<EntDetTarea>();
                while (dr.Read())
                {
                    EntDetTarea DetTarea = new EntDetTarea();

                    
                    DetTarea.Nom_Cliente = dr["Nom_Cliente"].ToString();
                    DetTarea.Num_OrdenServicio = dr["Num_OrdenServicio"].ToString();
                    DetTarea.Id_Responsable = dr["Id_Responsable"].ToString();
                    DetTarea.Actividad = dr["Actividad"].ToString();
                    DetTarea.Det_Tarea = dr["Det_Tarea"].ToString();
                    DetTarea.Det_Fecha = dr["Det_Fecha"].ToString().Substring(0,10);
                    DetTarea.Det_Fch_RegDetalleIni = dr["Det_Fch_RegDetalleIni"].ToString();
                    DetTarea.Det_Fch_RegDetalleFin = dr["Det_Fch_RegDetalleFin"].ToString();
                    DetTarea.Det_Tiempo = dr["Det_Tiempo"].ToString();
                    DetTarea.Det_Det_Tarea = dr["Det_Det_Tarea"].ToString();
                    DetTarea.Det_Det_TareaFin = dr["Det_Det_TareaFin"].ToString();
                    DetTarea.Motivo = dr["Motivo"].ToString();
                    DetTarea.EstadoInicial = dr["EstadoInicial"].ToString();
                    DetTarea.Cod_Sap = dr["Cod_Sap"].ToString();
                    DetTarea.OrdenOperacion = dr["Operacion"].ToString();
                    DetTarea.idTarea = Convert.ToInt32(dr["idTarea"].ToString());
                    DetTarea.idDetalleTarea = Convert.ToInt32(dr["idDetalleTarea"].ToString());
                    DetTarea.Det_Observaciones = dr["Observaciones"].ToString();
                    DetTarea.Det_Horas_Extras_Estado = Convert.ToInt32(dr["Det_Horas_Extras_Estado"].ToString());
                    DetTarea.Det_Horas_Extras_Tipo = Convert.ToInt32(dr["Det_Horas_Extras_Tipo"].ToString());
                    DetTarea.Det_Horas_Extras_Descripcion = dr["Det_Horas_Extras_Descripcion"].ToString();
                    DetTarea.Det_Horas_Extras_Envio_Correo = Convert.ToInt32(dr["Det_Horas_Extras_Envio_Correo"].ToString());
                    DetTarea.UsuarioResponsable = dr["UsuarioResponsable"].ToString();
                    if(dr["Det_Horas_Extras_Fecha_Solicitud"].ToString() == "01/01/1900 0:00:00")
                    {
                        DetTarea.Det_Horas_Extras_Fecha_Solicitud = "";
                    }
                    else
                    {
                        DetTarea.Det_Horas_Extras_Fecha_Solicitud = dr["Det_Horas_Extras_Fecha_Solicitud"].ToString();
                    }
                    if (dr["Det_Horas_Extras_Fecha_Aprobacion"].ToString() == "01/01/1900 0:00:00")
                    {
                        DetTarea.Det_Horas_Extras_Fecha_Aprobacion = "";
                    }
                    else
                    {
                        DetTarea.Det_Horas_Extras_Fecha_Aprobacion = dr["Det_Horas_Extras_Fecha_Solicitud"].ToString();
                    }

                    DetTarea.Det_Horas_Extras_Estado_Descripcion = dr["Det_Horas_Extras_Estado_Descripcion"].ToString();
                    DetTarea.Det_Total_Horas_Dia = dr["Det_Total_Horas_Dia"].ToString();
                    DetTarea.conteoArchivosAdjuntos = Convert.ToInt32(dr["conteoArchivosAdjuntos"].ToString());

                    DetTarea.Det_Aprobacion_Tarea_Estado = Convert.ToInt32(dr["Det_Aprobacion_Tarea_Estado"].ToString());
                    DetTarea.Det_Aprobacion_Tarea_Estado_Descripcion = dr["Det_Aprobacion_Tarea_Estado_Descripcion"].ToString();
                    if (dr["Det_Fecha_Aprobacion_Tarea"].ToString() == "01/01/1900 0:00:00")
                    {
                        DetTarea.Det_Fecha_Aprobacion_Tarea = "";
                    }
                    else
                    {
                        DetTarea.Det_Fecha_Aprobacion_Tarea = dr["Det_Fecha_Aprobacion_Tarea"].ToString();
                    }
                    DetTarea.Det_Aprobacion_Tarea_Estado_Class_Mensaje = dr["Det_Aprobacion_Tarea_Estado_Class_Mensaje"].ToString();

                    DetTarea.Det_Aprobacion_Tarea_Estado_QA = Convert.ToInt32(dr["Det_Aprobacion_Tarea_Estado_QA"].ToString());
                    DetTarea.Det_Aprobacion_Tarea_Estado_QA_Descripcion = dr["Det_Aprobacion_Tarea_Estado_QA_Descripcion"].ToString();
                    if (dr["Det_Fecha_Aprobacion_Tarea_QA"].ToString() == "01/01/1900 0:00:00")
                    {
                        DetTarea.Det_Fecha_Aprobacion_Tarea_QA = "";
                    }
                    else
                    {
                        DetTarea.Det_Fecha_Aprobacion_Tarea_QA = dr["Det_Fecha_Aprobacion_Tarea_QA"].ToString();
                    }
                    DetTarea.Det_Aprobacion_Tarea_Estado_QA_Class_Mensaje = dr["Det_Aprobacion_Tarea_Estado_QA_Class_Mensaje"].ToString();
                    DetTarea.DetCategoria= dr["DetCategoria"].ToString();
                    DetTarea.NumAranda = dr["NumAranda"].ToString();
                    listaDetTareas.Add(DetTarea);
                }

            }
            catch (Exception ex)
            {
                listaDetTareas = null;
            }
            finally
            {
                cmd.Connection.Close();
            }
            return listaDetTareas;
        }


        public static List<EntDetTarea> ListaDetTareasRechazadas(string Idusuario, string FchIni, string FchFin, string Det_Nom_Empresa, string Det_Num_OrdenServicio, int IdTipoGasto, int idTareaAprobada, int idTareaAprobadaQA, int idFecha, int Tipo)
        {
            List<EntDetTarea> listaDetTareas = null;
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("Sp_RTAListaDetTareasRechazadas", cnx);
                cmd.Parameters.AddWithValue("@Idusuario", Idusuario);
                cmd.Parameters.AddWithValue("@FchIni", FchIni);
                cmd.Parameters.AddWithValue("@FchFin", FchFin);
                cmd.Parameters.AddWithValue("@Det_Nom_Empresa", Det_Nom_Empresa);
                cmd.Parameters.AddWithValue("@Det_Num_OrdenServicio", Det_Num_OrdenServicio);
                cmd.Parameters.AddWithValue("@IdTipoGasto", IdTipoGasto);
                cmd.Parameters.AddWithValue("@idTareaAprobada", idTareaAprobada);
                cmd.Parameters.AddWithValue("@idTareaAprobadaQA", idTareaAprobadaQA);
                cmd.Parameters.AddWithValue("@idFecha", idFecha);
                cmd.Parameters.AddWithValue("@Tipo", Tipo);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                listaDetTareas = new List<EntDetTarea>();
                while (dr.Read())
                {
                    EntDetTarea DetTarea = new EntDetTarea();


                    DetTarea.Nom_Cliente = dr["Nom_Cliente"].ToString();
                    DetTarea.Num_OrdenServicio = dr["Num_OrdenServicio"].ToString();
                    DetTarea.Id_Responsable = dr["Id_Responsable"].ToString();
                    DetTarea.Actividad = dr["Actividad"].ToString();
                    DetTarea.Det_Tarea = dr["Det_Tarea"].ToString();
                    DetTarea.Det_Fecha = dr["Det_Fecha"].ToString().Substring(0, 10);
                    DetTarea.Det_Fch_RegDetalleIni = dr["Det_Fch_RegDetalleIni"].ToString();
                    DetTarea.Det_Fch_RegDetalleFin = dr["Det_Fch_RegDetalleFin"].ToString();
                    DetTarea.Det_Tiempo = dr["Det_Tiempo"].ToString();
                    DetTarea.Det_Det_Tarea = dr["Det_Det_Tarea"].ToString();
                    DetTarea.Det_Det_TareaFin = dr["Det_Det_TareaFin"].ToString();
                    DetTarea.Motivo = dr["Motivo"].ToString();
                    DetTarea.EstadoInicial = dr["EstadoInicial"].ToString();
                    DetTarea.Cod_Sap = dr["Cod_Sap"].ToString();
                    DetTarea.OrdenOperacion = dr["Operacion"].ToString();
                    DetTarea.idTarea = Convert.ToInt32(dr["idTarea"].ToString());
                    DetTarea.idDetalleTarea = Convert.ToInt32(dr["idDetalleTarea"].ToString());
                    DetTarea.Det_Observaciones = dr["Observaciones"].ToString();
                    DetTarea.Det_Horas_Extras_Estado = Convert.ToInt32(dr["Det_Horas_Extras_Estado"].ToString());
                    DetTarea.Det_Horas_Extras_Tipo = Convert.ToInt32(dr["Det_Horas_Extras_Tipo"].ToString());
                    DetTarea.Det_Horas_Extras_Descripcion = dr["Det_Horas_Extras_Descripcion"].ToString();
                    DetTarea.Det_Horas_Extras_Envio_Correo = Convert.ToInt32(dr["Det_Horas_Extras_Envio_Correo"].ToString());
                    DetTarea.UsuarioResponsable = dr["UsuarioResponsable"].ToString();
                    if (dr["Det_Horas_Extras_Fecha_Solicitud"].ToString() == "01/01/1900 0:00:00")
                    {
                        DetTarea.Det_Horas_Extras_Fecha_Solicitud = "";
                    }
                    else
                    {
                        DetTarea.Det_Horas_Extras_Fecha_Solicitud = dr["Det_Horas_Extras_Fecha_Solicitud"].ToString();
                    }
                    if (dr["Det_Horas_Extras_Fecha_Aprobacion"].ToString() == "01/01/1900 0:00:00")
                    {
                        DetTarea.Det_Horas_Extras_Fecha_Aprobacion = "";
                    }
                    else
                    {
                        DetTarea.Det_Horas_Extras_Fecha_Aprobacion = dr["Det_Horas_Extras_Fecha_Solicitud"].ToString();
                    }

                    DetTarea.Det_Horas_Extras_Estado_Descripcion = dr["Det_Horas_Extras_Estado_Descripcion"].ToString();
                    DetTarea.Det_Total_Horas_Dia = dr["Det_Total_Horas_Dia"].ToString();
                    DetTarea.conteoArchivosAdjuntos = Convert.ToInt32(dr["conteoArchivosAdjuntos"].ToString());

                    DetTarea.Det_Aprobacion_Tarea_Estado = Convert.ToInt32(dr["Det_Aprobacion_Tarea_Estado"].ToString());
                    DetTarea.Det_Aprobacion_Tarea_Estado_Descripcion = dr["Det_Aprobacion_Tarea_Estado_Descripcion"].ToString();
                    if (dr["Det_Fecha_Aprobacion_Tarea"].ToString() == "01/01/1900 0:00:00")
                    {
                        DetTarea.Det_Fecha_Aprobacion_Tarea = "";
                    }
                    else
                    {
                        DetTarea.Det_Fecha_Aprobacion_Tarea = dr["Det_Fecha_Aprobacion_Tarea"].ToString();
                    }
                    DetTarea.Det_Aprobacion_Tarea_Estado_Class_Mensaje = dr["Det_Aprobacion_Tarea_Estado_Class_Mensaje"].ToString();

                    DetTarea.Det_Aprobacion_Tarea_Estado_QA = Convert.ToInt32(dr["Det_Aprobacion_Tarea_Estado_QA"].ToString());
                    DetTarea.Det_Aprobacion_Tarea_Estado_QA_Descripcion = dr["Det_Aprobacion_Tarea_Estado_QA_Descripcion"].ToString();
                    if (dr["Det_Fecha_Aprobacion_Tarea_QA"].ToString() == "01/01/1900 0:00:00")
                    {
                        DetTarea.Det_Fecha_Aprobacion_Tarea_QA = "";
                    }
                    else
                    {
                        DetTarea.Det_Fecha_Aprobacion_Tarea_QA = dr["Det_Fecha_Aprobacion_Tarea_QA"].ToString();
                    }
                    DetTarea.Det_Aprobacion_Tarea_Estado_QA_Class_Mensaje = dr["Det_Aprobacion_Tarea_Estado_QA_Class_Mensaje"].ToString();
                    DetTarea.DetCategoria = dr["DetCategoria"].ToString();
                    DetTarea.NumAranda = dr["NumAranda"].ToString();
                    DetTarea.HORAS_DISPONIBLES = dr["HORAS_DISPONIBLES"].ToString();
                    DetTarea.SALDO_DE_COSTOS = dr["SALDO_DE_COSTOS"].ToString();
                    listaDetTareas.Add(DetTarea);
                }

            }
            catch (Exception ex)
            {
                listaDetTareas = null;
            }
            finally
            {
                cmd.Connection.Close();
            }
            return listaDetTareas;
        }

        public static EntRespuesta ListaHorasRecursosPorJefatura(string Idusuario, string FchIni, string FchFin, int Tipo,int estado)
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

                cmd = new SqlCommand("Sp_RTAListaHorasRecursosPorJefatura", cnx);
                cmd.Parameters.AddWithValue("@IdUsuarioJefe", Idusuario);
                cmd.Parameters.AddWithValue("@FechaInicio", FchIni);
                cmd.Parameters.AddWithValue("@FechaFin", FchFin);
                cmd.Parameters.AddWithValue("@idEstado", estado);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
				
                dtResultados.Load(dr);

                //if (dr.HasRows)
                //{
                //    Respuesta.mensaje = "OK";
                //}
                //else
                //{
                //    Respuesta.mensaje = "La consulta no tiene registros.";
                //}

                Respuesta.estado = "1";
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

        public static EntRespuesta ListaDetalleTareasDescarga(string Idusuario, string FchIni, string FchFin, string Det_Nom_Empresa, string Det_Num_OrdenServicio, int IdTipoGasto, int idTareaAprobada, int idTareaAprobadaQA, int idFecha, int Tipo)
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

                cmd = new SqlCommand("Sp_RTAListaDetalleTareasDescarga", cnx);
                cmd.Parameters.AddWithValue("@Idusuario", Idusuario);
                cmd.Parameters.AddWithValue("@FchIni", FchIni);
                cmd.Parameters.AddWithValue("@FchFin", FchFin);
                cmd.Parameters.AddWithValue("@Det_Nom_Empresa", Det_Nom_Empresa);
                cmd.Parameters.AddWithValue("@Det_Num_OrdenServicio", Det_Num_OrdenServicio);
                cmd.Parameters.AddWithValue("@IdTipoGasto", IdTipoGasto);
                cmd.Parameters.AddWithValue("@idTareaAprobada", idTareaAprobada);
                cmd.Parameters.AddWithValue("@idTareaAprobadaQA", idTareaAprobadaQA);
                cmd.Parameters.AddWithValue("@idFecha", idFecha);
                cmd.Parameters.AddWithValue("@Tipo", Tipo);
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

        public static List<EntKpi> ListaKpiTareas(string Idusuario, string FchIni, string FchFin, int Tipo)
        {
            List<EntKpi> listaDetTareas = null;
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cmd = new SqlCommand("Sp_RTAKpiTareas", cnx);
                cmd.Parameters.AddWithValue("@Idusuario", Idusuario);
                cmd.Parameters.AddWithValue("@FchIni", FchIni);
                cmd.Parameters.AddWithValue("@FchFin", FchFin);
                cmd.Parameters.AddWithValue("@Tipo", Tipo);
                cmd.CommandType = CommandType.StoredProcedure;
                cnx.Open();
                dr = cmd.ExecuteReader();
                listaDetTareas = new List<EntKpi>();
                while (dr.Read())
                {
                    EntDetTarea DetTarea = new EntDetTarea();


                }

            }
            catch (Exception ex)
            {
                listaDetTareas = null;
            }
            finally
            {
                cmd.Connection.Close();
            }
            return listaDetTareas;
        }

        public static int RTA_CreaTareasHorasExtras( Int32 Id_RegTareas)
        {
            int Respuesta = 0;
            SqlCommand cmd = null;
            SqlDataReader dr = null;

            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cmd = new SqlCommand("Sp_RTAInsertaTareaHorasExtras", cnx);
                cmd.Parameters.AddWithValue("@Id_RegTareas", Id_RegTareas);
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

        public static EntRespuesta RTA_CambioEstadoAprobacionTarea(string IdUsuarioJefe, string fechaDesde, string fechaHasta, string idUsuarioResponsable, string IdUsuarioSession, string ipCliente, Int32 estadoAprobacion)
        {
            EntRespuesta Respuesta = new EntRespuesta();
            int respuestaSP = 0;
            SqlCommand cmd = null;
            SqlDataReader dr = null;

            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cmd = new SqlCommand("Sp_RTACambioEstadoAprobacionTarea", cnx);
                cmd.Parameters.AddWithValue("@IdUsuarioJefe", IdUsuarioJefe);
                cmd.Parameters.AddWithValue("@FechaInicio", fechaDesde);
                cmd.Parameters.AddWithValue("@FechaFin", fechaHasta);
                cmd.Parameters.AddWithValue("@Id_Responsable", idUsuarioResponsable);
                cmd.Parameters.AddWithValue("@Det_Id_Usuario_Aprobacion_Tarea", IdUsuarioSession);
                cmd.Parameters.AddWithValue("@Det_Ip_Modificacion", ipCliente);
                cmd.Parameters.AddWithValue("@Det_Aprobacion_Tarea_Estado", estadoAprobacion);

                cmd.CommandType = CommandType.StoredProcedure;
                cnx.Open();
                dr = cmd.ExecuteReader();
                dr.Read();
                respuestaSP = Convert.ToInt16(dr["Respuestas"].ToString());

                if (respuestaSP == 1)
                {
                    Respuesta.estado = respuestaSP.ToString();
                    Respuesta.mensaje = "Aprobación de Tareas Exitoso.";
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
        }


        public static EntRespuesta RTA_CambioEstadoAprobacionTareaIndividual(string IdUsuarioJefe, string fechaDesde, string fechaHasta, string idUsuarioResponsable, string IdUsuarioSession, string ipCliente, Int32 estadoAprobacion, Int32 IdDetalleTarea)
        {
            EntRespuesta Respuesta = new EntRespuesta();
            int respuestaSP = 0;
            SqlCommand cmd = null;
            SqlDataReader dr = null;

            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cmd = new SqlCommand("Sp_RTACambioEstadoAprobacionTareaIndividual", cnx);
                cmd.Parameters.AddWithValue("@IdUsuarioJefe", IdUsuarioJefe);
                cmd.Parameters.AddWithValue("@FechaInicio", fechaDesde);
                cmd.Parameters.AddWithValue("@FechaFin", fechaHasta);
                cmd.Parameters.AddWithValue("@Id_Responsable", idUsuarioResponsable);
                cmd.Parameters.AddWithValue("@Det_Id_Usuario_Aprobacion_Tarea", IdUsuarioSession);
                cmd.Parameters.AddWithValue("@Det_Ip_Modificacion", ipCliente);
                cmd.Parameters.AddWithValue("@Det_Aprobacion_Tarea_Estado", estadoAprobacion);
                cmd.Parameters.AddWithValue("@IdDetalleTarea", IdDetalleTarea);
                cmd.CommandType = CommandType.StoredProcedure;
                cnx.Open();
                dr = cmd.ExecuteReader();
                dr.Read();
                respuestaSP = Convert.ToInt16(dr["Respuestas"].ToString());

                if (respuestaSP == 1)
                {
                    Respuesta.estado = respuestaSP.ToString();
                    Respuesta.mensaje = "Aprobación de Tareas Exitoso.";
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
        }

        public static EntRespuesta RTA_CambioEstadoAprobacionTareaRevisor(string IdUsuarioJefe, string fechaDesde, string fechaHasta, string idUsuarioResponsable, string IdUsuarioSession, string ipCliente, Int32 estadoAprobacion)
        {
            EntRespuesta Respuesta = new EntRespuesta();
            int respuestaSP = 0;
            SqlCommand cmd = null;
            SqlDataReader dr = null;

            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cmd = new SqlCommand("Sp_RTACambioEstadoAprobacionTareaRevisor", cnx);
                cmd.Parameters.AddWithValue("@IdUsuarioJefe", IdUsuarioJefe);
                cmd.Parameters.AddWithValue("@FechaInicio", fechaDesde);
                cmd.Parameters.AddWithValue("@FechaFin", fechaHasta);
                cmd.Parameters.AddWithValue("@Id_Responsable", idUsuarioResponsable);
                cmd.Parameters.AddWithValue("@Det_Id_Usuario_Aprobacion_Tarea", IdUsuarioSession);
                cmd.Parameters.AddWithValue("@Det_Ip_Modificacion", ipCliente);
                cmd.Parameters.AddWithValue("@Det_Aprobacion_Tarea_Estado", estadoAprobacion);

                cmd.CommandType = CommandType.StoredProcedure;
                cnx.Open();
                dr = cmd.ExecuteReader();
                dr.Read();
                respuestaSP = Convert.ToInt16(dr["Respuestas"].ToString());

                if (respuestaSP == 1)
                {
                    Respuesta.estado = respuestaSP.ToString();
                    Respuesta.mensaje = "Aprobación de Tareas Exitoso.";
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
        }

        public static List<EntTareas> ListaTareasHorasExtras(string Idusuario)
        {
            List<EntTareas> listaTareas = null;
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cmd = new SqlCommand("Sp_RTAListaTareasHorasExtras", cnx);
                cmd.Parameters.AddWithValue("@Idusuario", Idusuario);
                cmd.CommandType = CommandType.StoredProcedure;
                cnx.Open();
                dr = cmd.ExecuteReader();
                listaTareas = new List<EntTareas>();
                while (dr.Read())
                {
                    EntTareas Tarea = new EntTareas();


                    Tarea.Id_RegTareas = Convert.ToInt32(dr["Id_RegTareas"].ToString());
                    Tarea.Num_OrdenServicio = dr["Num_OrdenServicio"].ToString();
                    Tarea.Id_CompAranda = dr["Id_CompAranda"].ToString();
                    Tarea.Fch_Registro = dr["Fch_Registro"].ToString();
                    Tarea.Fch_EstAtencion = dr["Fch_EstAtencion"].ToString();
                    Tarea.Fch_EstSolucion = dr["Fch_EstSolucion"].ToString();
                    Tarea.Id_Responsable = dr["Id_Responsable"].ToString();
                    Tarea.Nom_Responsable = dr["Nom_Responsable"].ToString();
                    Tarea.Nom_Empresa = dr["Nom_Empresa"].ToString();
                    Tarea.Nom_SlaAranda = dr["Nom_SlaAranda"].ToString();
                    Tarea.Det_Tarea = dr["Det_Tarea"].ToString();
                    Tarea.Estado = dr["Estado"].ToString();
                    Tarea.Estado2 = dr["Estado2"].ToString();
                    Tarea.Id_RegTareasHorasExtras = Convert.ToInt32(dr["Id_RegTareasHorasExtras"].ToString());
                    Tarea.ResponsableAprobacion = dr["ResponsableAprobacion"].ToString();
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

        public static int RTAActuaAprobacionHorasExtras(EntTareas objEntTareas)
        {
            int Respuesta = 0;
            SqlCommand cmd = null;
            SqlDataReader dr = null;

            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cmd = new SqlCommand("Sp_RTAActuaAprobacionHorasExtras", cnx);

                cmd.Parameters.AddWithValue("@Id_RegTareasHorasExtras", objEntTareas.Id_RegTareasHorasExtras);
                cmd.Parameters.AddWithValue("@Id_RegTareas", objEntTareas.Id_RegTareas);
                cmd.Parameters.AddWithValue("@Estado", objEntTareas.Estado);
                cmd.Parameters.AddWithValue("@tipo", objEntTareas.Tipo);
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

        public static List<EntTareas> RTAConsultaTareaHorasExtras()
        {
            List<EntTareas> listaDetTareas = null;
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cmd = new SqlCommand("Sp_RTAConsultaTareaHorasExtras", cnx);
                cmd.CommandType = CommandType.StoredProcedure;
                cnx.Open();
                dr = cmd.ExecuteReader();
                listaDetTareas = new List<EntTareas>();
                while (dr.Read())
                {
                    EntTareas DetTarea = new EntTareas();
                    DetTarea.Id_RegTareasHorasExtras =Convert.ToInt32(dr["Id_RegTareasHorasExtras"].ToString());
                    DetTarea.Id_RegTareas = Convert.ToInt32(dr["Id_RegTareas"].ToString());
                    DetTarea.Num_OrdenServicio = dr["Num_OrdenServicio"].ToString();
                    DetTarea.Nom_Responsable = dr["Nom_Responsable"].ToString();
                    DetTarea.Nom_Empresa = dr["Nom_Empresa"].ToString();
                    DetTarea.Det_Tarea = dr["Det_Tarea"].ToString();
                    DetTarea.MailResponsableAprobacion = dr["MailResponsableAprobacion"].ToString();
                    DetTarea.MailAprobacionTitulo = dr["MailAprobacionTitulo"].ToString();
                    DetTarea.TipoAprobacion = Convert.ToInt32(dr["TipoAprobacion"].ToString());
                    listaDetTareas.Add(DetTarea);
                }

            }
            catch (Exception ex)
            {
                listaDetTareas = null;
            }
            finally
            {
                cmd.Connection.Close();
            }
            return listaDetTareas;
        }

        public static EntTareas RTA_ConsultaTareaHorasExtrasRTA(Int32 Id_RegTareasHorasExtras)
        {

            EntTareas objTarea = null;// new entUsuario();
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cmd = new SqlCommand("Sp_RTAConsultaTareaHorasExtrasPro", cnx);
                cmd.Parameters.AddWithValue("@Id_RegTareasHorasExtras", Id_RegTareasHorasExtras);
                cmd.CommandType = CommandType.StoredProcedure;
                cnx.Open();
                dr = cmd.ExecuteReader();
                objTarea = new EntTareas();
                dr.Read();

                objTarea.Id_RegTareas = Convert.ToInt32(dr["Id_RegTareas"].ToString());
                objTarea.Num_OrdenServicio = dr["Num_OrdenServicio"].ToString();
                objTarea.Id_CompAranda = dr["Id_CompAranda"].ToString();
                objTarea.Fch_Registro = dr["Fch_Registro"].ToString();
                objTarea.Fch_EstAtencion = dr["Fch_EstAtencion"].ToString();
                objTarea.Fch_EstSolucion = dr["Fch_EstSolucion"].ToString();
                objTarea.Id_Responsable = dr["Id_Responsable"].ToString();
                objTarea.Nom_Responsable = dr["Nom_Responsable"].ToString();
                objTarea.Nom_Empresa = dr["Nom_Empresa"].ToString();
                objTarea.Nom_SlaAranda = dr["Nom_SlaAranda"].ToString();
                objTarea.Det_Tarea = dr["Det_Tarea"].ToString();
                objTarea.Estado = dr["Estado"].ToString();
                objTarea.IdEstado = Convert.ToInt32(dr["IdEstado"].ToString());
                //resultado = dr["Respuesta"].ToString();
            }
            catch (Exception ex)
            {
                objTarea = null;

            }
            finally
            {
                cmd.Connection.Close();
            }
            return objTarea;
        }

        public static int RTAActualizaHisTareaHorasExtras(EntHisTarea objHisTarea)
        {
            int Respuesta = 0;
            SqlCommand cmd = null;
            SqlDataReader dr = null;

            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cmd = new SqlCommand("Sp_RTAActualizaHisTareaHorasExtras", cnx);

                cmd.Parameters.AddWithValue("@Id_RegTareas", objHisTarea.Id_RegTareas);
                cmd.Parameters.AddWithValue("@Det_Fch_RegDetalleFin", objHisTarea.Det_Fch_RegDetalleIni);
                cmd.Parameters.AddWithValue("@Det_EstadoFin", objHisTarea.Det_EstadoIni);
                cmd.Parameters.AddWithValue("@Det_Det_Tarea", objHisTarea.Det_Det_Tarea);
                cmd.Parameters.AddWithValue("@Det_Estado", objHisTarea.Det_Estado);
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

        public static List<EntDetTarea> ListaDetTareasHorasExtras(string Idusuario, string FchIni, string FchFin, int Tipo)
        {
            List<EntDetTarea> listaDetTareas = null;
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cmd = new SqlCommand("Sp_RTAListaDetTareasHorasExtras", cnx);
                cmd.Parameters.AddWithValue("@Idusuario", Idusuario);
                cmd.Parameters.AddWithValue("@FchIni", FchIni);
                cmd.Parameters.AddWithValue("@FchFin", FchFin);
                cmd.Parameters.AddWithValue("@Tipo", Tipo);
                cmd.CommandType = CommandType.StoredProcedure;
                cnx.Open();
                dr = cmd.ExecuteReader();
                listaDetTareas = new List<EntDetTarea>();
                while (dr.Read())
                {
                    EntDetTarea DetTarea = new EntDetTarea();


                    DetTarea.Nom_Cliente = dr["Nom_Cliente"].ToString();
                    DetTarea.Num_OrdenServicio = dr["Num_OrdenServicio"].ToString();
                    DetTarea.Id_Responsable = dr["Id_Responsable"].ToString();
                    DetTarea.Det_Tarea = dr["Det_Tarea"].ToString();
                    DetTarea.Det_Fecha = dr["Det_Fecha"].ToString().Substring(0, 10);
                    DetTarea.Det_Fch_RegDetalleIni = dr["Det_Fch_RegDetalleIni"].ToString();
                    DetTarea.Det_Fch_RegDetalleFin = dr["Det_Fch_RegDetalleFin"].ToString();
                    DetTarea.Det_Tiempo = dr["Det_Tiempo"].ToString();
                    DetTarea.Det_Det_Tarea = dr["Det_Det_Tarea"].ToString();

                    listaDetTareas.Add(DetTarea);
                }

            }
            catch (Exception ex)
            {
                listaDetTareas = null;
            }
            finally
            {
                cmd.Connection.Close();
            }
            return listaDetTareas;
        }

        public static List<EntDetTarea> Sp_RTAConsultaUsuarioPorJefe(string Idusuario)
        {
            List<EntDetTarea> listaDetTareas = null;
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cmd = new SqlCommand("Sp_RTAConsultaUsuarioPorJefe", cnx);
                cmd.Parameters.AddWithValue("@Cod_Jefe_Inm", Idusuario);
                cmd.CommandType = CommandType.StoredProcedure;
                cnx.Open();
                dr = cmd.ExecuteReader();
                listaDetTareas = new List<EntDetTarea>();
                while (dr.Read())
                {
                    EntDetTarea DetTarea = new EntDetTarea();
                    DetTarea.Id_Responsable = dr["Cod_Usuario"].ToString();
                    DetTarea.Nom_Cliente = dr["Nom_Usuario"].ToString();
                    listaDetTareas.Add(DetTarea);
                }

            }
            catch (Exception ex)
            {
                listaDetTareas = null;
            }
            finally
            {
                cmd.Connection.Close();
            }
            return listaDetTareas;
        }

        public static DataSet RTAConsultaCatalogo(Int32 IdEstado)
        {
            SqlCommand cmd = null;
            DataSet Temp = new DataSet();
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();
                cmd = new SqlCommand("Sp_RTAConsultaCatalogo", cnx);
                cmd.Parameters.AddWithValue("@IdCatalogo", IdEstado);
                cmd.CommandType = CommandType.StoredProcedure;
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(Temp);
            }
            catch(Exception ex)
            {
                Temp = null;
            }
            finally
            {
                cmd.Connection.Close();
            }
            return Temp;
        }

        
        public static DataSet RTAListaCatalogoSecuencia(Int32 IdEstado)
        {
            SqlCommand cmd = null;
            DataSet Temp = new DataSet();
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();
                cmd = new SqlCommand("Sp_RTAListaCatalogoSecuencia", cnx);
                cmd.Parameters.AddWithValue("@IdEstado", IdEstado);
                cmd.CommandType = CommandType.StoredProcedure;
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(Temp);
            }
            catch (Exception ex)
            {
                Temp = null;
            }
            finally
            {
                cmd.Connection.Close();
            }
            return Temp;
        }

        

        public static List<EntCombo> RTAListaEstadosCombo(Int32 idEstado, Int32 IdProyecto)
        {
            List<EntCombo> cmbEstados = null;
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();

                SqlConnection cnx = cn.conectar();
                cnx.Open();
                cmd = new SqlCommand("Sp_RTAListaCatalogoSecuencia", cnx);
                cmd.Parameters.AddWithValue("@IdEstado", idEstado);
                cmd.Parameters.AddWithValue("@IdProyecto", IdProyecto); //reversar
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                cmbEstados = new List<EntCombo>();
                while (dr.Read())
                {
                    EntCombo estado = new EntCombo();

                    estado.Id = dr["IdCatalogo"].ToString();
                    estado.Valor = dr["Descripcion"].ToString();

                    cmbEstados.Add(estado);
                }

            }
            catch (Exception ex)
            {
                cmbEstados = null;
            }
            finally
            {
                cmd.Connection.Close();
            }
            return cmbEstados;
        }

        
        public static List<EntCombo> RTAListaCatalogoCombo(Int32 idTipoCatalogo)
        {
            List<EntCombo> cmbEstados = null;
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();

                SqlConnection cnx = cn.conectar();
                cnx.Open();
                cmd = new SqlCommand("Sp_RTAListaCatalogoCombo", cnx);
                cmd.Parameters.AddWithValue("@IdTipoCatalogo", idTipoCatalogo);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                cmbEstados = new List<EntCombo>();
                while (dr.Read())
                {
                    EntCombo estado = new EntCombo();

                    estado.Id = dr["IdCatalogo"].ToString();
                    estado.Valor = dr["Descripcion"].ToString();

                    cmbEstados.Add(estado);
                }

            }
            catch (Exception ex)
            {
                cmbEstados = null;
            }
            finally
            {
                cmd.Connection.Close();
            }
            return cmbEstados;
        }

        public static List<EntCombo> RTAListaCatalogoTodosCombo(Int32 idTipoCatalogo)
        {
            List<EntCombo> cmbEstados = null;
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();

                SqlConnection cnx = cn.conectar();
                cnx.Open();
                cmd = new SqlCommand("Sp_RTAListaCatalogoTodosCombo", cnx);
                cmd.Parameters.AddWithValue("@IdTipoCatalogo", idTipoCatalogo);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                cmbEstados = new List<EntCombo>();
                while (dr.Read())
                {
                    EntCombo estado = new EntCombo();

                    estado.Id = dr["IdCatalogo"].ToString();
                    estado.Valor = dr["Descripcion"].ToString();

                    cmbEstados.Add(estado);
                }

            }
            catch (Exception ex)
            {
                cmbEstados = null;
            }
            finally
            {
                cmd.Connection.Close();
            }
            return cmbEstados;
        }

        public static List<EntCombo> RTAListaCatalogoFechaCombo(Int32 idTipoCatalogo)
        {
            List<EntCombo> cmbEstados = null;
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();

                SqlConnection cnx = cn.conectar();
                cnx.Open();
                cmd = new SqlCommand("Sp_RTAListaCatalogoFechaCombo", cnx);
                cmd.Parameters.AddWithValue("@IdTipoCatalogo", idTipoCatalogo);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                cmbEstados = new List<EntCombo>();
                while (dr.Read())
                {
                    EntCombo estado = new EntCombo();

                    estado.Id = dr["IdCatalogo"].ToString();
                    estado.Valor = dr["Descripcion"].ToString();

                    cmbEstados.Add(estado);
                }

            }
            catch (Exception ex)
            {
                cmbEstados = null;
            }
            finally
            {
                cmd.Connection.Close();
            }
            return cmbEstados;
        }

        public static List<EntCombo> RTAListaCatalogoAprobacionCombo(Int32 idTipoCatalogo)
        {
            List<EntCombo> cmbEstados = null;
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();

                SqlConnection cnx = cn.conectar();
                cnx.Open();
                cmd = new SqlCommand("Sp_RTAListaCatalogoAprobacionCombo", cnx);
                cmd.Parameters.AddWithValue("@IdTipoCatalogo", idTipoCatalogo);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                cmbEstados = new List<EntCombo>();
                while (dr.Read())
                {
                    EntCombo estado = new EntCombo();

                    estado.Id = dr["IdCatalogo"].ToString();
                    estado.Valor = dr["Descripcion"].ToString();

                    cmbEstados.Add(estado);
                }

            }
            catch (Exception ex)
            {
                cmbEstados = null;
            }
            finally
            {
                cmd.Connection.Close();
            }
            return cmbEstados;
        }

        public static DataSet RTAConsultaCatalogoPorPadre(Int32 IdPadre, Int32 IdItem)
        {
            SqlCommand cmd = null;
            DataSet Temp = new DataSet();
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cmd = new SqlCommand("Sp_RTAConsultaCatalogoPorPadre", cnx);
                cmd.Parameters.AddWithValue("@IdPadre", IdPadre);
                cmd.Parameters.AddWithValue("@IdItem", IdItem);
                cmd.CommandType = CommandType.StoredProcedure;
                cnx.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(Temp);
            }
            catch (Exception ex)
            {
                Temp = null;
            }
            finally
            {
                cmd.Connection.Close();
            }
            return Temp;
        }

    }
}
