using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POSServices.Models.Data
{
    public class WorkingDay
    {
        public string IdWorkingDaySession { get; set; }
        public string Date { get; set; }        
        public User User { get; set; }
        public bool mustClose { get; set; }
    }
}