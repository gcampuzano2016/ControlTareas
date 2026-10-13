using System;
using System.Data;
using System.Data.SqlClient;

namespace CapaDato
{
    /// <summary>
    /// Una conexion abierta a ReporTarea, con o sin transaccion, que comparten
    /// varias llamadas de DaoCurva.
    ///
    /// Existe porque en este modulo la transaccion la decide CapaNegocio:
    /// completar una evaluacion guarda respuestas, compromisos, seguimientos,
    /// la decision, el estado del proceso y genera la etapa siguiente, y la
    /// regla que decide si se genera la etapa siguiente (seccion 7.2) vive en
    /// C#, con pruebas. Cada procedimiento hace una sola cosa y ninguno hace
    /// COMMIT; el COMMIT lo hace quien abrio la unidad.
    ///
    /// Si se descarta sin Confirmar(), la transaccion se revierte.
    /// </summary>
    public sealed class DaoCurvaUnidad : IDisposable
    {
        internal SqlConnection Conexion { get; private set; }
        internal SqlTransaction Transaccion { get; private set; }

        private DaoCurvaUnidad() { }

        public static DaoCurvaUnidad Abrir(bool conTransaccion)
        {
            var u = new DaoCurvaUnidad();
            u.Conexion = new DaoReporTareaAranda().conectar();
            u.Conexion.Open();
            if (conTransaccion) u.Transaccion = u.Conexion.BeginTransaction(IsolationLevel.ReadCommitted);
            return u;
        }

        public void Confirmar()
        {
            if (Transaccion != null)
            {
                Transaccion.Commit();
                Transaccion = null;
            }
        }

        internal SqlCommand Procedimiento(string nombre)
        {
            var cmd = new SqlCommand(nombre, Conexion, Transaccion);
            cmd.CommandType = CommandType.StoredProcedure;
            return cmd;
        }

        public void Dispose()
        {
            if (Transaccion != null)
            {
                try { Transaccion.Rollback(); } catch (InvalidOperationException) { /* ya terminada */ }
                Transaccion = null;
            }
            if (Conexion != null)
            {
                Conexion.Dispose();
                Conexion = null;
            }
        }
    }
}
