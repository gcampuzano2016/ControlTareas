using CapaDato;
using CapaEntidad;
using System.Collections.Generic;

namespace CapaNegocio
{
    /// <summary>
    /// Los parametros de calculo, leidos de HE_Parametro.
    ///
    /// No se incrustan en el codigo porque son configurables por diseno: hay un
    /// permiso previsto para editarlos. Si una clave faltara, se usa el valor con
    /// el que se cargo la base, que es lo que el modulo ha usado siempre.
    /// </summary>
    public static class NegHeParametros
    {
        public static EntHeParametros Vigentes()
        {
            Dictionary<string, decimal> v = DaoHorasExtras.LeerParametrosVigentes();

            EntHeParametros p = new EntHeParametros();
            p.DiasMes = (int)Valor(v, "DiasMes", 30m);
            p.HorasMesJornadaCompleta = (int)Valor(v, "HorasMesJornadaCompleta", 240m);
            p.Factor50 = Valor(v, "Factor50", 1.5m);
            p.Factor100 = Valor(v, "Factor100", 2m);
            p.TopeDiario50 = (int)Valor(v, "TopeDiario50", 4m);
            p.TopeSemanal50 = (int)Valor(v, "TopeSemanal50", 12m);
            p.DecimalesMonto = (int)Valor(v, "DecimalesMonto", 2m);
            return p;
        }

        private static decimal Valor(Dictionary<string, decimal> v, string clave, decimal porOmision)
        {
            return v.ContainsKey(clave) ? v[clave] : porOmision;
        }
    }
}
