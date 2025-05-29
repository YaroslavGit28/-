using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using FitnessClubApp.Models;

namespace FitnessClubApp.Data
{
    public class MembershipRepository : BaseRepository, IRepository<Membership>
    {
        public MembershipRepository(string connectionString) : base(connectionString)
        {
        }

        public IEnumerable<Membership> GetAll()
        {
            var memberships = new List<Membership>();

            try
            {
                using var connection = CreateConnection();
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Memberships";

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    memberships.Add(MapFromReader(reader));
                }
            }
            catch (SqliteException ex)
            {
                throw new DatabaseException("Ошибка при получении списка абонементов", ex);
            }

            return memberships;
        }

        public Membership? GetById(int id)
        {
            try
            {
                using var connection = CreateConnection();
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Memberships WHERE Id = @Id";
                command.Parameters.AddWithValue("@Id", id);

                using var reader = command.ExecuteReader();
                return reader.Read() ? MapFromReader(reader) : null;
            }
            catch (SqliteException ex)
            {
                throw new DatabaseException($"Ошибка при получении абонемента с ID {id}", ex);
            }
        }

        public void Add(Membership membership)
        {
            const string sql = @"
                INSERT INTO Memberships (Name, Duration, Price)
                VALUES (@Name, @Duration, @Price)";

            ExecuteCommand(sql, command =>
            {
                command.Parameters.AddWithValue("@Name", membership.Name);
                command.Parameters.AddWithValue("@Duration", membership.Duration);
                command.Parameters.AddWithValue("@Price", membership.Price);
            });
        }

        public void Update(Membership membership)
        {
            const string sql = @"
                UPDATE Memberships 
                SET Name = @Name,
                    Duration = @Duration,
                    Price = @Price
                WHERE Id = @Id";

            ExecuteCommand(sql, command =>
            {
                command.Parameters.AddWithValue("@Id", membership.Id);
                command.Parameters.AddWithValue("@Name", membership.Name);
                command.Parameters.AddWithValue("@Duration", membership.Duration);
                command.Parameters.AddWithValue("@Price", membership.Price);
            });
        }

        public void Delete(int id)
        {
            // Проверяем наличие активных привязок
            const string checkSql = @"
                SELECT COUNT(*) FROM MemberMemberships 
                WHERE MembershipId = @Id AND EndDate >= date('now')";

            var activeCount = ExecuteScalar<long>(checkSql, command =>
            {
                command.Parameters.AddWithValue("@Id", id);
            });

            if (activeCount > 0)
            {
                throw new InvalidOperationException("Нельзя удалить абонемент с активными привязками");
            }

            const string sql = "DELETE FROM Memberships WHERE Id = @Id";
            ExecuteCommand(sql, command =>
            {
                command.Parameters.AddWithValue("@Id", id);
            });
        }

        private static Membership MapFromReader(SqliteDataReader reader)
        {
            return new Membership
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Duration = reader.GetInt32(reader.GetOrdinal("Duration")),
                Price = reader.GetDecimal(reader.GetOrdinal("Price"))
            };
        }
    }
} 