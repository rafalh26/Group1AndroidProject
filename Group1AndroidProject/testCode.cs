// See https://aka.ms/new-console-template for more information
using Npgsql;
using System.Data;
using System.Data.Common;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
Console.WriteLine("TEST\n");

myTest myTest = new myTest();
myTest.GatherSourceData();
myTest.OutputData();


public class myTest
{

    bool gatheringDataCompleted = false;
    List<Contact> contactsList = new List<Contact>();
    string currentUser = "EmmyW";

    public string connectionString = "Host=ep-old-rain-a9f5bi8q.gwc.azure.neon.tech;Database=Group1DB;Username=Group1DB_owner;Password=qvJUEdkSIf54;SSL Mode=Require;Trust Server Certificate=true";
    public void GatherSourceData()
    {
        gatheringDataCompleted = false;

        // Ensure contactsList is initialized
        if (contactsList == null)
        {
            contactsList = new List<Contact>();
        }

        int retryAttempts = 3;  // Maximum number of retries
        int delayBetweenRetries = 2000;  // Delay in milliseconds

        for (int attempt = 0; attempt < retryAttempts; attempt++)
        {
            try
            {
                if (string.IsNullOrEmpty(currentUser))
                {
                    // If currentUser is invalid, log and return early
                    Console.WriteLine("Current user is not set or invalid.");
                    return;
                }

                // Log the currentUser to ensure it is correct
                Console.WriteLine($"Current User: {currentUser}");

                // Create and open the connection
                using var connection = new NpgsqlConnection(connectionString);
                connection.Open();

                // Prepare the query with parameterized query
                const string query = @"SELECT id, nick, name, email, phone, address, geo_latitude, geo_longitude 
                                   FROM ""Contacts"" 
                                   WHERE nick NOT LIKE @currentUser";

                using var command = new NpgsqlCommand(query, connection);

                // Add parameter to prevent SQL injection
                command.Parameters.AddWithValue("@currentUser", currentUser);

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
                        id = row["id"] != DBNull.Value ? Convert.ToInt32(row["id"]) : 0,
                        nick = nick,
                        name = row["name"]?.ToString(),
                        email = row["email"]?.ToString(),
                        phone = row["phone"] != DBNull.Value ? Convert.ToInt32(row["phone"]) : 0,
                        address = row["address"]?.ToString(),
                        geo_latitude = row["geo_latitude"] != DBNull.Value ? Convert.ToDouble(row["geo_latitude"]) : 0,
                        geo_longitude = row["geo_longitude"] != DBNull.Value ? Convert.ToDouble(row["geo_longitude"]) : 0,
                    };

                    // Add the contact to the list
                    contactsList.Add(contact);
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
        gatheringDataCompleted = true;
    }

    public void OutputData()
    {
        foreach (var contact in contactsList)
        {
            Console.WriteLine("ID: " + contact.id);
            Console.WriteLine("Name: " + contact.name);
            Console.WriteLine("Nick: " + contact.nick);
            Console.WriteLine("Email: " + contact.email);
            Console.WriteLine("Phone: " + contact.phone);
            Console.WriteLine("Address: " + contact.address);
            Console.WriteLine("GeoLatitude: " + contact.geo_latitude);
            Console.WriteLine("GeoLongitude: " + contact.geo_longitude);
            Console.WriteLine(new string('-', 30)); // Separator
        }

        Console.WriteLine($"current user: {currentUser}, bool: {gatheringDataCompleted}");

    }
}
public class Contact
{
    public int id { get; set; }
    public required string nick { get; set; }
    public string? name { get; set; }
    public string? email { get; set; }
    public int? phone { get; set; }
    public string? address { get; set; }
    //public DateTime created_at { get; set; }
    public double geo_latitude { get; set; }
    public double geo_longitude { get; set; }
}
