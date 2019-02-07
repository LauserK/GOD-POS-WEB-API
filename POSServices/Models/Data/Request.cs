using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POSServices.Models.Data
{
    public class Request
    {
        public string IdRequest { get; set; }
        public string IdRequestType { get; set; }
        public string IdUser { get; set; }
        public string RequestDate { get; set; }
        public string DeliverDate { get; set; }
        public string ApproximateDeliverDate { get; set; }
        public string IdRequestStatus { get; set; }
        public RequestStatus RequestStatus { get; set; }
        public RequestType RequestType { get; set; }
    }
}