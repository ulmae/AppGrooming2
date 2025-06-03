using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppGrooming.Models
{
    public class ServiceModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? DurationMin { get; set; }
    }
}
