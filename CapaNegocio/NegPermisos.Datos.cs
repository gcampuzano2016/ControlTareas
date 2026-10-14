using CapaDato;
using CapaEntidad;

namespace CapaNegocio
{
    /// <summary>Unico archivo de NegPermisos que toca la base.</summary>
    public static partial class NegPermisos
    {
        /// <summary>Lanza si la base falla: quien llama decide negar.</summary>
        public static EntPermisos Cargar(int? idPerfil, string codUsuario)
        {
            return DaoPermisos.Cargar(idPerfil ?? 0, codUsuario);
        }
    }
}
