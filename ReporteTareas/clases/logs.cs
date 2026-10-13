using System;
using System.IO;

namespace logs
{
    public static class logs
    {
        #region VerErrores
        public static void VerErrores(string valor, string Carpeta)
        {
            try
            {
                string fecha;
                fecha = DateTime.Now.ToShortDateString().Replace("/", "-");
                if (!Directory.Exists(@"C:\\" + Carpeta + "\\" + fecha))
                {
                    Directory.CreateDirectory(@"C:\\" + Carpeta + "\\" + fecha);
                }

                string path = @"C:\\" + Carpeta + "\\" + fecha + "\\log.txt";
                TextWriter tw = new StreamWriter(path, true);
                tw.WriteLine("A fecha de : " + DateTime.Now.ToString() + ": " + valor);
                tw.Close();
            }
            catch (Exception ex)
            {
                System.Diagnostics.EventLog.WriteEntry("Application", "Exception: " + ex.Message);
            }
        }
        #endregion
    }
}