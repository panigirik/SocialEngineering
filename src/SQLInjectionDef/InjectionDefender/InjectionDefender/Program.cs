using Microsoft.Data.Sqlite;

class Program
{
    static void Main()
    {
        Console.WriteLine("=== SQL Injection Protection Demo ===\n");

        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        InitializeDatabase(connection);

        Console.Write("Enter username: ");
        string username = Console.ReadLine();

        Console.Write("Enter password: ");
        string password = Console.ReadLine();

        Console.WriteLine("\n--- Vulnerable Login Attempt ---");
        VulnerableLogin(connection, username, password);

        Console.WriteLine("\n--- Secure Login Attempt ---");
        SecureLogin(connection, username, password);

        Console.WriteLine("\nProgram finished.");
    }

    static void InitializeDatabase(SqliteConnection connection)
    {
        var command = connection.CreateCommand();
        command.CommandText =
        @"
            CREATE TABLE Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Username TEXT NOT NULL,
                Password TEXT NOT NULL
            );

            INSERT INTO Users (Username, Password)
            VALUES ('admin', '1234'),
                   ('user', 'password');
        ";
        command.ExecuteNonQuery();
    }

    static void VulnerableLogin(SqliteConnection connection, string username, string password)
    {
        var command = connection.CreateCommand();
        command.CommandText =
            $"SELECT COUNT(*) FROM Users WHERE Username = '{username}' AND Password = '{password}'";

        try
        {
            long result = (long)command.ExecuteScalar();
            if (result > 0)
                Console.WriteLine("Login successful (VULNERABLE).");
            else
                Console.WriteLine("Login failed (VULNERABLE).");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error occurred: " + ex.Message);
        }
    }

    static void SecureLogin(SqliteConnection connection, string username, string password)
    {
        var command = connection.CreateCommand();
        command.CommandText =
            "SELECT COUNT(*) FROM Users WHERE Username = @username AND Password = @password";

        command.Parameters.AddWithValue("@username", username);
        command.Parameters.AddWithValue("@password", password);

        long result = (long)command.ExecuteScalar();
        if (result > 0)
            Console.WriteLine("Login successful (SECURE).");
        else
            Console.WriteLine("Login failed (SECURE).");
    }
}
