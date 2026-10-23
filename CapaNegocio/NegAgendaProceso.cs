using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CapaNegocio
{
    /// <summary>
    /// Reglas de procesos entre areas y de sus plantillas. Clase pura: no toca
    /// base, sesion ni HttpContext. Los metodos de validacion devuelven null si
    /// se permite o el mensaje para mostrar. Los codigos de usuario se comparan
    /// con NegAgendaEquipo.Codigos (sin espacios y sin distinguir mayusculas).
    /// </summary>
    public static class NegAgendaProceso
    {
        private const int NombrePlantillaMax = 150;
        private const int TextoMax = 200;
        private const int MotivoMax = 400;
        private const int PlazoMin = 1;
        private const int PlazoMax = 60;

        private static bool Mismo(string a, string b)
        {
            string x = NegAgendaEquipo.Normalizar(a);
            return x != "" && NegAgendaEquipo.Codigos.Equals(x, NegAgendaEquipo.Normalizar(b));
        }

        private static bool Texto(string s, int max)
        {
            string t = (s ?? "").Trim();
            return t.Length >= 1 && t.Length <= max;
        }

        private static bool PlazoValido(int p) { return p >= PlazoMin && p <= PlazoMax; }

        private static string MsgPlazo = "el plazo tiene que estar entre 1 y 60 días hábiles.";

        private static EntAgendaPersona Buscar(string cod, IList<EntAgendaPersona> activas)
        {
            string c = NegAgendaEquipo.Normalizar(cod);
            if (c == "" || activas == null) { return null; }
            return activas.FirstOrDefault(a => a != null && Mismo(a.CodUsuario, c));
        }

        // ---------------------------------------------------------- permisos

        public static bool PuedeVer(string yo, EntAgendaProceso p, ISet<string> cadena, bool verTodos)
        {
            if (verTodos) { return true; }
            if (p == null) { return false; }
            if (Mismo(yo, p.CodIniciadoPor)) { return true; }

            List<string> involucrados = new List<string> { p.CodIniciadoPor };
            if (p.Pasos != null) { involucrados.AddRange(p.Pasos.Where(s => s != null).Select(s => s.CodResponsable)); }

            foreach (string c in involucrados)
            {
                if (Mismo(yo, c)) { return true; }
                string n = NegAgendaEquipo.Normalizar(c);
                if (n != "" && cadena != null && cadena.Contains(n)) { return true; }
            }
            return false;
        }

        public static bool PuedeGestionar(string yo, EntAgendaProceso p, bool verTodos)
        {
            if (p == null || p.Estado != AgendaProcesoEstados.EnCurso) { return false; }
            return verTodos || Mismo(yo, p.CodIniciadoPor);
        }

        // --------------------------------------------------------- plantillas

        public static string ValidarPlantilla(EntAgendaPlantilla t, ISet<int> areasActivas)
        {
            if (t == null) { return "La plantilla necesita al menos un paso."; }
            if (!Texto(t.Nombre, NombrePlantillaMax))
            {
                return "El nombre de la plantilla es obligatorio (hasta 150 caracteres).";
            }
            if (t.Pasos == null || t.Pasos.Count == 0) { return "La plantilla necesita al menos un paso."; }

            for (int i = 0; i < t.Pasos.Count; i++)
            {
                EntAgendaPlantillaPaso s = t.Pasos[i];
                string pre = "Paso " + (i + 1) + ": ";
                if (s == null || s.Orden != i + 1) { return pre + "el orden tiene que ser consecutivo desde 1."; }
                if (areasActivas == null || !areasActivas.Contains(s.IdArea)) { return pre + "el área no existe o está inactiva."; }
                if (!Texto(s.Titulo, TextoMax)) { return pre + "el título es obligatorio (hasta 200 caracteres)."; }
                if (!PlazoValido(s.PlazoDias)) { return pre + MsgPlazo; }
            }
            return null;
        }

        // ------------------------------------------------------------ inicio

        public static string ValidarInicio(string nombre, EntAgendaPlantilla t, IList<EntAgendaProcesoPaso> pasos, IList<EntAgendaPersona> activas)
        {
            if (!Texto(nombre, TextoMax)) { return "El nombre del proceso es obligatorio (hasta 200 caracteres)."; }
            if (t == null || !t.Activa) { return "La plantilla no está activa."; }

            List<EntAgendaPlantillaPaso> delaPlantilla = t.Pasos ?? new List<EntAgendaPlantillaPaso>();
            if (pasos == null || pasos.Count != delaPlantilla.Count || delaPlantilla.Count == 0)
            {
                return "Los pasos no coinciden con los de la plantilla.";
            }
            for (int i = 0; i < pasos.Count; i++)
            {
                if (pasos[i] == null || delaPlantilla[i] == null || pasos[i].Orden != delaPlantilla[i].Orden)
                {
                    return "Los pasos no coinciden con los de la plantilla.";
                }
            }

            // Primero el area sin nadie a quien asignar: el mensaje dice cual.
            foreach (EntAgendaPlantillaPaso s in delaPlantilla)
            {
                if (activas == null || !activas.Any(a => a != null && a.IdArea == s.IdArea))
                {
                    return "El área «" + (s.Area ?? "") + "» no tiene personas activas: no se puede iniciar el proceso.";
                }
            }

            for (int i = 0; i < pasos.Count; i++)
            {
                string pre = "Paso " + (i + 1) + ": ";
                EntAgendaPersona r = Buscar(pasos[i].CodResponsable, activas);
                if (r == null) { return pre + "el responsable no está activo."; }
                if (r.IdArea != delaPlantilla[i].IdArea)
                {
                    return pre + "el responsable no pertenece al área «" + (delaPlantilla[i].Area ?? "") + "».";
                }
                if (!Texto(pasos[i].Titulo, TextoMax)) { return pre + "el título es obligatorio (hasta 200 caracteres)."; }
                if (!PlazoValido(pasos[i].PlazoDias)) { return pre + MsgPlazo; }
            }
            return null;
        }

        // ------------------------------------------------ cambios sobre pasos

        public static string PuedeReasignar(EntAgendaProcesoPaso paso, string nuevoResponsable, IList<EntAgendaPersona> activas)
        {
            if (paso == null || (paso.Estado != AgendaPasoEstados.Pendiente && paso.Estado != AgendaPasoEstados.EnCurso))
            {
                return "Solo se puede reasignar un paso pendiente o en curso.";
            }
            EntAgendaPersona r = Buscar(nuevoResponsable, activas);
            if (r == null) { return "La persona elegida no está activa."; }
            if (r.IdArea != paso.IdArea) { return "La persona elegida no pertenece al área del paso."; }
            return null;
        }

        public static string PuedeCambiarPlazo(EntAgendaProcesoPaso paso, int plazo)
        {
            if (paso == null || paso.Estado != AgendaPasoEstados.Pendiente)
            {
                return "Solo se puede cambiar el plazo de un paso pendiente.";
            }
            if (!PlazoValido(plazo)) { return "El plazo tiene que estar entre 1 y 60 días hábiles."; }
            return null;
        }

        public static string PuedeCambiarFecha(EntAgendaProcesoPaso paso, DateTime fecha, DateTime hoy)
        {
            if (paso == null || paso.Estado != AgendaPasoEstados.EnCurso)
            {
                return "Solo se puede cambiar la fecha de un paso en curso.";
            }
            if (fecha.DayOfWeek == DayOfWeek.Saturday || fecha.DayOfWeek == DayOfWeek.Sunday)
            {
                return "La fecha de entrega no puede ser sábado ni domingo.";
            }
            if (fecha.Date < hoy.Date) { return "La fecha de entrega no puede ser anterior a hoy."; }
            return null;
        }

        public static string PuedeCancelar(EntAgendaProceso p, string motivo)
        {
            if (p == null || p.Estado != AgendaProcesoEstados.EnCurso) { return "Solo se puede cancelar un proceso en curso."; }
            if (!Texto(motivo, MotivoMax)) { return "El motivo es obligatorio (hasta 400 caracteres)."; }
            return null;
        }

        public static bool ResponsableInactivo(EntAgendaProcesoPaso paso, IList<EntAgendaPersona> activas)
        {
            if (paso == null) { return false; }
            if (paso.Estado != AgendaPasoEstados.Pendiente && paso.Estado != AgendaPasoEstados.EnCurso) { return false; }
            return Buscar(paso.CodResponsable, activas) == null;
        }

        // ------------------------------------------------------------ textos

        public static string AvisoAlCompletar(EntAgendaProceso p, EntAgendaProcesoPaso paso, string nombreSiguiente)
        {
            bool hayOtro = p != null && paso != null && p.Pasos != null && p.Pasos.Any(s => s != null && s.Orden > paso.Orden);
            if (!hayOtro)
            {
                return "Al completar este paso se terminará el proceso. No se puede deshacer.";
            }
            return "Al completar este paso se le avisará a " + (nombreSiguiente ?? "").Trim() + " para el paso siguiente. No se puede deshacer.";
        }

        public static string ResumenAvance(EntAgendaProceso p)
        {
            int total = p == null || p.Pasos == null ? 0 : p.Pasos.Count;
            int hechos = total == 0 ? 0 : p.Pasos.Count(s => s != null && s.Estado == AgendaPasoEstados.Completo);
            return hechos + "/" + total + " pasos completos";
        }
    }
}
