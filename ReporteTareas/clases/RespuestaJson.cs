using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

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