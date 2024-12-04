﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gateway.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public Guid PaymentUid { get; set; }
        public string Status { get; set; } = null!;
        public int Price { get; set; }
    }
}