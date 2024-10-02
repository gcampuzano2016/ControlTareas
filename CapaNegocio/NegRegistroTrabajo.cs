using System;
using System.Collections.Generic;
using CapaDato;
using CapaEntidad;
using System.Data;


namespace CapaNegocio
{
    public class NegRegistroTrabajo
    {
        public static List<ERegistroTrabajo> ConsultSp_RTAConsultaMantenimientoContrato(string FchIni, string FchFin, int Id_Usuario)
        {
            return DaoRegistroTrabajo.ConsultSp_RTAListaReporteHorario(FchIni, FchFin, Id_Usuario);
        }
    }
}
