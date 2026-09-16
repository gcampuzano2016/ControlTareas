namespace CapaEntidad
{
    /// <summary>
    /// Los parametros de calculo vigentes, ya resueltos a numeros.
    ///
    /// Viven en tabla y no en codigo -HE_Parametro- porque cambian sin que
    /// nadie recompile, y estan versionados por fecha de vigencia para que un
    /// periodo cerrado se pueda recalcular con los que regian entonces.
    /// </summary>
    public class EntHeParametros
    {
        public int DiasMes { get; set; }
        public int HorasMesJornadaCompleta { get; set; }
        public decimal Factor50 { get; set; }
        public decimal Factor100 { get; set; }
        public int TopeDiario50 { get; set; }
        public int TopeSemanal50 { get; set; }

        /// <summary>
        /// Decimales a los que se redondean los montos que se pagan. Es
        /// parametro y no constante porque el documento funcional lo declara
        /// asi, y si va a estar en la tabla tiene que mandar de verdad: un
        /// parametro configurable que el codigo ignora es peor que no tenerlo.
        /// </summary>
        public int DecimalesMonto { get; set; }
    }
}
