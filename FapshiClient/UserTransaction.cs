using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FapshiClient
{
    public class UserTransaction
    {
        public string TransId { get; set; } = string.Empty;
        public int Amount { get; set; }
        public PaymentStatusEnum Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
