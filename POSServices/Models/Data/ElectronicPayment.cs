using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POSServices.Models.Data
{
    public class ElectronicPayment
    {
        public String Code { get; set; }
        public float Amount { get; set; }
        public String Reference { get; set; }
        public ElectronicPaymentType Type { get; set; }
    }
}