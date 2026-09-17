using CapaDato;
using CapaEntidad;
using System;
using System.Collections.Generic;

namespace CapaNegocio
{
    /// <summary>
    /// La orquestacion de la pantalla de parametros de horas extras: listar el
    /// historial y guardar una nueva version.
    ///
    /// Vive aparte de NegHeParametros a proposito, igual que
    /// NegHorasExtrasPantalla vive aparte de NegHorasExtras: aquella resuelve
    /// "que valor regia a esta fecha" para el calculo, sin escribir nada;
    /// esta habla con el DAO de la pantalla, que si escribe, y traduce los
    /// codigos de rechazo de Sp_RTA_HeParametroGuardar a texto accionable.
    /// </summary>
    public static class NegHeParametroPantalla
    {
        /// <summary>
        /// Una clave conocida: su etiqueta para la pantalla y si esta activa
        /// -si algun calculo la usa-. TopeDiario50 y TopeSemanal50 estan
        /// cargados y la pantalla los muestra, pero ningun calculo los lee:
        /// RRHH no definio a que umbral mensual se traducen, y marcarlos
        /// como inactivos evita que alguien crea que cambiarlos hace algo.
        /// </summary>
        private class ParametroConocido
        {
            public string Clave;
            public string Etiqueta;
            public bool Activo;
        }

        private static readonly List<ParametroConocido> ClavesConocidas = new List<ParametroConocido>
        {
            new ParametroConocido { Clave = "Factor50", Etiqueta = "Factor de recargo al 50%", Activo = true },
            new ParametroConocido { Clave = "Factor100", Etiqueta = "Factor de recargo al 100%", Activo = true },
            new ParametroConocido { Clave = "DiasMes", Etiqueta = "Dias del mes", Activo = true },
            new ParametroConocido { Clave = "HorasMesJornadaCompleta", Etiqueta = "Horas mensuales de jornada completa", Activo = true },
            new ParametroConocido { Clave = "DecimalesMonto", Etiqueta = "Decimales del monto", Activo = true },
            new ParametroConocido { Clave = "TopeDiario50", Etiqueta = "Tope diario al 50% (no usado por ningun calculo)", Activo = false },
            new ParametroConocido { Clave = "TopeSemanal50", Etiqueta = "Tope semanal al 50% (no usado por ningun calculo)", Activo = false }
        };

        private static ParametroConocido Buscar(string clave)
        {
            string buscada = (clave ?? "").Trim();

            foreach (ParametroConocido p in ClavesConocidas)
            {
                if (string.Equals(p.Clave, buscada, StringComparison.OrdinalIgnoreCase)) { return p; }
            }

            return null;
        }

        /// <summary>Si la clave esta entre las siete que la pantalla conoce.</summary>
        public static bool EsClaveConocida(string clave)
        {
            return Buscar(clave) != null;
        }

        /// <summary>
        /// Si algun calculo usa esta clave. Una clave desconocida no esta
        /// activa: no hay nada que activar para lo que no existe.
        /// </summary>
        public static bool EsParametroActivo(string clave)
        {
            ParametroConocido p = Buscar(clave);
            return p != null && p.Activo;
        }

        /// <summary>
        /// Traduce el codigo de Sp_RTA_HeParametroGuardar a texto que diga que
        /// hacer, no que fallo.
        ///
        /// -5 importa en particular: sin traducirlo, quien escribe
        /// DecimalesMonto con mas de 2 decimales recibe en la cara el error
        /// crudo del CHECK de la tabla.
        /// </summary>
        public static string MensajeDeGuardado(int codigo)
        {
            if (codigo == 0) { return "Parametro guardado."; }
            if (codigo == -1) { return "Esa clave no existe entre los parametros conocidos."; }
            if (codigo == -2) { return "El valor tiene que ser mayor que cero."; }
            if (codigo == -3) { return "Ya existe una version de esa clave con esa misma fecha de inicio."; }
            if (codigo == -4) { return "La fecha nueva tiene que ser posterior a la de la version vigente."; }
            if (codigo == -5) { return "DecimalesMonto no puede tener mas de 2 decimales."; }

            return "No se pudo guardar el parametro.";
        }

        /// <summary>El historial completo, vigente y cerrado, para la pantalla.</summary>
        public static EntRespuesta Listar()
        {
            EntRespuesta respuesta = new EntRespuesta();
            List<EntHeParametroFila> filas = DaoHeParametro.Listar();

            respuesta.estado = "1";
            respuesta.resultado = filas;
            respuesta.mensaje = "";
            respuesta.tipoMensaje = "success";
            return respuesta;
        }

        /// <summary>
        /// Guarda una nueva version de una clave conocida.
        ///
        /// La validacion de fondo -clave, valor, fechas y el CHECK de
        /// decimales- la hace Sp_RTA_HeParametroGuardar; este metodo no la
        /// repite, solo traduce el codigo que devuelve y, si salio bien,
        /// recarga el historial para que la pantalla se repinte con lo que
        /// de verdad quedo en la base.
        /// </summary>
        public static EntRespuesta Guardar(string clave, decimal valor, DateTime desde, string usuario, string ip)
        {
            EntRespuesta respuesta = new EntRespuesta();
            int codigo = DaoHeParametro.Guardar(clave, valor, desde, usuario, ip);

            if (codigo != 0)
            {
                respuesta.estado = "0";
                respuesta.mensaje = MensajeDeGuardado(codigo);
                respuesta.tipoMensaje = "warning";
                return respuesta;
            }

            EntRespuesta resultado = Listar();

            if (resultado.estado == "1")
            {
                resultado.mensaje = MensajeDeGuardado(0);
                resultado.tipoMensaje = "success";
            }

            return resultado;
        }
    }
}
