using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Group1AndroidProject.Models
{
    public class Contact
    {
        public int id { get; set; }
        public required string nick { get; set; }
        public string? name { get; set; }
        public string? email { get; set; }
        public int? phone { get; set; }
        public string? address { get; set; }
        //public DateTime created_at { get; set; }
        public double geo_latitude { get; set; }
        public double geo_longitude { get; set; }
        public int distanceFromCurrentUserinMeters { get; set; }
    }
}
