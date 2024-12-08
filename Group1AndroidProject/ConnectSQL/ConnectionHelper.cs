using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Java.Nio.FileNio.Attributes;
using Npgsql;
using Group1AndroidProject.ConnectSQL;
using Security;
using System.Data;

namespace Group1AndroidProject.ConnectSQL
{
    internal class ConnectionHelper
    {

        public bool initialConnection = true;
        private const string ConnectionString = "User Id=postgres.jtjdvjrcxbdrdlgyqmzf;Password=testB@s3SQL123;Server=aws-0-eu-central-1.pooler.supabase.com;Port=6543;Database=postgres;";
        Contact contact;

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
        private List<Contact> sendCustomQuery(string customQuery, Page currentPage)
        {
            List<Contact> contacts = new List<Contact>();

            using (NpgsqlConnection sqlConnection = new NpgsqlConnection(ConnectionString))
            {
                try
                {
                    sqlConnection.Open();

                    using (NpgsqlCommand sqlCommand = new NpgsqlCommand(customQuery, sqlConnection))
                    {
                        using (NpgsqlDataReader reader = sqlCommand.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Contact contact = new Contact
                                {
                                    id = reader.GetInt32(reader.GetOrdinal("id")), // Adjust column name
                                    name = reader.GetString(reader.GetOrdinal("name")), // Adjust column name
                                    email = reader.GetString(reader.GetOrdinal("email")), // Adjust column name
                                    phone = reader.GetOrdinal("phone")
                                };
                                contacts.Add(contact);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    currentPage.DisplayAlert("Warning", ex.Message, "Ok");
                }
            }

            return contacts;
        }
    }
}
