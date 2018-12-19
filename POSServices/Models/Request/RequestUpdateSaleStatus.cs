using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POSServices.Models.Request
{
    public class RequestUpdateSaleStatus
    {
        public string IdClient { get; set; }
        public int IdStatus { get; set; }
        public string Document { get; set; }
        public string Barcode { get; set; }
    }
}