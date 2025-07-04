using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FapshiClient
{
    public class InitiatePayResponse : FapshiResponse
    {
        public string Link { get; set; } = string.Empty;
    }
}
