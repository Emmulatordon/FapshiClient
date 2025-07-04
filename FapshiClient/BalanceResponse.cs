using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FapshiClient
{
    public class BalanceResponse
    {
        public decimal Balance { get; set; }
        public string? Message { get; set; }
        public int StatusCode { get; set; }
    }
}
