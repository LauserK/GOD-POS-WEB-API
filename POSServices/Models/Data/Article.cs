using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POSServices.Models.Data
{
    public class Article
    {
        public int id { get; set; }
        public string name { get; set; }
        public string barcode { get; set; }
        public decimal IVA { get; set; }
        public decimal netPrice { get; set; }
        public string price { get; set; }
        public string photo { get; set; }
        public decimal unity { get; set; }
    }
}