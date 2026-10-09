using ProjektGrupowy.Models;

namespace ProjektGrupowy.Services
{
    public static class OperationParameters
    {
        public static string ConnectionString { get; } = "Host=ep-autumn-surf-b16vpy2i-pooler.c-5.eu-central-1.aws.neon.tech;Database=neondb;Username=neondb_owner;Password=npg_ErmjwAP84gRH;SSL Mode=Require;Channel Binding=Require;";
        public static string? currentUser { get; set; }
        public static string? errorDisplayer { get; set; } = "";
        public static bool gatheringDataCompleted { get; set; }
        public static bool newUser { get; set; }
        public static Location? MyCurrentLocation { get; set; }
        public static User userInfo { get; set; } = new User
        {
            nick = "",
            pass_hash = ""
        };
    }
}
