using POSServices.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POSServices.Models.Request
{
    public class RequestSaveSale
    {
        public string IdSale { get; set; }
        public float Total { get; set; }
        public float Received { get; set; }
        public float Change { get; set; }
        public List<Currency> Currencies = new List<Currency>();       
        public List<ElectronicPayment> Electronic = new List<ElectronicPayment>();       
    }
}