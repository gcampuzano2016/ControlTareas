using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;



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
                /* DataContractJsonSerializer escribe siempre UTF-8. Leerlo con
                   Encoding.Default (windows-1252) partia cada tilde o enie en dos
                   caracteres; eso solo se veia bien mientras los handlers
                   respondian en windows-1252, y dejo de verse el 21-09 al
                   pasarlos a UTF-8: el combo de usuarios salia con los nombres rotos. */
                jsonResult = Encoding.UTF8.GetString(ms.ToArray());
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