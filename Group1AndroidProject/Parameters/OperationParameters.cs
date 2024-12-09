namespace Group1AndroidProject.Parameters
{
    public static class OperationParameters
    {
        public static string? currentUser { get; set; }
        public static List<ConnectSQL.Contact> contactsList = new List<ConnectSQL.Contact>();
        public static Location? MyCurrentLocation { get; set; }
        public static List<ConnectSQL.Contact> contactsListInRange = new List<ConnectSQL.Contact>();
    }
}
