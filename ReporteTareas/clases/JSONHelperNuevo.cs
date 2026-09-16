using Newtonsoft.Json;
using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace JSONHelperNuevo
{
    public static class JsonSerializer
    {
        public static string SerializaToJson2(this object objeto)
        {
            if (objeto == null)
                return string.Empty;

            try
            {
                return JsonConvert.SerializeObject(objeto,
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        Formatting = Formatting.None
                    });
            }
            catch
            {
                throw;
            }
        }

        public static T DeserializarJsonTo2<T>(this string jsonSerializado)
        {
            try
            {
                T obj = Activator.CreateInstance<T>();
                using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonSerializado))) // Encoding.UTF8
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
                    obj = (T)serializer.ReadObject(ms);
                }
                return obj;
            }
            catch
            {
                return default(T);
            }
        }
    }
}

//namespace JSONHelperNuevo
//{
//    public static class JsonSerializer
//    {
//        /// <summary>
//        /// Método extensor para serializar a JSON cualquier objeto
//        /// </summary>
//        public static string SerializaToJson2(this object objeto) // Renombrado a SerializaToJson2
//        {
//            string jsonResult = string.Empty;

//            if (objeto == null)
//            {
//                return jsonResult;
//            }
//            try
//            {
//                DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(objeto.GetType());
//                using (MemoryStream ms = new MemoryStream())
//                {
//                    jsonSerializer.WriteObject(ms, objeto);
//                    jsonResult = Encoding.UTF8.GetString(ms.ToArray()); // Encoding.UTF8 para soportar caracteres especiales
//                }
//            }
//            catch
//            {
//                throw;
//            }
//            return jsonResult;
//        }

//        /// <summary>
//        /// Método extensor para deserializar JSON a un tipo específico
//        /// </summary>
//        public static T DeserializarJsonTo2<T>(this string jsonSerializado)
//        {
//            try
//            {
//                T obj = Activator.CreateInstance<T>();
//                using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonSerializado))) // Encoding.UTF8
//                {
//                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
//                    obj = (T)serializer.ReadObject(ms);
//                }
//                return obj;
//            }
//            catch
//            {
//                return default(T);
//            }
//        }
//    }
//}
