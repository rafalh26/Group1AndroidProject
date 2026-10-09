using Npgsql;
using ProjektGrupowy.Models;
using System.Security.Cryptography;
namespace ProjektGrupowy.Services
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

                const string query = "UPDATE \"Users\"" +
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

        public async Task<User?> GetUserByNickAsync(string nick)
        {
            if (string.IsNullOrWhiteSpace(nick)) return null;

            try
            {
                await using var conn = new NpgsqlConnection(OperationParameters.ConnectionString);
                await conn.OpenAsync();

                const string q = "SELECT id, nick, pass_hash, name, email, phone, address, geo_latitude, geo_longitude, created_at FROM \"users\" WHERE nick = @nick LIMIT 1";
                await using var cmd = new NpgsqlCommand(q, conn);
                cmd.Parameters.AddWithValue("@nick", nick);
                await using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var u = new User
                    {
                        id = reader["id"] != DBNull.Value ? Convert.ToInt32(reader["id"]) : 0,
                        nick = reader["nick"]?.ToString() ?? string.Empty,
                        pass_hash = reader["pass_hash"]?.ToString() ?? string.Empty,
                        name = reader["name"]?.ToString(),
                        email = reader["email"]?.ToString(),
                        phone = reader["phone"]?.ToString(),
                        address = reader["address"]?.ToString(),
                    };
                    return u;
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching user by nick '{nick}': {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Authentication and Track/GeoLog Helpers

        public static string ComputeHash(string input)
        {
            using var sha = SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(input);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public async Task<string?> GetPassHashAsync(string nick)
        {
            if (string.IsNullOrWhiteSpace(nick))
                return null;

            try
            {
                await using var sqlConnection = new NpgsqlConnection(OperationParameters.ConnectionString);
                await sqlConnection.OpenAsync();

                const string query = "SELECT pass_hash FROM \"users\" WHERE nick = @nick LIMIT 1";
                await using var command = new NpgsqlCommand(query, sqlConnection);
                command.Parameters.AddWithValue("@nick", nick);

                var result = await command.ExecuteScalarAsync();
                return result is DBNull or null ? null : result.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting pass hash for '{nick}': {ex.Message}");
                return null;
            }
        }

        public async Task RegisterNewUserAsync(User user, string password)
        {
            var hash = ComputeHash(password ?? string.Empty);


            if (string.IsNullOrWhiteSpace(user.nick))
                throw new ArgumentException("Nick cannot be null or empty.", nameof(user.nick));


            try
            {
                await using var sqlConnection = new NpgsqlConnection(OperationParameters.ConnectionString);
                await sqlConnection.OpenAsync();

                // Use UPSERT: insert if not exists, otherwise update fields
                const string query = @"INSERT INTO ""users"" (nick, name, pass_hash, email, phone, address, created_at)
                                       VALUES (@nick, @name,@pass_hash, @email, @phone, @address, now())
                                       ON CONFLICT (nick) DO UPDATE
                                       SET name = EXCLUDED.name,
                                           email = EXCLUDED.email,
                                           pass_hash = EXCLUDED.pass_hash
                                       RETURNING id;";

                await using var command = new NpgsqlCommand(query, sqlConnection);
                command.Parameters.AddWithValue("@nick", user.nick);
                command.Parameters.AddWithValue("@name", user.name ?? string.Empty);
                command.Parameters.AddWithValue("@email", user.email ?? string.Empty);
                command.Parameters.AddWithValue("@phone", user.phone ?? string.Empty);
                command.Parameters.AddWithValue("@address", user.address ?? string.Empty);
                command.Parameters.AddWithValue("@pass_hash", hash);

                var result = await command.ExecuteScalarAsync();
                if (result != null)
                {
                    Console.WriteLine($"User '{user.nick}' registered/updated with id={result}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error registering user '{user.nick}': {ex.Message}");
                throw;
            }
        }

        public async Task<int?> CreateTrackAsync(string nick)
        {
            if (string.IsNullOrWhiteSpace(nick))
                return null;

            try
            {
                await using var sqlConnection = new NpgsqlConnection(OperationParameters.ConnectionString);
                await sqlConnection.OpenAsync();

                const string query = "INSERT INTO \"Tracks\" (nick, trackStart) VALUES (@nick, @start) RETURNING id;";
                await using var command = new NpgsqlCommand(query, sqlConnection);
                command.Parameters.AddWithValue("@nick", nick);
                command.Parameters.AddWithValue("@start", DateTime.UtcNow);

                var result = await command.ExecuteScalarAsync();
                if (result is int id) return id;
                if (result is long l) return (int)l;
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating track for '{nick}': {ex.Message}");
                return null;
            }
        }

        public async Task StopTrackAsync(int trackId)
        {
            try
            {
                await using var sqlConnection = new NpgsqlConnection(OperationParameters.ConnectionString);
                await sqlConnection.OpenAsync();

                const string query = "UPDATE \"Tracks\" SET trackStop=@stop WHERE id=@id;";
                await using var command = new NpgsqlCommand(query, sqlConnection);
                command.Parameters.AddWithValue("@stop", DateTime.UtcNow);
                command.Parameters.AddWithValue("@id", trackId);
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping track {trackId}: {ex.Message}");
            }
        }

        public async Task InsertGeoLogAsync(int trackId, double latitude, double longitude, DateTime timestamp)
        {
            try
            {
                await using var sqlConnection = new NpgsqlConnection(OperationParameters.ConnectionString);
                await sqlConnection.OpenAsync();

                const string query = "INSERT INTO \"GeoLogs\" (track_id, geo_latitude, geo_longitude, locationDate) VALUES (@tid, @lat, @lon, @date);";
                await using var command = new NpgsqlCommand(query, sqlConnection);
                command.Parameters.AddWithValue("@tid", trackId);
                command.Parameters.AddWithValue("@lat", latitude);
                command.Parameters.AddWithValue("@lon", longitude);
                command.Parameters.AddWithValue("@date", timestamp);
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting geolog for track {trackId}: {ex.Message}");
                throw; // let caller handle queueing
            }
        }

        public async Task DeleteUserRecordsByNickAsync(string nick)
        {
            if (string.IsNullOrWhiteSpace(nick)) return;

            try
            {
                await using var sqlConnection = new NpgsqlConnection(OperationParameters.ConnectionString);
                await sqlConnection.OpenAsync();

                // Attempt to delete geologs and tracks related to the nick, then the contact
                const string delGeo = "DELETE FROM \"GeoLogs\" WHERE track_id IN (SELECT id FROM \"Tracks\" WHERE nick=@nick);";
                const string delTracks = "DELETE FROM \"Tracks\" WHERE nick=@nick;";
                const string delContact = "DELETE FROM \"users\" WHERE nick=@nick;";

                await using (var cmd = new NpgsqlCommand(delGeo, sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@nick", nick);
                    await cmd.ExecuteNonQueryAsync();
                }

                await using (var cmd = new NpgsqlCommand(delTracks, sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@nick", nick);
                    await cmd.ExecuteNonQueryAsync();
                }

                await using (var cmd = new NpgsqlCommand(delContact, sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@nick", nick);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting records for nick '{nick}': {ex.Message}");
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

                    // Check whether the three core tables exist. We check for either the exact-cased name or the lowercase variant
                    string[] tables = new[] { "users", "Tracks", "GeoLogs" };
                    int existCount = 0;

                    foreach (var t in tables)
                    {
                        // Use a simpler existence check to avoid SQL syntax issues in some environments.
                        // Compare lower(table_name) to lower(@t) to handle case variations.
                        const string existsSql = "SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND lower(table_name) = lower(@t) LIMIT 1";

                        using var cmd = new NpgsqlCommand(existsSql, connection);
                        cmd.Parameters.AddWithValue("@t", t);
                        var res = cmd.ExecuteScalar();
                        if (res != null && res != DBNull.Value)
                            existCount++;
                    }

                    // If all three exist, we're done
                    if (existCount == tables.Length)
                    {
                        return true;
                    }

                    // Otherwise, remove any of these tables (both quoted and lowercase variants) and recreate all three fresh
                    var dropSql = @"
                        DROP TABLE IF EXISTS public.""GeoLogs"" CASCADE;
                        DROP TABLE IF EXISTS public.geologs CASCADE;
                        DROP TABLE IF EXISTS public.""Tracks"" CASCADE;
                        DROP TABLE IF EXISTS public.tracks CASCADE;
                        DROP TABLE IF EXISTS public.""users"" CASCADE;
                        DROP TABLE IF EXISTS public.users CASCADE;
                    ";

                    using (var dropCmd = new NpgsqlCommand(dropSql, connection))
                    {
                        dropCmd.ExecuteNonQuery();
                    }

                    // Create tables fresh
                    var createContacts = @"
                        CREATE TABLE IF NOT EXISTS ""users"" (
                            id SERIAL PRIMARY KEY,
                            nick VARCHAR NOT NULL UNIQUE,
                            pass_hash TEXT NOT NULL,
                            name VARCHAR,
                            email VARCHAR,
                            phone VARCHAR,
                            address TEXT,
                            geo_latitude DOUBLE PRECISION,
                            geo_longitude DOUBLE PRECISION,
                            created_at TIMESTAMP DEFAULT now()
                        );";

                    var createTracks = @"
                        CREATE TABLE IF NOT EXISTS ""Tracks"" (
                            id SERIAL PRIMARY KEY,
                            nick VARCHAR NOT NULL,
                            trackStart TIMESTAMP,
                            trackStop TIMESTAMP,
                            CONSTRAINT fk_tracks_users_nick FOREIGN KEY (nick) REFERENCES ""users""(nick) ON DELETE CASCADE
                        );";

                    var createGeoLogs = @"
                        CREATE TABLE IF NOT EXISTS ""GeoLogs"" (
                            id SERIAL PRIMARY KEY,
                            track_id INTEGER REFERENCES ""Tracks""(id) ON DELETE CASCADE,
                            geo_latitude DOUBLE PRECISION,
                            geo_longitude DOUBLE PRECISION,
                            locationDate TIMESTAMP
                        );";

                    var createAll = createContacts + createTracks + createGeoLogs;

                    using (var createCmd = new NpgsqlCommand(createAll, connection))
                    {
                        createCmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                initialConnection = false;
                OperationParameters.errorDisplayer = ex.Message;
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

                const string query = "INSERT INTO \"users\" (nick)\r\n" +
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
                const string checkQuery = "SELECT name FROM \"users\" WHERE nick = @nick LIMIT 1";

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
                UPDATE ""users""
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
