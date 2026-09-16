using CapaEntidad;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace CapaNegocio
{
    /// <summary>
    /// Arma el archivo xlsx de un periodo de horas extras para que Nomina se lo
    /// pueda llevar. Todo ocurre en memoria: no hay plantilla en disco, porque
    /// no existe ninguna para este modulo y crear una agregaria un archivo de
    /// despliegue que puede faltar sin dar ningun error (ya paso con
    /// descargas/perfil/web.config). La hoja se construye desde cero.
    ///
    /// Dos hojas: "Horas Extras" con todo el periodo, y "Pagos" solo con las
    /// filas cuyo total sea mayor a cero -la lista que consume el proceso de
    /// pago, no el padron completo-.
    /// </summary>
    public static class NegHeExportacion
    {
        public const string NombreHojaDetalle = "Horas Extras";
        public const string NombreHojaPagos = "Pagos";

        /// <summary>Fila donde va el titulo con el periodo y su estado.</summary>
        public const int FilaDeEstado = 1;

        /// <summary>Fila donde arranca el detalle, un colaborador por fila.</summary>
        public const int PrimeraFilaDeDatos = 7;

        /// <summary>Columna del total a pagar por horas extras de cada fila.</summary>
        public const int ColumnaTotalHE = 19;

        /// <summary>Columna del valor hora ordinario: la usan las pruebas para comprobar el formato de 6 decimales.</summary>
        public const int ColumnaValorHoraOrdinaria = 11;

        private const int FilaDeCierre = 2;
        private const int FilaDeGeneracion = 3;
        private const int FilaDeResumen = 4;
        // Fila 5 queda en blanco a proposito, como separador antes de los encabezados.
        private const int FilaDeEncabezados = 6;
        private const int UltimaColumna = 23;

        private const string FormatoMoneda = "#,##0.00";
        private const string FormatoHoras = "#,##0.00";
        private const string FormatoEntero = "0";

        // NegHorasExtras redondea el valor hora a 6 decimales (DecimalesValorHora)
        // y la columna de la base es DECIMAL(18,6): si aqui se mostrara con dos,
        // Nomina veria horas x tarifa sin cuadrar con el total de la fila.
        private const string FormatoValorHora = "#,##0.000000";

        // El orden aqui define el orden de las columnas 1..23 en la hoja. Las
        // columnas de dinero llevan la moneda en el propio encabezado: el
        // archivo no la dice en ningun otro sitio.
        private static readonly string[] Encabezados = new string[]
        {
            "Período",
            "ID Empleado",
            "Cédula",
            "Nombre",
            "Empresa",
            "Cargo",
            "Jornada (h/día)",
            "Salario Base (USD)",
            "Aplica HE",
            "Divisor",
            "Valor Hora Ordinaria (USD)",
            "Valor Hora 50% (USD)",
            "Valor Hora 100% (USD)",
            "Horas 50%",
            "Horas 100%",
            "Total Pago 50% (USD)",
            "Total Pago 100% (USD)",
            "Total Horas",
            "Total HE (USD)",
            "Motivo No Aplica",
            "Advertencia",
            "Observación",
            "Origen de las Horas"
        };

        /// <summary>Los seis totales que se acumulan por fila, en dinero y horas.</summary>
        private struct Totales
        {
            public decimal Horas50;
            public decimal Horas100;
            public decimal Pago50;
            public decimal Pago100;
            public decimal TotalHoras;
            public decimal TotalHE;
        }

        /// <summary>
        /// Nombre de archivo con el codigo de documento controlado que usan los
        /// demas exportadores del sistema. Lleva el rango del periodo (para
        /// identificar que fechas cubre, que ya no tienen por que ser un mes
        /// calendario) y la fecha y hora de armado (para distinguir dos
        /// exportaciones del mismo periodo hechas en dias distintos, por
        /// ejemplo tras reabrir y volver a cerrar).
        /// </summary>
        public static string NombreDeArchivo(EntHePeriodo periodo)
        {
            DateTime ahora = DateTime.Now;
            return "F-CS-001 Reporte de Horas Extras"
                + periodo.FechaInicio.ToString("yyyyMMdd", CultureInfo.InvariantCulture)
                + "_" + periodo.FechaFin.ToString("yyyyMMdd", CultureInfo.InvariantCulture)
                + "_" + ahora.ToString("yyyyMMdd", CultureInfo.InvariantCulture)
                + "_" + ahora.ToString("HHmmss", CultureInfo.InvariantCulture)
                + ".xlsx";
        }

        /// <summary>
        /// Arma el libro completo y lo devuelve como bytes. Nunca toca disco:
        /// GetAsByteArray, nunca SaveAs(FileInfo).
        ///
        /// Precondicion: pantalla.Periodo no puede ser nulo. DaoHorasExtras /
        /// NegHorasExtrasPantalla.CargarPantalla devuelve un Periodo nulo como
        /// resultado legitimo cuando el periodo no existe, y quien llame aqui
        /// debe haber comprobado eso antes (por ejemplo, mirando el estado de
        /// esa respuesta). Si llega nulo de todos modos, se lanza ArgumentException
        /// en vez de dejar una NullReferenceException muda.
        /// </summary>
        public static byte[] Construir(EntHePantalla pantalla)
        {
            if (pantalla == null)
            {
                throw new ArgumentNullException(nameof(pantalla));
            }

            if (pantalla.Periodo == null)
            {
                throw new ArgumentException(
                    "No se puede armar el archivo: el periodo es nulo (el periodo no existe).",
                    nameof(pantalla));
            }

            using (ExcelPackage paquete = CrearPaquete())
            {
                string periodoTexto = TextoPeriodo(pantalla.Periodo);

                ExcelWorksheet hojaDetalle = paquete.Workbook.Worksheets.Add(NombreHojaDetalle);
                EscribirEncabezadoDeHoja(hojaDetalle, pantalla.Periodo, TextoResumenDetalle(pantalla.Filas));
                int proximaFilaDetalle = EscribirFilasEnHoja(hojaDetalle, pantalla.Filas, periodoTexto);

                // La fila de totales del detalle reutiliza los totales que ya
                // trae la pantalla -calculados por el servidor-, nunca los
                // recalcula a partir de las filas.
                Totales totalesServidor = new Totales();
                totalesServidor.Horas50 = pantalla.TotalHoras50;
                totalesServidor.Horas100 = pantalla.TotalHoras100;
                totalesServidor.Pago50 = pantalla.TotalPago50;
                totalesServidor.Pago100 = pantalla.TotalPago100;
                totalesServidor.TotalHoras = pantalla.TotalHoras;
                totalesServidor.TotalHE = pantalla.TotalPagar;

                EscribirTotalesFila(hojaDetalle, proximaFilaDetalle + 1, totalesServidor);
                ConfigurarVista(hojaDetalle, proximaFilaDetalle - 1);
                AjustarColumnas(hojaDetalle);

                List<EntHeFila> filasConPago = new List<EntHeFila>();
                foreach (EntHeFila f in pantalla.Filas)
                {
                    if (f.TotalHE > 0m)
                    {
                        filasConPago.Add(f);
                    }
                }

                ExcelWorksheet hojaPagos = paquete.Workbook.Worksheets.Add(NombreHojaPagos);
                EscribirEncabezadoDeHoja(hojaPagos, pantalla.Periodo, TextoResumenPagos(filasConPago, pantalla.Filas));
                int proximaFilaPagos = EscribirFilasEnHoja(hojaPagos, filasConPago, periodoTexto);

                // Aqui si se suma: el servidor no manda un total de "solo lo
                // que se paga", asi que es la unica manera de tener el total
                // de este subconjunto. No es recalcular la formula de horas
                // extras, es sumar columnas ya calculadas por fila.
                EscribirTotalesFila(hojaPagos, proximaFilaPagos + 1, SumarFilas(filasConPago));
                ConfigurarVista(hojaPagos, proximaFilaPagos - 1);
                AjustarColumnas(hojaPagos);

                return paquete.GetAsByteArray();
            }
        }

        /// <summary>
        /// Unico lugar del archivo donde se crea un ExcelPackage. EPPlus exige
        /// fijar el contexto de licencia antes del primer uso: concentrarlo aqui
        /// evita que un futuro metodo publico cree el suyo propio y se ejecute
        /// primero, sin que ninguna compilacion avise del olvido.
        /// </summary>
        private static ExcelPackage CrearPaquete()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            return new ExcelPackage();
        }

        /// <summary>
        /// La etiqueta del periodo: su rango completo. Un periodo ya no es un
        /// mes, asi que "09/2026" mentiria en cuanto alguien abra una quincena.
        /// </summary>
        private static string TextoPeriodo(EntHePeriodo periodo)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0:dd/MM/yyyy} - {1:dd/MM/yyyy}",
                periodo.FechaInicio,
                periodo.FechaFin);
        }

        /// <summary>Titulo, cierre, fecha de generacion y resumen: la cabecera comun a las dos hojas.</summary>
        private static void EscribirEncabezadoDeHoja(ExcelWorksheet hoja, EntHePeriodo periodo, string textoResumen)
        {
            EscribirTitulo(hoja, periodo);
            EscribirCierre(hoja, periodo);
            EscribirGeneracion(hoja);
            EscribirResumen(hoja, textoResumen);
            EscribirEncabezados(hoja);
        }

        private static void EscribirTitulo(ExcelWorksheet hoja, EntHePeriodo periodo)
        {
            bool cerrado = string.Equals(periodo.EstadoPeriodo, "Cerrado", StringComparison.OrdinalIgnoreCase);
            bool anulado = string.Equals(periodo.EstadoPeriodo, "Anulado", StringComparison.OrdinalIgnoreCase);

            string estadoTexto;
            if (cerrado)
            {
                estadoTexto = "Cerrado";
            }
            else if (anulado)
            {
                // Un periodo anulado no es un borrador por firmar -es lo
                // contrario-, asi que nunca lleva la palabra PROVISIONAL.
                estadoTexto = "Anulado";
            }
            else
            {
                estadoTexto = periodo.EstadoPeriodo + " (PROVISIONAL)";
            }

            string titulo = string.Format(
                CultureInfo.InvariantCulture,
                "Reporte de Horas Extras - Período {0} - {1}",
                TextoPeriodo(periodo),
                estadoTexto);

            using (ExcelRange celda = hoja.Cells[FilaDeEstado, 1, FilaDeEstado, UltimaColumna])
            {
                celda.Merge = true;
            }

            hoja.Cells[FilaDeEstado, 1].Value = titulo;
            hoja.Cells[FilaDeEstado, 1].Style.Font.Bold = true;
            hoja.Cells[FilaDeEstado, 1].Style.Font.Size = 12;
        }

        private static void EscribirCierre(ExcelWorksheet hoja, EntHePeriodo periodo)
        {
            bool cerrado = string.Equals(periodo.EstadoPeriodo, "Cerrado", StringComparison.OrdinalIgnoreCase);
            string texto = "";

            if (cerrado)
            {
                bool tieneUsuario = !string.IsNullOrEmpty(periodo.UsuarioCierre);
                bool tieneFecha = periodo.FechaCierre.HasValue;

                if (tieneUsuario && tieneFecha)
                {
                    texto = string.Format(
                        CultureInfo.InvariantCulture,
                        "Cerrado por {0} el {1:dd/MM/yyyy HH:mm}",
                        periodo.UsuarioCierre,
                        periodo.FechaCierre.Value);
                }
                else if (tieneUsuario)
                {
                    texto = "Cerrado por " + periodo.UsuarioCierre;
                }
                else if (tieneFecha)
                {
                    texto = string.Format(
                        CultureInfo.InvariantCulture,
                        "Cerrado el {0:dd/MM/yyyy HH:mm}",
                        periodo.FechaCierre.Value);
                }
            }

            EscribirLineaMerge(hoja, FilaDeCierre, texto);
        }

        /// <summary>
        /// Cuando se armo el archivo. No lleva quien lo genero: eso lo registra
        /// la bitacora de descarga que se esta implementando aparte, y llevar
        /// el mismo dato en dos sitios acaba divergiendo.
        /// </summary>
        private static void EscribirGeneracion(ExcelWorksheet hoja)
        {
            string texto = "Generado el " + DateTime.Now.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
            EscribirLineaMerge(hoja, FilaDeGeneracion, texto);
        }

        private static void EscribirResumen(ExcelWorksheet hoja, string texto)
        {
            EscribirLineaMerge(hoja, FilaDeResumen, texto);
        }

        private static void EscribirLineaMerge(ExcelWorksheet hoja, int fila, string texto)
        {
            using (ExcelRange celda = hoja.Cells[fila, 1, fila, UltimaColumna])
            {
                celda.Merge = true;
            }

            hoja.Cells[fila, 1].Value = texto;
            hoja.Cells[fila, 1].Style.Font.Italic = true;
        }

        private static string TextoResumenDetalle(IList<EntHeFila> filas)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "Colaboradores: {0} - Con horas: {1}",
                filas.Count,
                ContarConHoras(filas));
        }

        private static string TextoResumenPagos(IList<EntHeFila> filasConPago, IList<EntHeFila> todasLasFilas)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "Filas con pago (Total HE mayor a cero): {0} de {1} colaboradores",
                filasConPago.Count,
                todasLasFilas.Count);
        }

        private static int ContarConHoras(IList<EntHeFila> filas)
        {
            int contador = 0;
            foreach (EntHeFila f in filas)
            {
                if (f.TotalHoras > 0m)
                {
                    contador++;
                }
            }

            return contador;
        }

        private static void EscribirEncabezados(ExcelWorksheet hoja)
        {
            for (int i = 0; i < Encabezados.Length; i++)
            {
                ExcelRange celda = hoja.Cells[FilaDeEncabezados, i + 1];
                celda.Value = Encabezados[i];
                celda.Style.Font.Bold = true;
            }
        }

        /// <summary>
        /// Una fila esta marcada con advertencia si no aplica HE (aunque tenga
        /// horas guardadas de antes de que se lo apagaran), si tiene un motivo
        /// de revision aun aplicando, o si el propio calculo la marco -salario
        /// en cero, divisor en cero, horas negativas-. Es la misma regla que
        /// usa la pantalla para pintar la fila gris o roja (ver ConstruirFila
        /// en horasExtras.js), solo que aqui se anota en una columna en vez de
        /// en un color.
        /// </summary>
        private static bool TieneAdvertencia(EntHeFila f)
        {
            return f.TieneAdvertencia || !f.AplicaHESnapshot || !string.IsNullOrEmpty(f.MotivoNoAplica);
        }

        /// <summary>Escribe una fila por colaborador y devuelve la fila siguiente a la ultima escrita.</summary>
        private static int EscribirFilasEnHoja(ExcelWorksheet hoja, IList<EntHeFila> filas, string periodoTexto)
        {
            int fila = PrimeraFilaDeDatos;

            foreach (EntHeFila f in filas)
            {
                hoja.Cells[fila, 1].Value = periodoTexto;

                hoja.Cells[fila, 2].Value = f.IdEmpleado;
                hoja.Cells[fila, 2].Style.Numberformat.Format = FormatoEntero;

                hoja.Cells[fila, 3].Value = f.CedulaSnapshot;
                hoja.Cells[fila, 4].Value = f.NombreSnapshot;
                hoja.Cells[fila, 5].Value = f.EmpresaSnapshot;
                hoja.Cells[fila, 6].Value = f.CargoSnapshot;

                hoja.Cells[fila, 7].Value = f.JornadaHorasDiaSnapshot;
                hoja.Cells[fila, 7].Style.Numberformat.Format = FormatoEntero;

                hoja.Cells[fila, 8].Value = f.SalarioBaseSnapshot;
                hoja.Cells[fila, 8].Style.Numberformat.Format = FormatoMoneda;

                hoja.Cells[fila, 9].Value = f.AplicaHESnapshot ? "Sí" : "No";

                hoja.Cells[fila, 10].Value = f.Divisor;
                hoja.Cells[fila, 10].Style.Numberformat.Format = FormatoEntero;

                // Valor hora: NegHorasExtras redondea a 6 decimales (DECIMAL(18,6)
                // en la base). Mostrarlo con dos rompe la conciliacion horas x
                // tarifa = total que este archivo existe para permitir a mano.
                hoja.Cells[fila, ColumnaValorHoraOrdinaria].Value = f.ValorHoraOrdinaria;
                hoja.Cells[fila, ColumnaValorHoraOrdinaria].Style.Numberformat.Format = FormatoValorHora;

                hoja.Cells[fila, 12].Value = f.ValorHora50;
                hoja.Cells[fila, 12].Style.Numberformat.Format = FormatoValorHora;

                hoja.Cells[fila, 13].Value = f.ValorHora100;
                hoja.Cells[fila, 13].Style.Numberformat.Format = FormatoValorHora;

                hoja.Cells[fila, 14].Value = f.Horas50;
                hoja.Cells[fila, 14].Style.Numberformat.Format = FormatoHoras;

                hoja.Cells[fila, 15].Value = f.Horas100;
                hoja.Cells[fila, 15].Style.Numberformat.Format = FormatoHoras;

                hoja.Cells[fila, 16].Value = f.Total50;
                hoja.Cells[fila, 16].Style.Numberformat.Format = FormatoMoneda;

                hoja.Cells[fila, 17].Value = f.Total100;
                hoja.Cells[fila, 17].Style.Numberformat.Format = FormatoMoneda;

                hoja.Cells[fila, 18].Value = f.TotalHoras;
                hoja.Cells[fila, 18].Style.Numberformat.Format = FormatoHoras;

                // ColumnaTotalHE: el dinero va como decimal, nunca como texto ya
                // formateado. El formato visual es cosa aparte (Numberformat).
                hoja.Cells[fila, ColumnaTotalHE].Value = f.TotalHE;
                hoja.Cells[fila, ColumnaTotalHE].Style.Numberformat.Format = FormatoMoneda;

                hoja.Cells[fila, 20].Value = f.MotivoNoAplica ?? "";
                hoja.Cells[fila, 21].Value = TieneAdvertencia(f) ? "Sí" : "";
                hoja.Cells[fila, 22].Value = f.Observacion;

                // Ultima columna: de donde salieron las horas. Nomina necesita
                // distinguir lo que sembro el sistema desde las tareas aprobadas
                // de lo que corrigio una persona a mano.
                hoja.Cells[fila, 23].Value = f.HorasOrigen;

                fila++;
            }

            return fila;
        }

        private static Totales SumarFilas(IList<EntHeFila> filas)
        {
            Totales t = new Totales();

            foreach (EntHeFila f in filas)
            {
                t.Horas50 += f.Horas50;
                t.Horas100 += f.Horas100;
                t.Pago50 += f.Total50;
                t.Pago100 += f.Total100;
                t.TotalHoras += f.TotalHoras;
                t.TotalHE += f.TotalHE;
            }

            return t;
        }

        private static void EscribirTotalesFila(ExcelWorksheet hoja, int fila, Totales t)
        {
            // La etiqueta cubre las columnas que no se suman (Periodo..Valor
            // Hora 100%); las que si se suman van desde Horas 50% en adelante.
            using (ExcelRange etiqueta = hoja.Cells[fila, 1, fila, 13])
            {
                etiqueta.Merge = true;
            }

            hoja.Cells[fila, 1].Value = "TOTALES";

            hoja.Cells[fila, 14].Value = t.Horas50;
            hoja.Cells[fila, 14].Style.Numberformat.Format = FormatoHoras;

            hoja.Cells[fila, 15].Value = t.Horas100;
            hoja.Cells[fila, 15].Style.Numberformat.Format = FormatoHoras;

            hoja.Cells[fila, 16].Value = t.Pago50;
            hoja.Cells[fila, 16].Style.Numberformat.Format = FormatoMoneda;

            hoja.Cells[fila, 17].Value = t.Pago100;
            hoja.Cells[fila, 17].Style.Numberformat.Format = FormatoMoneda;

            hoja.Cells[fila, 18].Value = t.TotalHoras;
            hoja.Cells[fila, 18].Style.Numberformat.Format = FormatoHoras;

            hoja.Cells[fila, ColumnaTotalHE].Value = t.TotalHE;
            hoja.Cells[fila, ColumnaTotalHE].Style.Numberformat.Format = FormatoMoneda;

            for (int c = 1; c <= UltimaColumna; c++)
            {
                hoja.Cells[fila, c].Style.Font.Bold = true;
            }
        }

        /// <summary>
        /// Encabezados siempre visibles al bajar por las filas, y autofiltro
        /// para que Nomina pueda acotar sin pedirlo aparte.
        /// </summary>
        private static void ConfigurarVista(ExcelWorksheet hoja, int ultimaFilaConDatos)
        {
            hoja.View.FreezePanes(PrimeraFilaDeDatos, 1);

            int ultimaFilaDelFiltro = ultimaFilaConDatos > FilaDeEncabezados ? ultimaFilaConDatos : FilaDeEncabezados;
            hoja.Cells[FilaDeEncabezados, 1, ultimaFilaDelFiltro, UltimaColumna].AutoFilter = true;
        }

        private static void AjustarColumnas(ExcelWorksheet hoja)
        {
            for (int c = 1; c <= UltimaColumna; c++)
            {
                hoja.Column(c).Width = 16;
            }

            hoja.Column(1).Width = 10;
            hoja.Column(2).Width = 12;
            hoja.Column(4).Width = 28;
            hoja.Column(5).Width = 22;
            hoja.Column(6).Width = 22;
            hoja.Column(20).Width = 26;
            hoja.Column(22).Width = 30;
            hoja.Column(23).Width = 18;
        }
    }
}
