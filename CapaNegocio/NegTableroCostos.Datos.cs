using CapaDato;
using CapaEntidad;
using System;

namespace CapaNegocio
{
    /// <summary>Unico archivo de NegTableroCostos que toca la base.</summary>
    public static partial class NegTableroCostos
    {
        /// <summary>Largo de TC_Auditoria.Filtros.</summary>
        public const int LargoFiltrosAuditoria = 1000;

        public static EntTcClasificado Cargar(DateTime desde, DateTime hasta)
        {
            return Clasificar(DaoTableroCostos.Cargar(desde, hasta));
        }

        /// <summary>
        /// Registra la descarga ANTES de armar el archivo. Si no puede, lanza una
        /// excepcion y el handler no entrega nada: el Excel lleva valores
        /// calculados con salarios.
        /// </summary>
        public static void AuditarDescarga(string codUsuario, string filtros)
        {
            if (string.IsNullOrWhiteSpace(codUsuario))
            {
                throw new InvalidOperationException("No se puede registrar una descarga sin usuario.");
            }
            string texto = filtros ?? "";
            if (texto.Length > LargoFiltrosAuditoria)
            {
                texto = texto.Substring(0, LargoFiltrosAuditoria);
            }
            int id = DaoTableroCostos.Auditar(codUsuario.Trim(), "Descargar", texto);
            if (id <= 0)
            {
                throw new InvalidOperationException("La base no confirmó el registro de la descarga.");
            }
        }
    }
}
