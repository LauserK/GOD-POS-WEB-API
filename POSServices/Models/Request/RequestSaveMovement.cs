using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POSServices.Models.Request
{
    public class RequestSaveMovement
    {
        public string movementTypeId { get; set; }
        public string userId { get; set; }
    }
}