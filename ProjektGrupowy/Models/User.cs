namespace ProjektGrupowy.Models
{
    public class User
    {
        public int id { get; set; }
        public string nick { get; set; } = string.Empty;
        public string? name { get; set; }

        public string pass_hash { get; set; } = string.Empty;

        public string? email { get; set; }
        public string? phone { get; set; }
        public string? address { get; set; }
    }
}