using Npgsql;

namespace Group1AndroidProject.ConnectSQL
{
    internal class ConnectionHelper : IDisposable
    {

        public bool initialConnection = true;
        private const string ConnectionString = "User Id=postgres.jtjdvjrcxbdrdlgyqmzf;Password=testB@s3SQL123;Server=aws-0-eu-central-1.pooler.supabase.com;Port=6543;Database=postgres;";
        Contact contact;
        private bool disposedValue;
        #region IDisposable Implementation
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~ConnectionHelper()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion


        #region MainPage Initialization
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
        public string sendEnterQuery(string userInput_Nick, Page currentPage)
        {
            string nick = userInput_Nick;

            using (NpgsqlConnection sqlConnection = new NpgsqlConnection(ConnectionString))
            {
                try
                {
                    sqlConnection.Open();

                    // First, check if the nick already exists
                    string checkQuery = "INSERT INTO \"Contacts\" (nick)\r\n" +
                        "VALUES (@nick)\r\n" +
                        "ON CONFLICT (nick) DO NOTHING" +
                        "\r\nRETURNING nick;\r\n";

                    using (NpgsqlCommand checkCommand = new NpgsqlCommand(checkQuery, sqlConnection))
                    {
                        // Add parameter to avoid SQL injection
                        checkCommand.Parameters.AddWithValue("@nick", nick);

                        // Execute the command and get the result
                        var result = checkCommand.ExecuteScalar();

                        // Check if the result is not null, meaning the 'nick' was returned (either inserted or already exists)
                        if (result != null)
                        {
                            // The nick value is returned (either newly inserted or already existed)
                            nick = (string)result;
                        }
                        else
                        {
                            // Handle case where no value is returned (i.e., the 'nick' already exists)
                            // You can decide what to do if the nick doesn't exist or isn't inserted
                            nick = userInput_Nick; // Or handle accordingly
                        }
                    }
                }
                catch (Exception ex)
                {
                    currentPage.DisplayAlert("Warning", "User nick most likely already exist!", "Ok");
                }
            }
            return nick;
        }
        #endregion
        //private List<Contact> SendCustomQuery(string customQuery, Page currentPage)
        //{
        //    List<Contact> contacts = new List<Contact>();

        //    using (NpgsqlConnection sqlConnection = new NpgsqlConnection(ConnectionString))
        //    {
        //        try
        //        {
        //            sqlConnection.Open();

        //            using (NpgsqlCommand sqlCommand = new NpgsqlCommand(customQuery, sqlConnection))
        //            {
        //                using (NpgsqlDataReader reader = sqlCommand.ExecuteReader())
        //                {
        //                    while (reader.Read())
        //                    {
        //                        Contact contact = new Contact
        //                        {
        //                            id = reader.GetInt32(reader.GetOrdinal("id")), // Adjust column name
        //                            name = reader.GetString(reader.GetOrdinal("name")), // Adjust column name
        //                            email = reader.GetString(reader.GetOrdinal("email")), // Adjust column name
        //                            phone = reader.GetOrdinal("phone")
        //                        };
        //                        contacts.Add(contact);
        //                    }
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            currentPage.DisplayAlert("Warning", ex.Message, "Ok");
        //        }
        //    }

        //    return contacts;
        //}





        //private List<Contact> SendCustomQuery(string customQuery, Page currentPage)
        //{
        //    List<Contact> contacts = new List<Contact>();

        //    using (NpgsqlConnection sqlConnection = new NpgsqlConnection(ConnectionString))
        //    {
        //        try
        //        {
        //            sqlConnection.Open();

        //            using (NpgsqlCommand sqlCommand = new NpgsqlCommand(customQuery, sqlConnection))
        //            {
        //                using (NpgsqlDataReader reader = sqlCommand.ExecuteReader())
        //                {
        //                    while (reader.Read())
        //                    {
        //                        Contact contact = new Contact
        //                        {
        //                            id = reader.GetInt32(reader.GetOrdinal("id")), // Adjust column name
        //                            name = reader.GetString(reader.GetOrdinal("name")), // Adjust column name
        //                            email = reader.GetString(reader.GetOrdinal("email")), // Adjust column name
        //                            phone = reader.GetOrdinal("phone")
        //                        };
        //                        contacts.Add(contact);
        //                    }
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            currentPage.DisplayAlert("Warning", ex.Message, "Ok");
        //        }
        //    }

        //    return contacts;
        //}
    }
}
