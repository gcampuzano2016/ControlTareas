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
    public class DaoInfoFactProv
    {
        public static List<EntInfoFacturaProv> Consulta_Sp_RTAConsultarListaProveedores(string fecIni, string fecFin, string txtfactura, string txtcliente, int tipo)
        {
            List<EntInfoFacturaProv> listaFacturas = null;

            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("Sp_RTAReporteDeudas", cnx);
                cmd.Parameters.AddWithValue("@FechaInicio", fecIni);
                cmd.Parameters.AddWithValue("@FechaFinal", fecFin);
                cmd.Parameters.AddWithValue("@NumFactura", txtfactura);
                cmd.Parameters.AddWithValue("@Cliente", txtcliente);
                cmd.Parameters.AddWithValue("@tipo", tipo);

                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                listaFacturas = new List<EntInfoFacturaProv>();
                int i = 0;
                while (dr.Read())
                {
                    EntInfoFacturaProv factura = new EntInfoFacturaProv();
                    i= i+1;
                    factura.ID = i.ToString();
                    factura.Nombre = dr["NAME1"].ToString();
                    factura.Valor = dr["DMBTR"].ToString();
                    factura.Abono = dr["ABONO"].ToString();
                    factura.Saldo = dr["SALDO"].ToString();
                    try
                    {
                        object budatValue = dr["BUDAT"];
                        if (budatValue != DBNull.Value)
                        {
                            DateTime budatDate = Convert.ToDateTime(budatValue);
                            factura.Fecha1 = budatDate.ToString("dd/MM/yyyy");
                        }
                        else
                        {
                            factura.Fecha1 = string.Empty; // o cualquier otro valor por defecto
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error al formatear la fecha: " + ex.Message);
                        factura.Fecha1 = string.Empty; // o cualquier otro valor por defecto
                    }

                    factura.Factura = dr["XBLNR"].ToString();
                    factura.Descripcion = dr["SGTXT"].ToString();
                    factura.Otro = dr["BLART"].ToString();

                    listaFacturas.Add(factura);
                }

            }
            catch (Exception ex)
            {
                listaFacturas = null;
            }
            finally
            {
                cmd.Connection.Close();
            }

            return listaFacturas;
        }

        public static List<EntInfoFacturaProv> Consulta_Sp_RTAConsultarListaProveedoresRes(string txtfactura, string txtcliente)
        {
            List<EntInfoFacturaProv> listaFacturas = null;

            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("Sp_RTAReporteDeudas", cnx);
                cmd.Parameters.AddWithValue("@NumFactura", txtfactura);
                cmd.Parameters.AddWithValue("@Cliente", txtcliente);
                cmd.Parameters.AddWithValue("@tipo", 2);

                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                listaFacturas = new List<EntInfoFacturaProv>();
                while (dr.Read())
                {
                    EntInfoFacturaProv factura = new EntInfoFacturaProv();
                    factura.Nombre = dr["NAME1"].ToString();
                    factura.Valor = dr["DMBTR"].ToString();
                    factura.Abono = dr["ABONO"].ToString();
                    factura.Saldo = dr["SALDO"].ToString();

                    listaFacturas.Add(factura);
                }

            }
            catch (Exception ex)
            {
                listaFacturas = null;
            }
            finally
            {
                cmd.Connection.Close();
            }

            return listaFacturas;
        }

        public static List<EntInfoFacturaProv> Consulta_Sp_RTAConsultarListaProveedoresSum(string txtfactura, string txtcliente, int tipo)
        {
            List<EntInfoFacturaProv> listaFacturas = null;

            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("Sp_RTAReporteDeudas", cnx);
                cmd.Parameters.AddWithValue("@NumFactura", txtfactura);
                cmd.Parameters.AddWithValue("@Cliente", txtcliente);
                cmd.Parameters.AddWithValue("@tipo", tipo);

                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                listaFacturas = new List<EntInfoFacturaProv>();
                while (dr.Read())
                {
                    EntInfoFacturaProv factura = new EntInfoFacturaProv();
                    //factura.Nombre = dr["NAME1"].ToString();
                    factura.Valor = dr["DMBTR"].ToString();
                    try
                    {
                        object budatValue = dr["BUDAT"];
                        if (budatValue != DBNull.Value)
                        {
                            DateTime budatDate = Convert.ToDateTime(budatValue);
                            factura.Fecha1 = budatDate.ToString("dd/MM/yyyy");
                        }
                        else
                        {
                            factura.Fecha1 = string.Empty; // o cualquier otro valor por defecto
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error al formatear la fecha: " + ex.Message);
                        factura.Fecha1 = string.Empty; // o cualquier otro valor por defecto
                    }
                    factura.Descripcion = dr["XBLNR"].ToString();

                    listaFacturas.Add(factura);
                }

            }
            catch (Exception ex)
            {
                listaFacturas = null;
            }
            finally
            {
                cmd.Connection.Close();
            }

            return listaFacturas;
        }

    }
}
