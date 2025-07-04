using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FapshiClient
{
    public abstract class FapshiResponse
    {
        public string? TransId { get; set; }
        public string? Message { get; set; }
        public int StatusCode { get; set; }
    }
}
