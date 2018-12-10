using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POSServices.Models.Data
{
    public class ArticleSale
    {
        public int id { get; set; }
        public string name { get; set; }
        public string barcode { get; set; }
        public decimal IVA { get; set; }
        public decimal netPrice { get; set; }
        public decimal price { get; set; }
        public string photo { get; set; }
        public decimal unity { get; set; }
        public decimal tax { get; set; }
        public string lineSaleId { get; set; }
        public string Idtax { get; set; }
        public Client client { get; set; }
    }
}