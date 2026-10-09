namespace ProjektGrupowy.Models
{
    public class Track
    {
        public int id { get; set; }

        public required string nick { get; set; }

        public DateTime trackStart { get; set; }

        public DateTime? trackStop { get; set; }
    }
}