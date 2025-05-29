using FitnessClub.Business.Interfaces;
using FitnessClub.Business.Services;
using FitnessClub.Data;
using FitnessClub.Data.Interfaces;
using FitnessClub.Models;
using FitnessClub.UI;
using Microsoft.Data.Sqlite;
using System.Reflection;

namespace FitnessClub;

class Program
{
    static async Task Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        var connectionString = CreateDatabase();
        var services = ConfigureServices(connectionString);
        var ui = new ConsoleUI(services.clientService, services.membershipService);

        await ui.RunAsync();
    }

    private static string CreateDatabase()
    {
        var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fitness.db");
        var connectionString = $"Data Source={dbPath}";

        if (!File.Exists(dbPath))
        {
            // Создаем новую базу данных
            SqliteConnection.Create(connectionString).Dispose();

            // Читаем SQL-скрипт создания базы данных
            var assembly = Assembly.GetExecutingAssembly();
            var scriptPath = "Database.create_database.sql";
            
            using var stream = assembly.GetManifestResourceStream(scriptPath);
            if (stream == null)
            {
                // Если скрипт не найден в ресурсах, пытаемся прочитать его из файла
                var sqlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", "create_database.sql");
                if (!File.Exists(sqlPath))
                {
                    throw new FileNotFoundException("SQL-скрипт создания базы данных не найден");
                }

                // Выполняем скрипт создания базы данных
                using var connection = new SqliteConnection(connectionString);
                connection.Open();
                var sql = File.ReadAllText(sqlPath);
                using var command = new SqliteCommand(sql, connection);
                command.ExecuteNonQuery();
            }
            else
            {
                // Выполняем скрипт из ресурсов
                using var reader = new StreamReader(stream);
                var sql = reader.ReadToEnd();
                using var connection = new SqliteConnection(connectionString);
                connection.Open();
                using var command = new SqliteCommand(sql, connection);
                command.ExecuteNonQuery();
            }
        }

        return connectionString;
    }

    private static (IClientService clientService, IMembershipService membershipService) ConfigureServices(string connectionString)
    {
        // Создаем репозитории
        IRepository<Client> clientRepository = new ClientRepository(connectionString);
        IRepository<Membership> membershipRepository = new MembershipRepository(connectionString);

        // Создаем сервисы
        IClientService clientService = new ClientService(clientRepository, membershipRepository);
        IMembershipService membershipService = new MembershipService(membershipRepository);

        return (clientService, membershipService);
    }
} 