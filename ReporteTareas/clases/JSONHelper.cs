using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;



namespace JSONHelper
{
    public static class JsonSerializer
    {
        /// <summary>
        /// Método extensor para serializar JSON cualquier objeto
        /// </summary>

        public static string SerializaToJson(this object objeto)
        {
            string jsonResult = string.Empty;

            if (objeto == null)
            {
                return jsonResult;
            }
            try
            {
                DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(objeto.GetType());
                MemoryStream ms = new MemoryStream();
                jsonSerializer.WriteObject(ms, objeto);
                jsonResult = Encoding.Default.GetString(ms.ToArray());
            }
            catch { throw; }
            return jsonResult;
        }

        public static T DeserializarJsonTo<T>(this string jsonSerializado)
        {
            try
            {
                T obj = Activator.CreateInstance<T>();
                MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(jsonSerializado));
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
                obj = (T)serializer.ReadObject(ms);
                ms.Close();
                ms.Dispose();
                return obj;
            }
            catch { return default(T); }
        }
    }
}