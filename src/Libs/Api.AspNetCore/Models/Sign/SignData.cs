using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.AspNetCore.Models.Sign
{
    public class SignData
    {
        public string Xml { get; set; }
        public string SignedXml { get; set; }

        public string SerializeToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
