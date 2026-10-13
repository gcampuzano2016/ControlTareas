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
    }
}
