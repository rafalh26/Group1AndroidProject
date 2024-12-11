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

        public async Task GatherSourceDataAsync()
        {
            OperationParameters.gatheringDataCompleted = false;

            // Ensure contactsList is initialized
            if (OperationParameters.contactsList == null)
            {
                OperationParameters.contactsList = new List<Contact>();
            }

            try
            {
                if (string.IsNullOrEmpty(OperationParameters.currentUser))
                {
                    // If currentUser is invalid, log and return early
                    Console.WriteLine("Current user is not set or invalid.");
                    return;
                }

                await using var connection = new NpgsqlConnection(OperationParameters.ConnectionString);
                await connection.OpenAsync();

                const string query = @"SELECT nick, name, email, phone, address, geo_latitude, geo_longitude 
                               FROM ""Contacts"" 
                               WHERE nick NOT LIKE @currentUser 
                               LIMIT 100;";
                await using var command = new NpgsqlCommand(query, connection);

                // Add parameter to prevent SQL injection
                command.Parameters.AddWithValue("@currentUser", OperationParameters.currentUser);

                await using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    // Ensure "nick" is valid
                    var nick = reader["nick"]?.ToString();
                    if (string.IsNullOrWhiteSpace(nick))
                        continue;

                    // Create and populate the Contact object
                    var contact = new Contact
                    {
                        nick = nick,
                        name = reader["name"]?.ToString(),
                        email = reader["email"]?.ToString(),
                        phone = reader["phone"] != DBNull.Value ? Convert.ToInt32(reader["phone"]) : 0,
                        address = reader["address"]?.ToString(),
                        geo_latitude = reader["geo_latitude"] != DBNull.Value ? Convert.ToDouble(reader["geo_latitude"]) : 0,
                        geo_longitude = reader["geo_longitude"] != DBNull.Value ? Convert.ToDouble(reader["geo_longitude"]) : 0,
                    };

                    // Add the contact to the list
                    OperationParameters.contactsList.Add(contact);
                }
            }
            catch (Exception ex)
            {
                // Log the error and ensure the application continues running
                Console.WriteLine($"Error gathering source data: {ex.Message}");
            }
            finally
            {
                // Ensure gatheringDataCompleted is always set to true after the operation, even in case of errors
                OperationParameters.gatheringDataCompleted = true;
            }
        }



        #endregion
    }
}
