using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POSServices.Models.Request
{
    public class RequestUpdateDeviceCompany
    {
        public String MacDevice { get; set; }
        public String IdCompany { get; set; }
    }
}