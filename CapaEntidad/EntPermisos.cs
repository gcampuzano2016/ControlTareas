using System;
using System.Collections.Generic;

namespace CapaEntidad
{
    /// <summary>
    /// Lo que puede una persona: pantallas (Href) y acciones (Clave -> Href de
    /// su pantalla). Sale de Sp_RTA_PermisosUsuario. Ninguno de los dos
    /// distingue mayusculas: MenuDos.Href no tiene un formato unico.
    /// [Serializable] porque vive en Session.
    /// </summary>
    [Serializable]
    public class EntPermisos
    {
        public HashSet<string> Pantallas { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, string> Acciones { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>Los permisos de la sesion, con la identidad y la hora con que se cargaron.</summary>
    [Serializable]
    public class EntPermisosCache
    {
        public int? IdPerfil { get; set; }
        public string CodUsuario { get; set; } = "";
        public DateTime Cargado { get; set; }
        public EntPermisos Permisos { get; set; }
    }
}
