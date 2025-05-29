using System;
using System.Data;
using Microsoft.Data.Sqlite;

namespace FitnessClubApp.Data
{
    public abstract class BaseRepository
    {
        protected readonly string ConnectionString;

        protected BaseRepository(string connectionString)
        {
            ConnectionString = connectionString;
        }

        protected SqliteConnection CreateConnection()
        {
            try
            {
                var connection = new SqliteConnection(ConnectionString);
                connection.Open();
                return connection;
            }
            catch (SqliteException ex)
            {
                throw new DatabaseException("Ошибка при подключении к базе данных", ex);
            }
        }

        protected void ExecuteCommand(string sql, Action<SqliteCommand> configureCommand)
        {
            try
            {
                using var connection = CreateConnection();
                using var command = connection.CreateCommand();
                command.CommandText = sql;
                configureCommand(command);
                command.ExecuteNonQuery();
            }
            catch (SqliteException ex)
            {
                throw new DatabaseException("Ошибка при выполнении команды SQL", ex);
            }
        }

        protected T ExecuteScalar<T>(string sql, Action<SqliteCommand> configureCommand)
        {
            try
            {
                using var connection = CreateConnection();
                using var command = connection.CreateCommand();
                command.CommandText = sql;
                configureCommand(command);
                var result = command.ExecuteScalar();
                return result == DBNull.Value ? default! : (T)result;
            }
            catch (SqliteException ex)
            {
                throw new DatabaseException("Ошибка при выполнении запроса SQL", ex);
            }
            catch (InvalidCastException ex)
            {
                throw new DatabaseException("Ошибка при преобразовании типов данных", ex);
            }
        }
    }

    public class DatabaseException : Exception
    {
        public DatabaseException(string message) : base(message)
        {
        }

        public DatabaseException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}