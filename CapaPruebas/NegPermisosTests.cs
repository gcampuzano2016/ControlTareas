using CapaEntidad;
using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CapaPruebas
{
    [TestClass]
    public class NegPermisosTests
    {
        private static EntPermisos Con(string[] pantallas, params string[] accionesYPantalla)
        {
            EntPermisos p = new EntPermisos();
            foreach (string h in pantallas) { p.Pantallas.Add(h); }
            for (int i = 0; i + 1 < accionesYPantalla.Length; i += 2)
            {
                p.Acciones[accionesYPantalla[i]] = accionesYPantalla[i + 1];
            }
            return p;
        }

        [TestMethod]
        public void PuedeEntrar_SuperAdmin_NoNecesitaPermisos()
        {
            Assert.IsTrue(NegPermisos.PuedeEntrar(18, null, "TableroCostos.aspx"));
        }

        [TestMethod]
        public void PuedeEntrar_PantallaEnSusPermisos_Entra()
        {
            Assert.IsTrue(NegPermisos.PuedeEntrar(2, Con(new[] { "HorasExtras.aspx" }), "HorasExtras.aspx"));
        }

        [TestMethod]
        public void PuedeEntrar_PantallaQueNoTiene_NoEntra()
        {
            Assert.IsFalse(NegPermisos.PuedeEntrar(2, Con(new[] { "HorasExtras.aspx" }), "TableroCostos.aspx"));
        }

        [TestMethod]
        public void PuedeEntrar_MayusculasYEspacios_NoImportan()
        {
            Assert.IsTrue(NegPermisos.PuedeEntrar(2, Con(new[] { "HorasExtras.aspx" }), "  horasextras.ASPX "));
        }

        [TestMethod]
        public void PuedeEntrar_SinPermisosCargados_NoEntra()
        {
            Assert.IsFalse(NegPermisos.PuedeEntrar(2, null, "HorasExtras.aspx"));
            Assert.IsFalse(NegPermisos.PuedeEntrar(null, null, "HorasExtras.aspx"));
        }

        [TestMethod]
        public void PuedeEntrar_HrefVacio_NoEntra()
        {
            Assert.IsFalse(NegPermisos.PuedeEntrar(2, Con(new[] { "HorasExtras.aspx" }), ""));
            Assert.IsFalse(NegPermisos.PuedeEntrar(2, Con(new[] { "HorasExtras.aspx" }), null));
        }

        [TestMethod]
        public void PuedeHacer_SuperAdmin_NoNecesitaPermisos()
        {
            Assert.IsTrue(NegPermisos.PuedeHacer(18, null, "HorasExtras.Reabrir"));
        }

        [TestMethod]
        public void PuedeHacer_AccionConSuPantalla_Puede()
        {
            EntPermisos p = Con(new[] { "HorasExtras.aspx" }, "HorasExtras.EditarMaestro", "HorasExtras.aspx");
            Assert.IsTrue(NegPermisos.PuedeHacer(14, p, "HorasExtras.EditarMaestro"));
        }

        [TestMethod]
        public void PuedeHacer_AccionSinSuPantalla_SeNiega()
        {
            EntPermisos p = Con(new string[0], "HorasExtras.EditarMaestro", "HorasExtras.aspx");
            Assert.IsFalse(NegPermisos.PuedeHacer(14, p, "HorasExtras.EditarMaestro"));
        }

        [TestMethod]
        public void PuedeHacer_AccionQueNoTiene_SeNiega()
        {
            EntPermisos p = Con(new[] { "HorasExtras.aspx" }, "HorasExtras.EditarMaestro", "HorasExtras.aspx");
            Assert.IsFalse(NegPermisos.PuedeHacer(14, p, "HorasExtras.Reabrir"));
        }

        [TestMethod]
        public void PuedeHacer_ClaveDesconocidaOVacia_SeNiega()
        {
            EntPermisos p = Con(new[] { "HorasExtras.aspx" }, "HorasExtras.EditarMaestro", "HorasExtras.aspx");
            Assert.IsFalse(NegPermisos.PuedeHacer(14, p, "No.Existe"));
            Assert.IsFalse(NegPermisos.PuedeHacer(14, p, ""));
            Assert.IsFalse(NegPermisos.PuedeHacer(14, p, null));
            Assert.IsFalse(NegPermisos.PuedeHacer(14, null, "HorasExtras.EditarMaestro"));
        }

        [TestMethod]
        public void DebeRecargar_SinCache_Recarga()
        {
            Assert.IsTrue(NegPermisos.DebeRecargar(null, 2, "U1", DateTime.Now));
        }

        [TestMethod]
        public void DebeRecargar_MismaIdentidadDentroDeLaVigencia_NoRecarga()
        {
            DateTime t = new DateTime(2026, 10, 14, 10, 0, 0);
            EntPermisosCache c = new EntPermisosCache { IdPerfil = 2, CodUsuario = "U1", Cargado = t, Permisos = new EntPermisos() };
            Assert.IsFalse(NegPermisos.DebeRecargar(c, 2, "U1", t.AddSeconds(119)));
        }

        [TestMethod]
        public void DebeRecargar_PasaronDosMinutos_Recarga()
        {
            DateTime t = new DateTime(2026, 10, 14, 10, 0, 0);
            EntPermisosCache c = new EntPermisosCache { IdPerfil = 2, CodUsuario = "U1", Cargado = t, Permisos = new EntPermisos() };
            Assert.IsTrue(NegPermisos.DebeRecargar(c, 2, "U1", t.AddMinutes(2)));
        }

        [TestMethod]
        public void DebeRecargar_CambiaElPerfil_Recarga()
        {
            DateTime t = new DateTime(2026, 10, 14, 10, 0, 0);
            EntPermisosCache c = new EntPermisosCache { IdPerfil = 2, CodUsuario = "U1", Cargado = t, Permisos = new EntPermisos() };
            Assert.IsTrue(NegPermisos.DebeRecargar(c, 14, "U1", t.AddSeconds(5)));
        }

        [TestMethod]
        public void DebeRecargar_CambiaElUsuario_Recarga()
        {
            DateTime t = new DateTime(2026, 10, 14, 10, 0, 0);
            EntPermisosCache c = new EntPermisosCache { IdPerfil = 2, CodUsuario = "U1", Cargado = t, Permisos = new EntPermisos() };
            Assert.IsTrue(NegPermisos.DebeRecargar(c, 2, "U2", t.AddSeconds(5)));
        }

        [TestMethod]
        public void DebeRecargar_RelojHaciaAtras_Recarga()
        {
            DateTime t = new DateTime(2026, 10, 14, 10, 0, 0);
            EntPermisosCache c = new EntPermisosCache { IdPerfil = 2, CodUsuario = "U1", Cargado = t, Permisos = new EntPermisos() };
            Assert.IsTrue(NegPermisos.DebeRecargar(c, 2, "U1", t.AddMinutes(-1)));
        }

        [TestMethod]
        public void DebeRecargar_CacheSinPermisos_Recarga()
        {
            DateTime t = new DateTime(2026, 10, 14, 10, 0, 0);
            EntPermisosCache c = new EntPermisosCache { IdPerfil = 2, CodUsuario = "U1", Cargado = t, Permisos = null };
            Assert.IsTrue(NegPermisos.DebeRecargar(c, 2, "U1", t.AddSeconds(5)));
        }
    }
}
