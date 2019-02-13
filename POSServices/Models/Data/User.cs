using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POSServices.Models.Data
{
    public class User
    {
        public string id { get; set; }
        public string name { get; set; }
        public string identificationNumber { get; set; }
        public string token { get; set; }
        public List<Fingerprint> fingerprint = new List<Fingerprint>();
    }
}