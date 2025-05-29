using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using FitnessClubApp.Models;

namespace FitnessClubApp.Data
{
    public class MemberRepository : BaseRepository, IRepository<Member>
    {
        public MemberRepository(string connectionString) : base(connectionString)
        {
        }

        public IEnumerable<Member> GetAll()
        {
            var members = new List<Member>();

            try
            {
                using var connection = CreateConnection();
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Members";

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    members.Add(MapFromReader(reader));
                }
            }
            catch (SqliteException ex)
            {
                throw new DatabaseException("Ошибка при получении списка членов клуба", ex);
            }

            return members;
        }

        public Member? GetById(int id)
        {
            try
            {
                using var connection = CreateConnection();
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Members WHERE Id = @Id";
                command.Parameters.AddWithValue("@Id", id);

                using var reader = command.ExecuteReader();
                return reader.Read() ? MapFromReader(reader) : null;
            }
            catch (SqliteException ex)
            {
                throw new DatabaseException($"Ошибка при получении члена клуба с ID {id}", ex);
            }
        }

        public void Add(Member member)
        {
            const string sql = @"
                INSERT INTO Members (FirstName, LastName, Email, JoinDate)
                VALUES (@FirstName, @LastName, @Email, @JoinDate)";

            ExecuteCommand(sql, command =>
            {
                command.Parameters.AddWithValue("@FirstName", member.FirstName);
                command.Parameters.AddWithValue("@LastName", member.LastName);
                command.Parameters.AddWithValue("@Email", member.Email ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@JoinDate", member.JoinDate.ToString("yyyy-MM-dd"));
            });
        }

        public void Update(Member member)
        {
            const string sql = @"
                UPDATE Members 
                SET FirstName = @FirstName,
                    LastName = @LastName,
                    Email = @Email
                WHERE Id = @Id";

            ExecuteCommand(sql, command =>
            {
                command.Parameters.AddWithValue("@Id", member.Id);
                command.Parameters.AddWithValue("@FirstName", member.FirstName);
                command.Parameters.AddWithValue("@LastName", member.LastName);
                command.Parameters.AddWithValue("@Email", member.Email ?? (object)DBNull.Value);
            });
        }

        public void Delete(int id)
        {
            // Проверяем наличие активных абонементов
            const string checkSql = @"
                SELECT COUNT(*) FROM MemberMemberships 
                WHERE MemberId = @Id AND EndDate >= date('now')";

            var activeCount = ExecuteScalar<long>(checkSql, command =>
            {
                command.Parameters.AddWithValue("@Id", id);
            });

            if (activeCount > 0)
            {
                throw new InvalidOperationException("Нельзя удалить члена клуба с активным абонементом");
            }

            const string sql = "DELETE FROM Members WHERE Id = @Id";
            ExecuteCommand(sql, command =>
            {
                command.Parameters.AddWithValue("@Id", id);
            });
        }

        private static Member MapFromReader(SqliteDataReader reader)
        {
            return new Member
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email")),
                JoinDate = DateTime.Parse(reader.GetString(reader.GetOrdinal("JoinDate")))
            };
        }
    }
} 