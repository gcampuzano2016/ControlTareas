using CapaDato;
using CapaEntidad;

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

        /// <summary>Guarda el contacto editable del usuario de la sesion.</summary>
        public static EntRespuesta GuardarContacto(string codUsuario, EntPerfilContacto contacto, string ip)
        {
            return DaoPerfil.GuardarContacto(codUsuario, contacto, ip);
        }

        /// <summary>Alta o edicion de un contacto de emergencia del usuario de la sesion.</summary>
        public static EntRespuesta GuardarEmergencia(string codUsuario, EntPerfilEmergencia c, string ip)
        {
            return DaoPerfil.GuardarEmergencia(codUsuario, c, ip);
        }

        /// <summary>Borrado logico de un contacto de emergencia del usuario de la sesion.</summary>
        public static EntRespuesta EliminarEmergencia(string codUsuario, int idContacto, string ip)
        {
            return DaoPerfil.EliminarEmergencia(codUsuario, idContacto, ip);
        }

        public static EntRespuesta GuardarEstudio(string codUsuario, EntPerfilEstudio e, string ip)
        {
            return DaoPerfil.GuardarEstudio(codUsuario, e, ip);
        }

        public static EntRespuesta EliminarEstudio(string codUsuario, int idEstudio, string ip)
        {
            return DaoPerfil.EliminarEstudio(codUsuario, idEstudio, ip);
        }

        public static EntRespuesta GuardarCertificacion(string codUsuario, EntPerfilCertificacion c, string ip)
        {
            return DaoPerfil.GuardarCertificacion(codUsuario, c, ip);
        }

        public static EntRespuesta EliminarCertificacion(string codUsuario, int idCertificacion, string ip)
        {
            return DaoPerfil.EliminarCertificacion(codUsuario, idCertificacion, ip);
        }

        public static EntRespuesta GuardarExperiencia(string codUsuario, EntPerfilExperiencia x, string ip)
        {
            return DaoPerfil.GuardarExperiencia(codUsuario, x, ip);
        }

        public static EntRespuesta EliminarExperiencia(string codUsuario, int idExperiencia, string ip)
        {
            return DaoPerfil.EliminarExperiencia(codUsuario, idExperiencia, ip);
        }

        public static EntRespuesta GuardarCargaFamiliar(string codUsuario, EntPerfilCargaFamiliar c, string ip)
        {
            return DaoPerfil.GuardarCargaFamiliar(codUsuario, c, ip);
        }

        public static EntRespuesta EliminarCargaFamiliar(string codUsuario, int idCargaFam, string ip)
        {
            return DaoPerfil.EliminarCargaFamiliar(codUsuario, idCargaFam, ip);
        }

        /// <summary>Guarda o reemplaza la foto del usuario de la sesion.</summary>
        public static EntRespuesta GuardarFoto(string codUsuario, EntPerfilFoto foto, string ip)
        {
            return DaoPerfil.GuardarFoto(codUsuario, foto, ip);
        }

        /// <summary>Quita la foto del usuario de la sesion.</summary>
        public static EntRespuesta EliminarFoto(string codUsuario, string ip)
        {
            return DaoPerfil.EliminarFoto(codUsuario, ip);
        }

        /// <summary>Registra un documento de respaldo del usuario de la sesion.</summary>
        public static EntRespuesta GuardarDocumento(string codUsuario, EntPerfilDocumento doc, string ip)
        {
            return DaoPerfil.GuardarDocumento(codUsuario, doc, ip);
        }

        /// <summary>Borrado logico de un documento del usuario de la sesion.</summary>
        public static EntRespuesta EliminarDocumento(string codUsuario, int idDocumento, string ip)
        {
            return DaoPerfil.EliminarDocumento(codUsuario, idDocumento, ip);
        }
    }
}
