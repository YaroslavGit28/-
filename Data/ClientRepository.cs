using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using FitnessClub.Models;
using Microsoft.Data.Sqlite;
using FitnessClub.Data.Interfaces;

namespace FitnessClub.Data
{
    /// <summary>
    /// Репозиторий для работы с клиентами
    /// </summary>
    public class ClientRepository : BaseRepository, IRepository<Client>
    {
        /// <summary>
        /// Создает новый экземпляр репозитория клиентов
        /// </summary>
        /// <param name="connectionString">Строка подключения к БД</param>
        public ClientRepository(string connectionString) : base(connectionString)
        {
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Client>> GetAllAsync()
        {
            const string sql = @"
                SELECT ClientId, FirstName, LastName, JoinDate, MembershipId
                FROM Clients";

            var clients = new List<Client>();
            using var reader = await ExecuteReaderAsync(sql);
            
            while (await reader.ReadAsync())
            {
                clients.Add(new Client
                {
                    ClientId = reader.GetInt32(0),
                    FirstName = reader.GetString(1),
                    LastName = reader.GetString(2),
                    JoinDate = reader.GetDateTime(3),
                    MembershipId = reader.IsDBNull(4) ? null : reader.GetInt32(4)
                });
            }

            return clients;
        }

        /// <inheritdoc/>
        public async Task<Client?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT ClientId, FirstName, LastName, JoinDate, MembershipId
                FROM Clients
                WHERE ClientId = @ClientId";

            using var reader = await ExecuteReaderAsync(sql, 
                new SqliteParameter("@ClientId", id));

            if (await reader.ReadAsync())
            {
                return new Client
                {
                    ClientId = reader.GetInt32(0),
                    FirstName = reader.GetString(1),
                    LastName = reader.GetString(2),
                    JoinDate = reader.GetDateTime(3),
                    MembershipId = reader.IsDBNull(4) ? null : reader.GetInt32(4)
                };
            }

            return null;
        }

        /// <inheritdoc/>
        public async Task<int> AddAsync(Client client)
        {
            const string sql = @"
                INSERT INTO Clients (FirstName, LastName, JoinDate, MembershipId)
                VALUES (@FirstName, @LastName, @JoinDate, @MembershipId);
                SELECT last_insert_rowid();";

            var parameters = new[]
            {
                new SqliteParameter("@FirstName", client.FirstName),
                new SqliteParameter("@LastName", client.LastName),
                new SqliteParameter("@JoinDate", client.JoinDate),
                new SqliteParameter("@MembershipId", client.MembershipId ?? (object)DBNull.Value)
            };

            var result = await ExecuteScalarAsync(sql, parameters);
            return Convert.ToInt32(result);
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(Client client)
        {
            const string sql = @"
                UPDATE Clients
                SET FirstName = @FirstName,
                    LastName = @LastName,
                    JoinDate = @JoinDate,
                    MembershipId = @MembershipId
                WHERE ClientId = @ClientId";

            var parameters = new[]
            {
                new SqliteParameter("@ClientId", client.ClientId),
                new SqliteParameter("@FirstName", client.FirstName),
                new SqliteParameter("@LastName", client.LastName),
                new SqliteParameter("@JoinDate", client.JoinDate),
                new SqliteParameter("@MembershipId", client.MembershipId ?? (object)DBNull.Value)
            };

            await ExecuteNonQueryAsync(sql, parameters);
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(int id)
        {
            const string sql = "DELETE FROM Clients WHERE ClientId = @ClientId";
            await ExecuteNonQueryAsync(sql, new SqliteParameter("@ClientId", id));
        }

        public Client? GetById(int id)
        {
            try
            {
                EnsureConnectionOpen();
                using var command = new SqlCommand(
                    "SELECT c.*, m.Type as MembershipType, m.DurationDays, m.Price " +
                    "FROM clients c " +
                    "LEFT JOIN memberships m ON c.membership_id = m.membership_id " +
                    "WHERE c.client_id = @Id", _connection, _transaction);
                
                command.Parameters.AddWithValue("@Id", id);
                
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return MapClient(reader);
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting client by ID", ex);
            }
        }

        public List<Client> GetAll()
        {
            var clients = new List<Client>();
            try
            {
                EnsureConnectionOpen();
                using var command = new SqlCommand(
                    "SELECT c.*, m.Type as MembershipType, m.DurationDays, m.Price " +
                    "FROM clients c " +
                    "LEFT JOIN memberships m ON c.membership_id = m.membership_id", _connection, _transaction);
                
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    clients.Add(MapClient(reader));
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting all clients", ex);
            }
            return clients;
        }

        public int Create(Client client)
        {
            try
            {
                EnsureConnectionOpen();
                using var command = new SqlCommand(
                    "INSERT INTO clients (first_name, last_name, birth_date, phone, email, join_date, membership_id, health_info) " +
                    "VALUES (@FirstName, @LastName, @BirthDate, @Phone, @Email, @JoinDate, @MembershipId, @HealthInfo); " +
                    "SELECT CAST(SCOPE_IDENTITY() as int)", _connection, _transaction);

                SetParameters(command, client);
                
                return (int)command.ExecuteScalar()!;
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating client", ex);
            }
        }

        public void Update(Client client)
        {
            try
            {
                EnsureConnectionOpen();
                using var command = new SqlCommand(
                    "UPDATE clients SET " +
                    "first_name = @FirstName, " +
                    "last_name = @LastName, " +
                    "birth_date = @BirthDate, " +
                    "phone = @Phone, " +
                    "email = @Email, " +
                    "membership_id = @MembershipId, " +
                    "health_info = @HealthInfo " +
                    "WHERE client_id = @ClientId", _connection, _transaction);

                command.Parameters.AddWithValue("@ClientId", client.ClientId);
                SetParameters(command, client);
                
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating client", ex);
            }
        }

        public void Delete(int id)
        {
            try
            {
                EnsureConnectionOpen();
                using var command = new SqlCommand(
                    "DELETE FROM clients WHERE client_id = @Id", _connection, _transaction);
                
                command.Parameters.AddWithValue("@Id", id);
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting client", ex);
            }
        }

        private void SetParameters(SqlCommand command, Client client)
        {
            command.Parameters.AddWithValue("@FirstName", client.FirstName);
            command.Parameters.AddWithValue("@LastName", client.LastName);
            command.Parameters.AddWithValue("@BirthDate", client.BirthDate ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Phone", client.Phone);
            command.Parameters.AddWithValue("@Email", client.Email ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@JoinDate", client.JoinDate);
            command.Parameters.AddWithValue("@MembershipId", client.MembershipId);
            command.Parameters.AddWithValue("@HealthInfo", client.HealthInfo ?? (object)DBNull.Value);
        }

        private Client MapClient(SqlDataReader reader)
        {
            var client = new Client
            {
                ClientId = reader.GetInt32(reader.GetOrdinal("client_id")),
                FirstName = reader.GetString(reader.GetOrdinal("first_name")),
                LastName = reader.GetString(reader.GetOrdinal("last_name")),
                Phone = reader.GetString(reader.GetOrdinal("phone")),
                JoinDate = reader.GetDateTime(reader.GetOrdinal("join_date")),
                MembershipId = reader.GetInt32(reader.GetOrdinal("membership_id"))
            };

            if (!reader.IsDBNull(reader.GetOrdinal("birth_date")))
                client.BirthDate = reader.GetDateTime(reader.GetOrdinal("birth_date"));
            
            if (!reader.IsDBNull(reader.GetOrdinal("email")))
                client.Email = reader.GetString(reader.GetOrdinal("email"));
            
            if (!reader.IsDBNull(reader.GetOrdinal("health_info")))
                client.HealthInfo = reader.GetString(reader.GetOrdinal("health_info"));

            if (!reader.IsDBNull(reader.GetOrdinal("MembershipType")))
            {
                client.Membership = new Membership
                {
                    Type = reader.GetString(reader.GetOrdinal("MembershipType")),
                    DurationDays = reader.GetInt32(reader.GetOrdinal("DurationDays")),
                    Price = reader.GetDecimal(reader.GetOrdinal("Price"))
                };
            }

            return client;
        }
    }
}  
