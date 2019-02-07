using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POSServices.Models.Request
{
    public class RequestSaveRequest
    {
        public String IdRequestType { get; set; }
        public String IdUser { get; set; }        
        public String IdRequestStatus { get; set; }
    }
}