using System;
using System.Collections.Generic;
using CapaDato;
using CapaEntidad;
using System.Data;

namespace CapaNegocio
{
    public class NegCuotaAnual
    {

        public static List<EntCuotaAnual> ConsultaRTA_ConsultarCuotasAnual(string Anio, int tipo)
        {
            return DaoCuotaAnual.ConsultaRTA_ConsultarCuotasAnual(Anio, tipo);
        }
        public static EntRespuesta RTA_CrearCuotasAnual(EntCuotaAnual objEntCuotaAnual)
        {
            EntRespuesta respuesta = new EntRespuesta();

            respuesta = DaoCuotaAnual.RTA_CrearCuotasAnual(objEntCuotaAnual);

            return respuesta;
        }

        public static EntRespuesta RTA_CrearCuotasAnualEmpresa(EntCuotaAnual objEntCuotaAnual)
        {
            EntRespuesta respuesta = new EntRespuesta();

            respuesta = DaoCuotaAnual.RTA_CrearCuotasAnualEmpresa(objEntCuotaAnual);

            return respuesta;
        }

    }
}
