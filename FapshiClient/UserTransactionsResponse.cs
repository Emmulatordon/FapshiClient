using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FapshiClient
{
    public class UserTransactionsResponse
    {
        public List<UserTransaction> Transactions { get; set; } = new();
        public string? Message { get; set; }
        public int StatusCode { get; set; }
    }
}
