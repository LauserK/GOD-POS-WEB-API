using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POSServices.Models.Data
{
    public class Sale
    {
        public string UserId { get; set; }
        public string DeviceId { get; set; }
        public Article article { get; set; }
        public string FiscalClientId { get; set; }
    }
}