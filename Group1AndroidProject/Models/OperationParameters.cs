using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Group1AndroidProject.Models
{
    public static class OperationParameters
    {
        public static string ConnectionString { get; } = "DefaultConnection"
            ?? throw new InvalidOperationException("Database connection string not configured.");
        public static string? currentUser { get; set; }
        public static bool gatheringDataCompleted { get; set; }
        public static bool newUser { get; set; }
        public static List<Contact> contactsList = new List<Contact>();
        public static Location? MyCurrentLocation { get; set; }
        public static List<Contact> contactsInRange = new List<Contact>();
        public static Contact contactDetails { get; set; } = new Contact { nick = "" };

    }
}
