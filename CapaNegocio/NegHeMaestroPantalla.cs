using CapaDato;
using CapaEntidad;
using System;
using System.Globalization;

namespace CapaNegocio
{
    /// <summary>
    /// Completar los datos de una persona que no esta en el modulo, y registrar un
    /// ajuste de sueldo de quien ya esta, desde la pantalla de Horas Extras.
    ///
    /// NO comprueba perfiles, igual que el resto del modulo: la barrera esta en el
    /// handler (PerfilesQueEditanMaestro, 14 y 18).
    ///
    /// Nunca pisa un sueldo: los dos caminos insertan una fila nueva en
    /// HE_Salario, con usuario, fecha e ip. Ese es el rastro de quien cambio que.
    /// </summary>
    public static class NegHeMaestroPantalla
    {
        public const int JornadaMinima = 1;
        public const int JornadaMaxima = 12;

        /// <summary>
        /// Tope de plausibilidad de un sueldo mensual. El mas alto del modulo hoy es
        /// 3.000: un monto por encima de esto es casi seguro un error de dedo, y un
        /// sueldo multiplicado por 100 se recalcula al instante en el pago. El
        /// procedimiento tiene el mismo tope.
        /// </summary>
        public const decimal MontoMaximo = 20000m;

        /// <summary>
        /// Lee el sueldo tal como lo escribe una persona. Acepta coma o punto como
        /// separador decimal, y separador de miles cuando hay los dos ("1.850,50",
        /// "1,850.50") o cuando el mismo se repite ("1.250.000").
        ///
        /// Rechaza lo ambiguo: "1.850" puede ser mil ochocientos cincuenta o uno
        /// coma ochenta y cinco, y adivinar mal un sueldo es peor que pedir que se
        /// escriba de nuevo. Tambien rechaza cero, negativos y mas de 2 decimales.
        /// </summary>
        public static bool LeerMonto(string texto, out decimal monto)
        {
            monto = 0m;
            if (string.IsNullOrWhiteSpace(texto)) { return false; }

            string t = texto.Trim().Replace("$", "").Replace(" ", "");
            if (t.StartsWith("-")) { return false; }

            int puntos = Contar(t, '.');
            int comas = Contar(t, ',');
            string normal;

            if (puntos > 0 && comas > 0)
            {
                // El ultimo que aparece es el decimal y va una sola vez; el otro es de
                // miles y tiene que estar bien agrupado: "1.850,50" si, "1.85,50" no.
                char dec = t.LastIndexOf('.') > t.LastIndexOf(',') ? '.' : ',';
                char miles = dec == '.' ? ',' : '.';
                if (Contar(t, dec) > 1) { return false; }

                string entera = t.Substring(0, t.IndexOf(dec));
                string decimales = t.Substring(t.IndexOf(dec) + 1);
                if (!GruposDeMilesValidos(entera.Split(miles))) { return false; }

                normal = entera.Replace(miles.ToString(), "") + "." + decimales;
            }
            else if (puntos + comas > 1)
            {
                // Un mismo separador repetido ("1.850.50", "1.250.000") o es un error
                // de dedo o es un monto de millones: ninguno es un sueldo que cargar.
                return false;
            }
            else if (puntos + comas == 1)
            {
                char sep = puntos == 1 ? '.' : ',';
                int decimales = t.Length - t.IndexOf(sep) - 1;
                if (decimales == 3) { return false; }
                normal = t.Replace(sep, '.');
            }
            else
            {
                normal = t;
            }

            decimal valor;
            if (!decimal.TryParse(normal, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out valor)) { return false; }
            if (valor <= 0m || valor > MontoMaximo) { return false; }
            if (decimal.Round(valor, 2) != valor) { return false; }

            monto = valor;
            return true;
        }

        /// <summary>El primer grupo de 1 a 3 digitos y los siguientes de exactamente 3.</summary>
        private static bool GruposDeMilesValidos(string[] grupos)
        {
            if (grupos.Length == 0 || grupos[0].Length < 1 || grupos[0].Length > 3) { return false; }
            for (int i = 0; i < grupos.Length; i++)
            {
                foreach (char c in grupos[i]) { if (c < '0' || c > '9') { return false; } }
                if (i > 0 && grupos[i].Length != 3) { return false; }
            }
            return true;
        }

        private static int Contar(string texto, char c)
        {
            int n = 0;
            foreach (char x in texto) { if (x == c) { n++; } }
            return n;
        }

        /// <summary>null si esta todo; si no, el primer dato que falta o esta mal.</summary>
        public static string ValidarColaborador(string empresa, int jornada, decimal monto, DateTime vigencia)
        {
            if (string.IsNullOrWhiteSpace(empresa)) { return "Seleccione la empresa."; }
            if (jornada < JornadaMinima || jornada > JornadaMaxima)
            {
                return "La jornada tiene que estar entre " + JornadaMinima + " y " + JornadaMaxima + " horas por día.";
            }
            return ValidarAjuste(monto, vigencia);
        }

        public static string ValidarAjuste(decimal monto, DateTime vigencia)
        {
            if (monto <= 0m) { return "Indique el sueldo: tiene que ser mayor que cero."; }
            if (vigencia == DateTime.MinValue) { return "Indique la fecha desde la que rige el sueldo."; }
            return null;
        }

        public static string MensajeColaborador(int codigo)
        {
            switch (codigo)
            {
                case 0: return "Datos guardados.";
                case 1: return "Datos guardados. La persona estaba desactivada en el módulo y se reactivó; sus datos anteriores quedaron en la observación del sueldo.";
                case -1: return "No se guardó: la persona no es un empleado activo.";
                case -2: return "No se guardó: la persona ya está en el módulo. Para cambiar su sueldo use «Registrar ajuste de sueldo».";
                case -3: return "No se guardó: revise los datos (empresa, jornada, sueldo y fecha).";
                case -4: return "No se guardó: esta persona ya tiene en su historial un sueldo con fecha igual o posterior, que taparía a este. Use una fecha posterior.";
                default: return "No se pudieron guardar los datos. Intente nuevamente.";
            }
        }

        public static string MensajeAjuste(int codigo)
        {
            switch (codigo)
            {
                case 0: return "Ajuste de sueldo guardado.";
                case -1: return "No se guardó: la persona no está en el módulo de Horas Extras. Use «Completar datos».";
                case -2: return "No se guardó: ya hay un ajuste de sueldo con la misma fecha. Use otra fecha.";
                case -3: return "No se guardó: revise el sueldo y la fecha.";
                case -4: return "No se guardó: ya hay un sueldo con fecha posterior en el historial, así que este ajuste no regiría. Use una fecha posterior.";
                default: return "No se pudo guardar el ajuste. Intente nuevamente.";
            }
        }

        /// <summary>
        /// Lo que se dice despues de guardar, segun el periodo que se tenia abierto
        /// en la pantalla. Un periodo cerrado nunca se toca: lo pagado queda como se
        /// cerro y el dato se aplica al reabrirlo.
        /// </summary>
        public static string MensajeTrasGuardar(bool hayPeriodo, bool periodoAbierto, DateTime vigencia, DateTime finPeriodo)
        {
            if (!hayPeriodo) { return ""; }

            string texto = periodoAbierto
                ? "El período se recalculó con el dato nuevo."
                : "El período está cerrado: no se modificó. El dato se aplicará si se reabre.";

            if (vigencia > finPeriodo)
            {
                texto += " Atención: el sueldo rige desde el "
                       + vigencia.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)
                       + ", después del fin del período, así que no aplica a este período.";
            }

            return texto;
        }

        public static EntRespuesta AgregarColaborador(int idPeriodo, long idEmpleado, string empresa, string cargo,
                                                      int jornada, bool aplicaHE, string montoTexto, DateTime vigencia,
                                                      string usuario, string ip)
        {
            decimal monto;
            if (!LeerMonto(montoTexto, out monto))
            {
                return Rechazo(TextoMontoInvalido());
            }

            string error = ValidarColaborador(empresa, jornada, monto, vigencia);
            if (error != null) { return Rechazo(error); }

            int codigo = DaoHorasExtras.AgregarColaborador(idEmpleado, empresa.Trim(), cargo, jornada, aplicaHE,
                                                           monto, vigencia, usuario, ip);
            if (codigo != 0 && codigo != 1) { return Rechazo(MensajeColaborador(codigo)); }

            return Refrescar(idPeriodo, vigencia, MensajeColaborador(codigo), usuario, ip);
        }

        public static EntRespuesta AgregarAjusteSalario(int idPeriodo, long idEmpleado, string montoTexto, DateTime vigencia,
                                                        string observacion, string usuario, string ip)
        {
            decimal monto;
            if (!LeerMonto(montoTexto, out monto))
            {
                return Rechazo(TextoMontoInvalido());
            }

            string error = ValidarAjuste(monto, vigencia);
            if (error != null) { return Rechazo(error); }

            int codigo = DaoHorasExtras.AgregarAjusteSalario(idEmpleado, monto, vigencia, observacion, usuario, ip);
            if (codigo != 0) { return Rechazo(MensajeAjuste(codigo)); }

            return Refrescar(idPeriodo, vigencia, MensajeAjuste(0), usuario, ip);
        }

        private static string TextoMontoInvalido()
        {
            return "El sueldo no se entiende o no es válido. Escríbalo sin separador de miles, por ejemplo 1850 o 1850,50 "
                 + "(máximo " + MontoMaximo.ToString("#,##0", CultureInfo.InvariantCulture) + ").";
        }

        /// <summary>
        /// Despues de guardar: si el periodo esta abierto se recalcula (la misma
        /// accion idempotente de «Recalcular»), y si no, solo se vuelve a cargar.
        ///
        /// Lo guardado YA esta en la base. Si el recalculo falla -por error o por
        /// excepcion-, la respuesta sigue siendo "guardado" con un aviso: si dijera
        /// error, la persona reintentaria y chocaria con "ya esta en el modulo".
        /// </summary>
        private static EntRespuesta Refrescar(int idPeriodo, DateTime vigencia, string guardado, string usuario, string ip)
        {
            EntHePeriodo periodo = null;
            EntRespuesta refresco = null;

            try
            {
                periodo = DaoHorasExtras.CargarPeriodo(idPeriodo).Periodo;
                if (periodo != null)
                {
                    refresco = periodo.EstaAbierto
                        ? NegHorasExtrasPantalla.AbrirPeriodo(periodo.FechaInicio, periodo.FechaFin, usuario, ip)
                        : NegHorasExtrasPantalla.CargarPantalla(idPeriodo);
                }
            }
            catch (Exception)
            {
                refresco = null;
            }

            return CombinarTrasGuardar(guardado, periodo, vigencia, refresco);
        }

        /// <summary>
        /// La respuesta despues de un guardado que YA ocurrio. Siempre estado "1":
        /// lo que puede fallar aqui es el recalculo, no el guardado, y el mensaje
        /// dice cual de los dos paso.
        /// </summary>
        public static EntRespuesta CombinarTrasGuardar(string guardado, EntHePeriodo periodo, DateTime vigencia, EntRespuesta refresco)
        {
            EntRespuesta r = new EntRespuesta();
            r.estado = "1";

            if (periodo == null)
            {
                r.mensaje = guardado;
                r.tipoMensaje = "success";
                return r;
            }

            bool refrescoOk = refresco != null && refresco.estado == "1";
            string fecha = vigencia > periodo.FechaFin
                ? " Atención: el sueldo rige desde el " + vigencia.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)
                  + ", después del fin del período, así que no aplica a este período."
                : "";

            if (!refrescoOk)
            {
                r.mensaje = guardado + (periodo.EstaAbierto
                    ? " Pero no se pudo recalcular el período: pulse «Traer aprobaciones nuevas»."
                    : " No se pudo volver a cargar la pantalla: seleccione el período nuevamente.") + fecha;
                r.tipoMensaje = "warning";
                return r;
            }

            r.resultado = refresco.resultado;
            string extra = refresco.tipoMensaje == "warning" && !string.IsNullOrEmpty(refresco.mensaje) ? " " + refresco.mensaje : "";
            r.mensaje = guardado + " " + MensajeTrasGuardar(true, periodo.EstaAbierto, vigencia, periodo.FechaFin) + extra;
            r.tipoMensaje = fecha != "" || extra != "" ? "warning" : "success";
            return r;
        }

        private static EntRespuesta Rechazo(string mensaje)
        {
            EntRespuesta r = new EntRespuesta();
            r.estado = "0";
            r.mensaje = mensaje;
            r.tipoMensaje = "warning";
            return r;
        }
    }
}
