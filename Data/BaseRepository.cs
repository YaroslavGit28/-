using System;
using System.Configuration;
using Microsoft.Data.SqlClient;
using FitnessClub.Data.Exceptions;
using Microsoft.Data.Sqlite;

namespace FitnessClub.Data
{
    /// <summary>
    /// Базовый класс репозитория с общей функциональностью для работы с БД
    /// </summary>
    public abstract class BaseRepository : IDisposable
    {
        protected readonly SqlConnection _connection;
        protected SqlTransaction? _transaction;
        private readonly string _connectionString;

        /// <summary>
        /// Создает новый экземпляр базового репозитория
        /// </summary>
        /// <param name="connectionString">Строка подключения к БД</param>
        protected BaseRepository(string connectionString)
        {
            _connectionString = connectionString;
            try
            {
                var connection = new SqlConnection(connectionString);
                _connection = connection;
            }
            catch (ConfigurationErrorsException ex)
            {
                throw new DatabaseException("Ошибка при чтении конфигурации базы данных", ex);
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Ошибка при инициализации подключения к базе данных", ex);
            }
        }

        protected void EnsureConnectionOpen()
        {
            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                {
                    _connection.Open();
                }
            }
            catch (SqlException ex)
            {
                throw new ConnectionException("Не удалось открыть соединение с базой данных", ex);
            }
        }

        public void BeginTransaction()
        {
            try
            {
                EnsureConnectionOpen();
                _transaction = _connection.BeginTransaction();
            }
            catch (SqlException ex)
            {
                throw new DatabaseException("Ошибка при начале транзакции", ex);
            }
        }

        public void CommitTransaction()
        {
            try
            {
                _transaction?.Commit();
            }
            catch (SqlException ex)
            {
                throw new DatabaseException("Ошибка при подтверждении транзакции", ex);
            }
            finally
            {
                _transaction?.Dispose();
                _transaction = null;
            }
        }

        public void RollbackTransaction()
        {
            try
            {
                _transaction?.Rollback();
            }
            catch (SqlException ex)
            {
                throw new DatabaseException("Ошибка при откате транзакции", ex);
            }
            finally
            {
                _transaction?.Dispose();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            try
            {
                _transaction?.Dispose();
                if (_connection.State == System.Data.ConnectionState.Open)
                {
                    _connection.Close();
                }
                _connection.Dispose();
            }
            catch (SqlException ex)
            {
                throw new DatabaseException("Ошибка при освобождении ресурсов базы данных", ex);
            }
            GC.SuppressFinalize(this);
        }

        protected void ValidateTransaction()
        {
            if (_transaction == null)
            {
                throw new DatabaseException("Операция должна выполняться в транзакции");
            }
        }

        /// <summary>
        /// Создает и возвращает новое подключение к БД
        /// </summary>
        protected async Task<SqliteConnection> CreateConnectionAsync()
        {
            try
            {
                var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();
                return connection;
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Ошибка подключения к базе данных", ex);
            }
        }

        /// <summary>
        /// Выполняет SQL-команду и возвращает количество затронутых строк
        /// </summary>
        /// <param name="sql">SQL-запрос</param>
        /// <param name="parameters">Параметры запроса</param>
        protected async Task<int> ExecuteNonQueryAsync(string sql, params SqliteParameter[] parameters)
        {
            try
            {
                using var connection = await CreateConnectionAsync();
                using var command = connection.CreateCommand();
                command.CommandText = sql;
                command.Parameters.AddRange(parameters);
                return await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                throw new DatabaseException($"Ошибка выполнения запроса: {sql}", ex);
            }
        }

        /// <summary>
        /// Выполняет SQL-запрос и возвращает первый столбец первой строки результата
        /// </summary>
        /// <param name="sql">SQL-запрос</param>
        /// <param name="parameters">Параметры запроса</param>
        protected async Task<object?> ExecuteScalarAsync(string sql, params SqliteParameter[] parameters)
        {
            try
            {
                using var connection = await CreateConnectionAsync();
                using var command = connection.CreateCommand();
                command.CommandText = sql;
                command.Parameters.AddRange(parameters);
                return await command.ExecuteScalarAsync();
            }
            catch (Exception ex)
            {
                throw new DatabaseException($"Ошибка выполнения запроса: {sql}", ex);
            }
        }

        /// <summary>
        /// Выполняет SQL-запрос и возвращает SqliteDataReader для чтения результатов
        /// </summary>
        /// <param name="sql">SQL-запрос</param>
        /// <param name="parameters">Параметры запроса</param>
        protected async Task<SqliteDataReader> ExecuteReaderAsync(string sql, params SqliteParameter[] parameters)
        {
            try
            {
                var connection = await CreateConnectionAsync();
                var command = connection.CreateCommand();
                command.CommandText = sql;
                command.Parameters.AddRange(parameters);
                return await command.ExecuteReaderAsync(System.Data.CommandBehavior.CloseConnection);
            }
            catch (Exception ex)
            {
                throw new DatabaseException($"Ошибка выполнения запроса: {sql}", ex);
            }
        }
    }
}  
