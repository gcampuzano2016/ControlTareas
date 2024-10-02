using System;
using System.Collections.Generic;
using CapaDato;
using CapaEntidad;
using System.Data;
using System.IO;

namespace CapaNegocio
{
    public class NegVacaciones
    {
        public static EntRespuesta ConsultarVacaciones(int codSap)
        {
            return DaoVacaciones.ConsultarVacaciones(codSap);
        }

        public static List<EntVacaciones> ConsultarSaldoVacaciones(string CodSap, int tipo)
        {
            return DaoVacaciones.ConsultarSaldoVacaciones(CodSap, tipo);
        }

        #region EscribirLog
        public void EscribirLog(string valor, string Carpeta, string rucEmpresa,bool EstadoProceso)
        {
            try
            {
                if (EstadoProceso == true)
                {
                    string fecha;
                    fecha = DateTime.Now.ToString("dd-MM-yyyy");//DateTime.Now.ToShortDateString().Replace("/", "-");
                    if (!Directory.Exists(@"C:\\" + rucEmpresa + "\\" + Carpeta + "\\" + fecha))
                    {
                        Directory.CreateDirectory(@"C:\\" + rucEmpresa + "\\" + Carpeta + "\\" + fecha);
                    }

                    string path = @"C:\\" + rucEmpresa + "\\" + Carpeta + "\\" + fecha + "\\log.txt";
                    TextWriter tw = new StreamWriter(path, true);
                    tw.WriteLine("A fecha de : " + DateTime.Now.ToString() + ": " + valor);
                    tw.Close();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.EventLog.WriteEntry("Application", "Exception: " + ex.Message);
            }
        }
        #endregion

    }
}
