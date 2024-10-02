using System;
using System.Collections.Generic;
using System.IO;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using CapaEntidad;

namespace CapaNegocio
{
    public class TemplateExcelRead
    {
        public static List<EntEmpleado> LeerArchivoExcel(Stream stream, string nombreArchivo)
        {
            List<EntEmpleado> lista = new List<EntEmpleado>();

            IWorkbook workbook = null;

            // Detectar el formato del archivo Excel (xlsx o xls)
            if (Path.GetExtension(nombreArchivo) == ".xlsx")
            {
                workbook = new XSSFWorkbook(stream);
            }
            else
            {
                    // Utilizar HSSFWorkbook para archivos .xls
                    // workbook = new HSSFWorkbook(stream);
            }

            if (workbook != null)
            {
                ISheet sheet = workbook.GetSheetAt(0);

                // Iterar a través de las filas y crear objetos VMContacto
                 for (int i = 1; i <= sheet.LastRowNum; i++)
                 {
                     IRow row = sheet.GetRow(i);

                     if (row != null)
                     {
                         EntEmpleado contacto = new EntEmpleado
                         {
                             Nombre = GetCellValue(row.GetCell(1)),
                             Cedula = GetCellValue(row.GetCell(2)),
                             Telefono = GetCellValue(row.GetCell(3)),
                             Sociedad = GetCellValue(row.GetCell(4)),
                             Ciudad = GetCellValue(row.GetCell(5)),
                             AreaTrabajo = GetCellValue(row.GetCell(6)),
                             PuestoTrabajo = GetCellValue(row.GetCell(7)),
                             Direccion = GetCellValue(row.GetCell(8)),
                             Sexo = GetCellValue(row.GetCell(9)),
                             Fecha_Nacimiento = GetCellValue(row.GetCell(10)),
                             Provincia = GetCellValue(row.GetCell(11)),
                             Correo = GetCellValue(row.GetCell(12)),
                             EstadoCivil = GetCellValue(row.GetCell(13))    
                         };

                            lista.Add(contacto);
                     }
                 }
            }
            

            return lista;
        }


        private static string GetCellValue(ICell cell)
        {
            return cell != null ? cell.ToString() : string.Empty;
        }
    }
}
