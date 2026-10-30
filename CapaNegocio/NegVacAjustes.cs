using CapaDato;
using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CapaNegocio
{
    /// <summary>
    /// Mantenimiento de GTH: saldo de corte y compra de vacaciones, con
    /// bitacora (docs/sql/2026-10-29-vacaciones-ajustes.sql).
    ///
    /// Las reglas de saldo siguen siendo del motor: el corte reemplaza todo lo
    /// anterior a su fecha y la compra es un movimiento mas que descuenta sus
    /// dias. Aqui solo se valida lo que entra y se traduce lo que sale.
    /// </summary>
    public static class NegVacAjustes
    {
        public const string TipoCompra = "COMPRA DE VACACIONES";
        public const int MotivoMinimo = 10;
        public const int MotivoMaximo = 1000;
        /// <summary>Tres periodos acumulables de hasta 30 dias, con holgura.</summary>
        public const decimal SaldoCorteMaximo = 120m;

        /// <summary>Todo el maestro, activos e inactivos.</summary>
        public static List<EntVacColaborador> Colaboradores()
        {
            return DaoVacSaldos.ListaColaboradores();
        }

        /// <summary>Null si la persona no existe. El saldo, a hoy (a la salida, si es inactivo).</summary>
        public static EntVacAjustes Cargar(int idColaborador, DateTime hoy)
        {
            EntVacFicha f = DaoVacSaldos.Ficha(hoy.Date, idColaborador);
            if (f == null) { return null; }
            EntVacAjustes a = new EntVacAjustes { Ficha = f };
            DaoVacSaldos.Historial(idColaborador, a);
            return a;
        }

        /// <summary>Las compras de la persona, anuladas incluidas, la ultima primero.</summary>
        public static List<EntVacMovimiento> Compras(EntVacFicha f)
        {
            return f.Movimientos.Where(m => m.Tipo == TipoCompra)
                                .OrderByDescending(m => m.Desde).ThenByDescending(m => m.IdMovimiento).ToList();
        }

        public static string ValidarMotivo(string motivo)
        {
            string m = (motivo ?? "").Trim();
            if (m.Length < MotivoMinimo) { return "Escriba el motivo (al menos " + MotivoMinimo + " caracteres): queda en la bitácora."; }
            if (m.Length > MotivoMaximo) { return "El motivo no puede pasar de " + MotivoMaximo + " caracteres."; }
            return "";
        }

        /// <summary>fecha y saldo null = quitar el corte.</summary>
        public static string ValidarCorte(EntVacColaborador c, DateTime? fecha, decimal? saldo, DateTime hoy)
        {
            if (fecha.HasValue != saldo.HasValue) { return "Indique la fecha y el saldo de corte, o ninguno de los dos para quitarlo."; }
            if (!fecha.HasValue) { return c.FechaCorte.HasValue ? "" : "La persona no tiene saldo de corte que quitar."; }

            DateTime f = fecha.Value.Date;
            if (f < c.FechaIngreso.Date) { return "La fecha de corte no puede ser anterior al ingreso (" + Fecha(c.FechaIngreso) + ")."; }
            if (f > hoy.Date) { return "La fecha de corte no puede ser posterior a hoy."; }
            if (c.FechaSalida.HasValue && f > c.FechaSalida.Value.Date) { return "La fecha de corte no puede ser posterior a la salida (" + Fecha(c.FechaSalida.Value) + ")."; }

            decimal s = saldo.Value;
            if (s < 0m) { return "El saldo de corte no puede ser negativo."; }
            if (s > SaldoCorteMaximo) { return "El saldo de corte no puede pasar de " + SaldoCorteMaximo + " días."; }
            if (s != Math.Round(s, 2)) { return "El saldo de corte admite hasta 2 decimales."; }

            if (c.FechaCorte.HasValue && c.FechaCorte.Value.Date == f && c.SaldoCorte == s) { return "No hay cambios: ese ya es el saldo de corte."; }
            return "";
        }

        /// <summary>saldoActual: el saldo de hoy, antes de la compra.</summary>
        public static string ValidarCompra(EntVacColaborador c, DateTime fecha, decimal dias, decimal saldoActual, DateTime hoy)
        {
            DateTime f = fecha.Date;
            if (f < c.FechaIngreso.Date) { return "La fecha de la compra no puede ser anterior al ingreso (" + Fecha(c.FechaIngreso) + ")."; }
            if (f > hoy.Date) { return "La fecha de la compra no puede ser posterior a hoy."; }
            if (c.FechaSalida.HasValue && f > c.FechaSalida.Value.Date) { return "La fecha de la compra no puede ser posterior a la salida (" + Fecha(c.FechaSalida.Value) + ")."; }
            /* El motor ignora lo anterior al corte: la compra no descontaria nada. */
            if (c.FechaCorte.HasValue && f < c.FechaCorte.Value.Date) { return "La fecha de la compra es anterior al saldo de corte (" + Fecha(c.FechaCorte.Value) + "): no descontaría nada."; }

            if (dias <= 0m) { return "Los días comprados deben ser mayores que cero."; }
            if (dias != Math.Round(dias, 2)) { return "Los días comprados admiten hasta 2 decimales."; }
            if (dias > saldoActual) { return "No se pueden comprar " + Num(dias) + " días: el saldo es de " + Num(saldoActual) + "."; }
            return "";
        }

        /// <summary>"" si se guardo; si no, el mensaje para el usuario.</summary>
        public static string GuardarCorte(int idColaborador, DateTime? fecha, decimal? saldo, DateTime? fechaAnterior, decimal? saldoAnterior,
                                          string motivo, string usuario, string ip, DateTime hoy)
        {
            string error = ValidarMotivo(motivo);
            if (error != "") { return error; }
            EntVacColaborador c = Colaborador(idColaborador);
            if (c == null) { return Mensaje("NO EXISTE"); }
            error = ValidarCorte(c, fecha, saldo, hoy);
            if (error != "") { return error; }
            return Mensaje(DaoVacSaldos.GuardarCorte(idColaborador, fecha, saldo, fechaAnterior, saldoAnterior, motivo.Trim(), usuario, ip));
        }

        /// <summary>El saldo como quedaria con ese corte, a hoy. Lanza ArgumentException si el corte no es valido.</summary>
        public static EntVacSaldo SimularCorte(int idColaborador, DateTime? fecha, decimal? saldo, DateTime hoy)
        {
            EntVacColaborador c = Colaborador(idColaborador);
            if (c == null) { throw new ArgumentException(Mensaje("NO EXISTE")); }
            if (fecha.HasValue != saldo.HasValue) { throw new ArgumentException(ValidarCorte(c, fecha, saldo, hoy)); }
            if (fecha.HasValue)
            {
                string error = ValidarCorte(c, fecha, saldo, hoy);
                /* Simular el corte que ya tiene no es un error: muestra lo mismo. */
                if (error != "" && !(c.FechaCorte == fecha && c.SaldoCorte == saldo)) { throw new ArgumentException(error); }
            }
            return DaoVacSaldos.SimularCorte(idColaborador, fecha, saldo, hoy.Date);
        }

        /// <summary>
        /// Decision 7 de GTH (2026-10-30): la compra lleva su documento firmado.
        /// Sin documento no se registra.
        /// </summary>
        public static string RegistrarCompra(int idColaborador, DateTime fecha, decimal dias, EntVacArchivo documento,
                                             string motivo, string usuario, string ip, DateTime hoy)
        {
            string error = ValidarMotivo(motivo);
            if (error != "") { return error; }
            error = ValidarDocumento(documento);
            if (error != "") { return error; }
            EntVacFicha f = DaoVacSaldos.Ficha(hoy.Date, idColaborador);
            if (f == null || f.Saldo == null) { return Mensaje("NO EXISTE"); }
            error = ValidarCompra(f.Colaborador, fecha, dias, f.Saldo.Saldo, hoy);
            if (error != "") { return error; }
            return Mensaje(DaoVacSaldos.RegistrarCompra(idColaborador, fecha, dias, documento, motivo.Trim(), usuario, ip));
        }

        /// <summary>Adjunta o reemplaza el documento de una compra ya registrada (las del Excel no tienen).</summary>
        public static string AdjuntarDocumento(int idColaborador, int idMovimiento, EntVacArchivo documento,
                                               string motivo, string usuario, string ip)
        {
            string error = ValidarMotivo(motivo);
            if (error != "") { return error; }
            error = ValidarDocumento(documento);
            if (error != "") { return error; }
            return Mensaje(DaoVacSaldos.AdjuntarDocumento(idColaborador, idMovimiento, documento, motivo.Trim(), usuario, ip));
        }

        /// <summary>Null si la compra no es de esa persona o no tiene documento.</summary>
        public static EntVacArchivo Documento(int idColaborador, int idMovimiento)
        {
            return DaoVacSaldos.Documento(idColaborador, idMovimiento);
        }

        public const int DocumentoMaximo = 5 * 1024 * 1024;

        /// <summary>
        /// PDF, JPG o PNG de hasta 5 MB. Se mira el contenido, no solo el
        /// nombre: un "acta.pdf" que no empieza como PDF no entra. Si es valido,
        /// deja en el documento el nombre limpio y el TipoContenido que manda.
        /// </summary>
        public static string ValidarDocumento(EntVacArchivo documento)
        {
            if (documento == null || documento.Contenido == null || documento.Contenido.Length == 0)
            {
                return "Adjunte el documento firmado de la compra (PDF, JPG o PNG).";
            }
            if (documento.Contenido.Length > DocumentoMaximo) { return "El documento no puede pasar de 5 MB."; }

            string nombre = System.IO.Path.GetFileName((documento.NombreArchivo ?? "").Trim());
            string extension = System.IO.Path.GetExtension(nombre).TrimStart('.').ToLowerInvariant();
            byte[] b = documento.Contenido;
            string tipo;
            bool firma;
            switch (extension)
            {
                case "pdf":
                    tipo = "application/pdf";
                    firma = b.Length >= 4 && b[0] == 0x25 && b[1] == 0x50 && b[2] == 0x44 && b[3] == 0x46;   // %PDF
                    break;
                case "jpg":
                case "jpeg":
                    tipo = "image/jpeg";
                    firma = b.Length >= 3 && b[0] == 0xFF && b[1] == 0xD8 && b[2] == 0xFF;
                    break;
                case "png":
                    tipo = "image/png";
                    firma = b.Length >= 4 && b[0] == 0x89 && b[1] == 0x50 && b[2] == 0x4E && b[3] == 0x47;
                    break;
                default:
                    return "El documento debe ser PDF, JPG o PNG.";
            }
            if (!firma) { return "El archivo no es un " + extension.ToUpperInvariant() + " válido."; }
            if (nombre.Length > 200) { nombre = "documento." + extension; }

            documento.NombreArchivo = nombre;
            documento.TipoContenido = tipo;
            return "";
        }

        public static string AnularCompra(int idColaborador, int idMovimiento, string motivo, string usuario, string ip)
        {
            string error = ValidarMotivo(motivo);
            if (error != "") { return error; }
            return Mensaje(DaoVacSaldos.AnularCompra(idColaborador, idMovimiento, motivo.Trim(), usuario, ip));
        }

        /// <summary>El Resultado de los procedimientos, en palabras. OK = "".</summary>
        public static string Mensaje(string resultado)
        {
            switch ((resultado ?? "").Trim())
            {
                case "OK": return "";
                case "NO EXISTE": return "El colaborador o la compra ya no existe.";
                case "CAMBIADO": return "Alguien más cambió el saldo de corte mientras usted lo editaba. Vuelva a cargar a la persona y revise.";
                case "SIN CAMBIOS": return "No hay cambios: ese ya es el saldo de corte.";
                case "NO ES COMPRA": return "Ese movimiento no es una compra de vacaciones.";
                case "YA ANULADA": return "Esa compra ya estaba anulada.";
                default: return "No se pudo guardar. Intente nuevamente.";
            }
        }

        private static EntVacColaborador Colaborador(int idColaborador)
        {
            return DaoVacSaldos.Colaborador(idColaborador);
        }

        private static string Fecha(DateTime d)
        {
            return d.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
        }

        private static string Num(decimal v)
        {
            return NegVacEstado.Num(v, 2);
        }
    }
}
