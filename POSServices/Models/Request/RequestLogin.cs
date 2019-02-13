using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POSServices.Models.Request
{
    public class RequestLogin
    {
        public String Email { get; set; }
        public String Password { get; set; }
    }
}