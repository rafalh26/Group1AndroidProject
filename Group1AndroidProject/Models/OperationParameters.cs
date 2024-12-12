using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Group1AndroidProject.Models
{
    public static class OperationParameters
    {
        public static string ConnectionString { get; } = "Host=ep-old-rain-a9f5bi8q.gwc.azure.neon.tech;Database=Group1DB;Username=Group1DB_owner;Password=qvJUEdkSIf54;SSL Mode=Require;Trust Server Certificate=true";
        public static string? currentUser { get; set; }
        public static bool gatheringDataCompleted { get; set; }
        public static bool newUser { get; set; }
        public static List<Contact> contactsList = new List<Contact>();
        public static Location? MyCurrentLocation { get; set; }
        public static List<Contact> contactsInRange = new List<Contact>();
    }
}
