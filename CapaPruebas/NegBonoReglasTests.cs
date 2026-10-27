using CapaEntidad;
using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CapaPruebas
{
    [TestClass]
    public class NegBonoReglasTests
    {
        static DateTime D(int m, int d) => new DateTime(2026, m, d);

        static EntBonoPersona P(string cod, string jefe, string empresa, string sap = null, string cedula = null)
            => new EntBonoPersona { CodUsuario = cod, Nombre = "Persona " + cod, CodJefe = jefe, Empresa = empresa, CodSap = sap, Cedula = cedula };

        static EntBonoTipo T(int id, bool porSemana, decimal? sugerido, string nombre = "Tipo")
            => new EntBonoTipo { IdTipo = id, Nombre = nombre, PorSemana = porSemana, ValorSugerido = sugerido, Activo = true };

        static EntBono B(int id, string cod, int tipo, string estado, params DateTime[] lunes)
            => new EntBono { IdBono = id, IdPeriodo = 1, CodUsuario = cod, IdTipo = tipo, Estado = estado, CodJefe = "J", Empresa = "DOS", Valor = 100m, Lunes = lunes.ToList() };

        static EntBonoPeriodo Per(string estado, int id = 1) => new EntBonoPeriodo { IdPeriodo = id, Anio = 2026, Mes = 10, Estado = estado };

        static ISet<string> Gente(params string[] cods) => new HashSet<string>(cods);

        // ---------- MiGente / Normalizar ----------
        [TestMethod]
        public void MiGente_DirectosSiNietosNoYoNo()
        {
            var activas = new[] { P("A", "A", "DOS"), P("B", "A", "DOS"), P("C", "B", "DOS") };
            var r = NegBonoReglas.MiGente(activas, "A");
            CollectionAssert.AreEqual(new[] { "B" }, r.Select(p => p.CodUsuario).ToArray());
        }

        [TestMethod]
        public void MiGente_CodigoConEspaciosYMinusculas()
        {
            var activas = new[] { P("B", " a ", "DOS"), P(" c", "X", "DOS") };
            var r = NegBonoReglas.MiGente(activas, "A");
            Assert.AreEqual(1, r.Count);
            Assert.AreEqual("B", r[0].CodUsuario);
            Assert.AreEqual(1, NegBonoReglas.MiGente(activas, " a ").Count);
        }

        [TestMethod]
        public void MiGente_OrdenadaPorNombre()
        {
            var z = P("Z", "J", "DOS"); z.Nombre = "Zoila";
            var a = P("Y", "J", "DOS"); a.Nombre = "Ana";
            var r = NegBonoReglas.MiGente(new[] { z, a }, "J");
            CollectionAssert.AreEqual(new[] { "Ana", "Zoila" }, r.Select(p => p.Nombre).ToArray());
        }

        [TestMethod]
        public void Normalizar_TrimMayusculasYNull()
        {
            Assert.AreEqual("AB1", NegBonoReglas.Normalizar("  ab1 "));
            Assert.AreEqual("", NegBonoReglas.Normalizar(null));
        }

        // ---------- Empresa / Identificacion ----------
        [TestMethod]
        public void EmpresaNormalizada_ValidasYNoValidas()
        {
            Assert.AreEqual("AGILITY", NegBonoReglas.EmpresaNormalizada(" Agility "));
            Assert.AreEqual("DOS", NegBonoReglas.EmpresaNormalizada("dos"));
            Assert.AreEqual("", NegBonoReglas.EmpresaNormalizada("OTRA"));
            Assert.AreEqual("", NegBonoReglas.EmpresaNormalizada(null));
        }

        // ---------- Lugar ----------
        [TestMethod]
        public void LugarNormalizado_SucursalesVacioYOtros()
        {
            Assert.AreEqual("Guayaquil", NegBonoReglas.LugarNormalizado(" GUAYAQUIL "));
            Assert.AreEqual("Quito", NegBonoReglas.LugarNormalizado("quito"));
            Assert.AreEqual("Cuenca", NegBonoReglas.LugarNormalizado("Cuenca"));
            Assert.AreEqual("Ambato", NegBonoReglas.LugarNormalizado("aMbAtO"));
            Assert.AreEqual("", NegBonoReglas.LugarNormalizado(null));
            Assert.AreEqual("", NegBonoReglas.LugarNormalizado("  "));
            Assert.IsNull(NegBonoReglas.LugarNormalizado("Manta"));
            Assert.IsNull(NegBonoReglas.LugarNormalizado("Quito & Guayaquil"));
        }

        [TestMethod] public void Identificacion_Dos_UsaCodSap() => Assert.AreEqual("793", NegBonoReglas.Identificacion(P("1", "J", "DOS", " 793 ", null), out _));
        [TestMethod] public void Identificacion_Agility_RellenaCedula() => Assert.AreEqual("0912345678", NegBonoReglas.Identificacion(P("1", "J", "Agility", null, "912345678"), out _));

        [TestMethod]
        public void Identificacion_Agility_SoloDigitos()
        {
            Assert.AreEqual("0912345678", NegBonoReglas.Identificacion(P("1", "J", "AGILITY", null, "9123-456 78"), out string e));
            Assert.IsNull(e);
        }

        [TestMethod]
        public void Identificacion_DosSinSap_Error()
        {
            string id = NegBonoReglas.Identificacion(P("1", "J", "DOS", " ", null), out string e);
            Assert.AreEqual("", id);
            Assert.AreEqual("Falta el código SAP de Persona 1 en Administración de Usuarios.", e);
        }

        [TestMethod]
        public void Identificacion_AgilitySinCedula_Error()
        {
            NegBonoReglas.Identificacion(P("1", "J", "AGILITY", "5", null), out var e);
            StringAssert.Contains(e, "cédula");
            Assert.AreEqual("Falta la cédula de Persona 1 en Administración de Usuarios.", e);
        }

        [TestMethod]
        public void Identificacion_EmpresaRara_Error()
        {
            string id = NegBonoReglas.Identificacion(P("1", "J", "OTRA", "5", "1"), out var e);
            Assert.AreEqual("", id);
            StringAssert.Contains(e, "DOS ni AGILITY");
        }

        // ---------- Valor ----------
        [TestMethod] public void Sugerido_PorSemana_TarifaPorSemanas() => Assert.AreEqual(112.50m, NegBonoReglas.Sugerido(T(1, true, 37.50m), 3));
        [TestMethod] public void Sugerido_SinSugerido_Null() => Assert.IsNull(NegBonoReglas.Sugerido(T(1, false, null), 0));
        [TestMethod] public void Sugerido_NoSemanal_EsElSugerido() => Assert.AreEqual(50m, NegBonoReglas.Sugerido(T(1, false, 50m), 0));
        [TestMethod] public void Valor_DistintoDelSugeridoSinMotivo_Error() => Assert.IsNotNull(NegBonoReglas.ValidarValor(T(1, true, 37.50m), 2, 70m, ""));
        [TestMethod] public void Valor_DistintoDelSugeridoConMotivo_Vale() => Assert.IsNull(NegBonoReglas.ValidarValor(T(1, true, 37.50m), 2, 70m, "Una guardia parcial"));
        [TestMethod] public void Valor_IgualAlSugerido_NoPideMotivo() => Assert.IsNull(NegBonoReglas.ValidarValor(T(1, true, 37.50m), 2, 75m, ""));
        [TestMethod] public void Valor_SinSugerido_Libre() => Assert.IsNull(NegBonoReglas.ValidarValor(T(1, false, null), 0, 230m, ""));
        [TestMethod] public void Valor_CeroONegativo_Error()
        {
            Assert.IsNotNull(NegBonoReglas.ValidarValor(T(1, false, null), 0, 0m, "x"));
            Assert.IsNotNull(NegBonoReglas.ValidarValor(T(1, false, null), 0, -5m, "x"));
        }

        // ---------- Doble pago ----------
        [TestMethod]
        public void DoblePago_MismaSemanaOtroPeriodo_Choca()
        {
            var b = B(1, "U1", 7, BonoEstados.Borrador, D(8, 3), D(8, 10));
            var otro = B(2, "U1", 7, BonoEstados.Aprobado, D(8, 10));
            otro.IdPeriodo = 99;
            var r = NegBonoReglas.ChocaDoblePago(b, new[] { otro });
            CollectionAssert.AreEqual(new[] { D(8, 10) }, r);
        }

        [TestMethod]
        public void DoblePago_IgnoraRechazados()
        {
            var b = B(1, "U1", 7, BonoEstados.Borrador, D(8, 3));
            var otro = B(2, "U1", 7, BonoEstados.Rechazado, D(8, 3));
            Assert.AreEqual(0, NegBonoReglas.ChocaDoblePago(b, new[] { otro }).Count);
        }

        [TestMethod]
        public void DoblePago_OtroTipo_NoChoca()
        {
            // standby GPF por semana (tipo 7) contra standby GPF de valor fijo (tipo 8)
            var b = B(1, "U1", 7, BonoEstados.Borrador, D(8, 3));
            var otro = B(2, "U1", 8, BonoEstados.Aprobado, D(8, 3));
            Assert.AreEqual(0, NegBonoReglas.ChocaDoblePago(b, new[] { otro }).Count);
        }

        [TestMethod]
        public void DoblePago_ElMismoBono_NoChoca()
        {
            var b = B(1, "U1", 7, BonoEstados.Borrador, D(8, 3));
            var mismo = B(1, "U1", 7, BonoEstados.Borrador, D(8, 3));
            Assert.AreEqual(0, NegBonoReglas.ChocaDoblePago(b, new[] { mismo }).Count);
        }

        [TestMethod]
        public void DoblePago_OtraPersona_NoChoca()
        {
            var b = B(1, "U1", 7, BonoEstados.Borrador, D(8, 3));
            var otro = B(2, "U2", 7, BonoEstados.Aprobado, D(8, 3));
            Assert.AreEqual(0, NegBonoReglas.ChocaDoblePago(b, new[] { otro }).Count);
        }

        [TestMethod]
        public void DoblePago_CodigoConEspaciosYMinusculas_Choca()
        {
            var b = B(1, "U1", 7, BonoEstados.Borrador, D(8, 3));
            var otro = B(2, " u1 ", 7, BonoEstados.Enviado, D(8, 3));
            Assert.AreEqual(1, NegBonoReglas.ChocaDoblePago(b, new[] { otro }).Count);
        }

        [TestMethod]
        public void DoblePago_Mensaje_ListaLasSemanas()
        {
            string m = NegBonoReglas.MensajeDoblePago(new List<DateTime> { D(8, 4) });
            StringAssert.Contains(m, "semana del 4 de agosto");
            string dos = NegBonoReglas.MensajeDoblePago(new List<DateTime> { D(8, 10), D(8, 3) });
            StringAssert.Contains(dos, "semana del 3 de agosto");
            StringAssert.Contains(dos, "semana del 10 de agosto");
            Assert.IsTrue(dos.IndexOf("3 de agosto") < dos.IndexOf("10 de agosto"));
        }

        // ---------- Repetido ----------
        [TestMethod]
        public void Repetido_NoSemanalMismoPeriodo_Avisa()
        {
            var t = T(5, false, 50m, "Bono de ejemplo");
            var b = B(1, "U1", 5, BonoEstados.Borrador);
            var otro = B(2, "U1", 5, BonoEstados.Aprobado);
            string aviso = NegBonoReglas.AvisoRepetido(b, t, new[] { otro });
            Assert.IsNotNull(aviso);
            StringAssert.Contains(aviso, "Bono de ejemplo");
        }

        [TestMethod]
        public void Repetido_Semanal_NoAvisa()
        {
            var t = T(5, true, 37.50m);
            var b = B(1, "U1", 5, BonoEstados.Borrador, D(8, 3));
            var otro = B(2, "U1", 5, BonoEstados.Aprobado, D(8, 10));
            Assert.IsNull(NegBonoReglas.AvisoRepetido(b, t, new[] { otro }));
        }

        [TestMethod]
        public void Repetido_RechazadoOMismoBonoOOtraPersona_NoAvisa()
        {
            var t = T(5, false, 50m);
            var b = B(1, "U1", 5, BonoEstados.Borrador);
            Assert.IsNull(NegBonoReglas.AvisoRepetido(b, t, new[] { B(2, "U1", 5, BonoEstados.Rechazado) }));
            Assert.IsNull(NegBonoReglas.AvisoRepetido(b, t, new[] { B(1, "U1", 5, BonoEstados.Borrador) }));
            Assert.IsNull(NegBonoReglas.AvisoRepetido(b, t, new[] { B(2, "U2", 5, BonoEstados.Aprobado) }));
            Assert.IsNull(NegBonoReglas.AvisoRepetido(b, t, new[] { B(2, "U1", 6, BonoEstados.Aprobado) }));
        }

        // ---------- Permisos ----------
        [TestMethod]
        public void PuedeCrear_ExigeMesAbiertoYGenteMia()
        {
            Assert.IsNull(NegBonoReglas.PuedeCrear(Per(BonoPeriodoEstados.Abierto), Gente("U1"), " u1 "));
            Assert.AreEqual("No hay un mes de bonos abierto.", NegBonoReglas.PuedeCrear(null, Gente("U1"), "U1"));
            Assert.AreEqual("No hay un mes de bonos abierto.", NegBonoReglas.PuedeCrear(Per(BonoPeriodoEstados.Cerrado), Gente("U1"), "U1"));
            Assert.IsNotNull(NegBonoReglas.PuedeCrear(Per(BonoPeriodoEstados.Abierto), Gente("U1"), "U9"));
        }

        [TestMethod]
        public void PuedeEditar_BorradorORechazadoDeMiGente_Si()
        {
            var ab = Per(BonoPeriodoEstados.Abierto);
            var g = Gente("U1");
            Assert.IsNull(NegBonoReglas.PuedeEditar(B(1, "U1", 5, BonoEstados.Borrador), ab, "J", g));
            Assert.IsNull(NegBonoReglas.PuedeEditar(B(1, "U1", 5, BonoEstados.Rechazado), ab, " j ", g));
            Assert.IsNull(NegBonoReglas.PuedeEnviar(B(1, "U1", 5, BonoEstados.Borrador), ab, "J", g));
        }

        [TestMethod]
        public void PuedeEditar_EnviadoOAprobado_No()
        {
            var ab = Per(BonoPeriodoEstados.Abierto);
            var g = Gente("U1");
            Assert.IsNotNull(NegBonoReglas.PuedeEditar(B(1, "U1", 5, BonoEstados.Enviado), ab, "J", g));
            Assert.IsNotNull(NegBonoReglas.PuedeEditar(B(1, "U1", 5, BonoEstados.Aprobado), ab, "J", g));
            Assert.IsNotNull(NegBonoReglas.PuedeEnviar(B(1, "U1", 5, BonoEstados.Enviado), ab, "J", g));
        }

        [TestMethod]
        public void PuedeEnviar_RechazadoNo_SoloBorradores()
        {
            var ab = Per(BonoPeriodoEstados.Abierto);
            Assert.IsNotNull(NegBonoReglas.PuedeEnviar(B(1, "U1", 5, BonoEstados.Rechazado), ab, "J", Gente("U1")));
        }

        [TestMethod]
        public void PuedeEditar_JefeQueYaNoEsElJefe_No()
        {
            var ab = Per(BonoPeriodoEstados.Abierto);
            // b.CodJefe = "J" pero la persona ya no esta en MiGente("J")
            Assert.IsNotNull(NegBonoReglas.PuedeEditar(B(1, "U1", 5, BonoEstados.Borrador), ab, "J", Gente()));
            Assert.IsNotNull(NegBonoReglas.PuedeEnviar(B(1, "U1", 5, BonoEstados.Borrador), ab, "J", Gente()));
            // y quien no es el jefe del bono tampoco, aunque la persona este en su gente
            Assert.IsNotNull(NegBonoReglas.PuedeEditar(B(1, "U1", 5, BonoEstados.Borrador), ab, "NUEVO", Gente("U1")));
        }

        [TestMethod]
        public void PuedeEditar_SinPeriodoAbierto_No()
        {
            // pendiente sin periodo mientras no hay mes abierto: solo lectura
            var pendiente = B(1, "U1", 5, BonoEstados.Borrador);
            pendiente.IdPeriodo = null;
            Assert.AreEqual("No hay un mes de bonos abierto.", NegBonoReglas.PuedeEditar(pendiente, null, "J", Gente("U1")));
            Assert.AreEqual("No hay un mes de bonos abierto.", NegBonoReglas.PuedeEnviar(pendiente, null, "J", Gente("U1")));
            Assert.AreEqual("No hay un mes de bonos abierto.", NegBonoReglas.PuedeEditar(pendiente, Per(BonoPeriodoEstados.Cerrado), "J", Gente("U1")));
            // con mes abierto, el pendiente si se edita
            Assert.IsNull(NegBonoReglas.PuedeEditar(pendiente, Per(BonoPeriodoEstados.Abierto), "J", Gente("U1")));
        }

        [TestMethod]
        public void PuedeEditar_BonoDeOtroPeriodo_No()
        {
            var b = B(1, "U1", 5, BonoEstados.Borrador);
            b.IdPeriodo = 5;
            Assert.IsNotNull(NegBonoReglas.PuedeEditar(b, Per(BonoPeriodoEstados.Abierto, 6), "J", Gente("U1")));
        }

        [TestMethod]
        public void PuedeRevisar_AprobarSoloEnviado()
        {
            var ab = Per(BonoPeriodoEstados.Abierto);
            Assert.IsNull(NegBonoReglas.PuedeRevisar(B(1, "U1", 5, BonoEstados.Enviado), ab, "Aprobar"));
            Assert.IsNotNull(NegBonoReglas.PuedeRevisar(B(1, "U1", 5, BonoEstados.Aprobado), ab, "Aprobar"));
            Assert.IsNotNull(NegBonoReglas.PuedeRevisar(B(1, "U1", 5, BonoEstados.Borrador), ab, "Aprobar"));
            Assert.IsNotNull(NegBonoReglas.PuedeRevisar(B(1, "U1", 5, BonoEstados.Rechazado), ab, "Aprobar"));
        }

        [TestMethod]
        public void PuedeRevisar_AjustarORechazarAprobadoConMesAbierto_Si()
        {
            var ab = Per(BonoPeriodoEstados.Abierto);
            foreach (string accion in new[] { "Ajustar", "Rechazar" })
            {
                Assert.IsNull(NegBonoReglas.PuedeRevisar(B(1, "U1", 5, BonoEstados.Aprobado), ab, accion));
                Assert.IsNull(NegBonoReglas.PuedeRevisar(B(1, "U1", 5, BonoEstados.Enviado), ab, accion));
                Assert.IsNotNull(NegBonoReglas.PuedeRevisar(B(1, "U1", 5, BonoEstados.Rechazado), ab, accion));
            }
        }

        [TestMethod]
        public void PuedeRevisar_RechazarBorrador_Si_AjustarBorrador_No()
        {
            // Un borrador huerfano (la persona cambio de jefe o el jefe se fue) bloquea sus
            // semanas; Talento Humano lo rechaza para liberarlas. Ajustar y aprobar, no.
            var ab = Per(BonoPeriodoEstados.Abierto);
            Assert.IsNull(NegBonoReglas.PuedeRevisar(B(1, "U1", 5, BonoEstados.Borrador), ab, "Rechazar"));
            Assert.IsNotNull(NegBonoReglas.PuedeRevisar(B(1, "U1", 5, BonoEstados.Borrador), ab, "Ajustar"));
            Assert.IsNotNull(NegBonoReglas.PuedeRevisar(B(1, "U1", 5, BonoEstados.Borrador), ab, "Aprobar"));
            Assert.IsNotNull(NegBonoReglas.PuedeRevisar(B(1, "U1", 5, BonoEstados.Borrador), Per(BonoPeriodoEstados.Cerrado), "Rechazar"));
        }

        [TestMethod]
        public void PuedeBorrar_MiBorradorORechazado_AunqueYaNoSeaMiGente_Si()
        {
            var ab = Per(BonoPeriodoEstados.Abierto);
            Assert.IsNull(NegBonoReglas.PuedeBorrar(B(1, "U1", 5, BonoEstados.Borrador), ab, "J"));
            Assert.IsNull(NegBonoReglas.PuedeBorrar(B(1, "U1", 5, BonoEstados.Rechazado), ab, " j "));
        }

        [TestMethod]
        public void PuedeBorrar_BonoDeOtroJefe_No()
        {
            var ab = Per(BonoPeriodoEstados.Abierto);
            // El jefe nuevo no borra lo que propuso el viejo.
            Assert.IsNotNull(NegBonoReglas.PuedeBorrar(B(1, "U1", 5, BonoEstados.Borrador), ab, "NUEVO"));
            Assert.IsNotNull(NegBonoReglas.PuedeBorrar(B(1, "U1", 5, BonoEstados.Borrador), ab, ""));
            Assert.IsNotNull(NegBonoReglas.PuedeBorrar(B(1, "U1", 5, BonoEstados.Borrador), ab, null));
            // Un bono directo de Talento Humano (sin jefe) no lo borra nadie desde Proponer.
            var directo = B(1, "U1", 5, BonoEstados.Rechazado);
            directo.CodJefe = "";
            Assert.IsNotNull(NegBonoReglas.PuedeBorrar(directo, ab, ""));
        }

        [TestMethod]
        public void PuedeBorrar_EnviadoOAprobado_No()
        {
            var ab = Per(BonoPeriodoEstados.Abierto);
            Assert.IsNotNull(NegBonoReglas.PuedeBorrar(B(1, "U1", 5, BonoEstados.Enviado), ab, "J"));
            Assert.IsNotNull(NegBonoReglas.PuedeBorrar(B(1, "U1", 5, BonoEstados.Aprobado), ab, "J"));
        }

        [TestMethod]
        public void PuedeBorrar_SinMesAbiertoOOtroPeriodo_No()
        {
            Assert.AreEqual("No hay un mes de bonos abierto.", NegBonoReglas.PuedeBorrar(B(1, "U1", 5, BonoEstados.Borrador), null, "J"));
            Assert.AreEqual("No hay un mes de bonos abierto.", NegBonoReglas.PuedeBorrar(B(1, "U1", 5, BonoEstados.Borrador), Per(BonoPeriodoEstados.Cerrado), "J"));
            Assert.IsNotNull(NegBonoReglas.PuedeBorrar(B(1, "U1", 5, BonoEstados.Borrador), Per(BonoPeriodoEstados.Abierto, 6), "J"));
            var pendiente = B(1, "U1", 5, BonoEstados.Borrador);
            pendiente.IdPeriodo = null;
            Assert.IsNull(NegBonoReglas.PuedeBorrar(pendiente, Per(BonoPeriodoEstados.Abierto), "J"));
        }

        [TestMethod]
        public void PuedeRevisar_MesCerrado_No()
        {
            foreach (string estado in new[] { BonoPeriodoEstados.Cerrado, BonoPeriodoEstados.Historico })
                foreach (string accion in new[] { "Aprobar", "Ajustar", "Rechazar" })
                    Assert.IsNotNull(NegBonoReglas.PuedeRevisar(B(1, "U1", 5, BonoEstados.Enviado), Per(estado), accion));
            Assert.IsNotNull(NegBonoReglas.PuedeRevisar(B(1, "U1", 5, BonoEstados.Enviado), null, "Aprobar"));
        }

        [TestMethod]
        public void PuedeRevisar_AccionDesconocida_No()
        {
            Assert.IsNotNull(NegBonoReglas.PuedeRevisar(B(1, "U1", 5, BonoEstados.Enviado), Per(BonoPeriodoEstados.Abierto), "Borrar"));
        }

        [TestMethod]
        public void EstadoAlEditar_RechazadoVuelveABorrador() => Assert.AreEqual(BonoEstados.Borrador, NegBonoReglas.EstadoAlEditar(BonoEstados.Rechazado));
        [TestMethod]
        public void EstadoAlEditar_OtrosSeQuedan()
        {
            Assert.AreEqual(BonoEstados.Borrador, NegBonoReglas.EstadoAlEditar(BonoEstados.Borrador));
            Assert.AreEqual(BonoEstados.Aprobado, NegBonoReglas.EstadoAlEditar(BonoEstados.Aprobado));
        }

        // ---------- Cierre ----------
        static EntBonoTipo TipoConConceptos(int id, string nombre, params string[] empresasConConcepto)
        {
            var t = T(id, false, null, nombre);
            foreach (string e in empresasConConcepto) t.Conceptos[e] = "1M00";
            return t;
        }

        [TestMethod]
        public void Cierre_SinConceptoDeUnAprobado_NoPuedeCerrar()
        {
            var tipos = new[] { TipoConConceptos(7, "Standby GPF", "DOS") };
            var a = B(1, "U1", 7, BonoEstados.Aprobado); a.Empresa = "Agility";
            var c = NegBonoReglas.Cierre(new[] { a }, tipos);
            CollectionAssert.AreEqual(new[] { "Standby GPF (AGILITY)" }, c.SinConcepto);
            Assert.IsFalse(c.PuedeCerrar);
        }

        [TestMethod]
        public void Cierre_SinConceptoSoloEnUnBorrador_PuedeCerrar()
        {
            var tipos = new[] { TipoConConceptos(7, "Standby GPF", "DOS") };
            var b = B(1, "U1", 7, BonoEstados.Borrador); b.Empresa = "AGILITY";
            var ok = B(2, "U2", 7, BonoEstados.Aprobado); ok.Empresa = "DOS";
            var c = NegBonoReglas.Cierre(new[] { b, ok }, tipos);
            Assert.AreEqual(0, c.SinConcepto.Count);
            Assert.IsTrue(c.PuedeCerrar);
        }

        [TestMethod]
        public void Cierre_SinConcepto_NoSeRepitePorTipoYEmpresa()
        {
            var tipos = new[] { TipoConConceptos(7, "Standby GPF") };
            var a1 = B(1, "U1", 7, BonoEstados.Aprobado);
            var a2 = B(2, "U2", 7, BonoEstados.Aprobado);
            var c = NegBonoReglas.Cierre(new[] { a1, a2 }, tipos);
            CollectionAssert.AreEqual(new[] { "Standby GPF (DOS)" }, c.SinConcepto);
        }

        [TestMethod]
        public void Cierre_QuedanFueraBorradorEnviadoRechazado_TotalSoloAprobados()
        {
            var tipos = new[] { TipoConConceptos(7, "Standby GPF", "DOS") };
            var bor = B(1, "U1", 7, BonoEstados.Borrador); bor.Valor = 10m;
            var env = B(2, "U2", 7, BonoEstados.Enviado); env.Valor = 20m;
            var rec = B(3, "U3", 7, BonoEstados.Rechazado); rec.Valor = 30m;
            var ap1 = B(4, "U4", 7, BonoEstados.Aprobado); ap1.Valor = 40.25m;
            var ap2 = B(5, "U5", 7, BonoEstados.Aprobado); ap2.Valor = 50m;
            var c = NegBonoReglas.Cierre(new[] { bor, env, rec, ap1, ap2 }, tipos);
            CollectionAssert.AreEquivalent(new[] { 1, 2, 3 }, c.QuedanFuera.Select(x => x.IdBono).ToArray());
            Assert.AreEqual(90.25m, c.Total);
            Assert.IsTrue(c.PuedeCerrar);
        }

        // ---------- Totales ----------
        [TestMethod]
        public void Total_Abierto_SumaSoloAprobados()
        {
            var p = Per(BonoPeriodoEstados.Abierto);
            var a = B(1, "U1", 7, BonoEstados.Aprobado); a.Valor = 100m;
            var e = B(2, "U2", 7, BonoEstados.Enviado); e.Valor = 999m;
            Assert.AreEqual(100m, NegBonoReglas.Total(p, new[] { a, e }));
        }

        [TestMethod]
        public void Total_Historico_UsaTotalHistorico()
        {
            var p = Per(BonoPeriodoEstados.Historico);
            p.TotalHistorico = 3342.5m;
            var a = B(1, "U1", 7, BonoEstados.Aprobado); a.Valor = 100m;
            Assert.AreEqual(3342.5m, NegBonoReglas.Total(p, new[] { a }));
            p.TotalHistorico = null;
            Assert.AreEqual(0m, NegBonoReglas.Total(p, new EntBono[0]));
        }

        [TestMethod] public void Variacion_AnteriorCero_Null() => Assert.IsNull(NegBonoReglas.Variacion(100m, 0m));
        [TestMethod] public void Variacion_AnteriorNull_Null() => Assert.IsNull(NegBonoReglas.Variacion(100m, null));
        [TestMethod] public void Variacion_Septiembre() => Assert.AreEqual(-147.5m / 3342.5m, NegBonoReglas.Variacion(3195m, 3342.5m));
    }
}
