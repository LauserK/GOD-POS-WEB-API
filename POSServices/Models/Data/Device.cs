using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POSServices.Models.Data
{
    public class Device
    {
        public string IdDevice { get; set; }
        public string Name { get; set; }
        public string MAC { get; set; }     
        public string IdArea { get; set; }
    }
}