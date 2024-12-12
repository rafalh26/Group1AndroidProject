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
            // Validate the current user parameter
            if (string.IsNullOrWhiteSpace(OperationParameters.currentUser))
                throw new ArgumentException("Current user nick cannot be null or empty.", nameof(OperationParameters.currentUser));

            try
            {
                await using var sqlConnection = new NpgsqlConnection(OperationParameters.ConnectionString);
                await sqlConnection.OpenAsync();

                // Query to check if the user exists
                const string checkQuery = "SELECT name FROM \"Contacts\" WHERE nick = @nick LIMIT 1";

                await using var checkCommand = new NpgsqlCommand(checkQuery, sqlConnection);
                // Add parameter to prevent SQL injection
                checkCommand.Parameters.AddWithValue("@nick", OperationParameters.currentUser);

                // Execute the query and check the result
                var result = await checkCommand.ExecuteScalarAsync();

                // Assign newUser based on query result
                OperationParameters.newUser = result is null or DBNull;
            }
            catch (Exception ex)
            {
                // Log the exception with additional user context
                Console.WriteLine($"Error checking if user '{OperationParameters.currentUser}' is new: {ex.Message}");
                throw; // Re-throw the exception after logging
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
            OperationParameters.gatheringDataCompleted = false;

            // Ensure contactsList is initialized
            if (OperationParameters.contactsList == null)
            {
                OperationParameters.contactsList = new List<Contact>();
            }

            int retryAttempts = 3;  // Maximum number of retries
            int delayBetweenRetries = 2000;  // Delay in milliseconds

            for (int attempt = 0; attempt < retryAttempts; attempt++)
            {
                try
                {
                    if (string.IsNullOrEmpty(OperationParameters.currentUser))
                    {
                        // If currentUser is invalid, log and return early
                        Console.WriteLine("Current user is not set or invalid.");
                        return;
                    }

                    // Log the currentUser to ensure it is correct
                    Console.WriteLine($"Current User: {OperationParameters.currentUser}");

                    // Create and open the connection
                    using var connection = new NpgsqlConnection(OperationParameters.ConnectionString);
                    connection.Open();

                    // Prepare the query with parameterized query
                    const string query = @"SELECT id, nick, name, email, phone, address, geo_latitude, geo_longitude 
                                   FROM ""Contacts"" 
                                   WHERE nick NOT LIKE @currentUser";

                    using var command = new NpgsqlCommand(query, connection);

                    // Add parameter to prevent SQL injection
                    command.Parameters.AddWithValue("@currentUser", OperationParameters.currentUser);

                    // Create the data adapter
                    using var adapter = new NpgsqlDataAdapter(command);
                    var dataTable = new DataTable();

                    // Fill the dataTable with the query results
                    adapter.Fill(dataTable);

                    // Check if there are rows returned
                    if (dataTable.Rows.Count == 0)
                    {
                        Console.WriteLine("No contacts found matching the criteria.");
                    }

                    // Process the rows
                    foreach (DataRow row in dataTable.Rows)
                    {
                        // Ensure "nick" is valid
                        var nick = row["nick"]?.ToString();
                        if (string.IsNullOrWhiteSpace(nick))
                            continue;

                        // Create and populate the Contact object
                        var contact = new Contact
                        {
                            id = row["id"] !=DBNull.Value ? Convert.ToInt32(row["id"]) : 0,
                            nick = nick,
                            name = row["name"]?.ToString(),
                            email = row["email"]?.ToString(),
                            phone = row["phone"] != DBNull.Value ? Convert.ToInt32(row["phone"]) : 0,
                            address = row["address"]?.ToString(),
                            geo_latitude = row["geo_latitude"] != DBNull.Value ? Convert.ToDouble(row["geo_latitude"]) : 0,
                            geo_longitude = row["geo_longitude"] != DBNull.Value ? Convert.ToDouble(row["geo_longitude"]) : 0,
                        };

                        // Add the contact to the list
                        OperationParameters.contactsList.Add(contact);
                    }

                    // If successful, break out of the retry loop
                    break;
                }
                catch (Exception ex)
                {
                    // Log the error and retry if there are attempts left
                    Console.WriteLine($"Error gathering source data (Attempt {attempt + 1}): {ex.Message}");

                    // If this is the last attempt, throw the exception
                    if (attempt == retryAttempts - 1)
                    {
                        throw;  // Rethrow the exception after all retry attempts fail
                    }

                    // Wait before retrying
                    System.Threading.Thread.Sleep(delayBetweenRetries);
                }
            }

            // Ensure gatheringDataCompleted is always set to true after the operation
            OperationParameters.gatheringDataCompleted = true;
        }


        #endregion
    }
}
