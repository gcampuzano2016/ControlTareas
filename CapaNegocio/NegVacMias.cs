using CapaDato;
using CapaEntidad;
using System;
using System.Collections.Generic;

namespace CapaNegocio
{
    /// <summary>
    /// "Mis vacaciones": el colaborador ve su propio saldo, con el mismo
    /// calculo que GTH (Sp_RTA_VacCalcular) y el mismo periodo por solicitud
    /// que el estado individual.
    ///
    /// Quien es la persona sale SOLO del usuario de la sesion: no hay ningun
    /// parametro del navegador que elija a quien se consulta. El usuario se
    /// cruza con Vac_Colaborador por la cedula (de R_Usuarios y de Empleados);
    /// si no casa con nadie, o casa con mas de uno, no se muestra nada.
    /// </summary>
    public static class NegVacMias
    {
        public const string AvisoSinRegistro =
            "No encontramos tu registro de vacaciones. Escribe a Talento Humano para que lo revisen.";
        public const string AvisoAmbiguo =
            "Tu usuario coincide con más de un registro de vacaciones. Escribe a Talento Humano para que lo revisen.";

        public static EntVacMias Consultar(string codUsuario, DateTime hoy)
        {
            List<int> ids = DaoVacSaldos.ColaboradoresDeUsuario(codUsuario);
            string aviso = Aviso(ids);
            if (aviso != "") { return new EntVacMias { Aviso = aviso }; }

            EntVacFicha f = DaoVacSaldos.Ficha(hoy, ids[0]);
            if (f == null || f.Saldo == null) { return new EntVacMias { Aviso = AvisoSinRegistro }; }

            return new EntVacMias { Ficha = f, PeriodoDe = NegVacEstado.PeriodoDeCadaSolicitud(f) };
        }

        /// <summary>"" si hay exactamente un registro; si no, el texto para la persona.</summary>
        public static string Aviso(IList<int> ids)
        {
            if (ids == null || ids.Count == 0) { return AvisoSinRegistro; }
            if (ids.Count > 1) { return AvisoAmbiguo; }
            return "";
        }
    }
}
