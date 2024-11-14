using System;
using System.Collections.Generic;
using System.IO;
using OfficeOpenXml;
//using System.Web.Script.Serialization;
using Newtonsoft.Json;

namespace CapaNegocio
{
    public class EditExcel
    {
        // Método que recibe el nombre del archivo, nombre de la hoja, y el diccionario de campos con las celdas correspondientes
        public static string ObtenerDatosFormulario(string nomCarpeta, string nombre, string nombreArchivo, string nombreHoja, Dictionary<string, string> campos)
        {
            string jsonData = string.Empty; // Inicializa la variable jsonData

            try
            {
                // Establece el contexto de licencia para EPPlus
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                // Construye la ruta completa, incluyendo la carpeta principal, la subcarpeta 'nombre' y el archivo
                string templateFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, nomCarpeta, nombre, nombreArchivo);

                // Verificar si el archivo existe antes de continuar
                if (!File.Exists(templateFilePath))
                {
                    throw new FileNotFoundException($"El archivo {nombreArchivo} no fue encontrado en la carpeta {Path.Combine(nomCarpeta, nombre)}.");
                }

                // Abre el archivo Excel
                using (var package = new ExcelPackage(new FileInfo(templateFilePath)))
                {
                    // Obtiene la hoja de trabajo específica
                    var sheet = package.Workbook.Worksheets[nombreHoja];
                    if (sheet == null)
                    {
                        throw new Exception("La hoja especificada no existe en el archivo Excel.");
                    }

                    // Llama al método GetDatos para obtener los datos de las celdas específicas
                    var datos = GetDatos(sheet, campos);

                    // Serializa los datos a JSON usando Newtonsoft.Json
                    jsonData = JsonConvert.SerializeObject(datos); // Serialización con Newtonsoft.Json
                }
            }
            catch (Exception ex)
            {
                // Si ocurre algún error, devuelve los detalles en formato JSON
                jsonData = $"{{ \"error\": \"Error al procesar el archivo Excel.\", \"details\": \"{ex.Message}\", \"stackTrace\": \"{ex.StackTrace}\" }}";
            }

            return jsonData; // Retorna los datos serializados
        }

        // Método para obtener los valores de las celdas dinámicamente según los campos solicitados
        private static Dictionary<string, string> GetDatos(ExcelWorksheet sheet, Dictionary<string, string> campos)
        {
            var datos = new Dictionary<string, string>();

            // Recorre el diccionario de campos y obtiene los valores de las celdas correspondientes
            foreach (var campo in campos)
            {
                string celda = campo.Value; // Obtiene la referencia de la celda
                string valor = sheet.Cells[celda].Text; // Obtiene el valor de la celda
                datos.Add(campo.Key, valor); // Añade el campo y su valor al diccionario
            }

            return datos;
        }
    }
}