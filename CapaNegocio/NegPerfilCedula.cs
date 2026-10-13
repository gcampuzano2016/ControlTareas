namespace CapaNegocio
{
    /// <summary>
    /// El digito verificador de la cedula ecuatoriana.
    ///
    /// Vive en su propio archivo y no en NegPerfilCampos porque no depende de
    /// nada -ni de entidades, ni de la base, ni de HttpContext- y porque es el
    /// unico pedazo de este modulo que tiene una respuesta correcta conocida de
    /// antemano para cualquier entrada.
    ///
    /// El algoritmo es el mismo de cedula_valida() en
    /// docs/sql/generar-carga-horas-extras.py, que es un generador de un solo
    /// uso en Python y que la aplicacion no puede llamar. Esto NO es una
    /// reutilizacion: es la primera vez que la comprobacion existe en C#.
    /// </summary>
    public static class NegPerfilCedula
    {
        /// <summary>Una cedula tiene exactamente diez digitos.</summary>
        private const int LargoCedula = 10;

        /// <summary>Codigo de provincia mas alto que existe.</summary>
        private const int ProvinciaMaxima = 24;

        /// <summary>Los ecuatorianos nacidos en el exterior llevan 30.</summary>
        private const int ProvinciaExterior = 30;

        /// <summary>
        /// El tercer digito dice de que tipo es el documento. Menor que este
        /// tope es una persona natural; 6 y 9 son entidades publicas y
        /// juridicas, que no son cedulas aunque tengan diez digitos.
        /// </summary>
        private const int TopeTercerDigito = 6;

        /// <summary>
        /// true si el texto es una cedula ecuatoriana bien formada y con digito
        /// verificador correcto. Vacio, nulo y cualquier otra cosa dan false;
        /// quien decide si una cedula vacia se acepta es
        /// NegPerfilCampos.ValidarDatosPersonales, no esta funcion.
        /// </summary>
        public static bool EsValida(string cedula)
        {
            if (cedula == null) { return false; }

            string c = cedula.Trim();

            if (c.Length != LargoCedula) { return false; }

            /* Se comprueba digito por digito y no con int.TryParse: TryParse
               acepta signos y espacios internos, y aca "17 1003406" o "+17100340"
               tienen que dar false. */
            foreach (char ch in c)
            {
                if (ch < '0' || ch > '9') { return false; }
            }

            /* int.Parse sin TryParse porque el bucle de arriba ya garantizo que
               los dos primeros caracteres son digitos. */
            int provincia = int.Parse(c.Substring(0, 2));
            if (!((provincia >= 1 && provincia <= ProvinciaMaxima) || provincia == ProvinciaExterior))
            {
                return false;
            }

            if (c[2] - '0' >= TopeTercerDigito) { return false; }

            /* Coeficientes 2 y 1 alternados sobre los nueve primeros digitos. Un
               producto de dos cifras se reduce restandole 9, que es lo mismo que
               sumar sus dos digitos. */
            int suma = 0;
            for (int i = 0; i < LargoCedula - 1; i++)
            {
                int n = (c[i] - '0') * (i % 2 == 0 ? 2 : 1);
                suma += n > 9 ? n - 9 : n;
            }

            /* El modulo exterior no sobra: cuando la suma es multiplo de 10, el
               verificador es 0 y no 10. */
            int verificador = (10 - (suma % 10)) % 10;

            return verificador == c[LargoCedula - 1] - '0';
        }
    }
}
