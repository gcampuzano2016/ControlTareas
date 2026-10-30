using CapaEntidad;
using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace CapaPruebas
{
    [TestClass]
    public class NegVacCompraTests
    {
        static readonly DateTime Emitido = new DateTime(2026, 10, 30, 10, 15, 0);

        static EntVacAjustes Ajustes(string estadoCompra = "APROBADO")
        {
            var a = new EntVacAjustes
            {
                Ficha = new EntVacFicha
                {
                    Colaborador = new EntVacColaborador
                    {
                        IdColaborador = 7, Nombre = "PEREZ ANA", Cedula = "1700000000", Empresa = "DOS", Area = "GTH", Cargo = "ANALISTA",
                        FechaIngreso = new DateTime(2020, 1, 15), Estado = "ACTIVO"
                    },
                    Saldo = new EntVacSaldo { FechaCalculo = new DateTime(2026, 10, 30), Saldo = 9.5m },
                    Movimientos = new List<EntVacMovimiento>
                    {
                        new EntVacMovimiento { IdMovimiento = 40, Tipo = "VACACIONES", Desde = new DateTime(2026, 2, 1), Dias = 5m },
                        new EntVacMovimiento
                        {
                            IdMovimiento = 41, Tipo = NegVacAjustes.TipoCompra, Estado = estadoCompra, Desde = new DateTime(2026, 8, 31),
                            Dias = 10m, DiasDescuenta = 10m, Situacion = "DESCUENTA", Observacion = "Acta <firmada> del 31/08"
                        }
                    }
                }
            };
            a.Bitacora.Add(new EntVacBitacora { Tipo = "COMPRA", Accion = "REGISTRA", IdMovimiento = 41, Usuario = "4089", Fecha = new DateTime(2026, 8, 31, 9, 0, 0) });
            a.Documentos[41] = new EntVacCompraDocumento { IdMovimiento = 41, NombreArchivo = "acta.pdf" };
            return a;
        }

        static EntVacSueldos Sueldos(decimal sueldo)
        {
            var s = new EntVacSueldos { DiasMes = 30m };
            s.HorasExtras[7] = new List<EntHeSalario> { new EntHeSalario { Monto = sueldo, FechaVigenciaDesde = new DateTime(2025, 1, 1) } };
            return s;
        }

        [TestMethod]
        public void Armar_ValorEsDiasPorSueldoSobre30()
        {
            EntVacCompraAviso a = NegVacCompra.Armar(Ajustes(), 41, Sueldos(1200m), Emitido, "4089");
            Assert.IsNotNull(a);
            Assert.AreEqual(1200m, a.Sueldo);
            Assert.AreEqual(400m, a.Valor);            // 10 x 1200 / 30
            Assert.AreEqual("acta.pdf", a.Documento.NombreArchivo);
            Assert.AreEqual("4089", a.Registro.Usuario);
        }

        [TestMethod]
        public void Armar_SinSueldo_NoInventaValor()
        {
            EntVacCompraAviso a = NegVacCompra.Armar(Ajustes(), 41, new EntVacSueldos(), Emitido, "4089");
            Assert.IsNull(a.Sueldo);
            Assert.IsNull(a.Valor);
            StringAssert.Contains(NegVacCompra.Html(a, "", ""), "N&#243;mina debe calcular el valor");
        }

        [TestMethod]
        public void Armar_SoloCompras()
        {
            Assert.IsNull(NegVacCompra.Armar(Ajustes(), 40, Sueldos(1200m), Emitido, "4089"), "una vacacion no es una compra");
            Assert.IsNull(NegVacCompra.Armar(Ajustes(), 99, Sueldos(1200m), Emitido, "4089"));
        }

        [TestMethod]
        public void PuedeEmitir_UnaAnuladaNoSeAvisa()
        {
            Assert.AreEqual("", NegVacCompra.PuedeEmitir(NegVacCompra.Armar(Ajustes(), 41, Sueldos(1200m), Emitido, "4089")));
            Assert.AreNotEqual("", NegVacCompra.PuedeEmitir(NegVacCompra.Armar(Ajustes("DADA BAJA"), 41, Sueldos(1200m), Emitido, "4089")));
            Assert.AreNotEqual("", NegVacCompra.PuedeEmitir(null));
        }

        [TestMethod]
        public void Html_LlevaLosDatosYEscapaElMotivo()
        {
            string h = NegVacCompra.Html(NegVacCompra.Armar(Ajustes(), 41, Sueldos(1200m), Emitido, "4089"), "", "");
            StringAssert.Contains(h, "PEREZ ANA");
            StringAssert.Contains(h, "$400,00");
            StringAssert.Contains(h, "$1.200,00");
            StringAssert.Contains(h, "19,50 d&#237;as");             // saldo sin la compra: 9,5 + 10
            StringAssert.Contains(h, "Registro N.º 000041");
            StringAssert.Contains(h, "Acta &lt;firmada&gt;");
            Assert.IsFalse(h.Contains("<firmada>"));
        }
    }
}
