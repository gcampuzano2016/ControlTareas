using CapaEntidad;
using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace CapaPruebas
{
    /// <summary>
    /// Reglas de C:\ReporteExcel\generar_dashboard.py, que es la referencia del
    /// tablero. Los datos son inventados a proposito: el repositorio es publico.
    /// </summary>
    [TestClass]
    public class NegTableroCostosTests
    {
        private static long _id;

        internal static EntTcActividad Act(string estado = "Asignado", int? estadoIni = null, int tipoHe = 0,
            int estadoHe = 0, string tiempo = "01:00:00", string cod = "C1", string nombre = "PERSONA UNO",
            string empresa = "EMPRESA A", string contrato = "CONTRATO A", int anio = 2026, int mes = 3, int dia = 5)
        {
            _id++;
            return new EntTcActividad
            {
                IdDetalle = _id,
                FechaInicio = new DateTime(anio, mes, dia, 9, 0, 0),
                FechaFin = new DateTime(anio, mes, dia, 10, 0, 0),
                Tiempo = tiempo,
                CodResponsable = cod,
                NomResponsable = nombre,
                Empresa = empresa,
                Contrato = contrato,
                Estado = estado,
                IdEstadoIni = estadoIni,
                TipoHE = tipoHe,
                EstadoHE = estadoHe,
                Ticket = "T-1",
                Tarea = "TAREA DE PRUEBA"
            };
        }

        /// <summary>Parametros como los cargados en HE_Parametro (1,5 / 2 / 30).</summary>
        internal static EntTcDatos Datos()
        {
            EntTcDatos d = new EntTcDatos();
            d.Parametros["Factor50"] = 1.5m;
            d.Parametros["Factor100"] = 2m;
            d.Parametros["DiasMes"] = 30m;
            return d;
        }

        /// <summary>1200 / (8 x 30) = 5 $/h.</summary>
        internal static EntTcTarifa Tarifa(string cod, decimal? salario = 1200m, int? jornada = 8, bool? aplica = true)
        {
            return new EntTcTarifa { CodUsuario = cod, Salario = salario, JornadaHorasDia = jornada, AplicaHE = aplica };
        }

        private static string Tipo(EntTcActividad a) { return NegTableroCostos.Tipo(a); }
        private static string Rec(EntTcActividad a) { return NegTableroCostos.Recargo(a, Tipo(a)); }
        private static string Est(EntTcActividad a) { return NegTableroCostos.EstadoHe(a, Tipo(a), Rec(a)); }

        private static EntTcFila Una(EntTcDatos d)
        {
            return NegTableroCostos.Clasificar(d).Filas.Single();
        }

        // ------------------------------------------------------- tipo

        [TestMethod]
        public void Tipo_EstadoMovilizacionConTilde_EsTraslado()
        {
            Assert.AreEqual(NegTableroCostos.Traslado, Tipo(Act(estado: "Movilización")));
        }

        [TestMethod]
        public void Tipo_EstadoMovilizacionSinTildeConEspaciosYMayusculas_EsTraslado()
        {
            Assert.AreEqual(NegTableroCostos.Traslado, Tipo(Act(estado: "  MOVILIZACION A SITIO ")));
        }

        [TestMethod]
        public void Tipo_EstadoIniDeTraslado_EsTraslado()
        {
            foreach (int id in new[] { 14, 28, 62 })
            {
                Assert.AreEqual(NegTableroCostos.Traslado, Tipo(Act(estadoIni: id)), "IdDet_EstadoIni " + id);
            }
        }

        [TestMethod]
        public void Tipo_OtroEstadoIni_NoEsTraslado()
        {
            Assert.AreEqual(NegTableroCostos.Normal, Tipo(Act(estadoIni: 15)));
        }

        [TestMethod]
        public void Tipo_TrasladoTienePrioridadSobreExtra()
        {
            Assert.AreEqual(NegTableroCostos.Traslado, Tipo(Act(estado: "Movilización", tipoHe: 1)));
        }

        [TestMethod]
        public void Tipo_TipoHe1o2_EsExtra()
        {
            Assert.AreEqual(NegTableroCostos.Extra, Tipo(Act(tipoHe: 1)));
            Assert.AreEqual(NegTableroCostos.Extra, Tipo(Act(tipoHe: 2)));
        }

        [TestMethod]
        public void Tipo_TipoHe0_EsNormalAunqueTengaEstadoHe()
        {
            Assert.AreEqual(NegTableroCostos.Normal, Tipo(Act(tipoHe: 0, estadoHe: 2)));
        }

        [TestMethod]
        public void Tipo_EstadoNulo_NoFalla()
        {
            Assert.AreEqual(NegTableroCostos.Normal, Tipo(Act(estado: null)));
        }

        // ------------------------------------------------ recargo y estado

        [TestMethod]
        public void Recargo_PorTipoYCodigo()
        {
            Assert.AreEqual("50%", Rec(Act(tipoHe: 1)));
            Assert.AreEqual("100%", Rec(Act(tipoHe: 2)));
            Assert.AreEqual("Sin recargo", Rec(Act(estado: "Movilización")));
            Assert.AreEqual("100%", Rec(Act(estado: "Movilización", tipoHe: 2)));
            Assert.AreEqual("", Rec(Act()));
        }

        [TestMethod]
        public void EstadoHe_ConRecargo_TraduceElCatalogo()
        {
            Assert.AreEqual("Solicitado", Est(Act(tipoHe: 1, estadoHe: 1)));
            Assert.AreEqual("Aprobado", Est(Act(tipoHe: 1, estadoHe: 2)));
            Assert.AreEqual("Rechazado", Est(Act(tipoHe: 2, estadoHe: 3)));
            Assert.AreEqual("Sin solicitud", Est(Act(tipoHe: 2, estadoHe: 0)));
            Assert.AreEqual("Sin solicitud", Est(Act(tipoHe: 2, estadoHe: 9)));
            Assert.AreEqual("Aprobado", Est(Act(estado: "Movilización", tipoHe: 1, estadoHe: 2)));
        }

        [TestMethod]
        public void EstadoHe_TrasladoSinRecargo_EsNoAplica()
        {
            Assert.AreEqual("No aplica", Est(Act(estado: "Movilización", estadoHe: 2)));
        }

        [TestMethod]
        public void EstadoHe_Normal_EsVacio()
        {
            Assert.AreEqual("", Est(Act(estadoHe: 2)));
        }

        // ------------------------------------------------------- horas

        [TestMethod]
        public void Horas_ConvierteHhMmSs()
        {
            Assert.AreEqual(1.5m, NegTableroCostos.Horas("01:30:00"));
            Assert.AreEqual(0.3333m, NegTableroCostos.Horas("00:20:00"));
            Assert.AreEqual(25m, NegTableroCostos.Horas("25:00:00"));
            Assert.AreEqual(0.0083m, NegTableroCostos.Horas("00:00:30"));
        }

        [TestMethod]
        public void Horas_FormatoInvalido_DaCero()
        {
            foreach (string t in new[] { null, "", "abc", "1:30", "01:xx:00" })
            {
                Assert.AreEqual(0m, NegTableroCostos.Horas(t), "Tiempo '" + (t ?? "null") + "'");
            }
        }

        // ------------------------------------------------- valoracion

        [TestMethod]
        public void Clasificar_ValorHoraYFactores()
        {
            EntTcDatos d = Datos();
            d.Tarifas.Add(Tarifa("C1"));
            d.Actividades.Add(Act(tiempo: "02:00:00"));
            d.Actividades.Add(Act(tipoHe: 1, tiempo: "02:00:00"));
            d.Actividades.Add(Act(tipoHe: 2, tiempo: "01:00:00"));

            List<EntTcFila> f = NegTableroCostos.Clasificar(d).Filas;

            Assert.AreEqual(5m, f[0].ValorHora);
            Assert.AreEqual(1m, f[0].Factor);
            Assert.AreEqual(10m, f[0].Valor);
            Assert.AreEqual(1.5m, f[1].Factor);
            Assert.AreEqual(15m, f[1].Valor);
            Assert.AreEqual(2m, f[2].Factor);
            Assert.AreEqual(10m, f[2].Valor);
            Assert.IsFalse(f[0].SinSalario);
        }

        [TestMethod]
        public void Clasificar_NoAplicaHE_FactorUno()
        {
            EntTcDatos d = Datos();
            d.Tarifas.Add(Tarifa("C1", aplica: false));
            d.Actividades.Add(Act(tipoHe: 1, tiempo: "02:00:00"));

            EntTcFila f = Una(d);

            Assert.AreEqual(1m, f.Factor);
            Assert.AreEqual(10m, f.Valor);
        }

        [TestMethod]
        public void Clasificar_JornadaDeCuatroHoras_DivideEntre120()
        {
            EntTcDatos d = Datos();
            d.Tarifas.Add(Tarifa("C1", jornada: 4));
            d.Actividades.Add(Act());

            Assert.AreEqual(10m, Una(d).ValorHora);
        }

        [TestMethod]
        public void Clasificar_SinJornadaOJornadaCero_UsaOcho()
        {
            EntTcDatos d = Datos();
            d.Tarifas.Add(Tarifa("C1", jornada: null));
            d.Tarifas.Add(Tarifa("C2", jornada: 0));
            d.Actividades.Add(Act(cod: "C1", nombre: "PERSONA UNO"));
            d.Actividades.Add(Act(cod: "C2", nombre: "PERSONA DOS"));

            List<EntTcFila> f = NegTableroCostos.Clasificar(d).Filas;

            Assert.AreEqual(5m, f[0].ValorHora);
            Assert.AreEqual(5m, f[1].ValorHora);
        }

        [TestMethod]
        public void Clasificar_SinTarifa_ValorCeroYMarcado()
        {
            EntTcDatos d = Datos();
            d.Actividades.Add(Act(cod: "C9", tipoHe: 1));

            EntTcFila f = Una(d);

            Assert.AreEqual(0m, f.Valor);
            Assert.IsTrue(f.SinSalario);
        }

        [TestMethod]
        public void Clasificar_TarifaSinSalario_ValorCeroYMarcado()
        {
            EntTcDatos d = Datos();
            d.Tarifas.Add(Tarifa("C1", salario: null));
            d.Actividades.Add(Act());

            EntTcFila f = Una(d);

            Assert.AreEqual(0m, f.Valor);
            Assert.IsTrue(f.SinSalario);
        }

        [TestMethod]
        public void Clasificar_VariasCuentasMismoNombre_UsaLaQueTieneSalario()
        {
            EntTcDatos d = Datos();
            d.Tarifas.Add(Tarifa("C1", salario: null));
            d.Tarifas.Add(Tarifa("C2"));
            d.Actividades.Add(Act(cod: "C1", nombre: "PERSONA UNO"));
            d.Actividades.Add(Act(cod: "C2", nombre: "PERSONA UNO"));
            d.Actividades.Add(Act(cod: "C3", nombre: "PERSONA UNO"));

            foreach (EntTcFila f in NegTableroCostos.Clasificar(d).Filas)
            {
                Assert.AreEqual(5m, f.ValorHora, "Actividad " + f.IdDetalle);
                Assert.IsFalse(f.SinSalario);
            }
        }

        [TestMethod]
        public void Clasificar_ParametrosFaltantes_UsaLosDelExcelYAvisa()
        {
            EntTcDatos d = new EntTcDatos();
            d.Tarifas.Add(Tarifa("C1"));
            d.Actividades.Add(Act(tipoHe: 1));

            EntTcClasificado c = NegTableroCostos.Clasificar(d);

            Assert.IsTrue(c.ParametrosPorDefecto);
            Assert.AreEqual(1.5m, c.Filas[0].Factor);
            Assert.AreEqual(5m, c.Filas[0].ValorHora);
        }

        [TestMethod]
        public void Clasificar_ParametrosDeLaBase_SeRespetan()
        {
            EntTcDatos d = Datos();
            d.Parametros["Factor50"] = 1.25m;
            d.Parametros["DiasMes"] = 20m;
            d.Tarifas.Add(Tarifa("C1"));
            d.Actividades.Add(Act(tipoHe: 1));

            EntTcClasificado c = NegTableroCostos.Clasificar(d);

            Assert.IsFalse(c.ParametrosPorDefecto);
            Assert.AreEqual(1.25m, c.Filas[0].Factor);
            Assert.AreEqual(7.5m, c.Filas[0].ValorHora);
        }

        [TestMethod]
        public void Clasificar_TextosVaciosYEspacios()
        {
            EntTcDatos d = Datos();
            d.Actividades.Add(Act(cod: " C7 ", nombre: "", empresa: "   ", contrato: null));
            d.Actividades.Add(Act(nombre: " PERSONA UNO ", empresa: " EMPRESA A "));

            List<EntTcFila> f = NegTableroCostos.Clasificar(d).Filas;

            Assert.AreEqual("Cod C7", f[0].Responsable);
            Assert.AreEqual("(Sin empresa)", f[0].Empresa);
            Assert.AreEqual("(Sin contrato)", f[0].Contrato);
            Assert.AreEqual("PERSONA UNO", f[1].Responsable);
            Assert.AreEqual("EMPRESA A", f[1].Empresa);
        }

        [TestMethod]
        public void Clasificar_Periodo()
        {
            EntTcDatos d = Datos();
            d.Actividades.Add(Act(anio: 2026, mes: 3));

            EntTcFila f = Una(d);

            Assert.AreEqual("Mar-2026", f.Periodo);
            Assert.AreEqual(2026, f.Anio);
            Assert.AreEqual(3, f.Mes);
        }

        [TestMethod]
        public void Categoria_SeisCategorias()
        {
            EntTcDatos d = Datos();
            d.Actividades.Add(Act());
            d.Actividades.Add(Act(tipoHe: 1));
            d.Actividades.Add(Act(tipoHe: 2));
            d.Actividades.Add(Act(estado: "Movilización"));
            d.Actividades.Add(Act(estado: "Movilización", tipoHe: 1));
            d.Actividades.Add(Act(estado: "Movilización", tipoHe: 2));

            string[] cats = NegTableroCostos.Clasificar(d).Filas.Select(NegTableroCostos.Categoria).ToArray();

            CollectionAssert.AreEqual(new[]
            {
                "Normal", "Extra 50%", "Extra 100%", "Traslado sin recargo", "Traslado 50%", "Traslado 100%"
            }, cats);
        }

        // ===================================================== Armar (Task 3)

        /*  Juego de datos de las pruebas de Armar. Tarifas: C1 = 5 $/h, C2 = 10 $/h,
            C3 sin salario.
              UNO  EMPRESA A  Mar-2026  Normal               2 h   $10
              UNO  EMPRESA A  Mar-2026  Extra 50% Aprobado   1 h   $7,50
              UNO  EMPRESA A  Abr-2026  Extra 100% Solicit.  1 h   $10
              DOS  EMPRESA B  Mar-2026  Traslado sin recargo 1 h   $10
              DOS  EMPRESA B  Abr-2026  Traslado 50% Aprob.  2 h   $30
              DOS  EMPRESA B  Abr-2026  Normal               3 h   $30
              TRES EMPRESA A  Dic-2025  Normal               1 h   $0
            Totales: Normal 6 h $40 (3) | Extra 2 h $17,50 (2) | Traslado 3 h $40 (2)
                     Total 11 h $97,50 (7) */
        private static EntTcClasificado Base()
        {
            EntTcDatos d = Datos();
            d.Tarifas.Add(Tarifa("C1", salario: 1200m));
            d.Tarifas.Add(Tarifa("C2", salario: 2400m));
            d.Actividades.Add(Act(tiempo: "02:00:00", cod: "C1", nombre: "PERSONA UNO", empresa: "EMPRESA A", mes: 3));
            d.Actividades.Add(Act(tipoHe: 1, estadoHe: 2, cod: "C1", nombre: "PERSONA UNO", empresa: "EMPRESA A", mes: 3));
            d.Actividades.Add(Act(tipoHe: 2, estadoHe: 1, cod: "C1", nombre: "PERSONA UNO", empresa: "EMPRESA A", mes: 4));
            d.Actividades.Add(Act(estado: "Movilización", cod: "C2", nombre: "PERSONA DOS", empresa: "EMPRESA B", mes: 3));
            d.Actividades.Add(Act(estado: "Movilización", tipoHe: 1, estadoHe: 2, tiempo: "02:00:00", cod: "C2", nombre: "PERSONA DOS", empresa: "EMPRESA B", mes: 4));
            d.Actividades.Add(Act(tiempo: "03:00:00", cod: "C2", nombre: "PERSONA DOS", empresa: "EMPRESA B", mes: 4));
            d.Actividades.Add(Act(cod: "C3", nombre: "PERSONA TRES", empresa: "EMPRESA A", anio: 2025, mes: 12));
            return NegTableroCostos.Clasificar(d);
        }

        private static EntTableroCostos Armar(EntTcFiltro f = null)
        {
            return NegTableroCostos.Armar(Base(), f ?? new EntTcFiltro());
        }

        private static void Celda(EntTcCelda c, decimal horas, decimal valor, int actividades, string que)
        {
            Assert.AreEqual(horas, c.Horas, que + ": horas");
            Assert.AreEqual(valor, c.Valor, que + ": valor");
            Assert.AreEqual(actividades, c.Actividades, que + ": actividades");
        }

        private static EntTcResumenFila Resumen(EntTableroCostos t, string concepto)
        {
            return t.Resumen.Single(r => r.Concepto == concepto);
        }

        [TestMethod]
        public void Armar_SinFiltros_Indicadores()
        {
            EntTableroCostos t = Armar();

            Celda(t.Indicadores.Normal, 6m, 40m, 3, "Normal");
            Celda(t.Indicadores.Extra, 2m, 17.5m, 2, "Extra");
            Celda(t.Indicadores.Traslado, 3m, 40m, 2, "Traslado");
            Celda(t.Indicadores.Total, 11m, 97.5m, 7, "Total");
            Assert.AreEqual("", t.Aviso);
        }

        [TestMethod]
        public void Armar_TodosEsIgualASinFiltro()
        {
            EntTableroCostos t = Armar(new EntTcFiltro
            {
                Mes = "Todos", Responsable = "Todos", Empresa = "Todos", Contrato = "Todos", Recargo = "Todos", EstadoHE = "Todos"
            });

            Celda(t.Indicadores.Total, 11m, 97.5m, 7, "Total");
            Assert.AreEqual("", t.Aviso);
        }

        [TestMethod]
        public void Armar_FiltroRecargo_NoAfectaALasNormales()
        {
            EntTableroCostos t = Armar(new EntTcFiltro { Recargo = "50%" });

            Celda(t.Indicadores.Normal, 6m, 40m, 3, "Normal");
            Celda(t.Indicadores.Extra, 1m, 7.5m, 1, "Extra");
            Celda(t.Indicadores.Traslado, 2m, 30m, 1, "Traslado");
        }

        [TestMethod]
        public void Armar_FiltroEstadoHe_NoAfectaALasNormales()
        {
            EntTableroCostos t = Armar(new EntTcFiltro { EstadoHE = "Aprobado" });

            Celda(t.Indicadores.Normal, 6m, 40m, 3, "Normal");
            Celda(t.Indicadores.Extra, 1m, 7.5m, 1, "Extra");
            Celda(t.Indicadores.Traslado, 2m, 30m, 1, "Traslado");
        }

        [TestMethod]
        public void Armar_FiltroResponsable_ElTopLoIgnora()
        {
            EntTableroCostos t = Armar(new EntTcFiltro { Responsable = "PERSONA UNO" });

            Celda(t.Indicadores.Total, 4m, 27.5m, 3, "Total");
            CollectionAssert.AreEqual(new[] { "PERSONA DOS", "PERSONA UNO", "PERSONA TRES" },
                                      t.TopNormal.Select(x => x.Responsable).ToArray());
        }

        [TestMethod]
        public void Armar_FiltroMes_LaTablaPorMesLoIgnoraYLaPorAnioNo()
        {
            EntTableroCostos t = Armar(new EntTcFiltro { Mes = "Mar-2026" });

            Celda(t.Indicadores.Total, 4m, 27.5m, 3, "Total");
            Assert.AreEqual(6m, t.PorMes.Single(p => p.Periodo == "Abr-2026").HTotal);
            Assert.AreEqual(4m, t.PorAnio.Single(p => p.Periodo == "2026").HTotal);
            Assert.AreEqual(0m, t.PorAnio.Single(p => p.Periodo == "2025").HTotal);
        }

        [TestMethod]
        public void Armar_FiltroSinDistinguirMayusculas()
        {
            EntTableroCostos t = Armar(new EntTcFiltro { Empresa = "  empresa b " });

            Celda(t.Indicadores.Traslado, 3m, 40m, 2, "Traslado");
            Assert.AreEqual("", t.Aviso);
        }

        [TestMethod]
        public void Armar_Resumen_OnceFilasEnElOrdenDelExcel()
        {
            EntTableroCostos t = Armar();

            CollectionAssert.AreEqual(new[]
            {
                "Normal", "Extra 50%", "Extra 100%", "Subtotal Horas Extras",
                "Traslado sin recargo", "Traslado 50%", "Traslado 100%", "Subtotal Traslado",
                "Total", "Total al 50% (Extra + Traslado)", "Total al 100% (Extra + Traslado)"
            }, t.Resumen.Select(r => r.Concepto).ToArray());
        }

        [TestMethod]
        public void Armar_Resumen_SubtotalesPorcentajesYValorPorHora()
        {
            EntTableroCostos t = Armar();

            Assert.AreEqual(2m, Resumen(t, "Subtotal Horas Extras").Horas);
            Assert.AreEqual(3m, Resumen(t, "Subtotal Traslado").Horas);
            Assert.AreEqual(3m, Resumen(t, "Total al 50% (Extra + Traslado)").Horas);
            Assert.AreEqual(37.5m, Resumen(t, "Total al 50% (Extra + Traslado)").Valor);
            Assert.AreEqual(1m, Resumen(t, "Total al 100% (Extra + Traslado)").Horas);
            Assert.AreEqual(1m, Resumen(t, "Total").PctHoras);
            Assert.AreEqual(1m, Resumen(t, "Total").PctValor);
            Assert.AreEqual(6m / 11m, Resumen(t, "Normal").PctHoras);
            Assert.AreEqual(40m / 6m, Resumen(t, "Normal").ValorPorHora);
            Assert.IsTrue(Resumen(t, "Subtotal Traslado").EsSubtotal);
            Assert.IsFalse(Resumen(t, "Normal").EsSubtotal);
        }

        [TestMethod]
        public void Armar_SinActividades_NoFallaYPorcentajesEnCero()
        {
            EntTableroCostos t = NegTableroCostos.Armar(new EntTcClasificado(), new EntTcFiltro());

            Assert.AreEqual(11, t.Resumen.Count);
            Assert.IsTrue(t.Resumen.All(r => r.PctHoras == 0m && r.PctValor == 0m && r.ValorPorHora == 0m));
            Assert.AreEqual(0, t.Indicadores.Total.Actividades);
            Assert.AreEqual(0, t.Detalle.Count);
            Assert.AreEqual("", t.Aviso);
        }

        [TestMethod]
        public void Armar_ExtrasPorEstado()
        {
            EntTableroCostos t = Armar();

            CollectionAssert.AreEqual(new[] { "Aprobado", "Solicitado", "Rechazado", "Sin solicitud", "Total" },
                                      t.ExtrasPorEstado.Select(e => e.Estado).ToArray());
            EntTcEstadoExtra aprobado = t.ExtrasPorEstado[0];
            Assert.AreEqual(1m, aprobado.Horas50);
            Assert.AreEqual(7.5m, aprobado.Valor50);
            Assert.AreEqual(0m, aprobado.Horas100);
            EntTcEstadoExtra solicitado = t.ExtrasPorEstado[1];
            Assert.AreEqual(1m, solicitado.Horas100);
            Assert.AreEqual(10m, solicitado.Valor100);
            Assert.AreEqual(17.5m, t.ExtrasPorEstado[4].Total);
        }

        [TestMethod]
        public void Armar_TrasladosPorEstado_IncluyeNoAplica()
        {
            EntTableroCostos t = Armar();

            CollectionAssert.AreEqual(new[] { "Aprobado", "Solicitado", "Rechazado", "Sin solicitud", "No aplica", "Total" },
                                      t.TrasladosPorEstado.Select(e => e.Estado).ToArray());
            Assert.AreEqual(2m, t.TrasladosPorEstado[0].Horas50);
            Assert.AreEqual(1m, t.TrasladosPorEstado[4].HorasSinRecargo);
            Assert.AreEqual(3m, t.TrasladosPorEstado[5].TotalHoras);
            Assert.AreEqual(40m, t.TrasladosPorEstado[5].TotalValor);
        }

        [TestMethod]
        public void Armar_PorAnioYPorMes_LlevanFilaTotal()
        {
            EntTableroCostos t = Armar();

            CollectionAssert.AreEqual(new[] { "2025", "2026", "Total" }, t.PorAnio.Select(p => p.Periodo).ToArray());
            CollectionAssert.AreEqual(new[] { "Dic-2025", "Mar-2026", "Abr-2026", "Total" }, t.PorMes.Select(p => p.Periodo).ToArray());
            EntTcPeriodo total = t.PorAnio.Last();
            Assert.AreEqual(11m, total.HTotal);
            Assert.AreEqual(97.5m, total.VTotal);
            Assert.AreEqual(6m, total.HNormal);
            Assert.AreEqual(1m, total.HExtra50);
            Assert.AreEqual(1m, total.HExtra100);
            Assert.AreEqual(1m, total.HTrasladoSin);
            Assert.AreEqual(2m, total.HTraslado50);
            Assert.AreEqual(0m, total.HTraslado100);
        }

        [TestMethod]
        public void Armar_Top_OrdenadoPorValorYMaximoQuince()
        {
            EntTcDatos d = Datos();
            for (int i = 1; i <= 16; i++)
            {
                string cod = "P" + i.ToString("00", CultureInfo.InvariantCulture);
                d.Tarifas.Add(Tarifa(cod));
                d.Actividades.Add(Act(cod: cod, nombre: "PERSONA " + cod, tiempo: i.ToString("00", CultureInfo.InvariantCulture) + ":00:00"));
            }

            EntTableroCostos t = NegTableroCostos.Armar(NegTableroCostos.Clasificar(d), new EntTcFiltro());

            Assert.AreEqual(NegTableroCostos.TopMaximo, t.TopNormal.Count);
            Assert.AreEqual("PERSONA P16", t.TopNormal[0].Responsable);
            Assert.AreEqual(80m, t.TopNormal[0].Valor);
            Assert.IsFalse(t.TopNormal.Any(x => x.Responsable == "PERSONA P01"));
        }

        [TestMethod]
        public void Armar_TopExtraYTraslado_DesglosanRecargo()
        {
            EntTableroCostos t = Armar();

            EntTcTop extra = t.TopExtra.Single();
            Assert.AreEqual("PERSONA UNO", extra.Responsable);
            Assert.AreEqual(1m, extra.Horas50);
            Assert.AreEqual(1m, extra.Horas100);
            Assert.AreEqual(17.5m, extra.Valor);

            EntTcTop traslado = t.TopTraslado.Single();
            Assert.AreEqual(1m, traslado.HorasSinRecargo);
            Assert.AreEqual(2m, traslado.Horas50);
            Assert.AreEqual(40m, traslado.Valor);
        }

        [TestMethod]
        public void Armar_Listas_SonDependientesYMesesCronologicos()
        {
            EntTableroCostos todo = Armar();
            CollectionAssert.AreEqual(new[] { "Dic-2025", "Mar-2026", "Abr-2026" }, todo.Meses.ToArray());

            EntTableroCostos t = Armar(new EntTcFiltro { Responsable = "PERSONA DOS" });
            CollectionAssert.AreEqual(new[] { "EMPRESA B" }, t.Empresas.ToArray());
            CollectionAssert.AreEqual(new[] { "PERSONA DOS", "PERSONA TRES", "PERSONA UNO" }, t.Responsables.ToArray());
        }

        [TestMethod]
        public void Armar_ElegidoQueNoExisteConLosDemas_AvisaYLoConserva()
        {
            EntTableroCostos t = Armar(new EntTcFiltro { Empresa = "EMPRESA B", Responsable = "PERSONA UNO" });

            Assert.AreEqual(0, t.Indicadores.Total.Actividades);
            StringAssert.StartsWith(t.Aviso, "⚠ ");
            StringAssert.Contains(t.Aviso, "El responsable elegido");
            StringAssert.Contains(t.Aviso, "La empresa elegida");
            StringAssert.EndsWith(t.Aviso, "Elija otro valor o Todos.");
            CollectionAssert.Contains(t.Responsables, "PERSONA UNO");
            CollectionAssert.Contains(t.Empresas, "EMPRESA B");
        }

        /* Con Mes/Responsable/Empresa/Contrato elegidos, el aviso de la lista
           dependiente sale primero. "Ninguna actividad" queda para cuando solo
           Recargo o Estado HE vacian el resultado: como las Normales nunca se
           filtran por recargo, eso pasa solo si no hay Normales. */
        [TestMethod]
        public void Armar_RecargoQueVaciaElResultado_Avisa()
        {
            EntTcDatos d = Datos();
            d.Tarifas.Add(Tarifa("C1"));
            d.Actividades.Add(Act(tipoHe: 1));

            EntTableroCostos t = NegTableroCostos.Armar(NegTableroCostos.Clasificar(d), new EntTcFiltro { Recargo = "100%" });

            Assert.AreEqual(0, t.Indicadores.Total.Actividades);
            StringAssert.Contains(t.Aviso, "Ninguna actividad cumple los filtros elegidos.");
        }

        [TestMethod]
        public void Armar_SinSalario_SoloDelResultadoFiltrado()
        {
            CollectionAssert.AreEqual(new[] { "PERSONA TRES" }, Armar().SinSalario.ToArray());
            Assert.AreEqual(0, Armar(new EntTcFiltro { Responsable = "PERSONA UNO" }).SinSalario.Count);
        }

        [TestMethod]
        public void Armar_Detalle_OrdenadoPorFecha()
        {
            EntTableroCostos t = Armar();

            Assert.AreEqual(7, t.Detalle.Count);
            Assert.AreEqual("PERSONA TRES", t.Detalle[0].Responsable);
            Assert.IsTrue(t.Detalle.Zip(t.Detalle.Skip(1), (a, b) => a.FechaInicio <= b.FechaInicio).All(x => x));
        }

        [TestMethod]
        public void Armar_ParametrosPorDefecto_SePropaga()
        {
            EntTcDatos d = new EntTcDatos();
            d.Actividades.Add(Act());

            Assert.IsTrue(NegTableroCostos.Armar(NegTableroCostos.Clasificar(d), new EntTcFiltro()).ParametrosPorDefecto);
            Assert.IsFalse(Armar().ParametrosPorDefecto);
        }

        // ----------------------------------------------- rango y descripcion

        [TestMethod]
        public void ValidarRango_DosAniosCalendario_EsValido()
        {
            Assert.AreEqual("", NegTableroCostos.ValidarRango(new DateTime(2025, 1, 1), new DateTime(2026, 12, 31)));
        }

        [TestMethod]
        public void ValidarRango_HastaAnteriorADesde_Rechaza()
        {
            Assert.AreNotEqual("", NegTableroCostos.ValidarRango(new DateTime(2026, 3, 1), new DateTime(2026, 2, 28)));
        }

        [TestMethod]
        public void ValidarRango_MasDeVeinticuatroMeses_Rechaza()
        {
            StringAssert.Contains(NegTableroCostos.ValidarRango(new DateTime(2025, 1, 1), new DateTime(2027, 1, 1)), "24 meses");
        }

        [TestMethod]
        public void DescribirFiltro_ConCulturaEspanola_NoCambiaElFormato()
        {
            CultureInfo antes = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("th-TH");
                string s = NegTableroCostos.DescribirFiltro(new DateTime(2025, 1, 1), new DateTime(2026, 12, 31),
                                                            new EntTcFiltro { Empresa = " EMPRESA B ", Recargo = "Todos" });

                Assert.AreEqual("Desde 01/01/2025 · Hasta 31/12/2026 · Mes: Todos · Responsable: Todos · "
                              + "Empresa: EMPRESA B · Contrato: Todos · Recargo: Todos · Estado HE: Todos", s);
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = antes;
            }
        }
    }
}
