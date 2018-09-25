using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POSServices.Models.Response
{
    public class BasicResponse
    {
        public List<dynamic> data = new List<dynamic>();
        public bool error { get; set; }        
        public string description { get; set; }
    }
}