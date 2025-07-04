﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FapshiClient
{
    public class PaymentStatusResponse : FapshiResponse
    {
        public PaymentStatusEnum Status { get; set; }
        public int Amount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
