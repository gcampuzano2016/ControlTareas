using CapaEntidad;
using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CapaPruebas
{
    [TestClass]
    public class NegAgendaProcesoTests
    {
        private static EntAgendaPersona P(string cod, int area, string jefe = "") =>
            new EntAgendaPersona { CodUsuario = cod, IdArea = area, CodJefe = jefe };

        private static EntAgendaProcesoPaso Paso(string cod, string estado, int orden = 1) =>
            new EntAgendaProcesoPaso { Orden = orden, IdArea = 1, CodResponsable = cod, Estado = estado, Titulo = "T", PlazoDias = 3 };

        private static EntAgendaProceso Proc(string inicia, string estado, params EntAgendaProcesoPaso[] pasos) =>
            new EntAgendaProceso { CodIniciadoPor = inicia, Estado = estado, Pasos = pasos.ToList() };

        private static ISet<string> Cad(params string[] c) => new HashSet<string>(c, NegAgendaEquipo.Codigos);

        // ---- PuedeVer
        [TestMethod] public void PuedeVer_Iniciador_Si() =>
            Assert.IsTrue(NegAgendaProceso.PuedeVer(" ana ", Proc("ANA", "EnCurso", Paso("b", "EnCurso")), Cad(), false));
        [TestMethod] public void PuedeVer_ResponsableDeUnPaso_Si() =>
            Assert.IsTrue(NegAgendaProceso.PuedeVer("c", Proc("a", "EnCurso", Paso("b", "Completo"), Paso("C ", "EnCurso", 2)), Cad(), false));
        [TestMethod] public void PuedeVer_JefeDeUnResponsable_Si() =>
            Assert.IsTrue(NegAgendaProceso.PuedeVer("j", Proc("a", "EnCurso", Paso("b", "EnCurso")), Cad("B"), false));
        [TestMethod] public void PuedeVer_JefeDelIniciador_Si() =>
            Assert.IsTrue(NegAgendaProceso.PuedeVer("j", Proc("a", "EnCurso", Paso("b", "EnCurso")), Cad("a"), false));
        [TestMethod] public void PuedeVer_Ajeno_No() =>
            Assert.IsFalse(NegAgendaProceso.PuedeVer("z", Proc("a", "EnCurso", Paso("b", "EnCurso")), Cad("x"), false));
        [TestMethod] public void PuedeVer_VerTodos_Si() =>
            Assert.IsTrue(NegAgendaProceso.PuedeVer("z", Proc("a", "EnCurso", Paso("b", "EnCurso")), Cad(), true));
        [TestMethod] public void PuedeVer_Nulos_No() =>
            Assert.IsFalse(NegAgendaProceso.PuedeVer("z", null, null, false));

        // ---- PuedeGestionar
        [TestMethod] public void PuedeGestionar_SoloIniciadorOVerTodos()
        {
            EntAgendaProceso p = Proc("a", "EnCurso", Paso("b", "EnCurso"));
            Assert.IsTrue(NegAgendaProceso.PuedeGestionar("A ", p, false));
            Assert.IsTrue(NegAgendaProceso.PuedeGestionar("z", p, true));
            Assert.IsFalse(NegAgendaProceso.PuedeGestionar("b", p, false));
        }
        [TestMethod] public void PuedeGestionar_Terminado_No()
        {
            Assert.IsFalse(NegAgendaProceso.PuedeGestionar("a", Proc("a", "Terminado"), true));
            Assert.IsFalse(NegAgendaProceso.PuedeGestionar("a", Proc("a", "Cancelado"), true));
        }

        // ---- ValidarPlantilla
        private static EntAgendaPlantillaPaso PP(int orden, int area = 1, int plazo = 3, string titulo = "Paso") =>
            new EntAgendaPlantillaPaso { Orden = orden, IdArea = area, PlazoDias = plazo, Titulo = titulo };
        private static EntAgendaPlantilla Pl(params EntAgendaPlantillaPaso[] pasos) =>
            new EntAgendaPlantilla { Nombre = "Contrato", Activa = true, Pasos = pasos.ToList() };
        private static readonly ISet<int> Areas = new HashSet<int> { 1, 2 };

        [TestMethod] public void ValidarPlantilla_Valida_Null() =>
            Assert.IsNull(NegAgendaProceso.ValidarPlantilla(Pl(PP(1), PP(2, 2)), Areas));
        [TestMethod] public void ValidarPlantilla_SinNombre()
        {
            EntAgendaPlantilla t = Pl(PP(1)); t.Nombre = "  ";
            Assert.AreEqual("El nombre de la plantilla es obligatorio (hasta 150 caracteres).", NegAgendaProceso.ValidarPlantilla(t, Areas));
            t.Nombre = new string('x', 151);
            Assert.AreEqual("El nombre de la plantilla es obligatorio (hasta 150 caracteres).", NegAgendaProceso.ValidarPlantilla(t, Areas));
        }
        [TestMethod] public void ValidarPlantilla_SinPasos()
        {
            Assert.AreEqual("La plantilla necesita al menos un paso.", NegAgendaProceso.ValidarPlantilla(Pl(), Areas));
            Assert.AreEqual("La plantilla necesita al menos un paso.", NegAgendaProceso.ValidarPlantilla(null, Areas));
        }
        [TestMethod] public void ValidarPlantilla_OrdenNoConsecutivo()
        {
            Assert.AreEqual("Paso 2: el orden tiene que ser consecutivo desde 1.", NegAgendaProceso.ValidarPlantilla(Pl(PP(1), PP(3)), Areas));
            Assert.AreEqual("Paso 1: el orden tiene que ser consecutivo desde 1.", NegAgendaProceso.ValidarPlantilla(Pl(PP(2)), Areas));
        }
        [TestMethod] public void ValidarPlantilla_AreaInactiva() =>
            Assert.AreEqual("Paso 2: el área no existe o está inactiva.", NegAgendaProceso.ValidarPlantilla(Pl(PP(1), PP(2, 9)), Areas));
        [TestMethod] public void ValidarPlantilla_SinTitulo()
        {
            Assert.AreEqual("Paso 1: el título es obligatorio (hasta 200 caracteres).", NegAgendaProceso.ValidarPlantilla(Pl(PP(1, 1, 3, " ")), Areas));
            Assert.AreEqual("Paso 1: el título es obligatorio (hasta 200 caracteres).", NegAgendaProceso.ValidarPlantilla(Pl(PP(1, 1, 3, new string('x', 201))), Areas));
        }
        [TestMethod] public void ValidarPlantilla_Plazo0y61()
        {
            Assert.AreEqual("Paso 2: el plazo tiene que estar entre 1 y 60 días hábiles.", NegAgendaProceso.ValidarPlantilla(Pl(PP(1), PP(2, 1, 0)), Areas));
            Assert.AreEqual("Paso 1: el plazo tiene que estar entre 1 y 60 días hábiles.", NegAgendaProceso.ValidarPlantilla(Pl(PP(1, 1, 61)), Areas));
            Assert.IsNull(NegAgendaProceso.ValidarPlantilla(Pl(PP(1, 1, 1), PP(2, 1, 60)), Areas));
        }

        // ---- ValidarInicio
        private static EntAgendaPlantilla Plt(bool activa = true)
        {
            EntAgendaPlantilla t = Pl(
                new EntAgendaPlantillaPaso { Orden = 1, IdArea = 1, Area = "Legal", Titulo = "Revisar", PlazoDias = 2 },
                new EntAgendaPlantillaPaso { Orden = 2, IdArea = 2, Area = "Finanzas", Titulo = "Pagar", PlazoDias = 3 });
            t.Activa = activa;
            return t;
        }
        private static EntAgendaProcesoPaso Ip(int orden, string cod, string titulo = "X", int plazo = 2) =>
            new EntAgendaProcesoPaso { Orden = orden, CodResponsable = cod, Titulo = titulo, PlazoDias = plazo };
        private static List<EntAgendaPersona> Act() => new List<EntAgendaPersona> { P("ana", 1), P("beto", 2, "ana") };
        private static List<EntAgendaProcesoPaso> Ok() => new List<EntAgendaProcesoPaso> { Ip(1, "ANA "), Ip(2, "beto") };

        [TestMethod] public void ValidarInicio_Valido_Null() =>
            Assert.IsNull(NegAgendaProceso.ValidarInicio("Contrato X", Plt(), Ok(), Act()));
        [TestMethod] public void ValidarInicio_Nombre()
        {
            const string msg = "El nombre del proceso es obligatorio (hasta 200 caracteres).";
            Assert.AreEqual(msg, NegAgendaProceso.ValidarInicio(" ", Plt(), Ok(), Act()));
            Assert.AreEqual(msg, NegAgendaProceso.ValidarInicio(new string('x', 201), Plt(), Ok(), Act()));
            Assert.IsNull(NegAgendaProceso.ValidarInicio(new string('x', 200), Plt(), Ok(), Act()));
        }
        [TestMethod] public void ValidarInicio_PlantillaInactiva() =>
            Assert.AreEqual("La plantilla no está activa.", NegAgendaProceso.ValidarInicio("N", Plt(false), Ok(), Act()));
        [TestMethod] public void ValidarInicio_PlantillaNula() =>
            Assert.AreEqual("La plantilla no está activa.", NegAgendaProceso.ValidarInicio("N", null, Ok(), Act()));
        [TestMethod] public void ValidarInicio_PasosNoCoinciden() =>
            Assert.AreEqual("Los pasos no coinciden con los de la plantilla.", NegAgendaProceso.ValidarInicio("N", Plt(), new List<EntAgendaProcesoPaso> { Ip(1, "ana") }, Act()));
        [TestMethod] public void ValidarInicio_OrdenDistinto() =>
            Assert.AreEqual("Los pasos no coinciden con los de la plantilla.", NegAgendaProceso.ValidarInicio("N", Plt(),
                new List<EntAgendaProcesoPaso> { Ip(2, "beto"), Ip(1, "ana") }, Act()));
        [TestMethod] public void ValidarInicio_AreaSinPersonas()
        {
            List<EntAgendaPersona> solo1 = new List<EntAgendaPersona> { P("ana", 1) };
            Assert.AreEqual("El área «Finanzas» no tiene personas activas: no se puede iniciar el proceso.",
                NegAgendaProceso.ValidarInicio("N", Plt(), Ok(), solo1));
        }
        [TestMethod] public void ValidarInicio_ResponsableInactivo() =>
            Assert.AreEqual("Paso 2: el responsable no está activo.", NegAgendaProceso.ValidarInicio("N", Plt(),
                new List<EntAgendaProcesoPaso> { Ip(1, "ana"), Ip(2, "fantasma") }, Act()));
        [TestMethod] public void ValidarInicio_SinResponsable() =>
            Assert.AreEqual("Paso 1: el responsable no está activo.", NegAgendaProceso.ValidarInicio("N", Plt(),
                new List<EntAgendaProcesoPaso> { Ip(1, " "), Ip(2, "beto") }, Act()));
        [TestMethod] public void ValidarInicio_ResponsableDeOtraArea() =>
            Assert.AreEqual("Paso 2: el responsable no pertenece al área «Finanzas».", NegAgendaProceso.ValidarInicio("N", Plt(),
                new List<EntAgendaProcesoPaso> { Ip(1, "ana"), Ip(2, "ana") }, Act()));
        [TestMethod] public void ValidarInicio_ResponsableFueraDeLaCadena_Vale()
        {
            // beto no es subordinado de nadie: un proceso cruza areas, solo cuenta el area.
            List<EntAgendaPersona> act = new List<EntAgendaPersona> { P("ana", 1), P("beto", 2, "") };
            Assert.IsNull(NegAgendaProceso.ValidarInicio("N", Plt(), Ok(), act));
        }
        [TestMethod] public void ValidarInicio_Titulo()
        {
            const string msg = "Paso 1: el título es obligatorio (hasta 200 caracteres).";
            Assert.AreEqual(msg, NegAgendaProceso.ValidarInicio("N", Plt(), new List<EntAgendaProcesoPaso> { Ip(1, "ana", " "), Ip(2, "beto") }, Act()));
            Assert.AreEqual(msg, NegAgendaProceso.ValidarInicio("N", Plt(), new List<EntAgendaProcesoPaso> { Ip(1, "ana", new string('x', 201)), Ip(2, "beto") }, Act()));
        }
        [TestMethod] public void ValidarInicio_Plazo0y61()
        {
            const string msg = "Paso 2: el plazo tiene que estar entre 1 y 60 días hábiles.";
            Assert.AreEqual(msg, NegAgendaProceso.ValidarInicio("N", Plt(), new List<EntAgendaProcesoPaso> { Ip(1, "ana"), Ip(2, "beto", "X", 0) }, Act()));
            Assert.AreEqual(msg, NegAgendaProceso.ValidarInicio("N", Plt(), new List<EntAgendaProcesoPaso> { Ip(1, "ana"), Ip(2, "beto", "X", 61) }, Act()));
        }

        // ---- PuedeReasignar
        [TestMethod] public void PuedeReasignar_Valido_Null()
        {
            Assert.IsNull(NegAgendaProceso.PuedeReasignar(Paso("ana", "Pendiente"), "ANA", Act()));
            Assert.IsNull(NegAgendaProceso.PuedeReasignar(Paso("x", "EnCurso"), "ana", Act()));
        }
        [TestMethod] public void PuedeReasignar_Completo_No()
        {
            const string msg = "Solo se puede reasignar un paso pendiente o en curso.";
            Assert.AreEqual(msg, NegAgendaProceso.PuedeReasignar(Paso("x", "Completo"), "ana", Act()));
            Assert.AreEqual(msg, NegAgendaProceso.PuedeReasignar(Paso("x", "Cancelado"), "ana", Act()));
        }
        [TestMethod] public void PuedeReasignar_Inactivo_No() =>
            Assert.AreEqual("La persona elegida no está activa.", NegAgendaProceso.PuedeReasignar(Paso("x", "EnCurso"), "nadie", Act()));
        [TestMethod] public void PuedeReasignar_OtraArea_No() =>
            Assert.AreEqual("La persona elegida no pertenece al área del paso.", NegAgendaProceso.PuedeReasignar(Paso("x", "EnCurso"), "beto", Act()));

        // ---- PuedeCambiarPlazo
        [TestMethod] public void PuedeCambiarPlazo_Pendiente_Valido()
        {
            Assert.IsNull(NegAgendaProceso.PuedeCambiarPlazo(Paso("a", "Pendiente"), 1));
            Assert.IsNull(NegAgendaProceso.PuedeCambiarPlazo(Paso("a", "Pendiente"), 60));
        }
        [TestMethod] public void PuedeCambiarPlazo_EnCurso_No() =>
            Assert.AreEqual("Solo se puede cambiar el plazo de un paso pendiente.", NegAgendaProceso.PuedeCambiarPlazo(Paso("a", "EnCurso"), 5));
        [TestMethod] public void PuedeCambiarPlazo_Fuera()
        {
            const string msg = "El plazo tiene que estar entre 1 y 60 días hábiles.";
            Assert.AreEqual(msg, NegAgendaProceso.PuedeCambiarPlazo(Paso("a", "Pendiente"), 0));
            Assert.AreEqual(msg, NegAgendaProceso.PuedeCambiarPlazo(Paso("a", "Pendiente"), 61));
        }

        // ---- PuedeCambiarFecha (2026-10-23 es viernes)
        private static readonly DateTime Hoy = new DateTime(2026, 10, 23);
        [TestMethod] public void PuedeCambiarFecha_Valida_Null()
        {
            Assert.IsNull(NegAgendaProceso.PuedeCambiarFecha(Paso("a", "EnCurso"), new DateTime(2026, 10, 26), Hoy));
            Assert.IsNull(NegAgendaProceso.PuedeCambiarFecha(Paso("a", "EnCurso"), Hoy, Hoy.AddHours(15)));
        }
        [TestMethod] public void PuedeCambiarFecha_Sabado_No()
        {
            const string msg = "La fecha de entrega no puede ser sábado ni domingo.";
            Assert.AreEqual(msg, NegAgendaProceso.PuedeCambiarFecha(Paso("a", "EnCurso"), new DateTime(2026, 10, 24), Hoy));
            Assert.AreEqual(msg, NegAgendaProceso.PuedeCambiarFecha(Paso("a", "EnCurso"), new DateTime(2026, 10, 25), Hoy));
        }
        [TestMethod] public void PuedeCambiarFecha_Pendiente_No() =>
            Assert.AreEqual("Solo se puede cambiar la fecha de un paso en curso.", NegAgendaProceso.PuedeCambiarFecha(Paso("a", "Pendiente"), new DateTime(2026, 10, 26), Hoy));
        [TestMethod] public void PuedeCambiarFecha_Pasada_No() =>
            Assert.AreEqual("La fecha de entrega no puede ser anterior a hoy.", NegAgendaProceso.PuedeCambiarFecha(Paso("a", "EnCurso"), new DateTime(2026, 10, 22), Hoy));

        // ---- PuedeCancelar
        [TestMethod] public void PuedeCancelar_Valido_Null() =>
            Assert.IsNull(NegAgendaProceso.PuedeCancelar(Proc("a", "EnCurso"), "ya no hace falta"));
        [TestMethod] public void PuedeCancelar_SinMotivo()
        {
            const string msg = "El motivo es obligatorio (hasta 400 caracteres).";
            Assert.AreEqual(msg, NegAgendaProceso.PuedeCancelar(Proc("a", "EnCurso"), "  "));
            Assert.AreEqual(msg, NegAgendaProceso.PuedeCancelar(Proc("a", "EnCurso"), null));
            Assert.AreEqual(msg, NegAgendaProceso.PuedeCancelar(Proc("a", "EnCurso"), new string('x', 401)));
            Assert.IsNull(NegAgendaProceso.PuedeCancelar(Proc("a", "EnCurso"), new string('x', 400)));
        }
        [TestMethod] public void PuedeCancelar_NoEnCurso_No() =>
            Assert.AreEqual("Solo se puede cancelar un proceso en curso.", NegAgendaProceso.PuedeCancelar(Proc("a", "Terminado"), "x"));

        // ---- ResponsableInactivo
        [TestMethod] public void ResponsableInactivo_Si()
        {
            Assert.IsTrue(NegAgendaProceso.ResponsableInactivo(Paso("baja", "EnCurso"), Act()));
            Assert.IsTrue(NegAgendaProceso.ResponsableInactivo(Paso("baja", "Pendiente"), Act()));
        }
        [TestMethod] public void ResponsableInactivo_Activo_O_PasoCerrado_No()
        {
            Assert.IsFalse(NegAgendaProceso.ResponsableInactivo(Paso(" ANA", "EnCurso"), Act()));
            Assert.IsFalse(NegAgendaProceso.ResponsableInactivo(Paso("baja", "Completo"), Act()));
            Assert.IsFalse(NegAgendaProceso.ResponsableInactivo(Paso("baja", "Cancelado"), Act()));
        }

        // ---- AvisoAlCompletar / ResumenAvance
        [TestMethod] public void AvisoAlCompletar_UltimoPaso()
        {
            EntAgendaProcesoPaso ultimo = Paso("a", "EnCurso", 2);
            EntAgendaProceso p = Proc("i", "EnCurso", Paso("a", "Completo", 1), ultimo);
            Assert.AreEqual("Al completar este paso se terminará el proceso. No se puede deshacer.", NegAgendaProceso.AvisoAlCompletar(p, ultimo, null));
        }
        [TestMethod] public void AvisoAlCompletar_ConSiguiente()
        {
            EntAgendaProcesoPaso primero = Paso("a", "EnCurso", 1);
            EntAgendaProceso p = Proc("i", "EnCurso", primero, Paso("b", "Pendiente", 2));
            Assert.AreEqual("Al completar este paso se le avisará a Beto para el paso siguiente. No se puede deshacer.", NegAgendaProceso.AvisoAlCompletar(p, primero, "Beto"));
        }
        [TestMethod] public void ResumenAvance_CuentaCompletos()
        {
            EntAgendaProceso p = Proc("i", "EnCurso", Paso("a", "Completo", 1), Paso("b", "EnCurso", 2), Paso("c", "Pendiente", 3));
            Assert.AreEqual("1/3 pasos completos", NegAgendaProceso.ResumenAvance(p));
            Assert.AreEqual("0/0 pasos completos", NegAgendaProceso.ResumenAvance(Proc("i", "EnCurso")));
        }
    }
}
