using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace FilesHelper
{
    [DataContract]
    internal class FileHelper
    {

        public string iconFile(string extensionFile)
        {
            string iconName = "";
            try
            {
                switch (extensionFile)
                {
                    case "pdf":
                        iconName = "fa-file-pdf-o";
                        break;
                    case "doc":
                        iconName = "fa-file-word-o";
                        break;
                    case "docx":
                        iconName = "fa-file-word-o";
                        break;
                    case "xls":
                        iconName = "fa-file-excel-o";
                        break;
                    case "xlsx":
                        iconName = "fa-file-excel-o";
                        break;
                    case "ppt":
                        iconName = "fa-file-powerpoint-o";
                        break;
                    case "pptx":
                        iconName = "fa-file-powerpoint-o";
                        break;
                    case "png":
                        iconName = "fa-file-image-o";
                        break;
                    case "jpg":
                        iconName = "fa-file-image-o";
                        break;
                    case "bmp":
                        iconName = "fa-file-image-o";
                        break;
                    case "zip":
                        iconName = "fa-file-zip-o";
                        break;
                    case "rar":
                        iconName = "fa-file-zip-o";
                        break;
                    case "7z":
                        iconName = "fa-file-zip-o";
                        break;
                    default:
                        iconName = "fa-file-text-o";
                        break;
                }

            }
            catch
            {
                iconName = "fa-file-text-o";
            }

            return iconName;

        }
    }
}