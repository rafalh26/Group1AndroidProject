using System;

namespace ProjektGrupowy.WebApplication.Models
{
    public class GeoLog
    {
        public int id { get; set; }
        public int track_id { get; set; }
        public double geo_latitude { get; set; }
        public double geo_longitude { get; set; }
        public DateTime locationDate { get; set; }
    }
}
