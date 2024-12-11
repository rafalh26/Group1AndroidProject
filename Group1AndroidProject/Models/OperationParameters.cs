using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Group1AndroidProject.Models
{
    public static class OperationParameters
    {
        public static string ConnectionString { get; } = "User Id=postgres.jtjdvjrcxbdrdlgyqmzf;Password=testB@s3SQL123;Server=aws-0-eu-central-1.pooler.supabase.com;Port=6543;Database=postgres;";
        public static string? currentUser { get; set; }
        public static bool gatheringDataCompleted { get; set; }
        public static bool newUser { get; set; }
        public static List<Contact> contactsList = new List<Contact>();
        public static Location? MyCurrentLocation { get; set; }
        public static List<Contact> contactsInRange = new List<Contact>();
    }
}
