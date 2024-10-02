using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.RefAndLookup;
using OfficeOpenXml.Table;
using System.Data;
using System.IO;
using OfficeOpenXml.Drawing;
using System.Drawing;


namespace CapaNegocio
{
    public class TemplateExcel
    {
        /*public static void FillReport(string filename, string templatefilename, DataSet data)
        {
            FillReport(filename, templatefilename, data, new string[] { "%", "%" });
        }*/

        public static void FillReport(string filename, string templatefilename, string nomHoja, DataSet data, string[] deliminator, string ruta1, string ruta2, string nombrePaciente)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Establece el contexto de licencia según tus necesidades

            // Obtiene la ruta completa del archivo de plantilla desde la raíz del proyecto
            string templateFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, templatefilename);


            // Obtiene el nombre del archivo de salida
            string outputFileName = Path.GetFileName(filename);

            // Obtiene la ruta completa de la carpeta "HistoriasClinicas" dentro del proyecto
            //string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HistoriasClinicas");


            // Directorio base donde se almacenan las historias clínicas
            string historiasClinicasFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HistoriasClinicas");

            // Verifica si la carpeta del paciente ya existe
            string folderPath = Path.Combine(historiasClinicasFolder, nombrePaciente);



            // Combina la carpeta con el nombre del archivo de salida para obtener la ruta completa del archivo de salida
            string fullFilePath = Path.Combine(folderPath, outputFileName);

            if (File.Exists(fullFilePath))
                File.Delete(fullFilePath);

            // Crea la carpeta "HistoriasClinicas" si no existe
            Directory.CreateDirectory(folderPath);

            using (var file = new FileStream(fullFilePath, FileMode.CreateNew))
            {
                using (var temp = new FileStream(templateFilePath, FileMode.Open))
                {
                    using (var xls = new ExcelPackage(file, temp))
                    {
                        foreach (var n in xls.Workbook.Names)
                        {
                            FillWorksheetData(data, n.Worksheet, n, deliminator);
                        }

                        foreach (var ws in xls.Workbook.Worksheets)
                        {
                            foreach (var n in ws.Names)
                            {
                                FillWorksheetData(data, ws, n, deliminator);
                            }
                        }

                        foreach (var ws in xls.Workbook.Worksheets)
                        {
                            foreach (var c in ws.Cells)
                            {
                                var s = "" + c.Value;
                                if (s.StartsWith(deliminator[0]) == false &&
                                    s.EndsWith(deliminator[1]) == false)
                                    continue;
                                s = s.Replace(deliminator[0], "").Replace(deliminator[1], "");
                                var ss = s.Split('.');
                                try
                                {
                                    c.Value = data.Tables[ss[0]].Rows[0][ss[1]];
                                }
                                catch { }
                            }
                        }

                        InsertImageFromTag(xls, nomHoja, ruta1, "{info.imagen}", ruta2, "{info.imagenProfesional}");


                        xls.Save();
                    }
                }
            }
        }

        //Limpia la etiqueta para la imagen despues de ingresarla
        private static void ClearCellContent(ExcelWorksheet worksheet, int row, int col)
        {
            var cell = worksheet.Cells[row, col];
            cell.Value = "";
        }

        // Insertar una imagen en una hoja de trabajo específica por su nombre
        private static void InsertImageFromTag(ExcelPackage excelPackage, string worksheetName, string imagePath, string tag, string imagePath2, string tag2)
        {
            ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets[worksheetName];

            foreach (var cell in worksheet.Cells)
            {
                if (cell.Text == tag)
                {
                    var fileInfo = new FileInfo(imagePath);
                    //var image = Image.FromFile(imagePath);
                    var picture = worksheet.Drawings.AddPicture("ImageName", fileInfo);

                    // Configurar la posición y el tamaño de la imagen
                    picture.SetPosition(cell.Start.Row - 1, 0, cell.Start.Column - 1, 0);
                    picture.SetSize(50, 50); // Puedes ajustar el tamaño según tus necesidades

                    // Borra el contenido de la celda con la etiqueta
                    ClearCellContent(worksheet, cell.Start.Row, cell.Start.Column);

                    break; // Termina después de encontrar la primera etiqueta
                }

                if (cell.Text == tag2)
                {
                    var fileInfo2 = new FileInfo(imagePath2);
                    var picture2 = worksheet.Drawings.AddPicture("ImageName2", fileInfo2);

                    // Configurar la posición y el tamaño de la segunda imagen
                    picture2.SetPosition(cell.Start.Row - 1, 0, cell.Start.Column - 1, 0);
                    picture2.SetSize(50, 50); // Puedes ajustar el tamaño según tus necesidades

                    // Borra el contenido de la celda con la segunda etiqueta
                    ClearCellContent(worksheet, cell.Start.Row, cell.Start.Column);
                }
            }
        }





        private static void FillWorksheetData(DataSet data, ExcelWorksheet ws, ExcelNamedRange n, string[] deliminator)
        {
            if (data.Tables.Contains(n.Name) == false)
                return;

            var dt = data.Tables[n.Name];

            int row = n.Start.Row;

            var cn = new string[n.Columns];
            var st = new int[n.Columns];
            for (int i = 0; i < n.Columns; i++)
            {
                cn[i] = (n.Value as object[,])[0, i].ToString().Replace(deliminator[0], "").Replace(deliminator[1], "");
                if (cn[i].Contains("."))
                    cn[i] = cn[i].Split('.')[1];
                st[i] = ws.Cells[row, n.Start.Column + i].StyleID;
            }

            foreach (DataRow r in dt.Rows)
            {
                for (int col = 0; col < n.Columns; col++)
                {
                    if (dt.Columns.Contains(cn[col]))
                        ws.Cells[row, n.Start.Column + col].Value = r[cn[col]];
                    ws.Cells[row, n.Start.Column + col].StyleID = st[col];
                }
                row++;
            }

            // extend table formatting range to all rows
            foreach (var t in ws.Tables)
            {
                var a = t.Address;
                if (n.Start.Row >= a.Start.Row && n.Start.Row <= a.End.Row &&
                    n.Start.Column >= a.Start.Column && n.Start.Column <= a.End.Column)
                {
                    ExtendRows(t, dt.Rows.Count - 1);
                }
            }
        }
        public static void ExtendRows(ExcelTable excelTable, int count)
        {
            var ad = new ExcelAddress(excelTable.Address.Start.Row,
                                      excelTable.Address.Start.Column,
                                      excelTable.Address.End.Row + count,
                                      excelTable.Address.End.Column);
        }
    }

    public static class int_between
    {
        public static bool Between(this int v, int a, int b)
        {
            return v >= a && v <= b;
        }
    }
}