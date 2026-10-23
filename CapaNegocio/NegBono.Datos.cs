using CapaDato;
using CapaEntidad;
using System;
using System.Collections.Generic;

namespace CapaNegocio
{
    /// <summary>
    /// Unico archivo de bonos que toca la base. Sin logica: cada metodo pasa a
    /// DaoBono. Son privados para que nadie fuera de NegBono escriba sin pasar
    /// por las reglas. Todo lanza si la base falla: quien llama decide que responder.
    /// </summary>
    public static partial class NegBono
    {
        private static List<EntBonoPersona> LeerPersonasActivas()
        {
            return DaoBono.PersonasActivas();
        }

        private static List<EntBonoTipo> LeerTipos()
        {
            return DaoBono.Tipos();
        }

        private static int GuardarTipoEnBase(int idTipo, string nombre, string familia, decimal? valorSugerido, bool porSemana,
            bool activo, string conceptoDos, string conceptoAgility, string usuario)
        {
            return DaoBono.GuardarTipo(idTipo, nombre, familia, valorSugerido, porSemana, activo, conceptoDos, conceptoAgility, usuario);
        }

        private static List<EntBonoPeriodo> LeerPeriodos()
        {
            return DaoBono.Periodos();
        }

        private static int AbrirPeriodoEnBase(string usuario)
        {
            return DaoBono.AbrirPeriodo(usuario);
        }

        private static int CerrarPeriodoEnBase(int idPeriodo, string usuario)
        {
            return DaoBono.CerrarPeriodo(idPeriodo, usuario);
        }

        private static List<EntBono> LeerDetalles(int? idPeriodo)
        {
            return DaoBono.Detalles(idPeriodo);
        }

        private static List<EntBono> LeerDePersona(string codUsuario)
        {
            return DaoBono.DePersona(codUsuario);
        }

        private static int GuardarEnBase(EntBono b, string estadoEsperado, string estadoNuevo, string usuario)
        {
            return DaoBono.Guardar(b, estadoEsperado, estadoNuevo, usuario);
        }

        private static int CambiarEstadoEnBase(int idBono, string estadoEsperado, string estadoNuevo, decimal? valor, string motivo,
            string snapshotEmpresa, string snapshotNombre, string snapshotIdentificacion, string usuario)
        {
            return DaoBono.CambiarEstado(idBono, estadoEsperado, estadoNuevo, valor, motivo,
                snapshotEmpresa, snapshotNombre, snapshotIdentificacion, usuario);
        }

        private static int BorrarEnBase(int idBono, string estadoEsperado, string usuario)
        {
            return DaoBono.Borrar(idBono, estadoEsperado, usuario);
        }

        private static void BitacoraEnBase(int? idBono, int? idPeriodo, string accion, string valorAnterior, string valorNuevo,
            string detalle, string usuario)
        {
            DaoBono.Bitacora(idBono, idPeriodo, accion, valorAnterior, valorNuevo, detalle, usuario);
        }
    }
}
