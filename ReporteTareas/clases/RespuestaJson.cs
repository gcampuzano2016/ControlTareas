using System.Runtime.Serialization;

namespace JSONHelper
{
    [DataContract]
    internal class RespuestaJson
    {
        [DataMember]
        public string Item { get; set; }
        [DataMember]
        public string Valor { get; set; }
    }
}