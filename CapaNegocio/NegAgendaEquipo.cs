using CapaEntidad;
using System;
using System.Collections.Generic;

namespace CapaNegocio
{
    /// <summary>
    /// Quien ve a quien en la agenda. Clase pura: no toca base, sesion ni
    /// HttpContext.
    ///
    /// - Cada persona se ve a si misma.
    /// - Cada jefe ve a TODA su cadena hacia abajo (R_Usuarios.Cod_Jefe_Inm,
    ///   recursivo).
    /// - Quien tiene la accion Agenda.VerTodos ve a todos.
    ///
    /// La cadena se recorre con un conjunto de visitados: un ciclo de jefes
    /// (A jefe de B y B jefe de A, un error de datos) no cuelga el calculo, y
    /// nadie entra en su propia cadena por un ciclo. Los codigos se comparan
    /// sin espacios y sin distinguir mayusculas: Cod_Usuario no tiene formato
    /// unico.
    /// </summary>
    public static class NegAgendaEquipo
    {
        public static readonly StringComparer Codigos = StringComparer.OrdinalIgnoreCase;

        public static string Normalizar(string cod)
        {
            return (cod ?? "").Trim();
        }

        /// <summary>La cadena hacia abajo de codJefe, sin incluirlo a el.</summary>
        public static HashSet<string> Cadena(IEnumerable<EntAgendaPersona> personas, string codJefe)
        {
            HashSet<string> cadena = new HashSet<string>(Codigos);
            string raiz = Normalizar(codJefe);
            if (raiz == "" || personas == null) { return cadena; }

            Dictionary<string, List<string>> hijos = new Dictionary<string, List<string>>(Codigos);
            foreach (EntAgendaPersona p in personas)
            {
                if (p == null) { continue; }
                string cod = Normalizar(p.CodUsuario);
                string jefe = Normalizar(p.CodJefe);
                if (cod == "" || jefe == "" || Codigos.Equals(cod, jefe)) { continue; }

                List<string> directos;
                if (!hijos.TryGetValue(jefe, out directos))
                {
                    directos = new List<string>();
                    hijos[jefe] = directos;
                }
                directos.Add(cod);
            }

            Queue<string> pendientes = new Queue<string>();
            pendientes.Enqueue(raiz);
            while (pendientes.Count > 0)
            {
                List<string> directos;
                if (!hijos.TryGetValue(pendientes.Dequeue(), out directos)) { continue; }
                foreach (string h in directos)
                {
                    if (Codigos.Equals(h, raiz) || !cadena.Add(h)) { continue; }
                    pendientes.Enqueue(h);
                }
            }
            return cadena;
        }

        public static bool PuedeVer(string yo, string persona, ISet<string> cadena, bool verTodos)
        {
            string p = Normalizar(persona);
            if (p == "") { return false; }
            return verTodos
                || Codigos.Equals(Normalizar(yo), p)
                || (cadena != null && cadena.Contains(p));
        }

        /// <summary>A uno mismo no se le "asigna": se registra una actividad propia.</summary>
        public static bool PuedeAsignarA(string yo, string persona, ISet<string> cadena, bool verTodos)
        {
            return !Codigos.Equals(Normalizar(yo), Normalizar(persona))
                && PuedeVer(yo, persona, cadena, verTodos);
        }
    }
}
