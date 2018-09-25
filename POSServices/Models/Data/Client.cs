using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POSServices.Models.Data
{
    public class Client
    {
        public string IdVirtualQueue { get; set; }
        public string IdClient { get; set; }
        public string Name { get; set; }
        public string IdentificationNumber { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string BirthDay { get; set; }
    }
}