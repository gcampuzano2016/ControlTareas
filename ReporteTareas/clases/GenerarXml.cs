using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Data;
using System.IO;
using System.Drawing;
using OnBarcode.Barcode;
using Microsoft.Reporting.WinForms;
using iTextSharp.text.pdf;


namespace ReporteTareas.clases
{
    public class GenerarXml
    {

        #region RutaDeArchivosLogo
        public string RutaDeArchivosLogo(string nombreArchivo)
        {

            string mensaje = "";
            string path = "C:\\Desarrollo\\Desarrollo\\PRY_Sistema ReporteTareas\\ReporteTareas\\Formulario\\" + nombreArchivo;

            if (File.Exists(path))
            {
                // Leer contenido del archivo
                mensaje = File.ReadAllText(path);
            }

            return path;
        }
        #endregion


        #region RutaDeArchivosRide
        public string RutaDeArchivosRide(string nombreArchivo)
        {

            string mensaje = "";
            string path = "C:\\Desarrollo\\Desarrollo\\PRY_Sistema ReporteTareas\\ReporteTareas\\Formulario\\" + nombreArchivo;

            if (File.Exists(path))
            {
                // Leer contenido del archivo
                mensaje = File.ReadAllText(path);
            }

            return mensaje;
        }
        #endregion

        #region xml
        public XmlDocument xml(string DatosCliente, string Detalle, string seriedocumento,string Observacion,string LoginUsuario, string rucUsuario)
        {
            XmlDocument Doc = new XmlDocument();
            try
            {
                string fecha;
                fecha = DateTime.Now.ToString("yyyy-MM-dd");
                string RazonSocial = "COMPUTADORES Y EQUIPOS COMPUEQUIP DOS S.A";
                string NombreComercial = "COMPUTADORES Y EQUIPOS COMPUEQUIP DOS S.A";
                string Ruc = "1790885186001";
                string DirMatriz = "AV. OCCIDENTAL OE6-201 Y JOSE MIGUEL CARRION";
                string dirEstablecimiento = "AV. OCCIDENTAL OE6-201 Y JOSE MIGUEL CARRION";
                string dirPartida = "AV. OCCIDENTAL OE6-201 Y JOSE MIGUEL CARRION";
                string razonSocialTransportista = LoginUsuario;
                string tipoIdentificacionTransportista = "05";
                string rucTransportista = rucUsuario;
                string obligadoContabilidad = "SI";
                string fechaIniTransporte = fecha;
                string fechaFinTransporte = fecha;
                string placa = "001-GRT";
                string claveAcceso = "";
                string codDoc = "06";
                string estab = "001";
                string ptoEmi = "001";
                string Secuencial = seriedocumento.PadLeft(9, '0'); 
                XmlDocument xdoc = new XmlDocument();

                XmlDocument xml = new XmlDocument();
                byte[] bytes = null;
                XmlElement elementoLista = xml.CreateElement(string.Empty, "guiaRemision", string.Empty);
                elementoLista.SetAttribute("id", "", "comprobante");
                elementoLista.SetAttribute("version", "", "1.0.0");
                xml.AppendChild(elementoLista);

                //infoTributaria
                #region infoTributaria
                XmlElement elementoinfoTributaria = xml.CreateElement(string.Empty, "infoTributaria", string.Empty);
                elementoLista.AppendChild(elementoinfoTributaria);

                XmlElement elementoinfoDetAmbienteTributaria = xml.CreateElement(string.Empty, "ambiente", string.Empty);
                XmlText textAmbiente = null;
                textAmbiente = xml.CreateTextNode("1");
                elementoinfoDetAmbienteTributaria.AppendChild(textAmbiente);
                elementoinfoTributaria.AppendChild(elementoinfoDetAmbienteTributaria);

                XmlElement elementoinfoDetEmisionTributaria = xml.CreateElement(string.Empty, "tipoEmision", string.Empty);
                XmlText textEmision = null;
                textEmision = xml.CreateTextNode("1");
                elementoinfoDetEmisionTributaria.AppendChild(textEmision);
                elementoinfoTributaria.AppendChild(elementoinfoDetEmisionTributaria);

                XmlElement elementoinfoDetrazonSocialTributaria = xml.CreateElement(string.Empty, "razonSocial", string.Empty);
                XmlText textrazonSocial = null;
                textrazonSocial = xml.CreateTextNode(RazonSocial);
                elementoinfoDetrazonSocialTributaria.AppendChild(textrazonSocial);
                elementoinfoTributaria.AppendChild(elementoinfoDetrazonSocialTributaria);

                XmlElement elementoinfoDetnombreComercialTributaria = xml.CreateElement(string.Empty, "nombreComercial", string.Empty);
                XmlText textnombreComercial = null;
                textnombreComercial = xml.CreateTextNode(NombreComercial);
                elementoinfoDetnombreComercialTributaria.AppendChild(textnombreComercial);
                elementoinfoTributaria.AppendChild(elementoinfoDetnombreComercialTributaria);

                XmlElement elementoinforucTributaria = xml.CreateElement(string.Empty, "ruc", string.Empty);
                XmlText textruc = null;
                textruc = xml.CreateTextNode(Ruc);
                elementoinforucTributaria.AppendChild(textruc);
                elementoinfoTributaria.AppendChild(elementoinforucTributaria);

                XmlElement elementoinfoClaveAccesoTributaria = xml.CreateElement(string.Empty, "claveAcceso", string.Empty);
                XmlText textclaveAcceso = null;
                string fechaEmision;
                fechaEmision = DateTime.Now.ToString("dd/MM/yyyy");
                claveAcceso = fechaEmision.Replace("/", "") + codDoc + Ruc + "1" + estab + ptoEmi + Secuencial;
                claveAcceso = claveAcceso + leerCodigoNumerico() + "1";
                claveAcceso = claveAcceso + leerCodigoVerificacion(claveAcceso);
                textclaveAcceso = xml.CreateTextNode(claveAcceso);
                elementoinfoClaveAccesoTributaria.AppendChild(textclaveAcceso);
                elementoinfoTributaria.AppendChild(elementoinfoClaveAccesoTributaria);

                XmlElement elementoinfocodDocTributaria = xml.CreateElement(string.Empty, "codDoc", string.Empty);
                XmlText textcodDoc = null;
                textcodDoc = xml.CreateTextNode("06");
                elementoinfocodDocTributaria.AppendChild(textcodDoc);
                elementoinfoTributaria.AppendChild(elementoinfocodDocTributaria);

                XmlElement elementoinfoestabTributaria = xml.CreateElement(string.Empty, "estab", string.Empty);
                XmlText textestab = null;
                textestab = xml.CreateTextNode("000");
                elementoinfoestabTributaria.AppendChild(textestab);
                elementoinfoTributaria.AppendChild(elementoinfoestabTributaria);

                XmlElement elementoinfoptoEmiTributaria = xml.CreateElement(string.Empty, "ptoEmi", string.Empty);
                XmlText textptoEmi = null;
                textptoEmi = xml.CreateTextNode("000");
                elementoinfoptoEmiTributaria.AppendChild(textptoEmi);
                elementoinfoTributaria.AppendChild(elementoinfoptoEmiTributaria);

                XmlElement elementoinfosecuencialTributaria = xml.CreateElement(string.Empty, "secuencial", string.Empty);
                XmlText textsecuencial = null;
                textsecuencial = xml.CreateTextNode(Secuencial);
                elementoinfosecuencialTributaria.AppendChild(textsecuencial);
                elementoinfoTributaria.AppendChild(elementoinfosecuencialTributaria);

                XmlElement elementoinfodirMatrizTributaria = xml.CreateElement(string.Empty, "dirMatriz", string.Empty);
                XmlText textdirMatriz = null;
                textdirMatriz = xml.CreateTextNode(DirMatriz);
                elementoinfodirMatrizTributaria.AppendChild(textdirMatriz);
                elementoinfoTributaria.AppendChild(elementoinfodirMatrizTributaria);

                #endregion
                //infoTributaria

                //infoGuiaRemision
                #region infoGuiaRemision
                XmlElement elementoinfoFactura = xml.CreateElement(string.Empty, "infoGuiaRemision", string.Empty);
                elementoLista.AppendChild(elementoinfoFactura);
                if (dirEstablecimiento != "")
                {
                    XmlElement elementoinfoDetalledirEstablecimiento = xml.CreateElement(string.Empty, "dirEstablecimiento", string.Empty);
                    XmlText textdirEstablecimiento = null;
                    textdirEstablecimiento = xml.CreateTextNode(dirEstablecimiento);
                    elementoinfoDetalledirEstablecimiento.AppendChild(textdirEstablecimiento);
                    elementoinfoFactura.AppendChild(elementoinfoDetalledirEstablecimiento);
                }

                XmlElement elementoinfoDetalledirPartida = xml.CreateElement(string.Empty, "dirPartida", string.Empty);
                XmlText textdirPartida = null;
                
                textdirPartida = xml.CreateTextNode(dirPartida);
                elementoinfoDetalledirPartida.AppendChild(textdirPartida);
                elementoinfoFactura.AppendChild(elementoinfoDetalledirPartida);

                XmlElement elementoinfoDetallerazonSocialTransportista = xml.CreateElement(string.Empty, "razonSocialTransportista", string.Empty);
                XmlText textrazonSocialTransportista = null;
                textrazonSocialTransportista = xml.CreateTextNode(razonSocialTransportista.ToString().Trim());
                elementoinfoDetallerazonSocialTransportista.AppendChild(textrazonSocialTransportista);
                elementoinfoFactura.AppendChild(elementoinfoDetallerazonSocialTransportista);

                XmlElement elementoinfoDetalletipoIdentificacionTransportista = xml.CreateElement(string.Empty, "tipoIdentificacionTransportista", string.Empty);
                XmlText texttipoIdentificacionTransportista = null;
                texttipoIdentificacionTransportista = xml.CreateTextNode(tipoIdentificacionTransportista.ToString().Trim());
                elementoinfoDetalletipoIdentificacionTransportista.AppendChild(texttipoIdentificacionTransportista);
                elementoinfoFactura.AppendChild(elementoinfoDetalletipoIdentificacionTransportista);

                XmlElement elementoinfoDetallerucTransportista = xml.CreateElement(string.Empty, "rucTransportista", string.Empty);
                XmlText textrucTransportista = null;
                textrucTransportista = xml.CreateTextNode(rucTransportista.ToString().Trim());
                elementoinfoDetallerucTransportista.AppendChild(textrucTransportista);
                elementoinfoFactura.AppendChild(elementoinfoDetallerucTransportista);

                XmlElement elementoinfoDetalleobligadoContabilidad = xml.CreateElement(string.Empty, "obligadoContabilidad", string.Empty);
                XmlText textobligadoContabilidad = null;
                textobligadoContabilidad = xml.CreateTextNode(obligadoContabilidad);
                elementoinfoDetalleobligadoContabilidad.AppendChild(textobligadoContabilidad);
                elementoinfoFactura.AppendChild(elementoinfoDetalleobligadoContabilidad);

                XmlElement elementoinfoDetallefechaIniTransporte = xml.CreateElement(string.Empty, "fechaIniTransporte", string.Empty);
                XmlText textfechaIniTransporte = null;
                textfechaIniTransporte = xml.CreateTextNode(fechaIniTransporte.Trim());
                elementoinfoDetallefechaIniTransporte.AppendChild(textfechaIniTransporte);
                elementoinfoFactura.AppendChild(elementoinfoDetallefechaIniTransporte);

                XmlElement elementoinfoDetallefechaFinTransporte = xml.CreateElement(string.Empty, "fechaFinTransporte", string.Empty);
                XmlText textfechaFinTransporte = null;
                textfechaFinTransporte = xml.CreateTextNode(fechaFinTransporte.Trim());
                elementoinfoDetallefechaFinTransporte.AppendChild(textfechaFinTransporte);
                elementoinfoFactura.AppendChild(elementoinfoDetallefechaFinTransporte);

                XmlElement elementoinfoDetalleplaca = xml.CreateElement(string.Empty, "placa", string.Empty);
                XmlText textplaca = null;
                textplaca = xml.CreateTextNode(placa.Trim());
                elementoinfoDetalleplaca.AppendChild(textplaca);
                elementoinfoFactura.AppendChild(elementoinfoDetalleplaca);

                #endregion
                //infoGuiaRemision

                //detalles
                #region detalles
                XmlElement elementodetalles = xml.CreateElement(string.Empty, "destinatarios", string.Empty);
                elementoLista.AppendChild(elementodetalles);

                string[] listas = DatosCliente.Split('↨');



                XmlElement elementodetalle = xml.CreateElement(string.Empty, "destinatario", string.Empty);
                elementodetalles.AppendChild(elementodetalle);

                XmlElement elementoDetallesHijosidentificacionDestinatario = xml.CreateElement(string.Empty, "identificacionDestinatario", string.Empty);
                XmlText textDetalleidentificacionDestinatario = null;
                textDetalleidentificacionDestinatario = xml.CreateTextNode(listas[0].ToString().Trim());
                elementoDetallesHijosidentificacionDestinatario.AppendChild(textDetalleidentificacionDestinatario);
                elementodetalle.AppendChild(elementoDetallesHijosidentificacionDestinatario);

                XmlElement elementoDetallesHijosrazonSocialDestinatario = xml.CreateElement(string.Empty, "razonSocialDestinatario", string.Empty);
                XmlText textDetallerazonSocialDestinatario = null;
                textDetallerazonSocialDestinatario = xml.CreateTextNode(listas[1].ToString().Trim());
                elementoDetallesHijosrazonSocialDestinatario.AppendChild(textDetallerazonSocialDestinatario);
                elementodetalle.AppendChild(elementoDetallesHijosrazonSocialDestinatario);

                XmlElement elementoDetallesHijosdirDestinatario = xml.CreateElement(string.Empty, "dirDestinatario", string.Empty);
                XmlText textDetalledirDestinatario = null;
                textDetalledirDestinatario = xml.CreateTextNode(listas[2].ToString().Trim());
                elementoDetallesHijosdirDestinatario.AppendChild(textDetalledirDestinatario);
                elementodetalle.AppendChild(elementoDetallesHijosdirDestinatario);

                XmlElement elementoDetallesHijosmotivoTraslado = xml.CreateElement(string.Empty, "motivoTraslado", string.Empty);
                XmlText textDetallemotivoTraslado = null;
                textDetallemotivoTraslado = xml.CreateTextNode(listas[3].ToString().Trim());
                elementoDetallesHijosmotivoTraslado.AppendChild(textDetallemotivoTraslado);
                elementodetalle.AppendChild(elementoDetallesHijosmotivoTraslado);

                if (listas[4].ToString().Trim() != "")
                {
                    XmlElement elementoDetallesHijosdocAduaneroUnico = xml.CreateElement(string.Empty, "docAduaneroUnico", string.Empty);
                    XmlText textDetalledocAduaneroUnico = null;
                    textDetalledocAduaneroUnico = xml.CreateTextNode(listas[4].ToString().Trim());
                    elementoDetallesHijosdocAduaneroUnico.AppendChild(textDetalledocAduaneroUnico);
                    elementodetalle.AppendChild(elementoDetallesHijosdocAduaneroUnico);
                }

                if (listas[5].ToString().Trim() != "")
                {
                    XmlElement elementoDetallesHijoscodEstabDestino = xml.CreateElement(string.Empty, "codEstabDestino", string.Empty);
                    XmlText textDetallecodEstabDestino = null;
                    textDetallecodEstabDestino = xml.CreateTextNode(listas[5].ToString().Trim());
                    elementoDetallesHijoscodEstabDestino.AppendChild(textDetallecodEstabDestino);
                    elementodetalle.AppendChild(elementoDetallesHijoscodEstabDestino);
                }

                if (listas[6].ToString().Trim() != "")
                {
                    XmlElement elementoDetallesHijosruta = xml.CreateElement(string.Empty, "ruta", string.Empty);
                    XmlText textDetalleruta = null;
                    textDetalleruta = xml.CreateTextNode(listas[6].ToString().Trim());
                    elementoDetallesHijosruta.AppendChild(textDetalleruta);
                    elementodetalle.AppendChild(elementoDetallesHijosruta);
                }

                XmlElement elementoDetallesHijoscodDocSustento = xml.CreateElement(string.Empty, "codDocSustento", string.Empty);
                XmlText textDetallecodDocSustento = null;
                textDetallecodDocSustento = xml.CreateTextNode(listas[7].ToString().Trim());
                elementoDetallesHijoscodDocSustento.AppendChild(textDetallecodDocSustento);
                elementodetalle.AppendChild(elementoDetallesHijoscodDocSustento);

                XmlElement elementoDetallesHijosnumDocSustento = xml.CreateElement(string.Empty, "numDocSustento", string.Empty);
                XmlText textDetallenumDocSustento = null;
                textDetallenumDocSustento = xml.CreateTextNode(listas[8].ToString().Trim());
                elementoDetallesHijosnumDocSustento.AppendChild(textDetallenumDocSustento);
                elementodetalle.AppendChild(elementoDetallesHijosnumDocSustento);

                XmlElement elementoDetallesHijosnumAutDocSustento = xml.CreateElement(string.Empty, "numAutDocSustento", string.Empty);
                XmlText textDetallenumAutDocSustento = null;
                textDetallenumAutDocSustento = xml.CreateTextNode(listas[9].ToString().Trim());
                elementoDetallesHijosnumAutDocSustento.AppendChild(textDetallenumAutDocSustento);
                elementodetalle.AppendChild(elementoDetallesHijosnumAutDocSustento);

                XmlElement elementoDetallesHijosfechaEmisionDocSustento = xml.CreateElement(string.Empty, "fechaEmisionDocSustento", string.Empty);
                XmlText textDetallefechaEmisionDocSustento = null;
                textDetallefechaEmisionDocSustento = xml.CreateTextNode(listas[10].ToString().Trim());
                elementoDetallesHijosfechaEmisionDocSustento.AppendChild(textDetallefechaEmisionDocSustento);
                elementodetalle.AppendChild(elementoDetallesHijosfechaEmisionDocSustento);

                //DetalleImpuesto
                #region DetalleImpuesto
                XmlElement elementoImpuestos = xml.CreateElement(string.Empty, "detalles", string.Empty);
                elementodetalle.AppendChild(elementoImpuestos);

                string[] listas2 = Detalle.Split('↨');

                foreach (string r2 in listas2.ToArray())
                {
                    if (r2 != "")
                    {
                        string[] lista2 = r2.Split(';');
                        string[] lista3 = lista2[0].Split('à');
                        XmlElement elementoImpuesto = xml.CreateElement(string.Empty, "detalle", string.Empty);
                        elementoImpuestos.AppendChild(elementoImpuesto);

                        XmlElement elementoDetalleImpuestoscodigoInterno = xml.CreateElement(string.Empty, "codigoInterno", string.Empty);
                        XmlText textcodigoInterno = null;
                        textcodigoInterno = xml.CreateTextNode(lista3[3].ToString().Trim());
                        elementoDetalleImpuestoscodigoInterno.AppendChild(textcodigoInterno);
                        elementoImpuesto.AppendChild(elementoDetalleImpuestoscodigoInterno);

                        XmlElement elementoDetallecodigoAdicional = xml.CreateElement(string.Empty, "codigoAdicional", string.Empty);
                        XmlText textcodigoAdicional = null;
                        textcodigoAdicional = xml.CreateTextNode(lista3[2].ToString().Trim());
                        elementoDetallecodigoAdicional.AppendChild(textcodigoAdicional);
                        elementoImpuesto.AppendChild(elementoDetallecodigoAdicional);

                        XmlElement elementoDetalledescripcion = xml.CreateElement(string.Empty, "descripcion", string.Empty);
                        XmlText textdescripcion = null;
                        textdescripcion = xml.CreateTextNode(lista2[2].ToString().Trim());
                        elementoDetalledescripcion.AppendChild(textdescripcion);
                        elementoImpuesto.AppendChild(elementoDetalledescripcion);

                        XmlElement elementoDetallecantidad = xml.CreateElement(string.Empty, "cantidad", string.Empty);
                        XmlText textcantidad = null;
                        textcantidad = xml.CreateTextNode(lista2[1].ToString().Trim());
                        elementoDetallecantidad.AppendChild(textcantidad);
                        elementoImpuesto.AppendChild(elementoDetallecantidad);

                        XmlElement elementoDetalleNumSerie = xml.CreateElement(string.Empty, "NumSerie", string.Empty);
                        XmlText textNumSerie = null;
                        if (lista3[1].ToString().Trim() == "")
                        {
                            textNumSerie = xml.CreateTextNode("----");
                        }
                        else
                        {
                            textNumSerie = xml.CreateTextNode(lista3[1].ToString().Trim());
                        }
                        elementoDetalleNumSerie.AppendChild(textNumSerie);
                        elementoImpuesto.AppendChild(elementoDetalleNumSerie);
                    }
                }

                #endregion
                //DetalleImpuesto


                #endregion
                //detalles

                //infoAdicional
                #region infoAdicional
                XmlElement elementoinfoAdicional = xml.CreateElement(string.Empty, "infoAdicional", string.Empty);
                elementoLista.AppendChild(elementoinfoAdicional);

                XmlElement elementoDetallesHijos1 = xml.CreateElement(string.Empty, "campoAdicional", string.Empty);
                XmlText textDetalleAdicional1 = null;
                textDetalleAdicional1 = xml.CreateTextNode(RazonSocial.Trim());
                elementoDetallesHijos1.SetAttribute("nombre", "", "nombre");
                elementoDetallesHijos1.AppendChild(textDetalleAdicional1);
                elementoinfoAdicional.AppendChild(elementoDetallesHijos1);

                XmlElement elementoDetallesHijos2 = xml.CreateElement(string.Empty, "campoAdicional", string.Empty);
                XmlText textDetalleAdicional2 = null;
                textDetalleAdicional2 = xml.CreateTextNode(Observacion.ToString().Trim());
                elementoDetallesHijos2.SetAttribute("nombre", "", "Referencia/Observacion");
                elementoDetallesHijos2.AppendChild(textDetalleAdicional2);
                elementoinfoAdicional.AppendChild(elementoDetallesHijos2);

                #endregion
                //infoAdicional

                bytes = System.Text.Encoding.UTF8.GetBytes(xml.OuterXml);
                string xmlDoc = System.Text.Encoding.UTF8.GetString(bytes);
                Doc.LoadXml(xmlDoc);
            }
            catch (Exception ex)
            {
                
            }
                return Doc;
        }
        #endregion

        #region object ->crear CodigoNumerico
        public static string leerCodigoNumerico()
        {
            int num = 17;// DateTime.Now.Day;
            string str1 = num.ToString().PadLeft(2, '0');
            num = DateTime.Now.Hour;
            string str2 = num.ToString().PadLeft(2, '0');
            num = DateTime.Now.Minute;
            string str3 = num.ToString().PadLeft(2, '0');
            num = DateTime.Now.Second;
            string str4 = num.ToString().Trim().PadLeft(2, '0');
            return str1 + str2 + str3 + str4;
        }
        #endregion

        #region object ->crear Modulo11
        public static string leerCodigoVerificacion(string CodigoInicial)
        {
            int num1 = 0;
            int num2 = 2;
            if (CodigoInicial.Length != 48)
                return "Error, logitud " + CodigoInicial.Length.ToString() + " no es correcta";
            foreach (char c in CodigoInicial)
            {
                if (!char.IsNumber(c))
                    return "el codigo solo admite numeros";
            }
            for (int index = 47; index >= 0; --index)
            {
                num1 += int.Parse(CodigoInicial[index].ToString()) * num2;
                ++num2;
                if (num2 == 8)
                    num2 = 2;
            }
            int num3 = 11 - num1 % 11;
            if (num3 == 11)
                num3 = 0;
            if (num3 == 10)
                num3 = 1;
            return num3.ToString();
        }
        #endregion

        #region PasarXmlDataset
        public byte[] PasarXmlDataset(string DirecionXml, string TipoDocumento, string Logo, string RutaRide, string tipoRide, ref string MensajeError, ref string RutaPdf,ref string Archivo)
        {
            string RIDE = "";
            DataSet dataSet = new DataSet();
            XmlDocument doc = new XmlDocument();
            byte[] buffer2 = null;
            try
            {
                #region ParametrizacionDocumentos

                if (tipoRide == "SRI" && TipoDocumento == "06")
                    RIDE = "GuiaRemision.rdlc";
                else if (tipoRide == "ND" && TipoDocumento == "06")
                    RIDE = "GuiaRemisionPerso.rdlc";
                else if (tipoRide == "SRI" && TipoDocumento == "01")
                    RIDE = "Factura.rdlc";
                else if (tipoRide == "ND" && TipoDocumento == "01")
                    RIDE = "FacturaPerso.rdlc";
                else if (tipoRide == "SRI" && TipoDocumento == "04")
                    RIDE = "NotaCredito.rdlc";
                else if (tipoRide == "ND" && TipoDocumento == "04")
                    RIDE = "NotaCreditoPerso.rdlc";
                else if (tipoRide == "SRI" && TipoDocumento == "05")
                    RIDE = "NotaDebito.rdlc";
                else if (tipoRide == "ND" && TipoDocumento == "05")
                    RIDE = "NotaDebitoPerso.rdlc";
                else if (tipoRide == "SRI" && TipoDocumento == "07")
                    RIDE = "Retencion.rdlc";
                else if (tipoRide == "ND" && TipoDocumento == "07")
                    RIDE = "RetencionPerso.rdlc";

                RutaRide = RutaRide + RIDE;

                #endregion ParametrizacionDocumentos

                #region infoGuiaRemision
                if (TipoDocumento == "06")
                {
                    string fecha;
                    fecha = DateTime.Now.ToString("dd/MM/yyyy");
                    doc.LoadXml(DirecionXml);
                    //XmlNode XmlNumAutorizacion = doc.LastChild.ChildNodes[1];
                    //XmlNode XmlFechaAutorizacion = doc.LastChild.ChildNodes[2];
                    //XmlNode XmlComprobante = doc.LastChild.ChildNodes[4];
                    string s = doc.InnerXml;
                    if (s.Contains("<ds:Signature") & s.Contains("</ds:Signature>"))
                    {
                        int num4 = s.IndexOf("<ds:Signature");
                        int count = (s.IndexOf("</ds:Signature>") - num4) + 15;
                        s = s.Remove(num4, count).Trim();
                    }
                    s = s.Replace("<![CDATA[", "").Replace("]]>", "");
                    s = s.Replace("<infoGuiaRemision>", "").Replace("</infoTributaria>", "");
                    s = s.Replace("</infoGuiaRemision>", "</infoTributaria>");
                    dataSet.ReadXml(new XmlTextReader(new StringReader(s)));
                    buffer2 = GenerarDocumento(GenerarDataSetGuiaRemision(dataSet, "0000000000000000000000000000000000000000000000000", fecha, RutaDeArchivosLogo(Logo), ref MensajeError), RutaDeArchivosRide(RutaRide), TipoDocumento, ref MensajeError,ref RutaPdf,ref Archivo);
                }
                #endregion

                dataSet.Dispose();
            }
            catch (Exception ex)
            {
                MensajeError = ex.Message.ToString();
            }
            return buffer2;
        }
        #endregion

        #region GenerarDataSetGuiaRemision
        public DataSet GenerarDataSetGuiaRemision(DataSet InformacionPorGenerar, string NumAutorizacion, string FechaAutorizacion, string rutaImagen, ref string MensajeError)
        {
            string Ambiente = "";
            string tipoEmision = "";
            DataSet GenerarProceso = new DataSet();
            try
            {
                #region Encabezado Reporte
                #region proceso 1
                DataTable dt = new DataTable();
                dt.Columns.Add("ambiente");
                dt.Columns.Add("tipoEmision");
                dt.Columns.Add("razonSocial");
                dt.Columns.Add("nombreComercial");
                dt.Columns.Add("ruc");
                dt.Columns.Add("claveAcceso");
                dt.Columns.Add("codDoc");
                dt.Columns.Add("estab");
                dt.Columns.Add("ptoEmi");
                dt.Columns.Add("secuencial");
                dt.Columns.Add("SecuencialCompleto");
                dt.Columns.Add("dirMatriz");
                dt.Columns.Add("dirEstablecimiento");
                dt.Columns.Add("dirPartida");
                dt.Columns.Add("razonSocialTransportista");
                dt.Columns.Add("tipoIdentificacionTransportista");
                dt.Columns.Add("rucTransportista");
                dt.Columns.Add("obligadoContabilidad");
                dt.Columns.Add("contribuyenteEspecial");
                dt.Columns.Add("fechaIniTransporte", typeof(DateTime));
                dt.Columns.Add("fechaFinTransporte", typeof(DateTime));
                dt.Columns.Add("placa");
                dt.Columns.Add("Imagen", typeof(byte[]));
                dt.Columns.Add("NumAutorizacion");
                dt.Columns.Add("FechaAutorizacion");
                dt.Columns.Add("CodigoBarra", typeof(byte[]));
                #endregion

                #region proceso 2
                if (InformacionPorGenerar.Tables[1].Rows[0]["ambiente"].ToString() == "2")
                    Ambiente = "Producción";
                else
                    Ambiente = "Pruebas";

                if (InformacionPorGenerar.Tables[1].Rows[0]["tipoEmision"].ToString() == "1")
                    tipoEmision = "Normal";
                else
                    tipoEmision = "PRUEBAS";

                DataRow row = dt.NewRow();
                row["ambiente"] = Ambiente;
                row["tipoEmision"] = tipoEmision;
                row["razonSocial"] = InformacionPorGenerar.Tables[1].Rows[0]["razonSocial"].ToString();

                if (InformacionPorGenerar.Tables[1].Columns.Contains("nombreComercial"))
                    row["nombreComercial"] = InformacionPorGenerar.Tables[1].Rows[0]["nombreComercial"].ToString();
                else
                    row["nombreComercial"] = "";

                row["ruc"] = InformacionPorGenerar.Tables[1].Rows[0]["ruc"].ToString();
                row["claveAcceso"] = InformacionPorGenerar.Tables[1].Rows[0]["claveAcceso"].ToString();
                row["codDoc"] = InformacionPorGenerar.Tables[1].Rows[0]["codDoc"].ToString();
                row["estab"] = InformacionPorGenerar.Tables[1].Rows[0]["estab"].ToString();
                row["ptoEmi"] = InformacionPorGenerar.Tables[1].Rows[0]["ptoEmi"].ToString();
                row["secuencial"] = InformacionPorGenerar.Tables[1].Rows[0]["secuencial"].ToString();
                row["SecuencialCompleto"] = InformacionPorGenerar.Tables[1].Rows[0]["estab"].ToString() + "-" + InformacionPorGenerar.Tables[1].Rows[0]["ptoEmi"].ToString() + "-" + InformacionPorGenerar.Tables[1].Rows[0]["secuencial"].ToString();
                row["dirMatriz"] = InformacionPorGenerar.Tables[1].Rows[0]["dirMatriz"].ToString();

                if (InformacionPorGenerar.Tables[1].Columns.Contains("dirEstablecimiento"))
                    row["dirEstablecimiento"] = InformacionPorGenerar.Tables[1].Rows[0]["dirEstablecimiento"].ToString();
                else
                    row["dirEstablecimiento"] = "";

                row["dirPartida"] = InformacionPorGenerar.Tables[1].Rows[0]["dirPartida"].ToString();
                row["razonSocialTransportista"] = InformacionPorGenerar.Tables[1].Rows[0]["razonSocialTransportista"].ToString();
                row["tipoIdentificacionTransportista"] = InformacionPorGenerar.Tables[1].Rows[0]["tipoIdentificacionTransportista"].ToString();
                row["rucTransportista"] = InformacionPorGenerar.Tables[1].Rows[0]["rucTransportista"].ToString();

                if (InformacionPorGenerar.Tables[1].Columns.Contains("obligadoContabilidad"))
                    row["obligadoContabilidad"] = InformacionPorGenerar.Tables[1].Rows[0]["obligadoContabilidad"].ToString();
                else
                    row["obligadoContabilidad"] = "";

                if (InformacionPorGenerar.Tables[1].Columns.Contains("contribuyenteEspecial"))
                    row["contribuyenteEspecial"] = InformacionPorGenerar.Tables[1].Rows[0]["contribuyenteEspecial"].ToString();
                else if (InformacionPorGenerar.Tables[1].Columns.Contains("contribuyenteEspecial"))
                    row["contribuyenteEspecial"] = "";

                row["fechaIniTransporte"] = Convert.ToDateTime(InformacionPorGenerar.Tables[1].Rows[0]["fechaIniTransporte"].ToString());
                row["fechaFinTransporte"] = Convert.ToDateTime(InformacionPorGenerar.Tables[1].Rows[0]["fechaFinTransporte"].ToString());
                row["placa"] = InformacionPorGenerar.Tables[1].Rows[0]["placa"].ToString();
                byte[] CodigoBarra = null;
                CodigoBarra = Convertir_Imagen_Bytes(codigo128(InformacionPorGenerar.Tables[1].Rows[0]["claveAcceso"].ToString()));

                row["Imagen"] = File.ReadAllBytes(rutaImagen);
                row["NumAutorizacion"] = NumAutorizacion;
                row["FechaAutorizacion"] = FechaAutorizacion;
                row["CodigoBarra"] = CodigoBarra;
                dt.Rows.Add(row);
                #endregion
                #endregion

                #region Detalle Reporte
                #region Proceso 1
                DataTable dtDetalle = new DataTable();
                dtDetalle.Columns.Add("idDetalle");
                dtDetalle.Columns.Add("identificacionDestinatario");
                dtDetalle.Columns.Add("razonSocialDestinatario");
                dtDetalle.Columns.Add("dirDestinatario");
                dtDetalle.Columns.Add("motivoTraslado");
                dtDetalle.Columns.Add("fechaEmisionDocSustento");
                dtDetalle.Columns.Add("idDetallePadre");
                dtDetalle.Columns.Add("codigoInterno");
                dtDetalle.Columns.Add("codigoAdicional");
                dtDetalle.Columns.Add("descripcion");
                dtDetalle.Columns.Add("cantidad");
                dtDetalle.Columns.Add("NumSerie");
                dtDetalle.Columns.Add("detAdicionalNombre");
                dtDetalle.Columns.Add("detAdicionalValor");
                dtDetalle.Columns.Add("codDocSustento");
                dtDetalle.Columns.Add("numDocSustento");
                dtDetalle.Columns.Add("numAutDocSustento");
                dtDetalle.Columns.Add("docAduaneroUnico");
                dtDetalle.Columns.Add("codEstabDestino");
                dtDetalle.Columns.Add("ruta");
                #endregion

                #region Proceso 2
                foreach (DataRow datos in InformacionPorGenerar.Tables[5].Rows)
                {
                    DataRow row2 = dtDetalle.NewRow();

                    if (InformacionPorGenerar.Tables[5].Columns.Contains("detalle_Id"))
                        row2["idDetalle"] = datos["detalle_Id"].ToString();
                    else if (InformacionPorGenerar.Tables[5].Columns.Contains("detalles_Id"))
                        row2["idDetalle"] = datos["detalles_Id"].ToString();

                    row2["identificacionDestinatario"] = InformacionPorGenerar.Tables[3].Rows[0]["identificacionDestinatario"].ToString();
                    row2["razonSocialDestinatario"] = InformacionPorGenerar.Tables[3].Rows[0]["razonSocialDestinatario"].ToString();
                    row2["dirDestinatario"] = InformacionPorGenerar.Tables[3].Rows[0]["dirDestinatario"].ToString();
                    row2["motivoTraslado"] = InformacionPorGenerar.Tables[3].Rows[0]["motivoTraslado"].ToString();

                    if (InformacionPorGenerar.Tables[3].Columns.Contains("fechaEmisionDocSustento"))
                        row2["fechaEmisionDocSustento"] = InformacionPorGenerar.Tables[3].Rows[0]["fechaEmisionDocSustento"].ToString();
                    else
                        row2["fechaEmisionDocSustento"] = "";

                    row2["idDetallePadre"] = "";
                    row2["codigoInterno"] = datos["codigoInterno"].ToString();

                    if (InformacionPorGenerar.Tables[5].Columns.Contains("codigoAdicional"))
                        row2["codigoAdicional"] = datos["codigoAdicional"].ToString();
                    else
                        row2["codigoAdicional"] = "";

                    row2["descripcion"] = datos["descripcion"].ToString();
                    row2["cantidad"] = datos["cantidad"].ToString();
                    row2["NumSerie"] = datos["NumSerie"].ToString();

                    row2["detAdicionalNombre"] = "";

                    row2["detAdicionalValor"] = "";

                    if (InformacionPorGenerar.Tables[3].Columns.Contains("codDocSustento"))
                    {
                        string tipoDocumento = "";
                        if (InformacionPorGenerar.Tables[3].Rows[0]["codDocSustento"].ToString() == "01")
                            tipoDocumento = "FACTURA";
                        else if (InformacionPorGenerar.Tables[3].Rows[0]["codDocSustento"].ToString() == "04")
                            tipoDocumento = "NOTA DE CRÉDITO";
                        else if (InformacionPorGenerar.Tables[3].Rows[0]["codDocSustento"].ToString() == "05")
                            tipoDocumento = "NOTA DE DÉBITO";
                        else if (InformacionPorGenerar.Tables[3].Rows[0]["codDocSustento"].ToString() == "06")
                            tipoDocumento = "GUÍA DE REMISIÓN";
                        else if (InformacionPorGenerar.Tables[3].Rows[0]["codDocSustento"].ToString() == "07")
                            tipoDocumento = "COMPROBANTE DE RETENCIÓN";

                        row2["codDocSustento"] = tipoDocumento;
                    }
                    else
                        row2["codDocSustento"] = "";

                    if (InformacionPorGenerar.Tables[3].Columns.Contains("numDocSustento"))
                        row2["numDocSustento"] = InformacionPorGenerar.Tables[3].Rows[0]["numDocSustento"].ToString();
                    else
                        row2["numDocSustento"] = "";

                    if (InformacionPorGenerar.Tables[3].Columns.Contains("numAutDocSustento"))
                        row2["numAutDocSustento"] = InformacionPorGenerar.Tables[3].Rows[0]["numAutDocSustento"].ToString();
                    else
                        row2["numAutDocSustento"] = "";

                    if (InformacionPorGenerar.Tables[3].Columns.Contains("docAduaneroUnico"))
                        row2["docAduaneroUnico"] = InformacionPorGenerar.Tables[3].Rows[0]["docAduaneroUnico"].ToString();
                    else
                        row2["docAduaneroUnico"] = "";

                    if (InformacionPorGenerar.Tables[3].Columns.Contains("codEstabDestino"))
                        row2["codEstabDestino"] = InformacionPorGenerar.Tables[3].Rows[0]["codEstabDestino"].ToString();
                    else
                        row2["codEstabDestino"] = "";

                    if (InformacionPorGenerar.Tables[3].Columns.Contains("ruta"))
                        row2["ruta"] = InformacionPorGenerar.Tables[3].Rows[0]["ruta"].ToString();
                    else
                        row2["ruta"] = "";

                    dtDetalle.Rows.Add(row2);
                }
                #endregion

                #region CargarDetalles
                foreach (DataRow detalle in dtDetalle.Rows)
                {
                    if (InformacionPorGenerar.Tables.Contains("detallesAdicionales"))
                    {
                        DataView Resul = InformacionPorGenerar.Tables[6].DefaultView;
                        Resul.RowFilter = "detalle_Id=" + detalle["idDetalle"].ToString() + "";
                        DataTable Planes = Resul.ToTable("UniqueLastNames", true, "detallesAdicionales_Id");
                        if (Planes.Rows.Count > 0)
                        {
                            DataView Resul2 = InformacionPorGenerar.Tables[7].DefaultView;
                            Resul2.RowFilter = "detallesAdicionales_Id=" + Planes.Rows[0]["detallesAdicionales_Id"].ToString() + "";
                            DataTable Planes2 = Resul2.ToTable("UniqueLastNames", true, "nombre", "valor", "detallesAdicionales_Id");
                            detalle["detAdicionalNombre"] = Planes2.Rows[0]["nombre"].ToString();
                            detalle["detAdicionalValor"] = Planes2.Rows[0]["valor"].ToString();
                        }
                    }
                }
                #endregion

                #endregion

                #region Datos Adicionales
                #region Proceso 1
                DataTable dtAdicional = new DataTable();
                dtAdicional.Columns.Add("idAdicional");
                dtAdicional.Columns.Add("NombreAdic");
                dtAdicional.Columns.Add("DescripcionAdic");
                #endregion

                #region Proceso 2
                if (InformacionPorGenerar.Tables.Contains("campoAdicional"))
                {
                    if (InformacionPorGenerar.Tables["campoAdicional"].Rows.Count > 0)
                    {
                        foreach (DataRow datos in InformacionPorGenerar.Tables["campoAdicional"].Rows)
                        {
                            DataRow row3 = dtAdicional.NewRow();
                            row3["idAdicional"] = datos["infoAdicional_Id"].ToString();
                            row3["NombreAdic"] = datos["nombre"].ToString();
                            row3["DescripcionAdic"] = datos["campoAdicional_Text"].ToString();
                            dtAdicional.Rows.Add(row3);
                        }
                    }
                }
                #endregion
                #endregion

                dt.TableName = "GenrEncabezadoGuiaRemision";
                GenerarProceso.Tables.Add(dt);
                dtDetalle.TableName = "GenrDetalleGuiaRemision";
                GenerarProceso.Tables.Add(dtDetalle);
                dtAdicional.TableName = "GenrInfoAdicionalGuiaRemision";
                GenerarProceso.Tables.Add(dtAdicional);
            }
            catch (Exception ex)
            {
                MensajeError = ex.Message.ToString();
            }
            return GenerarProceso;
        }
        #endregion

        #region GenerarCodigoBarra
        public Image codigo128(string _code)
        {
            Image bmT = default(Image);

            if (_code == "")
            { }
            else
            {
                try
                {
                    Barcode128 bcode = new Barcode128();
                    bcode.BarHeight = 50;
                    bcode.Code = _code;
                    bcode.GenerateChecksum = true;
                    bcode.CodeType = Barcode.CODE128;

                    Bitmap bm = new Bitmap(bcode.CreateDrawingImage(Color.Black, Color.White));
                    bmT = new Bitmap(bm.Width, bm.Height + 14);
                    Graphics g = Graphics.FromImage(bmT);
                    g.FillRectangle(new SolidBrush(Color.White), 0, 0, bm.Width + 150, bm.Height + 14);

                    Font pintarTexto = new Font("Arial", 8);
                    SolidBrush brocha = new SolidBrush(Color.Black);

                    SizeF stringSize = new SizeF();
                    stringSize = g.MeasureString(_code, pintarTexto);
                    float centrox = (bm.Width - stringSize.Width) / 2;
                    float x = centrox;
                    float y = bm.Height;

                    StringFormat drawformat = new StringFormat();
                    drawformat.FormatFlags = StringFormatFlags.NoWrap;
                    g.DrawImage(bm, 0, 0);

                    //string ncode = _code.Substring(1, _code.Length - 2);
                    g.DrawString(_code, pintarTexto, brocha, x, y, drawformat);
                    //PictureBox1.Image = bmT;
                    //string strPath = AppDomain.CurrentDomain.BaseDirectory;
                    //bmT.Save(strPath + "Imagenes/bc.png");
                }
                catch (Exception ex)
                {

                }
            }
            return bmT;
        }
        #endregion

        #region Convertir_Imagen_Bytes
        public static byte[] Convertir_Imagen_Bytes(Image img)
        {
            string sTemp = Path.GetTempFileName();
            FileStream fs = new FileStream(sTemp, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            img.Save(fs, System.Drawing.Imaging.ImageFormat.Png);
            fs.Position = 0;

            int imgLength = Convert.ToInt32(fs.Length);
            byte[] bytes = new byte[imgLength];
            fs.Read(bytes, 0, imgLength);
            fs.Close();
            return bytes;
        }
        #endregion

        #region GenerarDocumento
        public byte[] GenerarDocumento(DataSet Data, string RutaRide, string TipoDocumento, ref string mensaje, ref string rutaPdf ,ref string Archivo)
        {

            byte[] buffer2 = null;
            string mimeType = null;
            string encoding = null;
            string fileNameExtension = null;
            string[] streams = null;
            Warning[] war = null;
            ReportViewer viewer = new ReportViewer();
            try
            {
                if (TipoDocumento == "06")
                {
                    ReportDataSource rdsEncabezado = new ReportDataSource("GenrEncabezadoGuiaRemision", Data.Tables["GenrEncabezadoGuiaRemision"]);
                    viewer.LocalReport.DataSources.Add(rdsEncabezado);
                    ReportDataSource rdsDetalle = new ReportDataSource("GenrDetalleGuiaRemision", Data.Tables["GenrDetalleGuiaRemision"]);
                    viewer.LocalReport.DataSources.Add(rdsDetalle);
                    ReportDataSource rdsAdicionales = new ReportDataSource("GenrInfoAdicionalGuiaRemision", Data.Tables["GenrInfoAdicionalGuiaRemision"]);
                    viewer.LocalReport.DataSources.Add(rdsAdicionales);
                    viewer.LocalReport.ReportPath = RutaRide;
                    viewer.RefreshReport();
                    string folderPath = "";
                    folderPath = HttpContext.Current.Server.MapPath("~/descargas/");
                    buffer2 = viewer.LocalReport.Render("PDF", null, out mimeType, out encoding, out fileNameExtension, out streams, out war);
                    FileStream fs = new FileStream(folderPath + "/" + Data.Tables["GenrEncabezadoGuiaRemision"].Rows[0]["claveAcceso"].ToString() + "@.pdf", FileMode.Create);
                    rutaPdf = folderPath + Data.Tables["GenrEncabezadoGuiaRemision"].Rows[0]["claveAcceso"].ToString() + "@.pdf";
                    Archivo = Data.Tables["GenrEncabezadoGuiaRemision"].Rows[0]["claveAcceso"].ToString() + "@.pdf";
                    fs.Write(buffer2, 0, buffer2.Length);
                    fs.Close();
                    viewer.Dispose();
                }
            }
            catch (Exception ex)
            {
                mensaje = "Generar 2: " + ex.Message.ToString() + "  :  " + ex.InnerException + "  :  " + ex.StackTrace;

            }
            return buffer2;
        }
        #endregion

        #region VerErrores
        public void VerErrores(string valor, string Carpeta, string rucEmpresa)
        {
            try
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
            catch (Exception ex)
            {
                System.Diagnostics.EventLog.WriteEntry("Application", "Exception: " + ex.Message);
            }
        }
        #endregion

    }
}