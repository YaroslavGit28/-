using System;
using System.IO;
using Microsoft.Data.Sqlite;
using FitnessClubApp.UI;

namespace FitnessClubApp
{
    class Program
    {
        private const string ConnectionString = "Data Source=fitnessclub.db";

        static void Main(string[] args)
        {
            try
            {
                Console.OutputEncoding = System.Text.Encoding.UTF8;
                InitializeDatabase();

                var menu = new ConsoleMenu(ConnectionString);
                menu.ShowMainMenu();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Критическая ошибка: {ex.Message}");
                Console.WriteLine("Нажмите любую клавишу для выхода...");
                Console.ReadKey();
            }
        }

        private static void InitializeDatabase()
        {
            // Создаем таблицы в базе данных, если они еще не существуют
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Members (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    FirstName TEXT NOT NULL,
                    LastName TEXT NOT NULL,
                    Email TEXT,
                    JoinDate TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS Memberships (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Duration INTEGER NOT NULL,
                    Price DECIMAL NOT NULL
                );

                CREATE TABLE IF NOT EXISTS MemberMemberships (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    MemberId INTEGER NOT NULL,
                    MembershipId INTEGER NOT NULL,
                    StartDate TEXT NOT NULL,
                    EndDate TEXT NOT NULL,
                    FOREIGN KEY (MemberId) REFERENCES Members(Id),
                    FOREIGN KEY (MembershipId) REFERENCES Memberships(Id)
                );";
            
            command.ExecuteNonQuery();
        }
    }
}