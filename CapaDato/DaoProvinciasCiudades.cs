using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDato
{
    public class DaoProvinciasCiudades
    {

        public static List<EntProvinciaCiudad> ObtenerProvinciasCiudades(int opcion, string provincia)
        {
            List<EntProvinciaCiudad> resultados = null;

            SqlCommand cmd = null;
            SqlDataReader dr = null;

            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("sp_RTAConsultarProvinciasCiudades", cnx);
                cmd.Parameters.AddWithValue("@opcion", opcion);
                cmd.Parameters.AddWithValue("@Provincia", provincia);

                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();

                resultados = new List<EntProvinciaCiudad>();

                while (dr.Read()){
                                
                    EntProvinciaCiudad provinciaCiudad = new EntProvinciaCiudad();

                    if (opcion == 1)
                    {
                         provinciaCiudad.Id = Convert.ToInt32(dr["idProvincia"].ToString());
                         provinciaCiudad.Nombre = dr["nombreProvincia"].ToString();
                         //provinciaCiudad.ProvinciaId = Convert.ToInt32(dr[""].ToString());
                    }
                    else
                    {
                         provinciaCiudad.Id = Convert.ToInt32(dr["idCiudad"].ToString());
                         provinciaCiudad.Nombre = dr["nombreCiudad"].ToString();
                         provinciaCiudad.ProvinciaId = Convert.ToInt32(dr["idProvincia"].ToString());
                    }

                    resultados.Add(provinciaCiudad);
                }
                    
                
            }
            catch (Exception ex)
            {
                // Handle the exception
            }

            return resultados;
        }


    }
}
