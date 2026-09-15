using CapaEntidad;
using System;
using System.Data;
using System.Data.SqlClient;

namespace CapaDato
{
    /// <summary>
    /// Acceso a datos del modulo de perfil.
    ///
    /// Las consultas van con parametros tipados (SqlDbType explicito), no con
    /// AddWithValue: en esta base Cod_Usuario es varchar(50) y AddWithValue lo
    /// manda como nvarchar, lo que descarta el indice en tablas grandes.
    ///
    /// Esta clase NO calcula la edad, aunque tenga la fecha a mano: la
    /// dependencia de la solucion va CapaNegocio -> CapaDato, nunca al reves.
    /// Llamar a NegPerfilCampos desde aca seria una referencia circular y no
    /// compila. La edad la pone NegPerfil despues de leer.
    /// </summary>
    public class DaoPerfil
    {
        /// <summary>
        /// Todo el perfil en una sola ida. Recorre los result sets con
        /// NextResult() en el mismo orden en que los declara el procedimiento.
        /// </summary>
        public static EntPerfilCompleto CargarPerfil(string codUsuario)
        {
            EntPerfilCompleto perfil = new EntPerfilCompleto();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_PerfilColaborador", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Cod_Usuario", SqlDbType.VarChar, 50).Value = codUsuario;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    /* 1. cabecera */
                    if (dr.Read())
                    {
                        perfil.PerfilEncontrado            = true;
                        perfil.Cabecera.CodUsuario         = Texto(dr, "Cod_Usuario");
                        perfil.Cabecera.NombreCompleto     = Texto(dr, "NombreCompleto");
                        perfil.Cabecera.Cedula             = Texto(dr, "Cedula");
                        perfil.Cabecera.FechaNacTexto      = Texto(dr, "FechaNacTexto");
                        perfil.Cabecera.Cargo              = Texto(dr, "Cargo");
                        perfil.Cabecera.Area               = Texto(dr, "Area");
                        perfil.Cabecera.Ciudad             = Texto(dr, "Ciudad");
                        perfil.Cabecera.CorreoNotificacion = Texto(dr, "CorreoNotificacion");
                        perfil.Cabecera.JefeInmediato      = Texto(dr, "JefeInmediato");
                        perfil.Cabecera.Horario            = Texto(dr, "Horario");
                        perfil.Cabecera.TieneFicha         = Texto(dr, "TieneFicha") == "1";
                        perfil.Cabecera.EsJefe             = Texto(dr, "EsJefe") == "1";
                    }

                    /* 2. contacto personal */
                    if (dr.NextResult() && dr.Read())
                    {
                        perfil.Contacto.CorreoPersonal   = Texto(dr, "CorreoPersonal");
                        perfil.Contacto.TelefonoPersonal = Texto(dr, "TelefonoPersonal");
                        perfil.Contacto.Direccion        = Texto(dr, "Direccion");
                        perfil.Contacto.EstadoCivil      = Texto(dr, "EstadoCivil");
                    }

                    /* 3. contactos de emergencia */
                    if (dr.NextResult())
                    {
                        while (dr.Read())
                        {
                            perfil.Emergencia.Add(new EntPerfilEmergencia
                            {
                                IdContacto = int.Parse(Texto(dr, "IdContacto")),
                                Nombre     = Texto(dr, "Nombre"),
                                Parentesco = Texto(dr, "Parentesco"),
                                Telefono   = Texto(dr, "Telefono")
                            });
                        }
                    }

                    /* 4. estudios */
                    if (dr.NextResult())
                    {
                        while (dr.Read())
                        {
                            perfil.Estudios.Add(new EntPerfilEstudio
                            {
                                IdEstudio      = EnteroDe(dr, "IdEstudio"),
                                Nivel          = Texto(dr, "Nivel"),
                                Institucion    = Texto(dr, "Institucion"),
                                Titulo         = Texto(dr, "Titulo"),
                                AnioGraduacion = EnteroNuloDe(dr, "AnioGraduacion")
                            });
                        }
                    }

                    /* 5. certificaciones */
                    if (dr.NextResult())
                    {
                        while (dr.Read())
                        {
                            perfil.Certificaciones.Add(new EntPerfilCertificacion
                            {
                                IdCertificacion = EnteroDe(dr, "IdCertificacion"),
                                Nombre          = Texto(dr, "Nombre"),
                                Entidad         = Texto(dr, "Entidad"),
                                /* La fecha viaja como "yyyy-MM" para que el input
                                   type="month" del navegador la reciba tal cual. */
                                FechaObtencion  = FechaMesDe(dr, "FechaObtencion")
                            });
                        }
                    }

                    /* 6. experiencia */
                    if (dr.NextResult())
                    {
                        while (dr.Read())
                        {
                            perfil.Experiencia.Add(new EntPerfilExperiencia
                            {
                                IdExperiencia = EnteroDe(dr, "IdExperiencia"),
                                Empresa       = Texto(dr, "Empresa"),
                                Cargo         = Texto(dr, "Cargo"),
                                AnioDesde     = EnteroNuloDe(dr, "AnioDesde"),
                                AnioHasta     = EnteroNuloDe(dr, "AnioHasta"),
                                Funciones     = Texto(dr, "Funciones")
                            });
                        }
                    }

                    /* 7. documentos de respaldo */
                    if (dr.NextResult())
                    {
                        while (dr.Read())
                        {
                            perfil.Documentos.Add(new EntPerfilDocumento
                            {
                                IdDocumento   = EnteroDe(dr, "IdDocumento"),
                                Origen        = Texto(dr, "Origen"),
                                IdOrigen      = EnteroDe(dr, "IdOrigen"),
                                NombreArchivo = Texto(dr, "NombreArchivo")

                                /* NombreArchivoCodigo y Ruta se dejan vacias a
                                   proposito aunque el conjunto las traiga: esta
                                   lista se serializa entera al navegador y el
                                   nombre del archivo en disco no tiene nada que
                                   hacer alli. La descarga los pide aparte, con
                                   ObtenerDocumento, que ademas comprueba de
                                   quien es el documento. */
                            });
                        }
                    }

                    /* 8. cargas familiares */
                    if (dr.NextResult())
                    {
                        while (dr.Read())
                        {
                            perfil.CargasFamiliares.Add(new EntPerfilCargaFamiliar
                            {
                                IdCargaFam      = EnteroDe(dr, "IdCargaFam"),
                                Nombre          = Texto(dr, "Nombre"),
                                Parentesco      = Texto(dr, "Parentesco"),
                                FechaNacimiento = Texto(dr, "FechaNacTexto")
                            });
                        }
                    }

                    /* 9. foto de perfil. Cero filas es el caso normal: nadie
                       tiene foto el primer dia. */
                    if (dr.NextResult() && dr.Read())
                    {
                        string base64 = Texto(dr, "FotoBase64");

                        if (base64 != "")
                        {
                            string tipo = Texto(dr, "FotoTipo");
                            if (tipo == "") { tipo = "image/jpeg"; }

                            /* Se llena solo DataUri y no Base64 ni Tipo: ver el
                               comentario de EntPerfilFoto. El molde del data URI
                               es el de DaoFirmaUsuario. */
                            perfil.Foto.DataUri = "data:" + tipo + ";base64," + base64;
                        }
                    }
                }
            }

            return perfil;
        }

        /// <summary>
        /// Guarda el contacto editable. Devuelve el resultado listo para el cliente.
        ///
        /// El procedimiento devuelve -2 en Respuestas cuando el Cod_Usuario esta
        /// repetido entre usuarios activos: el mismo caso que Sp_RTA_PerfilColaborador
        /// bloquea en la lectura. Sin esta traduccion, dos sesiones con el mismo
        /// codigo podrian pisarse el contacto sin que ninguna se entere -la lectura
        /// ya esta cerrada para ese caso, pero el guardado no lo estaba-. RespuestaDe
        /// centraliza el mensaje de ese -2; el "No se pudo guardar" de abajo no se
        /// alcanza hoy -el procedimiento solo devuelve 0 o -2-, pero RespuestaDe lo
        /// exige para el caso general.
        /// </summary>
        public static EntRespuesta GuardarContacto(string codUsuario, EntPerfilContacto contacto, string ip)
        {
            int r = EjecutarEscritura("Sp_RTA_PerfilGuardarContacto", cmd =>
            {
                cmd.Parameters.Add("@Cod_Usuario",      SqlDbType.VarChar,  50).Value = codUsuario;
                cmd.Parameters.Add("@CorreoPersonal",   SqlDbType.VarChar, 150).Value = contacto.CorreoPersonal;
                cmd.Parameters.Add("@TelefonoPersonal", SqlDbType.VarChar,  50).Value = contacto.TelefonoPersonal;
                cmd.Parameters.Add("@Direccion",        SqlDbType.VarChar, 400).Value = contacto.Direccion;
                cmd.Parameters.Add("@EstadoCivil",      SqlDbType.VarChar, 100).Value = contacto.EstadoCivil;
                cmd.Parameters.Add("@Ip",               SqlDbType.VarChar,  64).Value = ip ?? "";
            });

            return RespuestaDe(r, "Sus datos de contacto se guardaron correctamente.", "No se pudo guardar el contacto.");
        }

        /// <summary>
        /// Alta o edicion de un contacto de emergencia.
        ///
        /// El procedimiento devuelve -2 en Respuestas cuando el Cod_Usuario esta
        /// repetido entre usuarios activos: el mismo caso que
        /// Sp_RTA_PerfilColaborador y Sp_RTA_PerfilGuardarContacto ya bloquean.
        /// Sin esta traduccion, dos personas distintas compartiendo el mismo
        /// codigo verian y podrian borrar los contactos de emergencia de la
        /// otra -el dato cuyo proposito es que alguien reciba una llamada
        /// cuando hay una urgencia-. RespuestaDe centraliza ese mensaje.
        ///
        /// El unico camino al mensaje de "no encontrado" es un IdContacto que no
        /// es de esta persona, es decir, un intento de editar un contacto ajeno
        /// (idContacto != 0 y no encontrado). El procedimiento soporta la
        /// edicion, pero la pantalla solo ofrece agregar (idContacto = 0) y
        /// quitar: no hay boton ni flujo que arme un GuardarEmergencia con
        /// idContacto > 0, asi que esta rama no se alcanza hoy desde la
        /// interfaz. Se deja lista para cuando se habilite editar.
        /// </summary>
        public static EntRespuesta GuardarEmergencia(string codUsuario, EntPerfilEmergencia c, string ip)
        {
            int r = EjecutarEscritura("Sp_RTA_PerfilGuardarEmergencia", cmd =>
            {
                cmd.Parameters.Add("@Cod_Usuario", SqlDbType.VarChar,  50).Value = codUsuario;
                cmd.Parameters.Add("@IdContacto",  SqlDbType.Int).Value          = c.IdContacto;
                cmd.Parameters.Add("@Nombre",      SqlDbType.VarChar, 150).Value = c.Nombre;
                cmd.Parameters.Add("@Parentesco",  SqlDbType.VarChar,  50).Value = c.Parentesco;
                cmd.Parameters.Add("@Telefono",    SqlDbType.VarChar,  50).Value = c.Telefono;
                cmd.Parameters.Add("@Ip",          SqlDbType.VarChar,  64).Value = ip ?? "";
            });

            return RespuestaDe(r, "Contacto de emergencia guardado.", "No se encontró ese contacto de emergencia.");
        }

        /// <summary>
        /// Borrado logico de un contacto de emergencia.
        ///
        /// Misma traduccion del -2 que GuardarEmergencia y por la misma razon:
        /// el borrado tambien queda cerrado cuando el Cod_Usuario esta repetido.
        /// </summary>
        public static EntRespuesta EliminarEmergencia(string codUsuario, int idContacto, string ip)
        {
            int r = EjecutarEscritura("Sp_RTA_PerfilEliminarEmergencia", cmd =>
            {
                cmd.Parameters.Add("@Cod_Usuario", SqlDbType.VarChar, 50).Value = codUsuario;
                cmd.Parameters.Add("@IdContacto",  SqlDbType.Int).Value         = idContacto;
                cmd.Parameters.Add("@Ip",          SqlDbType.VarChar, 64).Value = ip ?? "";
            });

            return RespuestaDe(r, "Contacto eliminado.", "No se encontró ese contacto.");
        }

        /// <summary>
        /// Traduce el codigo que devuelven los procedimientos del modulo.
        /// 0 correcto, -1 el registro no es suyo o no existe, -2 su Cod_Usuario
        /// esta repetido y no se puede saber de quien seria el dato.
        /// </summary>
        private static EntRespuesta RespuestaDe(int resultado, string mensajeExito, string mensajeNoEncontrado)
        {
            EntRespuesta respuesta = new EntRespuesta();

            if (resultado == -2)
            {
                respuesta.estado = "0";
                respuesta.mensaje = "No pudimos identificar su perfil de forma única. Escriba a Talento Humano para que corrijan su código de usuario.";
                respuesta.tipoMensaje = "warning";
            }
            else if (resultado == 0)
            {
                respuesta.estado = "1";
                respuesta.mensaje = mensajeExito;
                respuesta.tipoMensaje = "success";
            }
            else
            {
                respuesta.estado = "0";
                respuesta.mensaje = mensajeNoEncontrado;
                respuesta.tipoMensaje = "warning";
            }

            return respuesta;
        }

        /// <summary>Ejecuta un procedimiento de escritura y devuelve su Respuestas.</summary>
        private static int EjecutarEscritura(string procedimiento, Action<SqlCommand> ponerParametros)
        {
            int resultado = -1;
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand(procedimiento, cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                ponerParametros(cmd);
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read()) { resultado = Convert.ToInt32(dr["Respuestas"]); }
                }
            }

            return resultado;
        }

        public static EntRespuesta GuardarEstudio(string codUsuario, EntPerfilEstudio e, string ip)
        {
            int r = EjecutarEscritura("Sp_RTA_PerfilGuardarEstudio", cmd =>
            {
                cmd.Parameters.Add("@Cod_Usuario",    SqlDbType.VarChar,  50).Value = codUsuario;
                cmd.Parameters.Add("@IdEstudio",      SqlDbType.Int).Value          = e.IdEstudio;
                cmd.Parameters.Add("@Nivel",          SqlDbType.VarChar,  60).Value = e.Nivel;
                cmd.Parameters.Add("@Institucion",    SqlDbType.VarChar, 200).Value = e.Institucion;
                cmd.Parameters.Add("@Titulo",         SqlDbType.VarChar, 200).Value = e.Titulo;
                cmd.Parameters.Add("@AnioGraduacion", SqlDbType.SmallInt).Value     = e.AnioGraduacion ?? 0;
                cmd.Parameters.Add("@Ip",             SqlDbType.VarChar,  64).Value = ip ?? "";
            });

            return RespuestaDe(r, "Estudio guardado.", "No se encontró ese estudio.");
        }

        public static EntRespuesta EliminarEstudio(string codUsuario, int idEstudio, string ip)
        {
            int r = EjecutarEscritura("Sp_RTA_PerfilEliminarEstudio", cmd =>
            {
                cmd.Parameters.Add("@Cod_Usuario", SqlDbType.VarChar, 50).Value = codUsuario;
                cmd.Parameters.Add("@IdEstudio",   SqlDbType.Int).Value         = idEstudio;
                cmd.Parameters.Add("@Ip",          SqlDbType.VarChar, 64).Value = ip ?? "";
            });

            return RespuestaDe(r, "Estudio eliminado.", "No se encontró ese estudio.");
        }

        public static EntRespuesta GuardarCertificacion(string codUsuario, EntPerfilCertificacion c, string ip)
        {
            int r = EjecutarEscritura("Sp_RTA_PerfilGuardarCertificacion", cmd =>
            {
                cmd.Parameters.Add("@Cod_Usuario",     SqlDbType.VarChar,  50).Value = codUsuario;
                cmd.Parameters.Add("@IdCertificacion", SqlDbType.Int).Value          = c.IdCertificacion;
                cmd.Parameters.Add("@Nombre",          SqlDbType.VarChar, 200).Value = c.Nombre;
                cmd.Parameters.Add("@Entidad",         SqlDbType.VarChar, 200).Value = c.Entidad;
                cmd.Parameters.Add("@FechaObtencion",  SqlDbType.VarChar,  10).Value = c.FechaObtencion ?? "";
                cmd.Parameters.Add("@Ip",              SqlDbType.VarChar,  64).Value = ip ?? "";
            });

            return RespuestaDe(r, "Certificación guardada.", "No se encontró esa certificación.");
        }

        public static EntRespuesta EliminarCertificacion(string codUsuario, int idCertificacion, string ip)
        {
            int r = EjecutarEscritura("Sp_RTA_PerfilEliminarCertificacion", cmd =>
            {
                cmd.Parameters.Add("@Cod_Usuario",     SqlDbType.VarChar, 50).Value = codUsuario;
                cmd.Parameters.Add("@IdCertificacion", SqlDbType.Int).Value         = idCertificacion;
                cmd.Parameters.Add("@Ip",              SqlDbType.VarChar, 64).Value = ip ?? "";
            });

            return RespuestaDe(r, "Certificación eliminada.", "No se encontró esa certificación.");
        }

        public static EntRespuesta GuardarExperiencia(string codUsuario, EntPerfilExperiencia x, string ip)
        {
            int r = EjecutarEscritura("Sp_RTA_PerfilGuardarExperiencia", cmd =>
            {
                cmd.Parameters.Add("@Cod_Usuario",   SqlDbType.VarChar,  50).Value = codUsuario;
                cmd.Parameters.Add("@IdExperiencia", SqlDbType.Int).Value          = x.IdExperiencia;
                cmd.Parameters.Add("@Empresa",       SqlDbType.VarChar, 200).Value = x.Empresa;
                cmd.Parameters.Add("@Cargo",         SqlDbType.VarChar, 200).Value = x.Cargo;
                cmd.Parameters.Add("@AnioDesde",     SqlDbType.SmallInt).Value     = x.AnioDesde ?? 0;
                cmd.Parameters.Add("@AnioHasta",     SqlDbType.SmallInt).Value     = x.AnioHasta ?? 0;
                cmd.Parameters.Add("@Funciones",     SqlDbType.VarChar, -1).Value  = x.Funciones ?? "";
                cmd.Parameters.Add("@Ip",            SqlDbType.VarChar,  64).Value = ip ?? "";
            });

            return RespuestaDe(r, "Experiencia guardada.", "No se encontró esa experiencia.");
        }

        public static EntRespuesta EliminarExperiencia(string codUsuario, int idExperiencia, string ip)
        {
            int r = EjecutarEscritura("Sp_RTA_PerfilEliminarExperiencia", cmd =>
            {
                cmd.Parameters.Add("@Cod_Usuario",   SqlDbType.VarChar, 50).Value = codUsuario;
                cmd.Parameters.Add("@IdExperiencia", SqlDbType.Int).Value         = idExperiencia;
                cmd.Parameters.Add("@Ip",            SqlDbType.VarChar, 64).Value = ip ?? "";
            });

            return RespuestaDe(r, "Experiencia eliminada.", "No se encontró esa experiencia.");
        }

        public static EntRespuesta GuardarCargaFamiliar(string codUsuario, EntPerfilCargaFamiliar c, string ip)
        {
            int r = EjecutarEscritura("Sp_RTA_PerfilGuardarCargaFamiliar", cmd =>
            {
                cmd.Parameters.Add("@Cod_Usuario",     SqlDbType.VarChar,  50).Value = codUsuario;
                cmd.Parameters.Add("@IdCargaFam",      SqlDbType.Int).Value          = c.IdCargaFam;
                cmd.Parameters.Add("@Nombre",          SqlDbType.VarChar, 150).Value = c.Nombre;
                cmd.Parameters.Add("@Parentesco",      SqlDbType.VarChar,  50).Value = c.Parentesco;
                cmd.Parameters.Add("@FechaNacimiento", SqlDbType.VarChar,  10).Value = c.FechaNacimiento ?? "";
                cmd.Parameters.Add("@Ip",              SqlDbType.VarChar,  64).Value = ip ?? "";
            });

            return RespuestaDe(r, "Carga familiar guardada.", "No se encontró esa carga familiar.");
        }

        public static EntRespuesta EliminarCargaFamiliar(string codUsuario, int idCargaFam, string ip)
        {
            int r = EjecutarEscritura("Sp_RTA_PerfilEliminarCargaFamiliar", cmd =>
            {
                cmd.Parameters.Add("@Cod_Usuario", SqlDbType.VarChar, 50).Value = codUsuario;
                cmd.Parameters.Add("@IdCargaFam",  SqlDbType.Int).Value         = idCargaFam;
                cmd.Parameters.Add("@Ip",          SqlDbType.VarChar, 64).Value = ip ?? "";
            });

            return RespuestaDe(r, "Carga familiar eliminada.", "No se encontró esa carga familiar.");
        }

        /// <summary>
        /// Guarda o reemplaza la foto. Una fila por persona: no hay historial.
        /// </summary>
        public static EntRespuesta GuardarFoto(string codUsuario, EntPerfilFoto foto, string ip)
        {
            int r = EjecutarEscritura("Sp_RTA_PerfilGuardarFoto", cmd =>
            {
                cmd.Parameters.Add("@Cod_Usuario", SqlDbType.VarChar, 50).Value = codUsuario;
                /* -1 es el largo que SqlDbType.VarChar usa para VARCHAR(MAX). Sin
                   el, el parametro se trunca a 8000 caracteres y la foto llega
                   cortada: se guarda sin error y no se ve. */
                cmd.Parameters.Add("@FotoBase64",  SqlDbType.VarChar, -1).Value = foto.Base64;
                cmd.Parameters.Add("@FotoTipo",    SqlDbType.VarChar, 50).Value = foto.Tipo;
                cmd.Parameters.Add("@Ip",          SqlDbType.VarChar, 64).Value = ip ?? "";
            });

            return RespuestaDe(r, "Su foto se actualizó.", "No se pudo guardar la foto.");
        }

        /// <summary>
        /// Quita la foto. Borrado fisico -Perfil_Foto no tiene columna Estado-,
        /// a diferencia del resto del modulo.
        /// </summary>
        public static EntRespuesta EliminarFoto(string codUsuario, string ip)
        {
            int r = EjecutarEscritura("Sp_RTA_PerfilEliminarFoto", cmd =>
            {
                cmd.Parameters.Add("@Cod_Usuario", SqlDbType.VarChar, 50).Value = codUsuario;
                cmd.Parameters.Add("@Ip",          SqlDbType.VarChar, 64).Value = ip ?? "";
            });

            return RespuestaDe(r, "Su foto se quitó.", "No tenía ninguna foto guardada.");
        }

        private static string Texto(SqlDataReader dr, string columna)
        {
            return dr[columna] == System.DBNull.Value ? "" : dr[columna].ToString().Trim();
        }

        /// <summary>Entero de una columna que el esquema declara NOT NULL.</summary>
        private static int EnteroDe(SqlDataReader dr, string columna)
        {
            return dr[columna] == System.DBNull.Value ? 0 : Convert.ToInt32(dr[columna]);
        }

        /// <summary>
        /// Entero de una columna que si puede venir nula. Devuelve null y no cero:
        /// para un anio, el cero se leeria como un dato real.
        /// </summary>
        private static int? EnteroNuloDe(SqlDataReader dr, string columna)
        {
            if (dr[columna] == System.DBNull.Value) { return null; }
            return Convert.ToInt32(dr[columna]);
        }

        /// <summary>
        /// Una fecha como texto "yyyy-MM", que es lo que consume un input
        /// type="month". Cadena vacia si la columna viene nula.
        /// </summary>
        private static string FechaMesDe(SqlDataReader dr, string columna)
        {
            if (dr[columna] == System.DBNull.Value) { return ""; }
            return Convert.ToDateTime(dr[columna]).ToString("yyyy-MM",
                       System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
