using CapaEntidad;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CapaPruebas
{
    /// <summary>
    /// La restriccion de la vista de jefatura, hecha prueba.
    ///
    /// La matriz de permisos del diseno le niega a la jefatura la cedula, la
    /// fecha de nacimiento, la edad, el domicilio, el correo y el telefono
    /// personales, el estado civil, las cargas familiares y los documentos de
    /// respaldo. El procedimiento almacenado no los devuelve, pero eso solo se
    /// puede comprobar contra una base de datos. Lo que si se puede comprobar
    /// aqui, sin base y en cada compilacion, es que la clase que viaja al
    /// navegador NO TIENE DONDE PONERLOS.
    ///
    /// Si alguien le agrega manana una propiedad Cedula a EntPerfilEquipoCabecera
    /// -por simetria con EntPerfilCabecera, que es lo que va a parecer razonable-,
    /// esta prueba se pone roja antes de que ese campo llegue a la pantalla de
    /// nadie. Es el unico guardian de esta decision que sobrevive a que todos
    /// olvidemos por que se tomo.
    ///
    /// El recorrido es EN PROFUNDIDAD: por cada propiedad cuyo tipo sea una
    /// clase del espacio de nombres CapaEntidad, o una List&lt;T&gt; de una de
    /// ellas, tambien se entra en ese tipo. EntPerfilEquipo trae listas de
    /// EntPerfilEmergencia, EntPerfilEstudio, EntPerfilCertificacion y
    /// EntPerfilExperiencia, y un EntPerfilFoto; sin este recorrido, un campo
    /// prohibido colgado de cualquiera de esos tipos anidados pasaria sin que
    /// esta prueba lo viera.
    /// </summary>
    [TestClass]
    public class EntPerfilEquipoTests
    {
        /// <summary>
        /// Nombres de propiedad prohibidos, en minusculas. Se compara por nombre
        /// y no por tipo a proposito: el riesgo es que alguien reproduzca el
        /// campo, se llame como se llame su tipo.
        /// </summary>
        private static readonly string[] Prohibidos =
        {
            "cedula", "fechanactexto", "fechanacimiento", "edad",
            "direccion", "correopersonal", "telefonopersonal", "estadocivil",
            "cargasfamiliares", "documentos"
        };

        private static void NoDebeTenerCamposProhibidos(System.Type tipo)
        {
            NoDebeTenerCamposProhibidos(tipo, new HashSet<System.Type>());
        }

        /// <summary>
        /// Recorre <paramref name="tipo"/> y, en profundidad, cualquier clase de
        /// CapaEntidad que cuelgue de sus propiedades -directamente o dentro de
        /// una List&lt;T&gt;-. <paramref name="visitados"/> corta los ciclos: dos
        /// tipos de CapaEntidad que se referencien entre si no deben hacer que
        /// esto recurra sin fin.
        /// </summary>
        private static void NoDebeTenerCamposProhibidos(System.Type tipo, HashSet<System.Type> visitados)
        {
            if (!visitados.Add(tipo))
                return;

            foreach (PropertyInfo p in tipo.GetProperties())
            {
                string nombre = p.Name.ToLowerInvariant();

                Assert.IsFalse(Prohibidos.Contains(nombre),
                    "La clase " + tipo.Name + " tiene la propiedad '" + p.Name +
                    "', que la matriz de permisos le niega a la jefatura. " +
                    "Si de verdad hace falta, hay que cambiar el diseno primero, " +
                    "no la clase.");

                System.Type tipoAnidado = TipoDeCapaEntidadAnidado(p.PropertyType);
                if (tipoAnidado != null)
                    NoDebeTenerCamposProhibidos(tipoAnidado, visitados);
            }
        }

        /// <summary>
        /// Si <paramref name="tipoPropiedad"/> es una clase de CapaEntidad, o una
        /// List&lt;T&gt; de una, devuelve ese tipo T (o el mismo tipo). Si no,
        /// devuelve null.
        /// </summary>
        private static System.Type TipoDeCapaEntidadAnidado(System.Type tipoPropiedad)
        {
            System.Type candidato = tipoPropiedad;

            if (candidato.IsGenericType && candidato.GetGenericTypeDefinition() == typeof(List<>))
                candidato = candidato.GetGenericArguments()[0];

            if (candidato.Namespace == "CapaEntidad" && candidato.IsClass)
                return candidato;

            return null;
        }

        [TestMethod]
        public void EntPerfilEquipoCabecera_NoTieneCamposQueLaJefaturaNoPuedeVer()
        {
            NoDebeTenerCamposProhibidos(typeof(EntPerfilEquipoCabecera));
        }

        [TestMethod]
        public void EntPerfilEquipo_NoTieneCamposQueLaJefaturaNoPuedeVer()
        {
            NoDebeTenerCamposProhibidos(typeof(EntPerfilEquipo));
        }

        [TestMethod]
        public void EntPerfilEquipoItem_NoTieneCamposQueLaJefaturaNoPuedeVer()
        {
            NoDebeTenerCamposProhibidos(typeof(EntPerfilEquipoItem));
        }

        /// <summary>
        /// La cabecera de la jefatura tiene que ser estrictamente mas pobre que
        /// la del dueno del perfil. Si algun dia tuvieran las mismas propiedades,
        /// es que alguien reuso la clase equivocada.
        /// </summary>
        [TestMethod]
        public void LaCabeceraDeJefaturaTieneMenosCamposQueLaDelDueno()
        {
            int jefatura = typeof(EntPerfilEquipoCabecera).GetProperties().Length;
            int dueno = typeof(EntPerfilCabecera).GetProperties().Length;

            Assert.IsTrue(jefatura < dueno,
                "EntPerfilEquipoCabecera tiene " + jefatura + " propiedades y " +
                "EntPerfilCabecera tiene " + dueno + ". La de jefatura debe ser " +
                "estrictamente mas pobre.");
        }

        /// <summary>
        /// Las listas se inicializan en el constructor: si llegaran nulas, el
        /// serializador las manda como null y el JavaScript revienta al recorrer
        /// -y solo para quien todavia no registro nada, que el primer dia es
        /// todo el mundo-.
        /// </summary>
        [TestMethod]
        public void EntPerfilEquipo_NaceConSusListasYSuCabecera()
        {
            EntPerfilEquipo equipo = new EntPerfilEquipo();

            Assert.IsNotNull(equipo.Cabecera);
            Assert.IsNotNull(equipo.Emergencia);
            Assert.IsNotNull(equipo.Estudios);
            Assert.IsNotNull(equipo.Certificaciones);
            Assert.IsNotNull(equipo.Experiencia);
            Assert.IsNotNull(equipo.Foto);
            Assert.IsFalse(equipo.PerfilEncontrado);
        }
    }
}
