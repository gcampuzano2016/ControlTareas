using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using CapaEntidad;
using CapaNegocio;
using WinCorreo;


namespace WinSerEnvioCorreo
{
    public partial class SerEnvioCorreo : ServiceBase
    {
        #region Variables
        private Timer tmr_EnvioCorreo;
        public bool Depurar;
        private bool Detener;
        #endregion

        #region SerEnvioCorreo
        public SerEnvioCorreo()
        {
            InitializeComponent();
        }
        #endregion

        #region OnStart
        protected override void OnStart(string[] args)
        {
            try
            {
                this.tmr_EnvioCorreo = new Timer(1000.0);
                this.tmr_EnvioCorreo.Elapsed += new ElapsedEventHandler(this.tiempo_ElapsedEnvioCorreo);
                this.tmr_EnvioCorreo.Enabled = true;
            }
            catch(Exception ex)
            {

            }
        }
        #endregion

        #region OnStop
        protected override void OnStop()
        {
        }
        #endregion

        #region tiempo_ElapsedEnvioCorreo
        public void tiempo_ElapsedEnvioCorreo(object sender, EventArgs e)
        {
            if (this.Detener)
            {
                this.tmr_EnvioCorreo.Enabled = false;
                DetenerServicio("WinSerEnvioCorreo");
            }
            this.tmr_EnvioCorreo.Interval = 1000;//Conversions.ToDouble(10) * 1000.0;
            this.tmr_EnvioCorreo.Enabled = false;
            ClsEnvioCorreo envioCorreo = new ClsEnvioCorreo();
            envioCorreo.EnvioCorreo();
            this.tmr_EnvioCorreo.Enabled = true;
        }
        #endregion

        #region  DetenerServicioAlIniciar
        public void DetenerServicioAlIniciar()
        {
            this.Detener = true;
            this.tmr_EnvioCorreo = new Timer(2000.0);
            this.tmr_EnvioCorreo.Elapsed += new ElapsedEventHandler(this.tiempo_ElapsedEnvioCorreo);
            this.tmr_EnvioCorreo.Enabled = true;
        }
        #endregion

        #region DetenerServicio
        public static void DetenerServicio(string nombre)
        {
            ServiceController controller = new ServiceController(nombre);
            try
            {
                if (controller.Status == ServiceControllerStatus.Running)
                {
                    controller.Stop();
                    controller.WaitForStatus(ServiceControllerStatus.Stopped);
                    controller.Close();
                }
            }
            catch (Exception exception1)
            {

                Exception exception = exception1;
            }
        }
        #endregion
    }
}
