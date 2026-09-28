using CapaEntidad;
using System;
using System.Data;
using System.Data.SqlClient;

namespace CapaDato
{
    /// <summary>
    /// Los dos procedimientos de la calificacion. El de lista devuelve dos
    /// resultados en este orden: oportunidades y tasas de exito.
    /// </summary>
    public class DaoCalificacionForeCast
    {
        public static EntDatosCalificacion Cargar(string idUsuario)
        {
            EntDatosCalificacion d = new EntDatosCalificacion();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTAListaCalificacionForeCast", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 90;
                cmd.Parameters.Add("@Idusuario", SqlDbType.VarChar, 10).Value = idUsuario ?? "";
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        EntCalificacionForeCast o = new EntCalificacionForeCast
                        {
                            IdForeCast            = Convert.ToInt64(dr["IdForeCast"]),
                            IdCliente             = EnteroNulo(dr, "IdCliente") ?? 0,
                            Cliente               = Texto(dr, "Cliente"),
                            DetalleProyecto       = Texto(dr, "DetalleProyecto"),
                            Marca                 = Texto(dr, "Marca"),
                            Prioridad             = Texto(dr, "Prioridad"),
                            GerentedeCuenta       = Texto(dr, "GerentedeCuenta"),
                            Sucursal              = Texto(dr, "Sucursal"),
                            SegmentodeMercado     = Texto(dr, "SegmentodeMercado"),
                            TipoEmpresa           = Texto(dr, "TipoEmpresa"),
                            PVPEstimado           = DecimalNulo(dr, "PVPEstimado"),
                            UtilidadBrutaEstimada = DecimalNulo(dr, "UtilidadBrutaEstimada"),
                            IdNegocio             = EnteroNulo(dr, "IdNegocio"),
                            Porcentaje            = DecimalNulo(dr, "Porcentaje"),
                            DescripcionNegocio    = Texto(dr, "DescripcionNegocio"),
                            FechaCierre           = SinCentinela(FechaNula(dr, "FechaCierre")),
                            FechaModificacion     = FechaNula(dr, "FechaModificacion"),
                            Contacto              = Texto(dr, "Contacto"),
                            LiderProyectoCliente  = Texto(dr, "LiderProyectoCliente"),
                            Observaciones         = Texto(dr, "Observaciones"),
                            FechasCierreDistintas = EnteroNulo(dr, "FechasCierreDistintas") ?? 0,
                            GanadosCliente        = EnteroNulo(dr, "GanadosCliente") ?? 0,
                            ContactosCuenta       = EnteroNulo(dr, "ContactosCuenta") ?? 0
                        };

                        if (dr["FechaCalificacion"] != DBNull.Value)
                        {
                            o.Calificacion = new EntCalificacionRespuesta
                            {
                                IdForeCast         = o.IdForeCast,
                                Presupuesto        = EnteroNulo(dr, "Presupuesto") ?? 0,
                                Autoridad          = EnteroNulo(dr, "Autoridad") ?? 0,
                                Necesidad          = EnteroNulo(dr, "Necesidad") ?? 0,
                                DecisorCargo       = Texto(dr, "DecisorCargo"),
                                PropuestaEnviada   = dr["PropuestaEnviada"] != DBNull.Value && Convert.ToBoolean(dr["PropuestaEnviada"]),
                                AccionComprometida = Texto(dr, "AccionComprometida"),
                                FechaAccion        = FechaNula(dr, "FechaAccion"),
                                Usuario            = Texto(dr, "UsuarioCalificacion"),
                                FechaRegistro      = Convert.ToDateTime(dr["FechaCalificacion"])
                            };
                        }

                        d.Oportunidades.Add(o);
                    }

                    if (dr.NextResult())
                    {
                        while (dr.Read())
                        {
                            d.Tasas.Add(new EntTasaExito
                            {
                                Dimension = Texto(dr, "Dimension"),
                                Valor     = Texto(dr, "Valor"),
                                Ganados   = EnteroNulo(dr, "Ganados") ?? 0,
                                Perdidos  = EnteroNulo(dr, "Perdidos") ?? 0
                            });
                        }
                    }
                }
            }

            return d;
        }

        public static EntRespuesta Guardar(EntCalificacionRespuesta r)
        {
            EntRespuesta respuesta = new EntRespuesta();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTAInsertaCalificacionForeCast", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@IdForeCast", SqlDbType.BigInt).Value = r.IdForeCast;
                cmd.Parameters.Add("@Presupuesto", SqlDbType.TinyInt).Value = r.Presupuesto;
                cmd.Parameters.Add("@Autoridad", SqlDbType.TinyInt).Value = r.Autoridad;
                cmd.Parameters.Add("@Necesidad", SqlDbType.TinyInt).Value = r.Necesidad;
                cmd.Parameters.Add("@DecisorCargo", SqlDbType.VarChar, 150).Value = (object)r.DecisorCargo ?? DBNull.Value;
                cmd.Parameters.Add("@PropuestaEnviada", SqlDbType.Bit).Value = r.PropuestaEnviada;
                cmd.Parameters.Add("@AccionComprometida", SqlDbType.VarChar, 300).Value = (object)r.AccionComprometida ?? DBNull.Value;
                cmd.Parameters.Add("@FechaAccion", SqlDbType.Date).Value = r.FechaAccion.HasValue ? (object)r.FechaAccion.Value.Date : DBNull.Value;
                cmd.Parameters.Add("@Usuario", SqlDbType.VarChar, 50).Value = r.Usuario ?? "";
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        respuesta.estado = Convert.ToString(dr["Estado"]);
                        respuesta.mensaje = Texto(dr, "Mensaje");
                    }
                    else
                    {
                        respuesta.estado = "0";
                        respuesta.mensaje = "El procedimiento no devolvio respuesta.";
                    }
                }
            }

            respuesta.tipoMensaje = respuesta.estado == "1" ? "success" : "warning";
            return respuesta;
        }

        private static string Texto(IDataRecord dr, string columna)
        {
            object v = dr[columna];
            return v == DBNull.Value ? "" : Convert.ToString(v).Trim();
        }

        private static int? EnteroNulo(IDataRecord dr, string columna)
        {
            object v = dr[columna];
            return v == DBNull.Value ? (int?)null : Convert.ToInt32(v);
        }

        private static decimal? DecimalNulo(IDataRecord dr, string columna)
        {
            object v = dr[columna];
            return v == DBNull.Value ? (decimal?)null : Convert.ToDecimal(v);
        }

        private static DateTime? FechaNula(IDataRecord dr, string columna)
        {
            object v = dr[columna];
            return v == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(v);
        }

        /// <summary>
        /// Defensa en profundidad para FechaCierre: el SP ya devuelve NULL para el
        /// centinela 1900-01-01, pero si alguien vuelve a exponer la columna cruda
        /// esto evita tratarla como una fecha real. FechaNula se mantiene generica
        /// para las demas columnas de fecha.
        /// </summary>
        private static DateTime? SinCentinela(DateTime? fecha)
        {
            return fecha.HasValue && fecha.Value.Year <= 1900 ? null : fecha;
        }
    }
}
