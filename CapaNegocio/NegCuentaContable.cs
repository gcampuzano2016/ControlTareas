using CapaDato;
using CapaEntidad;
using System.Collections.Generic;

namespace CapaNegocio
{
    public class NegCuentaContable
    {

        public static List<EntCuentaContable> ConsultaSp_RTAListaCuentasContables(string FchIni, string FchFin, string sucursal, string area)
        {
            return DaoCuentaContable.ConsultaSp_RTAListaCuentasContables(FchIni, FchFin, sucursal, area);
        }

        public static EntRespuesta ConsultaSp_RTAListaCuentasContablesDescargar(string FchIni, string FchFin, string sucursal, string area)
        {
            return DaoCuentaContable.ConsultaSp_RTAListaCuentasContablesDescargar(FchIni, FchFin, sucursal, area);
        }

    }
}
