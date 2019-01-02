using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POSServices.Models.Data
{
    public class MovementType
    {
        public string IdUserMovementType { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; }
    }
}