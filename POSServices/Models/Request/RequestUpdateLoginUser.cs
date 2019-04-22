using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POSServices.Models.Request
{
    public class RequestUpdateLoginUser
    {
        public String IdUser { get; set; }
        public String IdDevice { get; set; }
        public String Fingerprint { get; set; }
        public String Password { get; set; }
    }
}