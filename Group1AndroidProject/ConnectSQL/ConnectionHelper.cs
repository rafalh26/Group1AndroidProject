using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Java.Nio.FileNio.Attributes;
using Npgsql;

namespace Group1AndroidProject.ConnectSQL
{
    internal class ConnectionHelper
    {

        public bool initialConnection = true;
        private const string ConnectionString = "User Id=postgres.jtjdvjrcxbdrdlgyqmzf;Password=testB@s3SQL123;Server=aws-0-eu-central-1.pooler.supabase.com;Port=6543;Database=postgres;";

        public bool CheckConnection()
        {
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    connection.Open();
                }
            }
            catch (Exception ex)
            {
                initialConnection = false;
                return false;
            }
            return true;
        }
        private void sendCustomQuery(string customQuery)
        {
        }
    }
}
