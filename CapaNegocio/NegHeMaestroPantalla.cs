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
                // El ultimo que aparece es el decimal; el otro, de miles.
                char dec = t.LastIndexOf('.') > t.LastIndexOf(',') ? '.' : ',';
                char miles = dec == '.' ? ',' : '.';
                if (Contar(t, dec) > 1) { return false; }
                normal = t.Replace(miles.ToString(), "").Replace(dec, '.');
            }
            else if (puntos + comas > 1)
            {
                // Un mismo separador repetido solo puede ser de miles.
                normal = t.Replace(".", "").Replace(",", "");
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
            if (valor <= 0m) { return false; }
            if (decimal.Round(valor, 2) != valor) { return false; }

            monto = valor;
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
                case -1: return "No se guardó: la persona no es un empleado activo.";
                case -2: return "No se guardó: la persona ya está en el módulo. Para cambiar su sueldo use «Registrar ajuste de sueldo».";
                case -3: return "No se guardó: revise los datos (empresa, jornada, sueldo y fecha).";
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
                return Rechazo("El sueldo no se entiende. Escríbalo sin separador de miles, por ejemplo 1850 o 1850,50.");
            }

            string error = ValidarColaborador(empresa, jornada, monto, vigencia);
            if (error != null) { return Rechazo(error); }

            int codigo = DaoHorasExtras.AgregarColaborador(idEmpleado, empresa.Trim(), cargo, jornada, aplicaHE,
                                                           monto, vigencia, usuario, ip);
            if (codigo != 0) { return Rechazo(MensajeColaborador(codigo)); }

            return Refrescar(idPeriodo, vigencia, MensajeColaborador(0), usuario, ip);
        }

        public static EntRespuesta AgregarAjusteSalario(int idPeriodo, long idEmpleado, string montoTexto, DateTime vigencia,
                                                        string observacion, string usuario, string ip)
        {
            decimal monto;
            if (!LeerMonto(montoTexto, out monto))
            {
                return Rechazo("El sueldo no se entiende. Escríbalo sin separador de miles, por ejemplo 1850 o 1850,50.");
            }

            string error = ValidarAjuste(monto, vigencia);
            if (error != null) { return Rechazo(error); }

            int codigo = DaoHorasExtras.AgregarAjusteSalario(idEmpleado, monto, vigencia, observacion, usuario, ip);
            if (codigo != 0) { return Rechazo(MensajeAjuste(codigo)); }

            return Refrescar(idPeriodo, vigencia, MensajeAjuste(0), usuario, ip);
        }

        /// <summary>
        /// Despues de guardar: si el periodo esta abierto se recalcula (la misma
        /// accion idempotente de «Recalcular»), y si no, solo se vuelve a cargar.
        /// En los dos casos la respuesta trae la pantalla para repintar.
        /// </summary>
        private static EntRespuesta Refrescar(int idPeriodo, DateTime vigencia, string guardado, string usuario, string ip)
        {
            EntHePantalla existente = DaoHorasExtras.CargarPeriodo(idPeriodo);
            EntHePeriodo periodo = existente.Periodo;

            if (periodo == null)
            {
                EntRespuesta sinPeriodo = new EntRespuesta();
                sinPeriodo.estado = "1";
                sinPeriodo.mensaje = guardado;
                sinPeriodo.tipoMensaje = "success";
                return sinPeriodo;
            }

            EntRespuesta r = periodo.EstaAbierto
                ? NegHorasExtrasPantalla.AbrirPeriodo(periodo.FechaInicio, periodo.FechaFin, usuario, ip)
                : NegHorasExtrasPantalla.CargarPantalla(idPeriodo);

            string aviso = MensajeTrasGuardar(true, periodo.EstaAbierto, vigencia, periodo.FechaFin);
            string extra = r.estado == "1" && r.tipoMensaje == "warning" && !string.IsNullOrEmpty(r.mensaje) ? " " + r.mensaje : "";

            r.mensaje = guardado + " " + aviso + extra;
            r.tipoMensaje = vigencia > periodo.FechaFin || extra != "" || r.estado != "1" ? "warning" : "success";
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
