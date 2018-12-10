using POSServices.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POSServices.Models.Request
{
    public class RequestSaveMigrationArticle
    {
        public List<ArticleMigration> articles { get; set; }
    }
}