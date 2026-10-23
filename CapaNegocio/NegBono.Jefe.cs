using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CapaNegocio
{
    /// <summary>Acciones del jefe: guardar en borrador, borrar y enviar a Talento Humano.</summary>
    public static partial class NegBono
    {
        /// <summary>
        /// Crea (b.IdBono = 0) o edita un bono de mi gente. Al editar, b.Estado
        /// es el estado que el jefe tenia en pantalla (el estado esperado); un
        /// Rechazado editado vuelve a Borrador. Nuevo: Origen = Jefe, CodJefe = yo.
        /// Si a la persona le falta la identificacion se guarda igual, con Aviso.
        /// </summary>
        public static EntBonoResultado GuardarBorrador(EntBono b, string yo, bool confirmarRepetido, DateTime hoy)
        {
            if (b == null) return Error("Faltan los datos del bono.");
            yo = NegBonoReglas.Normalizar(yo);

            EntBonoPeriodo periodo = Abierto(LeerPeriodos());
            if (periodo == null) return Error(MsgSinMes);

            List<EntBonoPersona> activas = LeerPersonasActivas();
            HashSet<string> miGente = CodigosMiGente(activas, yo);
            string error = NegBonoReglas.PuedeCrear(periodo, miGente, b.CodUsuario);
            if (error != null) return Error(error);

            EntBono actual = null;
            string estadoEsperado = "";
            string estadoNuevo = BonoEstados.Borrador;
            if (b.IdBono != 0)
            {
                actual = BonoVivo(b.IdBono);
                if (actual == null || actual.Estado != (b.Estado ?? "")) return Error(MsgCambio);
                if (NegBonoReglas.Normalizar(b.CodUsuario) != NegBonoReglas.Normalizar(actual.CodUsuario))
                    return Error("No se puede cambiar la persona de un bono; bórrelo y cree uno nuevo.");
                error = NegBonoReglas.PuedeEditar(actual, periodo, yo, miGente);
                if (error != null) return Error(error);
                estadoEsperado = actual.Estado;
                estadoNuevo = NegBonoReglas.EstadoAlEditar(actual.Estado);
            }

            EntBonoTipo tipo = Tipo(LeerTipos(), b.IdTipo);
            if (tipo == null) return Error("El tipo de bono no existe.");
            if (!tipo.Activo && (actual == null || actual.IdTipo != tipo.IdTipo))
                return Error("El tipo «" + tipo.Nombre + "» está desactivado.");

            EntBonoPersona persona = Persona(activas, b.CodUsuario);
            EntBono nuevo = Preparar(b, persona, tipo, periodo);
            error = ValidarContenido(nuevo, tipo, hoy);
            if (error != null) return Error(error);

            EntBonoResultado repetido = ChequearRepeticiones(nuevo, tipo, confirmarRepetido);
            if (repetido != null) return repetido;

            // Copia provisional para que la lista muestre la persona; se vuelve a tomar al enviar.
            string falta;
            nuevo.Identificacion = NegBonoReglas.Identificacion(persona, out falta);
            nuevo.Origen = actual == null ? BonoOrigenes.Jefe : actual.Origen;
            nuevo.CodJefe = yo;

            int r = GuardarEnBase(nuevo, estadoEsperado, estadoNuevo, yo);
            if (r <= 0) return Error(MensajeCodigo(r, nuevo));

            return Bien("Bono guardado como borrador.",
                new[] { falta == null ? null : falta + " Sin ese dato el bono no se puede enviar." });
        }

        /// <summary>
        /// Borra un bono que yo propuse, en Borrador o Rechazado del mes abierto,
        /// aunque la persona ya no este a mi cargo (NegBonoReglas.PuedeBorrar).
        /// </summary>
        public static EntBonoResultado Borrar(int idBono, string estadoEsperado, string yo)
        {
            yo = NegBonoReglas.Normalizar(yo);
            EntBonoPeriodo periodo = Abierto(LeerPeriodos());
            if (periodo == null) return Error(MsgSinMes);

            EntBono actual = BonoVivo(idBono);
            if (actual == null || actual.Estado != (estadoEsperado ?? "")) return Error(MsgCambio);

            string error = NegBonoReglas.PuedeBorrar(actual, periodo, yo);
            if (error != null) return Error(error);

            int r = BorrarEnBase(idBono, actual.Estado, yo);
            if (r != 0) return Error(MensajeCodigo(r, actual));
            return Bien("Bono borrado.", null);
        }

        /// <summary>
        /// Envia todos mis Borrador del mes abierto, cada uno con la copia de
        /// empresa, nombre e identificacion de ese momento. El que no cumple una
        /// regla se salta y se nombra en el mensaje; los demas se envian. Un
        /// correo a cada usuario activo de perfil 14 con el resumen de lo enviado.
        /// Ok = true si se envio al menos uno.
        /// </summary>
        public static EntBonoResultado Enviar(string yo, DateTime hoy, Func<string, string, string, bool> enviarCorreo, string urlRevisar)
        {
            yo = NegBonoReglas.Normalizar(yo);
            EntBonoPeriodo periodo = Abierto(LeerPeriodos());
            if (periodo == null) return Error(MsgSinMes);

            List<EntBono> borradores = LeerDetalles(null)
                .Where(b => b.Estado == BonoEstados.Borrador && NegBonoReglas.Normalizar(b.CodJefe) == yo)
                .OrderBy(b => b.Nombre ?? "", StringComparer.Create(Es, true))
                .ThenBy(b => b.IdBono)
                .ToList();
            if (borradores.Count == 0) return Error("No tiene bonos en borrador para enviar.");

            List<EntBonoPersona> activas = LeerPersonasActivas();
            HashSet<string> miGente = CodigosMiGente(activas, yo);
            List<EntBonoTipo> tipos = LeerTipos();
            Dictionary<string, List<EntBono>> dePersona = new Dictionary<string, List<EntBono>>();

            List<string> saltados = new List<string>();
            int enviados = 0;
            decimal total = 0m;
            foreach (EntBono b in borradores)
            {
                EntBonoTipo tipo = Tipo(tipos, b.IdTipo);
                string error;
                try
                {
                    error = EnviarUno(b, tipo, periodo, yo, hoy, miGente, activas, dePersona);
                }
                catch (Exception)
                {
                    // Uno que falla en la base no deja a medias el resumen: se nombra y se sigue.
                    error = "falló la base de datos; recargue la pantalla para ver si quedó enviado.";
                }

                if (error != null)
                {
                    saltados.Add(NombreBono(b, tipo) + ": " + error);
                    continue;
                }
                enviados++;
                total += b.Valor;
            }

            if (enviados == 0)
                return Error("No se envió ningún bono. " + string.Join(" ", saltados));

            List<string> avisos = new List<string>();
            EntBonoPersona jefe = Persona(activas, yo);
            string nombreJefe = jefe != null && !string.IsNullOrWhiteSpace(jefe.Nombre) ? jefe.Nombre.Trim() : yo;
            string asunto = NegBonoCorreo.AsuntoEnvio(nombreJefe, enviados, total, urlRevisar);
            string cuerpo = NegBonoCorreo.CuerpoEnvio(nombreJefe, enviados, total, urlRevisar);
            List<string> destinatarios = NegBonoCorreo.DestinatariosEnvio(activas);
            if (destinatarios.Count == 0)
                EnviarYAnotar(enviarCorreo, "", "Talento Humano (perfil 14)", asunto, cuerpo, null, periodo.IdPeriodo, yo, avisos);
            foreach (string d in destinatarios)
                EnviarYAnotar(enviarCorreo, d, d, asunto, cuerpo, null, periodo.IdPeriodo, yo, avisos);

            string mensaje = "Se enviaron " + enviados.ToString(CultureInfo.InvariantCulture)
                           + " bono(s) a Talento Humano por " + Dinero(total) + ".";
            if (saltados.Count > 0) mensaje += " No se enviaron: " + string.Join(" ", saltados);
            EntBonoResultado resultado = Bien(mensaje, avisos);
            resultado.Incompleto = saltados.Count > 0;
            return resultado;
        }

        /// <summary>Reglas y envio de un borrador. null = enviado; si no, por que se salto. Lanza si falla la base.</summary>
        private static string EnviarUno(EntBono b, EntBonoTipo tipo, EntBonoPeriodo periodo, string yo, DateTime hoy,
            HashSet<string> miGente, List<EntBonoPersona> activas, Dictionary<string, List<EntBono>> dePersona)
        {
            string error = NegBonoReglas.PuedeEnviar(b, periodo, yo, miGente);
            if (error != null) return error;
            if (tipo == null) return "su tipo de bono ya no existe.";
            error = NegBonoFechas.ValidarSemanas(b.Lunes, tipo.PorSemana, hoy);
            if (error != null) return error;
            // Contra el sugerido guardado: un cambio de tarifa en el catalogo no invalida lo ya propuesto.
            error = NegBonoReglas.ValidarValor(new EntBonoTipo { ValorSugerido = b.ValorSugerido }, b.Semanas, b.Valor, b.MotivoAjuste);
            if (error != null) return error;

            EntBonoPersona persona = Persona(activas, b.CodUsuario);
            string ident = NegBonoReglas.Identificacion(persona, out error);
            if (error != null) return error;

            string clave = NegBonoReglas.Normalizar(b.CodUsuario);
            List<EntBono> suyos;
            if (!dePersona.TryGetValue(clave, out suyos))
            {
                suyos = LeerDePersona(b.CodUsuario);
                dePersona[clave] = suyos;
            }
            List<DateTime> choques = NegBonoReglas.ChocaDoblePago(b, suyos);
            if (choques.Count > 0) return NegBonoReglas.MensajeDoblePago(choques);

            int r = CambiarEstadoEnBase(b.IdBono, BonoEstados.Borrador, BonoEstados.Enviado, null, null,
                NegBonoReglas.EmpresaNormalizada(persona.Empresa), (persona.Nombre ?? "").Trim(), ident, yo);
            return r == 0 ? null : MensajeCodigo(r, b);
        }

        private static string NombreBono(EntBono b, EntBonoTipo tipo)
        {
            string persona = string.IsNullOrWhiteSpace(b.Nombre) ? b.CodUsuario : b.Nombre.Trim();
            string nombreTipo = tipo != null ? tipo.Nombre : b.NombreTipo;
            return string.IsNullOrWhiteSpace(nombreTipo) ? persona : persona + " (" + nombreTipo + ")";
        }
    }
}
