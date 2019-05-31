using POSServices.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POSServices.Models.Request
{
    public class RequestCreateCompany
    {
        public String Name { get; set; }
        public String FiscalNumber { get; set; }
        public User user { get; set; }
    }
}