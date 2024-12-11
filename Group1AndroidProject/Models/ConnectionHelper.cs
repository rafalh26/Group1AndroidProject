using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Group1AndroidProject;
using Microsoft.Maui.Graphics.Text;
using Npgsql;
namespace Group1AndroidProject.Models
{
    public class ConnectionHelper
    {
        public bool initialConnection = true;



        #region Edit Contact SQL Functions

        public async Task UpdateCurrentContactInformation(string nameInput,string emailInput)
        {
            string? nick = OperationParameters.currentUser;

            if (string.IsNullOrWhiteSpace(nick))
                throw new ArgumentException("Nick cannot be null or empty.", nameof(nick));

            try
            {
                await using var sqlConnection = new NpgsqlConnection(OperationParameters.ConnectionString);
                await sqlConnection.OpenAsync();

                const string query = "UPDATE \"Contacts\"" +
                                     "SET name = @name,\r\n" +
                                     "email = @email" +
                                     "\r\nWHERE nick = @nick;\r\n";

                await using var command = new NpgsqlCommand(query, sqlConnection);
                command.Parameters.AddWithValue("@nick", nick);
                command.Parameters.AddWithValue("@name", nameInput);
                command.Parameters.AddWithValue("@email", emailInput);


                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                // Handle the exception (e.g., log it or notify the user)
                Console.WriteLine($"Error updating details: {ex.Message}");
            }
        }

        #endregion

        #region MainPage Initialization
        public bool CheckConnection()
        {
            try
            {
                using (var connection = new NpgsqlConnection(OperationParameters.ConnectionString))
                {
                    connection.Open();
                }
            }
            catch (Exception)
            {
                initialConnection = false;
                return false;
            }
            return true;
        }

        //Entry nick checkout
        public async Task SendEnterQueryAsync()
        {
            string? nick = OperationParameters.currentUser;

            if (string.IsNullOrWhiteSpace(nick))
                throw new ArgumentException("Nick cannot be null or empty.", nameof(nick));

            try
            {
                await using var sqlConnection = new NpgsqlConnection(OperationParameters.ConnectionString);
                await sqlConnection.OpenAsync();

            const string query = "INSERT INTO \"Contacts\" (nick)\r\n" +
                                 "VALUES (@nick)\r\n" +
                                 "ON CONFLICT (nick) DO NOTHING" +
                                 "\r\nRETURNING nick;\r\n";

                await using var command = new NpgsqlCommand(query, sqlConnection);
                command.Parameters.AddWithValue("@nick", nick);

                var result = await command.ExecuteScalarAsync();

                // If no value is returned, the nick already exists
                OperationParameters.currentUser = result as string ?? nick;
            }
            catch (Exception ex)
            {
                // Handle the exception (e.g., log it or notify the user)
                Console.WriteLine($"Error inserting nick: {ex.Message}");
            }
        }
        #endregion

        #region ContactsListInRangePageInitialization
        public async Task CheckIfUserIsNewAsync()
        {
            if (string.IsNullOrWhiteSpace(OperationParameters.currentUser))
                throw new ArgumentException("Current user nick cannot be null or empty.", nameof(OperationParameters.currentUser));

            try
            {
                await using var sqlConnection = new NpgsqlConnection(OperationParameters.ConnectionString);
                await sqlConnection.OpenAsync();

                // Query to check if the user exists
                const string checkQuery = @"SELECT name FROM ""Contacts"" WHERE nick = @nick LIMIT 1";

                await using var checkCommand = new NpgsqlCommand(checkQuery, sqlConnection);
                // Add parameter to prevent SQL injection
                checkCommand.Parameters.AddWithValue("@nick", OperationParameters.currentUser);

                // Execute the query and get the result
                var result = await checkCommand.ExecuteScalarAsync();

                // Update the static property to reflect whether the user is new
                OperationParameters.newUser = result == null || result == DBNull.Value;
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                Console.WriteLine($"Error checking if user is new: {ex.Message}");
                throw; // Optionally rethrow the exception
            }
        }
        public async Task SendMyCurrentLocationAsync()
        {
            var location = OperationParameters.MyCurrentLocation;
            var currentUser = OperationParameters.currentUser;

            if (location == null || string.IsNullOrWhiteSpace(currentUser))
            {
                // Handle cases where location or user is not properly initialized
                throw new InvalidOperationException("Location or current user is not set.");
            }

            string query = @"
                UPDATE ""Contacts""
                SET geo_latitude = @latitude, geo_longitude = @longitude
                WHERE nick = @nick";

            using (NpgsqlConnection sqlConnection = new NpgsqlConnection(OperationParameters.ConnectionString))
            {
                try
                {
                    await sqlConnection.OpenAsync();

                    using (NpgsqlCommand command = new NpgsqlCommand(query, sqlConnection))
                    {
                        // Add parameters to the query
                        command.Parameters.AddWithValue("@latitude", location.Latitude);
                        command.Parameters.AddWithValue("@longitude", location.Longitude);
                        command.Parameters.AddWithValue("@nick", currentUser);

                        // Execute the command asynchronously
                        int rowsAffected = await command.ExecuteNonQueryAsync();

                        if (rowsAffected == 0)
                        {
                            // Handle cases where no rows were updated (e.g., user doesn't exist)
                            throw new InvalidOperationException("No rows were updated. Ensure the user exists in the database.");
                        }
                    }
                }
                catch (Exception)
                {
                    // Handle exceptions (e.g., log the error or show an alert)
                    // await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
                    throw;
                }
            }
        }
        public void GatherSourceData()
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(OperationParameters.ConnectionString))
            {
                string query = $"select * from \"Contacts\" where nick is not like {OperationParameters.currentUser}";

                NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(query, connection);
                DataTable dataTable = new DataTable();

                adapter.Fill(dataTable); // Fill the DataTable with the query results

                // Convert rows in DataTable to Employee objects
                foreach (DataRow row in dataTable.Rows)
                {
                    // Ensure that "Nick" is not null or empty, as it is required
                    string? nick = row["nick"]?.ToString();

                    //if is not needed as at entry page its already validated so it can be left as extra validation but it can be skipped as well.
                    if (string.IsNullOrWhiteSpace(nick))
                    {
                        // Skip rows where Nick is missing or invalid
                        continue;
                    }

                    // Create and populate the Contact object
                    Contact contact = new Contact
                    {
                        id = row["id"] != DBNull.Value ? Convert.ToInt32(row["id"]) : 0,
                        nick = nick, // Required field, already validated
                        name = row["name"]?.ToString(),
                        email = row["email"]?.ToString(),
                        phone = row["phone"] != DBNull.Value ? Convert.ToInt32(row["phone"]) : 0,
                        address = row["address"]?.ToString(),
                        created_at = DateTime.TryParse(row["created_at"]?.ToString(), out var tempDate) ? tempDate : DateTime.MinValue,
                        geo_latitude = row["geo_latitude"] != DBNull.Value ? Convert.ToDouble(row["geo_latitude"]) : 0,
                        geo_longitude = row["geo_longitude"] != DBNull.Value ? Convert.ToDouble(row["geo_longitude"]) : 0,
                    };

                    // Add the contact to the list
                    OperationParameters.contactsList.Add(contact);
                }
            }
        }
        #endregion    
    }
}
