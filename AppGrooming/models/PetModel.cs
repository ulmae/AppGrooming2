using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppGrooming.Models
{
    public class PetModel
    {
        public string Id { get; set; }
        public string CustomerId { get; set; }
        public string Name { get; set; }
        public string Species { get; set; }
        public string Breed { get; set; }
        public decimal? WeightKg { get; set; }
    }
}