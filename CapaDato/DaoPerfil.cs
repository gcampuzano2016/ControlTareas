using CapaEntidad;
using System;
using System.Collections.Generic;
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
        public static EntRespuesta GuardarContacto(string codUsuario, string codAutor,
                                                   EntPerfilContacto contacto, string ip)
        {
            int r = EjecutarEscritura("Sp_RTA_PerfilGuardarContacto", cmd =>
            {
                cmd.Parameters.Add("@Cod_Usuario",      SqlDbType.VarChar,  50).Value = codUsuario;
                cmd.Parameters.Add("@CorreoPersonal",   SqlDbType.VarChar, 150).Value = contacto.CorreoPersonal;
                cmd.Parameters.Add("@TelefonoPersonal", SqlDbType.VarChar,  50).Value = contacto.TelefonoPersonal;
                cmd.Parameters.Add("@Direccion",        SqlDbType.VarChar, 400).Value = contacto.Direccion;
                cmd.Parameters.Add("@EstadoCivil",      SqlDbType.VarChar, 100).Value = contacto.EstadoCivil;
                cmd.Parameters.Add("@Ip",               SqlDbType.VarChar,  64).Value = ip ?? "";
                cmd.Parameters.Add("@Usu_Accion",       SqlDbType.VarChar,  50).Value = codAutor ?? "";
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
        public static EntRespuesta GuardarEmergencia(string codUsuario, string codAutor,
                                                     EntPerfilEmergencia c, string ip)
        {
            int r = EjecutarEscritura("Sp_RTA_PerfilGuardarEmergencia", cmd =>
            {
                cmd.Parameters.Add("@Cod_Usuario", SqlDbType.VarChar,  50).Value = codUsuario;
                cmd.Parameters.Add("@IdContacto",  SqlDbType.Int).Value          = c.IdContacto;
                cmd.Parameters.Add("@Nombre",      SqlDbType.VarChar, 150).Value = c.Nombre;
                cmd.Parameters.Add("@Parentesco",  SqlDbType.VarChar,  50).Value = c.Parentesco;
                cmd.Parameters.Add("@Telefono",    SqlDbType.VarChar,  50).Value = c.Telefono;
                cmd.Parameters.Add("@Ip",          SqlDbType.VarChar,  64).Value = ip ?? "";
                cmd.Parameters.Add("@Usu_Accion",  SqlDbType.VarChar,  50).Value = codAutor ?? "";
            });

            return RespuestaDe(r, "Contacto de emergencia guardado.", "No se encontró ese contacto de emergencia.");
        }

        /// <summary>
        /// Borrado logico de un contacto de emergencia.
        ///
        /// Misma traduccion del -2 que GuardarEmergencia y por la misma razon:
        /// el borrado tambien queda cerrado cuando el Cod_Usuario esta repetido.
        /// </summary>
        public static EntRespuesta EliminarEmergencia(string codUsuario, string codAutor, int idContacto, string ip)
        {
            int r = EjecutarEscritura("Sp_RTA_PerfilEliminarEmergencia", cmd =>
            {
                cmd.Parameters.Add("@Cod_Usuario", SqlDbType.VarChar, 50).Value = codUsuario;
                cmd.Parameters.Add("@IdContacto",  SqlDbType.Int).Value         = idContacto;
                cmd.Parameters.Add("@Ip",          SqlDbType.VarChar, 64).Value = ip ?? "";
                cmd.Parameters.Add("@Usu_Accion",  SqlDbType.VarChar, 50).Value = codAutor ?? "";
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

        /// <summary>
        /// Como EjecutarEscritura, pero ademas devuelve el identificador que el
        /// procedimiento entrega junto a Respuestas. Existe para que quien escribe
        /// pueda deshacer lo escrito: sin el Id, una fila recien insertada no se
        /// puede compensar si el paso siguiente falla.
        /// </summary>
        private static int EjecutarEscrituraConId(string procedimiento, Action<SqlCommand> ponerParametros,
                                                  string columnaId, out int id)
        {
            int resultado = -1;
            id = 0;
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand(procedimiento, cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                ponerParametros(cmd);
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        resultado = Convert.ToInt32(dr["Respuestas"]);
                        if (dr[columnaId] != System.DBNull.Value)
                        {
                            id = Convert.ToInt32(dr[columnaId]);
                        }
                    }
                }
            }

            return resultado;
        }

        public static EntRespuesta GuardarEstudio(string codUsuario, string codAutor, EntPerfilEstudio e, string ip)
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
                cmd.Parameters.Add("@Usu_Accion",     SqlDbType.VarChar,  50).Value = codAutor ?? "";
            });

            return RespuestaDe(r, "Estudio guardado.", "No se encontró ese estudio.");
        }

        public static EntRespuesta EliminarEstudio(string codUsuario, string codAutor, int idEstudio, string ip)
        {
            int r = EjecutarEscritura("Sp_RTA_PerfilEliminarEstudio", cmd =>
            {
                cmd.Parameters.Add("@Cod_Usuario", SqlDbType.VarChar, 50).Value = codUsuario;
                cmd.Parameters.Add("@IdEstudio",   SqlDbType.Int).Value         = idEstudio;
                cmd.Parameters.Add("@Ip",          SqlDbType.VarChar, 64).Value = ip ?? "";
                cmd.Parameters.Add("@Usu_Accion",  SqlDbType.VarChar, 50).Value = codAutor ?? "";
            });

            return RespuestaDe(r, "Estudio eliminado.", "No se encontró ese estudio.");
        }

        public static EntRespuesta GuardarCertificacion(string codUsuario, string codAutor,
                                                        EntPerfilCertificacion c, string ip)
        {
            int r = EjecutarEscritura("Sp_RTA_PerfilGuardarCertificacion", cmd =>
            {
                cmd.Parameters.Add("@Cod_Usuario",     SqlDbType.VarChar,  50).Value = codUsuario;
                cmd.Parameters.Add("@IdCertificacion", SqlDbType.Int).Value          = c.IdCertificacion;
                cmd.Parameters.Add("@Nombre",          SqlDbType.VarChar, 200).Value = c.Nombre;
                cmd.Parameters.Add("@Entidad",         SqlDbType.VarChar, 200).Value = c.Entidad;
                cmd.Parameters.Add("@FechaObtencion",  SqlDbType.VarChar,  10).Value = c.FechaObtencion ?? "";
                cmd.Parameters.Add("@Ip",              SqlDbType.VarChar,  64).Value = ip ?? "";
                cmd.Parameters.Add("@Usu_Accion",      SqlDbType.VarChar,  50).Value = codAutor ?? "";
            });

            return RespuestaDe(r, "Certificación guardada.", "No se encontró esa certificación.");
        }

        public static EntRespuesta EliminarCertificacion(string codUsuario, string codAutor,
                                                         int idCertificacion, string ip)
        {
            int r = EjecutarEscritura("Sp_RTA_PerfilEliminarCertificacion", cmd =>
            {
                cmd.Parameters.Add("@Cod_Usuario",     SqlDbType.VarChar, 50).Value = codUsuario;
                cmd.Parameters.Add("@IdCertificacion", SqlDbType.Int).Value         = idCertificacion;
                cmd.Parameters.Add("@Ip",              SqlDbType.VarChar, 64).Value = ip ?? "";
                cmd.Parameters.Add("@Usu_Accion",      SqlDbType.VarChar, 50).Value = codAutor ?? "";
            });

            return RespuestaDe(r, "Certificación eliminada.", "No se encontró esa certificación.");
        }

        public static EntRespuesta GuardarExperiencia(string codUsuario, string codAutor,
                                                      EntPerfilExperiencia x, string ip)
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
                cmd.Parameters.Add("@Usu_Accion",    SqlDbType.VarChar,  50).Value = codAutor ?? "";
            });

            return RespuestaDe(r, "Experiencia guardada.", "No se encontró esa experiencia.");
        }

        public static EntRespuesta EliminarExperiencia(string codUsuario, string codAutor, int idExperiencia, string ip)
        {
            int r = EjecutarEscritura("Sp_RTA_PerfilEliminarExperiencia", cmd =>
            {
                cmd.Parameters.Add("@Cod_Usuario",   SqlDbType.VarChar, 50).Value = codUsuario;
                cmd.Parameters.Add("@IdExperiencia", SqlDbType.Int).Value         = idExperiencia;
                cmd.Parameters.Add("@Ip",            SqlDbType.VarChar, 64).Value = ip ?? "";
                cmd.Parameters.Add("@Usu_Accion",    SqlDbType.VarChar, 50).Value = codAutor ?? "";
            });

            return RespuestaDe(r, "Experiencia eliminada.", "No se encontró esa experiencia.");
        }

        public static EntRespuesta GuardarCargaFamiliar(string codUsuario, string codAutor,
                                                        EntPerfilCargaFamiliar c, string ip)
        {
            int r = EjecutarEscritura("Sp_RTA_PerfilGuardarCargaFamiliar", cmd =>
            {
                cmd.Parameters.Add("@Cod_Usuario",     SqlDbType.VarChar,  50).Value = codUsuario;
                cmd.Parameters.Add("@IdCargaFam",      SqlDbType.Int).Value          = c.IdCargaFam;
                cmd.Parameters.Add("@Nombre",          SqlDbType.VarChar, 150).Value = c.Nombre;
                cmd.Parameters.Add("@Parentesco",      SqlDbType.VarChar,  50).Value = c.Parentesco;
                cmd.Parameters.Add("@FechaNacimiento", SqlDbType.VarChar,  10).Value = c.FechaNacimiento ?? "";
                cmd.Parameters.Add("@Ip",              SqlDbType.VarChar,  64).Value = ip ?? "";
                cmd.Parameters.Add("@Usu_Accion",      SqlDbType.VarChar,  50).Value = codAutor ?? "";
            });

            return RespuestaDe(r, "Carga familiar guardada.", "No se encontró esa carga familiar.");
        }

        public static EntRespuesta EliminarCargaFamiliar(string codUsuario, string codAutor, int idCargaFam, string ip)
        {
            int r = EjecutarEscritura("Sp_RTA_PerfilEliminarCargaFamiliar", cmd =>
            {
                cmd.Parameters.Add("@Cod_Usuario", SqlDbType.VarChar, 50).Value = codUsuario;
                cmd.Parameters.Add("@IdCargaFam",  SqlDbType.Int).Value         = idCargaFam;
                cmd.Parameters.Add("@Ip",          SqlDbType.VarChar, 64).Value = ip ?? "";
                cmd.Parameters.Add("@Usu_Accion",  SqlDbType.VarChar, 50).Value = codAutor ?? "";
            });

            return RespuestaDe(r, "Carga familiar eliminada.", "No se encontró esa carga familiar.");
        }

        /// <summary>
        /// Guarda o reemplaza la foto. Una fila por persona: no hay historial.
        /// </summary>
        public static EntRespuesta GuardarFoto(string codUsuario, string codAutor, EntPerfilFoto foto, string ip)
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
                cmd.Parameters.Add("@Usu_Accion",  SqlDbType.VarChar, 50).Value = codAutor ?? "";
            });

            return RespuestaDe(r, "Su foto se actualizó.", "No se pudo guardar la foto.");
        }

        /// <summary>
        /// Quita la foto. Borrado fisico -Perfil_Foto no tiene columna Estado-,
        /// a diferencia del resto del modulo.
        /// </summary>
        public static EntRespuesta EliminarFoto(string codUsuario, string codAutor, string ip)
        {
            /* codAutor se recibe y no se usa, a proposito.
               Sp_RTA_PerfilEliminarFoto hace un DELETE fisico sobre Perfil_Foto:
               borrada la fila, no hay columna donde anotar al autor. El parametro
               esta para que esta capa no tenga una excepcion de firma que haya que
               recordar en cada sitio que la llama. */
            int r = EjecutarEscritura("Sp_RTA_PerfilEliminarFoto", cmd =>
            {
                cmd.Parameters.Add("@Cod_Usuario", SqlDbType.VarChar, 50).Value = codUsuario;
                cmd.Parameters.Add("@Ip",          SqlDbType.VarChar, 64).Value = ip ?? "";
            });

            return RespuestaDe(r, "Su foto se quitó.", "No tenía ninguna foto guardada.");
        }

        /// <summary>
        /// Registra un documento de respaldo.
        ///
        /// El procedimiento devuelve -1 cuando el IdOrigen no es de esta persona:
        /// es la unica comprobacion posible de que el documento se cuelga de algo
        /// suyo, porque el handler ve un numero y no sabe de quien es.
        /// </summary>
        public static EntRespuesta GuardarDocumento(string codUsuario, string codAutor,
                                                    EntPerfilDocumento doc, string ip)
        {
            int idDocumento;
            int r = EjecutarEscrituraConId("Sp_RTA_PerfilGuardarDocumento", cmd =>
            {
                cmd.Parameters.Add("@Cod_Usuario",         SqlDbType.VarChar,  50).Value = codUsuario;
                cmd.Parameters.Add("@Origen",              SqlDbType.VarChar,  20).Value = doc.Origen;
                cmd.Parameters.Add("@IdOrigen",            SqlDbType.Int).Value          = doc.IdOrigen;
                cmd.Parameters.Add("@NombreArchivo",       SqlDbType.VarChar, 260).Value = doc.NombreArchivo;
                cmd.Parameters.Add("@NombreArchivoCodigo", SqlDbType.VarChar, 260).Value = doc.NombreArchivoCodigo;
                cmd.Parameters.Add("@Ruta",                SqlDbType.VarChar, 400).Value = doc.Ruta;
                cmd.Parameters.Add("@Ip",                  SqlDbType.VarChar,  64).Value = ip ?? "";
                cmd.Parameters.Add("@Usu_Accion",          SqlDbType.VarChar,  50).Value = codAutor ?? "";
            }, "IdDocumento", out idDocumento);

            /* El Id se devuelve por la entidad -y no como parametro nuevo, para no
               tocar la firma de este metodo ni la de NegPerfil.GuardarDocumento-
               para que el handler pueda compensar: si el SaveAs en disco falla
               despues de este insert, necesita saber que fila dar de baja. */
            doc.IdDocumento = idDocumento;

            return RespuestaDe(r, "Documento adjuntado.", "No se encontró el registro al que quiere adjuntarlo.");
        }

        /// <summary>Borrado logico de un documento. El archivo se queda en el disco.</summary>
        public static EntRespuesta EliminarDocumento(string codUsuario, string codAutor, int idDocumento, string ip)
        {
            int r = EjecutarEscritura("Sp_RTA_PerfilEliminarDocumento", cmd =>
            {
                cmd.Parameters.Add("@Cod_Usuario", SqlDbType.VarChar, 50).Value = codUsuario;
                cmd.Parameters.Add("@IdDocumento", SqlDbType.Int).Value         = idDocumento;
                cmd.Parameters.Add("@Ip",          SqlDbType.VarChar, 64).Value = ip ?? "";
                cmd.Parameters.Add("@Usu_Accion",  SqlDbType.VarChar, 50).Value = codAutor ?? "";
            });

            return RespuestaDe(r, "Documento quitado.", "No se encontró ese documento.");
        }

        /// <summary>
        /// Los datos de archivo de un documento, SOLO si es de esta persona y
        /// sigue activo. Devuelve null en cualquier otro caso.
        ///
        /// Este null es la guarda de la descarga entera: el handler no decide
        /// nada, pregunta. Devolver un objeto vacio en vez de null seria peor
        /// -habria que acordarse de mirar si viene vacio-, y una excepcion
        /// convertiria en error lo que tambien es el caso normal de un enlace
        /// viejo a un documento ya quitado.
        /// </summary>
        public static EntPerfilDocumento ObtenerDocumento(string codUsuario, int idDocumento)
        {
            EntPerfilDocumento doc = null;
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_PerfilDocumentoArchivo", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Cod_Usuario", SqlDbType.VarChar, 50).Value = codUsuario;
                cmd.Parameters.Add("@IdDocumento", SqlDbType.Int).Value         = idDocumento;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        doc = new EntPerfilDocumento
                        {
                            IdDocumento         = idDocumento,
                            NombreArchivo       = Texto(dr, "NombreArchivo"),
                            NombreArchivoCodigo = Texto(dr, "NombreArchivoCodigo"),
                            Ruta                = Texto(dr, "Ruta")
                        };
                    }
                }
            }

            return doc;
        }

        /// <summary>
        /// El equipo directo de una jefatura, filtrado por texto en el servidor.
        ///
        /// Sin paginacion a proposito: el equipo mas grande de la empresa es de
        /// 49 personas y una lista de ese tamanio no necesita paginarse. Si algun
        /// dia deja de ser cierto, se vera en esta consulta antes que en ningun
        /// otro sitio.
        /// </summary>
        public static List<EntPerfilEquipoItem> ListaEquipo(string codJefe, string filtro)
        {
            List<EntPerfilEquipoItem> lista = new List<EntPerfilEquipoItem>();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_PerfilEquipoLista", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Cod_Jefe", SqlDbType.VarChar,  50).Value = codJefe;
                cmd.Parameters.Add("@Filtro",   SqlDbType.VarChar, 100).Value = filtro ?? string.Empty;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new EntPerfilEquipoItem
                        {
                            CodUsuario     = Texto(dr, "CodUsuario"),
                            NombreCompleto = Texto(dr, "NombreCompleto"),
                            Cargo          = Texto(dr, "Cargo"),
                            Area           = Texto(dr, "Area"),
                            Ciudad         = Texto(dr, "Ciudad")
                        });
                    }
                }
            }

            return lista;
        }

        /// <summary>
        /// Todo el personal activo que calce con el filtro.
        ///
        /// Devuelve EntPerfilEquipoItem y no una entidad propia porque las cinco
        /// columnas son las mismas que las de la lista de equipo: codigo, nombre,
        /// cargo, area y ciudad. Una entidad nueva identica seria dos sitios donde
        /// agregar una columna en vez de uno.
        ///
        /// No comprueba perfiles: eso lo hace AdministrarPerfil.ashx.cs antes de
        /// llegar aca. Esta capa no conoce la sesion.
        ///
        /// Sin paginacion, igual que ListaEquipo: el procedimiento exige dos
        /// caracteres de filtro, asi que nunca devuelve la plantilla entera.
        /// </summary>
        public static List<EntPerfilEquipoItem> ListaPersonal(string filtro)
        {
            List<EntPerfilEquipoItem> lista = new List<EntPerfilEquipoItem>();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_PerfilPersonalLista", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Filtro", SqlDbType.VarChar, 100).Value = filtro ?? string.Empty;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new EntPerfilEquipoItem
                        {
                            CodUsuario     = Texto(dr, "CodUsuario"),
                            NombreCompleto = Texto(dr, "NombreCompleto"),
                            Cargo          = Texto(dr, "Cargo"),
                            Area           = Texto(dr, "Area"),
                            Ciudad         = Texto(dr, "Ciudad")
                        });
                    }
                }
            }

            return lista;
        }

        /// <summary>
        /// El perfil de un subordinado, tal como lo ve su jefatura.
        ///
        /// Recorre SEIS result sets por posicion, igual que CargarPerfil. El
        /// procedimiento devuelve los seis siempre, aunque vacios: por eso aqui
        /// no hay ningun atajo que se salte los restantes cuando la cabecera
        /// viene sin filas.
        ///
        /// Cero filas en la cabecera NO es un error que haya que distinguir:
        /// significa que esa persona no le reporta a quien pregunta, o que
        /// alguno de los dos codigos esta repetido. Los dos casos salen igual
        /// -PerfilEncontrado en false- a proposito.
        /// </summary>
        public static EntPerfilEquipo PerfilEquipo(string codJefe, string codUsuario)
        {
            EntPerfilEquipo perfil = new EntPerfilEquipo();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_PerfilEquipo", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Cod_Jefe",    SqlDbType.VarChar, 50).Value = codJefe;
                cmd.Parameters.Add("@Cod_Usuario", SqlDbType.VarChar, 50).Value = codUsuario;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    /* 1. cabecera recortada */
                    if (dr.Read())
                    {
                        perfil.PerfilEncontrado            = true;
                        perfil.Cabecera.CodUsuario         = Texto(dr, "CodUsuario");
                        perfil.Cabecera.NombreCompleto     = Texto(dr, "NombreCompleto");
                        perfil.Cabecera.Cargo              = Texto(dr, "Cargo");
                        perfil.Cabecera.Area               = Texto(dr, "Area");
                        perfil.Cabecera.Ciudad             = Texto(dr, "Ciudad");
                        perfil.Cabecera.CorreoNotificacion = Texto(dr, "CorreoNotificacion");
                        perfil.Cabecera.JefeInmediato      = Texto(dr, "JefeInmediato");
                        perfil.Cabecera.Horario            = Texto(dr, "Horario");
                    }

                    /* 2. contactos de emergencia */
                    if (dr.NextResult())
                    {
                        while (dr.Read())
                        {
                            perfil.Emergencia.Add(new EntPerfilEmergencia
                            {
                                IdContacto = EnteroDe(dr, "IdContacto"),
                                Nombre     = Texto(dr, "Nombre"),
                                Parentesco = Texto(dr, "Parentesco"),
                                Telefono   = Texto(dr, "Telefono")
                            });
                        }
                    }

                    /* 3. estudios */
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

                    /* 4. certificaciones */
                    if (dr.NextResult())
                    {
                        while (dr.Read())
                        {
                            perfil.Certificaciones.Add(new EntPerfilCertificacion
                            {
                                IdCertificacion = EnteroDe(dr, "IdCertificacion"),
                                Nombre          = Texto(dr, "Nombre"),
                                Entidad         = Texto(dr, "Entidad"),
                                FechaObtencion  = FechaMesDe(dr, "FechaObtencion")
                            });
                        }
                    }

                    /* 5. experiencia */
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

                    /* 6. foto */
                    if (dr.NextResult() && dr.Read())
                    {
                        string base64 = Texto(dr, "FotoBase64");

                        if (base64 != "")
                        {
                            string tipo = Texto(dr, "FotoTipo");
                            if (tipo == "") { tipo = "image/jpeg"; }

                            /* Solo DataUri, igual que en CargarPerfil: las tres
                               propiedades juntas duplicarian el JSON por nada. */
                            perfil.Foto.DataUri = "data:" + tipo + ";base64," + base64;
                        }
                    }
                }
            }

            return perfil;
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
