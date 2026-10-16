using CapaEntidad;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CapaNegocio
{
    /// <summary>
    /// Catalogo de areas de la agenda. Clase pura. Los mensajes viven aca y no
    /// en Sp_RTA_AgendaGuardarArea: el procedimiento devuelve un codigo, para
    /// que el script SQL no lleve tildes.
    /// </summary>
    public static class NegAgendaAreas
    {
        private static readonly Regex ColorValido = new Regex("^#[0-9A-Fa-f]{6}$");

        public static string Validar(string nombre, string color)
        {
            string n = (nombre ?? "").Trim();
            if (n == "") { return "Escriba el nombre del área."; }
            if (n.Length > 100) { return "El nombre admite hasta 100 caracteres."; }
            if (color == null || !ColorValido.IsMatch(color.Trim())) { return "Elija un color válido."; }
            return null;
        }

        /// <summary>null si el resultado es un IdArea (positivo).</summary>
        public static string MensajeResultado(int resultado)
        {
            switch (resultado)
            {
                case -1: return "Ya existe un área con ese nombre.";
                case -2: return "«Sin área» no se puede renombrar ni desactivar.";
                case -3: return "El área no existe.";
            }
            return resultado > 0 ? null : "No se pudo guardar el área.";
        }

        public static List<EntAgendaArea> ConPersonas(IList<EntAgendaArea> areas, IList<EntAgendaPersona> personas)
        {
            Dictionary<int, int> cuenta = (personas ?? new List<EntAgendaPersona>())
                .Where(p => p != null)
                .GroupBy(p => p.IdArea)
                .ToDictionary(g => g.Key, g => g.Count());
            List<EntAgendaArea> r = (areas ?? new List<EntAgendaArea>()).ToList();
            foreach (EntAgendaArea a in r)
            {
                int n;
                a.Personas = cuenta.TryGetValue(a.IdArea, out n) ? n : 0;
            }
            return r;
        }
    }
}
