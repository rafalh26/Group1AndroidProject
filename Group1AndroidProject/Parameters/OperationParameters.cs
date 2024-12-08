using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Group1AndroidProject.ConnectSQL;

namespace Group1AndroidProject.Parameters
{
    public static class OperationParameters
    {
        public static string? currentUser { get; set; }
        public static List<ConnectSQL.Contact> contactsList = new List<ConnectSQL.Contact>();
    }
}
