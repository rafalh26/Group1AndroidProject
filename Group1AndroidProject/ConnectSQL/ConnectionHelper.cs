using Group1AndroidProject.Parameters;
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

        //Entry nick checkout
        public async Task<string> SendEnterQueryAsync(string userInput_Nick, Page currentPage)
        {
            string nick = userInput_Nick;

            using (NpgsqlConnection sqlConnection = new NpgsqlConnection(ConnectionString))
            {
                try
                {
                    await sqlConnection.OpenAsync();

                    // First, check if the nick already exists
                    string checkQuery = "INSERT INTO \"Contacts\" (nick)\r\n" +
                                        "VALUES (@nick)\r\n" +
                                        "ON CONFLICT (nick) DO NOTHING" +
                                        "\r\nRETURNING nick;\r\n";

                    using (NpgsqlCommand checkCommand = new NpgsqlCommand(checkQuery, sqlConnection))
                    {
                        // Add parameter to avoid SQL injection
                        checkCommand.Parameters.AddWithValue("@nick", nick);

                        // Execute the command and get the result asynchronously
                        var result = await checkCommand.ExecuteScalarAsync();

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
                    // Handle exception appropriately (e.g., log or show an alert)
                    // await currentPage.DisplayAlert("Warning", "User nick most likely already exists!", "Ok");
                }
            }

            return nick;
        }
        #endregion

        #region ContactsListInRangePageInitialization

        public async Task<bool> IsTheUserNewAsync()
        {
            using (NpgsqlConnection sqlConnection = new NpgsqlConnection(ConnectionString))
            {
                try
                {
                    await sqlConnection.OpenAsync();

                    // Query to check if the user exists
                    string checkQuery = $"SELECT Name FROM \"Contacts\" WHERE nick = @nick";

                    using (NpgsqlCommand checkCommand = new NpgsqlCommand(checkQuery, sqlConnection))
                    {
                        // Add parameter to prevent SQL injection
                        checkCommand.Parameters.AddWithValue("@nick", OperationParameters.currentUser);

                        // Execute the query and get the result
                        var result = await checkCommand.ExecuteScalarAsync();

                        // Check if the result is DBNull or null
                        if (result == null || result == DBNull.Value)
                        {
                            // User is new (doesn't exist in the database)
                            return true;
                        }
                        else
                        {
                            // User already exists
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle exceptions appropriately (e.g., logging)
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    throw; // Re-throw the exception or handle it as needed
                }
            }
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
