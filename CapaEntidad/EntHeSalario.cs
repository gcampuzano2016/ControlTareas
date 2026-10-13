using System;

namespace CapaEntidad
{
    /// <summary>
    /// Una fila del historial de sueldos. En el Excel esto era una columna
    /// llamada con el nombre de un mes; aqui cada monto tiene su fecha de
    /// vigencia y el calculo toma el vigente al corte del periodo.
    /// </summary>
    public class EntHeSalario
    {
        public decimal Monto { get; set; }
        public DateTime FechaVigenciaDesde { get; set; }

        /// <summary>
        /// "Rol" o "Ajuste". DECIDE, no es informativo: cuando dos filas
        /// comparten la misma FechaVigenciaDesde, NegHorasExtras.SalarioVigente
        /// desempata por esta columna y gana "Ajuste" sobre "Rol" (documento
        /// funcional 2.2).
        ///
        /// El DAO de la fase 2 tiene OBLIGACION de mapear esta columna en el
        /// SELECT. La propiedad se inicializa en "": una columna sin mapear
        /// es indistinguible de un valor vacio, el desempate deja de
        /// reconocer el ajuste y vuelve a decidir por el orden de la lista
        /// -que es justo lo que costo tres rondas quitar. El resultado no es
        /// un error visible: cinco personas cobrarian sobre el sueldo
        /// anterior a su ajuste, en silencio.
        /// </summary>
        public string Origen { get; set; } = "";
    }
}
