using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POSServices.Models.Request
{
    public class RequestUpdateVirtualQueueStatus
    {
        public string AreaId { get; set; }
        public string StatusId { get; set; }
        public string IdVirtualQueue { get; set; }
    }
}