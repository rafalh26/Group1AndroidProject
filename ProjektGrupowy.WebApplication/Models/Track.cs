using System;

namespace ProjektGrupowy.WebApplication.Models
{
    public class Track
    {
        public int id { get; set; }
        public string nick { get; set; } = string.Empty;
        public DateTime? trackStart { get; set; }
        public DateTime? trackStop { get; set; }
    }
}
