using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Group1AndroidProject;
using Npgsql;
namespace Group1AndroidProject.Models
{
    public class ConnectionHelper
    {
        public bool initialConnection = true;






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
        public bool IsTheUserNew()
        {

            //string MyUser = OperationParameters.currentUser;
            bool resultFromSQL = false;
            using (NpgsqlConnection sqlConnection = new NpgsqlConnection(OperationParameters.ConnectionString))
            {
                try
                {
                    sqlConnection.Open();

                    // Query to check if the user exists
                    string checkQuery = $"SELECT name FROM \"Contacts\" WHERE nick = @nick limit 1";

                    using (NpgsqlCommand checkCommand = new NpgsqlCommand(checkQuery, sqlConnection))
                    {
                        // Add parameter to prevent SQL injection
                        checkCommand.Parameters.AddWithValue("@nick", OperationParameters.currentUser);

                        // Execute the query and get the result
                        var result = checkCommand.ExecuteScalar();

                        // Check if the result is DBNull or null
                        if (result == null || result == DBNull.Value)
                        {
                            // User is new (doesn't exist in the database)
                            resultFromSQL = true;
                        }
                        else
                        {
                            // User already exists
                            resultFromSQL = false;
                        }
                    }
                }
                catch (Exception)
                {
                    // Handle exceptions appropriately (e.g., logging)
                }
            }
            return resultFromSQL;
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
        #endregion    
    }
}
