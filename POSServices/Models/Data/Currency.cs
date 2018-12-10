using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POSServices.Models.Data
{
    public class Currency
    {
        public String IdCurrency { get; set; }        
        public float Denomination { get; set; }
        public int Quantity { get; set; }
        public String Sign { get; set; }
    }
}