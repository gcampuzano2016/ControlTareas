using CapaDato;
using CapaEntidad;
using System.Collections.Generic;

namespace CapaNegocio
{
    /// <summary>Fachada del modulo de perfil, igual que el resto de la capa.</summary>
    public class NegPerfil
    {
        /// <summary>
        /// Lee el perfil y le pone la edad.
        ///
        /// La edad se calcula aca y no en el Dao porque NegPerfilCampos vive en
        /// esta capa y CapaDato no puede referenciarla sin cerrar un ciclo. No es
        /// un rodeo: leer y calcular son dos cosas distintas y esta es la capa que
        /// calcula.
        ///
        /// Tampoco se calcula en SQL, que seria el otro lugar tentador: alli el
        /// formato dd/MM/yyyy depende de acertarle al estilo 103 y no hay forma de
        /// probarlo. Aca esta cubierto por NegPerfilCamposTests.
        /// </summary>
        public static EntPerfilCompleto CargarPerfil(string codUsuario)
        {
            EntPerfilCompleto perfil = DaoPerfil.CargarPerfil(codUsuario);

            perfil.Cabecera.Edad =
                NegPerfilCampos.EdadDesdeTexto(perfil.Cabecera.FechaNacTexto);

            return perfil;
        }

        /// <summary>
        /// Guarda el contacto editable de un perfil.
        ///
        /// codUsuario es DE QUIEN es el perfil; codAutor es QUIEN lo esta
        /// tocando. Hasta la edicion por Talento Humano eran siempre el mismo y
        /// por eso habia un solo parametro.
        /// </summary>
        public static EntRespuesta GuardarContacto(string codUsuario, string codAutor,
                                                   EntPerfilContacto contacto, string ip)
        {
            return DaoPerfil.GuardarContacto(codUsuario, codAutor, contacto, ip);
        }

        /// <summary>
        /// Guarda los ocho campos de datos personales. codAutor: ver
        /// GuardarContacto.
        ///
        /// Quien valida es NegPerfilCampos.ValidarDatosPersonales, y lo llama el
        /// handler, no esta fachada: necesita la cabecera guardada para saber que
        /// campos cambiaron, y leerla otra vez aca seria una segunda ida a la
        /// base para repetir una comprobacion que ya se hizo.
        /// </summary>
        public static EntRespuesta GuardarDatosPersonales(string codUsuario, string codAutor,
                                                          EntPerfilDatosPersonales datos, string ip)
        {
            return DaoPerfil.GuardarDatosPersonales(codUsuario, codAutor, datos, ip);
        }

        /// <summary>Los candidatos a jefe inmediato, sin la persona misma.</summary>
        public static List<EntPerfilJefe> ListarJefes(string codUsuario)
        {
            return DaoPerfil.ListarJefes(codUsuario);
        }

        /// <summary>Alta o edicion de un contacto de emergencia. codAutor: ver GuardarContacto.</summary>
        public static EntRespuesta GuardarEmergencia(string codUsuario, string codAutor,
                                                     EntPerfilEmergencia c, string ip)
        {
            return DaoPerfil.GuardarEmergencia(codUsuario, codAutor, c, ip);
        }

        /// <summary>Borrado logico de un contacto de emergencia. codAutor: ver GuardarContacto.</summary>
        public static EntRespuesta EliminarEmergencia(string codUsuario, string codAutor, int idContacto, string ip)
        {
            return DaoPerfil.EliminarEmergencia(codUsuario, codAutor, idContacto, ip);
        }

        public static EntRespuesta GuardarEstudio(string codUsuario, string codAutor, EntPerfilEstudio e, string ip)
        {
            return DaoPerfil.GuardarEstudio(codUsuario, codAutor, e, ip);
        }

        public static EntRespuesta EliminarEstudio(string codUsuario, string codAutor, int idEstudio, string ip)
        {
            return DaoPerfil.EliminarEstudio(codUsuario, codAutor, idEstudio, ip);
        }

        public static EntRespuesta GuardarCertificacion(string codUsuario, string codAutor,
                                                        EntPerfilCertificacion c, string ip)
        {
            return DaoPerfil.GuardarCertificacion(codUsuario, codAutor, c, ip);
        }

        public static EntRespuesta EliminarCertificacion(string codUsuario, string codAutor,
                                                         int idCertificacion, string ip)
        {
            return DaoPerfil.EliminarCertificacion(codUsuario, codAutor, idCertificacion, ip);
        }

        public static EntRespuesta GuardarExperiencia(string codUsuario, string codAutor,
                                                      EntPerfilExperiencia x, string ip)
        {
            return DaoPerfil.GuardarExperiencia(codUsuario, codAutor, x, ip);
        }

        public static EntRespuesta EliminarExperiencia(string codUsuario, string codAutor, int idExperiencia, string ip)
        {
            return DaoPerfil.EliminarExperiencia(codUsuario, codAutor, idExperiencia, ip);
        }

        public static EntRespuesta GuardarCargaFamiliar(string codUsuario, string codAutor,
                                                        EntPerfilCargaFamiliar c, string ip)
        {
            return DaoPerfil.GuardarCargaFamiliar(codUsuario, codAutor, c, ip);
        }

        public static EntRespuesta EliminarCargaFamiliar(string codUsuario, string codAutor, int idCargaFam, string ip)
        {
            return DaoPerfil.EliminarCargaFamiliar(codUsuario, codAutor, idCargaFam, ip);
        }

        /// <summary>Guarda o reemplaza la foto. codAutor: ver GuardarContacto.</summary>
        public static EntRespuesta GuardarFoto(string codUsuario, string codAutor, EntPerfilFoto foto, string ip)
        {
            return DaoPerfil.GuardarFoto(codUsuario, codAutor, foto, ip);
        }

        /// <summary>Quita la foto. codAutor se recibe y no se usa: ver DaoPerfil.EliminarFoto.</summary>
        public static EntRespuesta EliminarFoto(string codUsuario, string codAutor, string ip)
        {
            return DaoPerfil.EliminarFoto(codUsuario, codAutor, ip);
        }

        /// <summary>Registra un documento de respaldo. codAutor: ver GuardarContacto.</summary>
        public static EntRespuesta GuardarDocumento(string codUsuario, string codAutor,
                                                    EntPerfilDocumento doc, string ip)
        {
            return DaoPerfil.GuardarDocumento(codUsuario, codAutor, doc, ip);
        }

        /// <summary>Borrado logico de un documento. codAutor: ver GuardarContacto.</summary>
        public static EntRespuesta EliminarDocumento(string codUsuario, string codAutor, int idDocumento, string ip)
        {
            return DaoPerfil.EliminarDocumento(codUsuario, codAutor, idDocumento, ip);
        }

        /// <summary>
        /// El documento con sus datos de archivo, o null si no es de esta persona.
        /// </summary>
        public static EntPerfilDocumento ObtenerDocumento(string codUsuario, int idDocumento)
        {
            return DaoPerfil.ObtenerDocumento(codUsuario, idDocumento);
        }

        /// <summary>El equipo directo de una jefatura.</summary>
        public static List<EntPerfilEquipoItem> ListaEquipo(string codJefe, string filtro)
        {
            return DaoPerfil.ListaEquipo(codJefe, filtro);
        }

        /// <summary>
        /// El personal activo que calce con el filtro, para la pantalla de
        /// Talento Humano.
        ///
        /// La validacion del filtro va aqui y no solo en el navegador: el handler
        /// es alcanzable por HTTP directo. Con el filtro invalido devuelve lista
        /// vacia en vez de lanzar, para que la pantalla no tenga que distinguir
        /// "no valido" de "sin resultados" -el mensaje se lo da el handler-.
        /// </summary>
        public static List<EntPerfilEquipoItem> ListaPersonal(string filtro)
        {
            if (NegPerfilCampos.ValidarFiltroPersonal(filtro) != "")
            {
                return new List<EntPerfilEquipoItem>();
            }

            return DaoPerfil.ListaPersonal(filtro);
        }

        /// <summary>
        /// El perfil recortado de un subordinado. Devuelve PerfilEncontrado en
        /// false si esa persona no le reporta a quien pregunta: la comprobacion
        /// la hace el procedimiento, no esta capa.
        /// </summary>
        public static EntPerfilEquipo PerfilEquipo(string codJefe, string codUsuario)
        {
            return DaoPerfil.PerfilEquipo(codJefe, codUsuario);
        }
    }
}
