using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POSServices.Models.Data
{
    public class ArticleMigration
    {        
        public String name { get; set; }
        public String barcode { get; set; }
        public String tax { get; set; }
        public String groupId { get; set; }
        public int isSoldByWeight { get; set; }
        public decimal price { get; set; }
        public String IdCompany { get; set; }
    }
}