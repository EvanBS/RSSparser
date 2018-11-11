using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RSSref.Models
{
    public class MainResource
    {
        public int Id { get; set; }

        public string ResourceName { get; set; }

        public string URL { get; set; }

        public int? MainCollection_Id { get; set; }

        [ForeignKey("MainCollection_Id")]
        public virtual MainCollection MainCollection { get; set; }
    }
}