using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CapaDato
{
    /// <summary>
    /// Acceso a datos del certificado laboral. Parametros tipados, como
    /// DaoPerfil: Cod_Usuario es varchar(50) y AddWithValue lo mandaria como
    /// nvarchar.
    ///
    /// No elige el sueldo vigente ni arma nada: eso es de CapaNegocio, y la
    /// dependencia va de alla para aca.
    /// </summary>
    public class DaoCertificadoLaboral
    {
        public static EntCertificadoDatos Datos(string codUsuario)
        {
            EntCertificadoDatos d = new EntCertificadoDatos();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_PerfilCertificadoDatos", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Cod_Usuario", SqlDbType.VarChar, 50).Value = codUsuario ?? "";
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        d.Encontrado   = true;
                        d.EnMaestro    = Convert.ToBoolean(dr["EnMaestro"]);
                        d.CodUsuario   = Texto(dr, "Cod_Usuario");
                        d.Cedula       = Texto(dr, "Cedula");
                        d.Nombre       = Texto(dr, "Nombre");
                        d.Cargo        = Texto(dr, "Cargo");
                        d.Empresa      = Texto(dr, "Empresa");
                        d.FechaIngreso = FechaNula(dr, "FechaIngreso");
                        d.Ciudad       = Texto(dr, "Ciudad");
                    }

                    if (dr.NextResult())
                    {
                        while (dr.Read())
                        {
                            d.Salarios.Add(new EntHeSalario
                            {
                                Monto              = Convert.ToDecimal(dr["Monto"]),
                                FechaVigenciaDesde = Convert.ToDateTime(dr["FechaVigenciaDesde"]),
                                Origen             = Texto(dr, "Origen")
                            });
                        }
                    }
                }
            }

            return d;
        }

        /// <summary>Guarda la foto y devuelve su Id, o 0 si no se guardo.</summary>
        public static int Registrar(EntCertificadoLaboral c, string ip)
        {
            int id = 0;
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_PerfilCertificadoRegistrar", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Cod_Usuario",  SqlDbType.VarChar,   50).Value = c.CodUsuario;
                cmd.Parameters.Add("@Tipo",         SqlDbType.VarChar,    3).Value = c.Tipo;
                cmd.Parameters.Add("@Empresa",      SqlDbType.VarChar,   20).Value = c.Empresa;
                cmd.Parameters.Add("@Nombre",       SqlDbType.NVarChar, 400).Value = c.Nombre;
                cmd.Parameters.Add("@Cedula",       SqlDbType.VarChar,   20).Value = c.Cedula;
                cmd.Parameters.Add("@Cargo",        SqlDbType.NVarChar, 200).Value = c.Cargo;
                cmd.Parameters.Add("@FechaIngreso", SqlDbType.Date).Value          = c.FechaIngreso.Value;

                SqlParameter salario = cmd.Parameters.Add("@Salario", SqlDbType.Decimal);
                salario.Precision = 18;
                salario.Scale = 2;
                salario.Value = c.Salario.HasValue ? (object)c.Salario.Value : DBNull.Value;

                cmd.Parameters.Add("@Ciudad",       SqlDbType.NVarChar, 100).Value = c.Ciudad;
                cmd.Parameters.Add("@Usu_Solicita", SqlDbType.VarChar,   50).Value = c.UsuSolicita;
                cmd.Parameters.Add("@Ip",           SqlDbType.VarChar,   64).Value = ip ?? "";
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read() && Convert.ToInt32(dr["Respuestas"]) == 0 && dr["IdCertificado"] != DBNull.Value)
                    {
                        id = Convert.ToInt32(dr["IdCertificado"]);
                    }
                }
            }

            return id;
        }

        /// <summary>codUsuario vacio = todos. El sueldo no viene en la lista.</summary>
        public static List<EntCertificadoLaboral> Listar(string codUsuario)
        {
            List<EntCertificadoLaboral> lista = new List<EntCertificadoLaboral>();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_PerfilCertificadoListar", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Cod_Usuario", SqlDbType.VarChar, 50).Value = codUsuario ?? "";
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new EntCertificadoLaboral
                        {
                            IdCertificado  = Convert.ToInt32(dr["IdCertificado"]),
                            CodUsuario     = Texto(dr, "Cod_Usuario"),
                            Tipo           = Texto(dr, "Tipo"),
                            Empresa        = Texto(dr, "Empresa"),
                            Nombre         = Texto(dr, "Nombre"),
                            Cedula         = Texto(dr, "Cedula"),
                            Cargo          = Texto(dr, "Cargo"),
                            FechaSolicitud = Convert.ToDateTime(dr["Fec_Solicitud"]),
                            UsuSolicita    = Texto(dr, "Usu_Solicita")
                        });
                    }
                }
            }

            return lista;
        }

        /// <summary>
        /// La foto completa, o null si no existe o -con codUsuario- no es de esa
        /// persona. codUsuario vacio = sin filtro de dueno.
        /// </summary>
        public static EntCertificadoLaboral Obtener(int idCertificado, string codUsuario)
        {
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_PerfilCertificadoObtener", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@IdCertificado", SqlDbType.Int).Value        = idCertificado;
                cmd.Parameters.Add("@Cod_Usuario",   SqlDbType.VarChar, 50).Value = codUsuario ?? "";
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (!dr.Read()) { return null; }

                    return new EntCertificadoLaboral
                    {
                        IdCertificado  = Convert.ToInt32(dr["IdCertificado"]),
                        CodUsuario     = Texto(dr, "Cod_Usuario"),
                        Tipo           = Texto(dr, "Tipo"),
                        Empresa        = Texto(dr, "Empresa"),
                        Nombre         = Texto(dr, "Nombre"),
                        Cedula         = Texto(dr, "Cedula"),
                        Cargo          = Texto(dr, "Cargo"),
                        FechaIngreso   = FechaNula(dr, "FechaIngreso"),
                        Salario        = dr["Salario"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(dr["Salario"]),
                        Ciudad         = Texto(dr, "Ciudad"),
                        FechaSolicitud = Convert.ToDateTime(dr["Fec_Solicitud"]),
                        UsuSolicita    = Texto(dr, "Usu_Solicita")
                    };
                }
            }
        }

        private static string Texto(SqlDataReader dr, string columna)
        {
            return dr[columna] == DBNull.Value ? "" : dr[columna].ToString().Trim();
        }

        private static DateTime? FechaNula(SqlDataReader dr, string columna)
        {
            return dr[columna] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr[columna]);
        }
    }
}
