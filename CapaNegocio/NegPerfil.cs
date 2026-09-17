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

        /// <summary>TEMPORAL. Se borra al final de la entrega 1. Ver DaoPerfil.</summary>
        public static EntRespuesta GuardarContacto(string codUsuario, EntPerfilContacto contacto, string ip)
        {
            return GuardarContacto(codUsuario, codUsuario, contacto, ip);
        }

        /// <summary>Alta o edicion de un contacto de emergencia. codAutor: ver GuardarContacto.</summary>
        public static EntRespuesta GuardarEmergencia(string codUsuario, string codAutor,
                                                     EntPerfilEmergencia c, string ip)
        {
            return DaoPerfil.GuardarEmergencia(codUsuario, codAutor, c, ip);
        }

        /// <summary>TEMPORAL. Se borra al final de la entrega 1. Ver DaoPerfil.</summary>
        public static EntRespuesta GuardarEmergencia(string codUsuario, EntPerfilEmergencia c, string ip)
        {
            return GuardarEmergencia(codUsuario, codUsuario, c, ip);
        }

        /// <summary>Borrado logico de un contacto de emergencia. codAutor: ver GuardarContacto.</summary>
        public static EntRespuesta EliminarEmergencia(string codUsuario, string codAutor, int idContacto, string ip)
        {
            return DaoPerfil.EliminarEmergencia(codUsuario, codAutor, idContacto, ip);
        }

        /// <summary>TEMPORAL. Se borra al final de la entrega 1. Ver DaoPerfil.</summary>
        public static EntRespuesta EliminarEmergencia(string codUsuario, int idContacto, string ip)
        {
            return EliminarEmergencia(codUsuario, codUsuario, idContacto, ip);
        }

        public static EntRespuesta GuardarEstudio(string codUsuario, string codAutor, EntPerfilEstudio e, string ip)
        {
            return DaoPerfil.GuardarEstudio(codUsuario, codAutor, e, ip);
        }

        /// <summary>TEMPORAL. Se borra al final de la entrega 1. Ver DaoPerfil.</summary>
        public static EntRespuesta GuardarEstudio(string codUsuario, EntPerfilEstudio e, string ip)
        {
            return GuardarEstudio(codUsuario, codUsuario, e, ip);
        }

        public static EntRespuesta EliminarEstudio(string codUsuario, string codAutor, int idEstudio, string ip)
        {
            return DaoPerfil.EliminarEstudio(codUsuario, codAutor, idEstudio, ip);
        }

        /// <summary>TEMPORAL. Se borra al final de la entrega 1. Ver DaoPerfil.</summary>
        public static EntRespuesta EliminarEstudio(string codUsuario, int idEstudio, string ip)
        {
            return EliminarEstudio(codUsuario, codUsuario, idEstudio, ip);
        }

        public static EntRespuesta GuardarCertificacion(string codUsuario, string codAutor,
                                                        EntPerfilCertificacion c, string ip)
        {
            return DaoPerfil.GuardarCertificacion(codUsuario, codAutor, c, ip);
        }

        /// <summary>TEMPORAL. Se borra al final de la entrega 1. Ver DaoPerfil.</summary>
        public static EntRespuesta GuardarCertificacion(string codUsuario, EntPerfilCertificacion c, string ip)
        {
            return GuardarCertificacion(codUsuario, codUsuario, c, ip);
        }

        public static EntRespuesta EliminarCertificacion(string codUsuario, string codAutor,
                                                         int idCertificacion, string ip)
        {
            return DaoPerfil.EliminarCertificacion(codUsuario, codAutor, idCertificacion, ip);
        }

        /// <summary>TEMPORAL. Se borra al final de la entrega 1. Ver DaoPerfil.</summary>
        public static EntRespuesta EliminarCertificacion(string codUsuario, int idCertificacion, string ip)
        {
            return EliminarCertificacion(codUsuario, codUsuario, idCertificacion, ip);
        }

        public static EntRespuesta GuardarExperiencia(string codUsuario, string codAutor,
                                                      EntPerfilExperiencia x, string ip)
        {
            return DaoPerfil.GuardarExperiencia(codUsuario, codAutor, x, ip);
        }

        /// <summary>TEMPORAL. Se borra al final de la entrega 1. Ver DaoPerfil.</summary>
        public static EntRespuesta GuardarExperiencia(string codUsuario, EntPerfilExperiencia x, string ip)
        {
            return GuardarExperiencia(codUsuario, codUsuario, x, ip);
        }

        public static EntRespuesta EliminarExperiencia(string codUsuario, string codAutor, int idExperiencia, string ip)
        {
            return DaoPerfil.EliminarExperiencia(codUsuario, codAutor, idExperiencia, ip);
        }

        /// <summary>TEMPORAL. Se borra al final de la entrega 1. Ver DaoPerfil.</summary>
        public static EntRespuesta EliminarExperiencia(string codUsuario, int idExperiencia, string ip)
        {
            return EliminarExperiencia(codUsuario, codUsuario, idExperiencia, ip);
        }

        public static EntRespuesta GuardarCargaFamiliar(string codUsuario, string codAutor,
                                                        EntPerfilCargaFamiliar c, string ip)
        {
            return DaoPerfil.GuardarCargaFamiliar(codUsuario, codAutor, c, ip);
        }

        /// <summary>TEMPORAL. Se borra al final de la entrega 1. Ver DaoPerfil.</summary>
        public static EntRespuesta GuardarCargaFamiliar(string codUsuario, EntPerfilCargaFamiliar c, string ip)
        {
            return GuardarCargaFamiliar(codUsuario, codUsuario, c, ip);
        }

        public static EntRespuesta EliminarCargaFamiliar(string codUsuario, string codAutor, int idCargaFam, string ip)
        {
            return DaoPerfil.EliminarCargaFamiliar(codUsuario, codAutor, idCargaFam, ip);
        }

        /// <summary>TEMPORAL. Se borra al final de la entrega 1. Ver DaoPerfil.</summary>
        public static EntRespuesta EliminarCargaFamiliar(string codUsuario, int idCargaFam, string ip)
        {
            return EliminarCargaFamiliar(codUsuario, codUsuario, idCargaFam, ip);
        }

        /// <summary>Guarda o reemplaza la foto. codAutor: ver GuardarContacto.</summary>
        public static EntRespuesta GuardarFoto(string codUsuario, string codAutor, EntPerfilFoto foto, string ip)
        {
            return DaoPerfil.GuardarFoto(codUsuario, codAutor, foto, ip);
        }

        /// <summary>TEMPORAL. Se borra al final de la entrega 1. Ver DaoPerfil.</summary>
        public static EntRespuesta GuardarFoto(string codUsuario, EntPerfilFoto foto, string ip)
        {
            return GuardarFoto(codUsuario, codUsuario, foto, ip);
        }

        /// <summary>Quita la foto. codAutor se recibe y no se usa: ver DaoPerfil.EliminarFoto.</summary>
        public static EntRespuesta EliminarFoto(string codUsuario, string codAutor, string ip)
        {
            return DaoPerfil.EliminarFoto(codUsuario, codAutor, ip);
        }

        /// <summary>TEMPORAL. Se borra al final de la entrega 1. Ver DaoPerfil.</summary>
        public static EntRespuesta EliminarFoto(string codUsuario, string ip)
        {
            return EliminarFoto(codUsuario, codUsuario, ip);
        }

        /// <summary>Registra un documento de respaldo. codAutor: ver GuardarContacto.</summary>
        public static EntRespuesta GuardarDocumento(string codUsuario, string codAutor,
                                                    EntPerfilDocumento doc, string ip)
        {
            return DaoPerfil.GuardarDocumento(codUsuario, codAutor, doc, ip);
        }

        /// <summary>TEMPORAL. Se borra al final de la entrega 1. Ver DaoPerfil.</summary>
        public static EntRespuesta GuardarDocumento(string codUsuario, EntPerfilDocumento doc, string ip)
        {
            return GuardarDocumento(codUsuario, codUsuario, doc, ip);
        }

        /// <summary>Borrado logico de un documento. codAutor: ver GuardarContacto.</summary>
        public static EntRespuesta EliminarDocumento(string codUsuario, string codAutor, int idDocumento, string ip)
        {
            return DaoPerfil.EliminarDocumento(codUsuario, codAutor, idDocumento, ip);
        }

        /// <summary>TEMPORAL. Se borra al final de la entrega 1. Ver DaoPerfil.</summary>
        public static EntRespuesta EliminarDocumento(string codUsuario, int idDocumento, string ip)
        {
            return EliminarDocumento(codUsuario, codUsuario, idDocumento, ip);
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
