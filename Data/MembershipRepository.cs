using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using FitnessClub.Models;
using FitnessClub.Data.Exceptions;
using FitnessClub.Data.Interfaces;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

namespace FitnessClub.Data
{
    /// <summary>
    /// Репозиторий для работы с абонементами
    /// </summary>
    public class MembershipRepository : BaseRepository, IRepository<Membership>
    {
        /// <summary>
        /// Создает новый экземпляр репозитория абонементов
        /// </summary>
        /// <param name="connectionString">Строка подключения к БД</param>
        public MembershipRepository(string connectionString) : base(connectionString)
        {
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Membership>> GetAllAsync()
        {
            const string sql = @"
                SELECT MembershipId, Type, DurationDays, Price, Benefits
                FROM Memberships";

            var memberships = new List<Membership>();
            using var reader = await ExecuteReaderAsync(sql);
            
            while (await reader.ReadAsync())
            {
                memberships.Add(new Membership
                {
                    MembershipId = reader.GetInt32(0),
                    Type = reader.GetString(1),
                    DurationDays = reader.GetInt32(2),
                    Price = reader.GetDecimal(3),
                    Benefits = reader.IsDBNull(4) ? null : reader.GetString(4)
                });
            }

            return memberships;
        }

        /// <inheritdoc/>
        public async Task<Membership?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT MembershipId, Type, DurationDays, Price, Benefits
                FROM Memberships
                WHERE MembershipId = @MembershipId";

            using var reader = await ExecuteReaderAsync(sql, 
                new SqliteParameter("@MembershipId", id));

            if (await reader.ReadAsync())
            {
                return new Membership
                {
                    MembershipId = reader.GetInt32(0),
                    Type = reader.GetString(1),
                    DurationDays = reader.GetInt32(2),
                    Price = reader.GetDecimal(3),
                    Benefits = reader.IsDBNull(4) ? null : reader.GetString(4)
                };
            }

            return null;
        }

        /// <inheritdoc/>
        public async Task<int> AddAsync(Membership membership)
        {
            const string sql = @"
                INSERT INTO Memberships (Type, DurationDays, Price, Benefits)
                VALUES (@Type, @DurationDays, @Price, @Benefits);
                SELECT last_insert_rowid();";

            var parameters = new[]
            {
                new SqliteParameter("@Type", membership.Type),
                new SqliteParameter("@DurationDays", membership.DurationDays),
                new SqliteParameter("@Price", membership.Price),
                EnsureConnectionOpen();
                using var command = new SqlCommand(
                    "SELECT * FROM memberships WHERE is_active = 1", _connection, _transaction);
                
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    memberships.Add(MapMembership(reader));
                }
            }
            catch (SqlException ex)
            {
                throw new QueryException("Ошибка при получении списка абонементов", ex);
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Неожиданная ошибка при работе с базой данных", ex);
            }
            return memberships;
        }

        public int Create(Membership membership)
        {
            try
            {
                EnsureConnectionOpen();
                using var command = new SqlCommand(
                    @"INSERT INTO memberships (type, duration_days, price, description, access_level, is_active) 
                    VALUES (@Type, @DurationDays, @Price, @Description, @AccessLevel, @IsActive);
                    SELECT CAST(SCOPE_IDENTITY() as int)", _connection, _transaction);

                SetParameters(command, membership);
                
                return (int)command.ExecuteScalar()!;
            }
            catch (SqlException ex)
            {
                throw new QueryException("Ошибка при создании абонемента", ex);
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Неожиданная ошибка при работе с базой данных", ex);
            }
        }

        public void Update(Membership membership)
        {
            try
            {
                EnsureConnectionOpen();
                using var command = new SqlCommand(
                    @"UPDATE memberships SET 
                    type = @Type,
                    duration_days = @DurationDays,
                    price = @Price,
                    description = @Description,
                    access_level = @AccessLevel,
                    is_active = @IsActive
                    WHERE membership_id = @MembershipId", _connection, _transaction);

                command.Parameters.AddWithValue("@MembershipId", membership.MembershipId);
                SetParameters(command, membership);
                
                int rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected == 0)
                {
                    throw new EntityNotFoundException("Абонемент", membership.MembershipId);
                }
            }
            catch (SqlException ex)
            {
                throw new QueryException($"Ошибка при обновлении абонемента с ID {membership.MembershipId}", ex);
            }
            catch (EntityNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Неожиданная ошибка при работе с базой данных", ex);
            }
        }

        public void Delete(int id)
        {
            try
            {
                EnsureConnectionOpen();
                // Мягкое удаление - просто деактивируем абонемент
                using var command = new SqlCommand(
                    "UPDATE memberships SET is_active = 0 WHERE membership_id = @Id", _connection, _transaction);
                
                command.Parameters.AddWithValue("@Id", id);
                int rowsAffected = command.ExecuteNonQuery();
                
                if (rowsAffected == 0)
                {
                    throw new EntityNotFoundException("Абонемент", id);
                }
            }
            catch (SqlException ex)
            {
                throw new QueryException($"Ошибка при удалении абонемента с ID {id}", ex);
            }
            catch (EntityNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Неожиданная ошибка при работе с базой данных", ex);
            }
        }

        private void SetParameters(SqlCommand command, Membership membership)
        {
            command.Parameters.AddWithValue("@Type", membership.Type);
            command.Parameters.AddWithValue("@DurationDays", membership.DurationDays);
            command.Parameters.AddWithValue("@Price", membership.Price);
            command.Parameters.AddWithValue("@Description", membership.Description ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@AccessLevel", membership.AccessLevel);
            command.Parameters.AddWithValue("@IsActive", membership.IsActive);
        }

        private Membership MapMembership(SqlDataReader reader)
        {
            return new Membership
            {
                MembershipId = reader.GetInt32(reader.GetOrdinal("membership_id")),
                Type = reader.GetString(reader.GetOrdinal("type")),
                DurationDays = reader.GetInt32(reader.GetOrdinal("duration_days")),
                Price = reader.GetDecimal(reader.GetOrdinal("price")),
                Description = reader.IsDBNull(reader.GetOrdinal("description")) 
                    ? null 
                    : reader.GetString(reader.GetOrdinal("description")),
                AccessLevel = reader.GetInt32(reader.GetOrdinal("access_level")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("is_active"))
            };
        }

        // Дополнительные методы для работы с абонементами
        public List<Membership> GetActiveTypes()
        {
            var memberships = new List<Membership>();
            try
            {
                EnsureConnectionOpen();
                using var command = new SqlCommand(
                    "SELECT * FROM memberships WHERE is_active = 1 ORDER BY price", 
                    _connection, _transaction);
                
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    memberships.Add(MapMembership(reader));
                }
            }
            catch (SqlException ex)
            {
                throw new QueryException("Ошибка при получении списка активных абонементов", ex);
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Неожиданная ошибка при работе с базой данных", ex);
            }
            return memberships;
        }

        public bool IsTypeUnique(string type, int? excludeId = null)
        {
            try
            {
                EnsureConnectionOpen();
                string sql = "SELECT COUNT(*) FROM memberships WHERE type = @Type";
                if (excludeId.HasValue)
                {
                    sql += " AND membership_id != @ExcludeId";
                }

                using var command = new SqlCommand(sql, _connection, _transaction);
                command.Parameters.AddWithValue("@Type", type);
                if (excludeId.HasValue)
                {
                    command.Parameters.AddWithValue("@ExcludeId", excludeId.Value);
                }

                int count = (int)command.ExecuteScalar()!;
                return count == 0;
            }
            catch (SqlException ex)
            {
                throw new QueryException("Ошибка при проверке уникальности типа абонемента", ex);
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Неожиданная ошибка при работе с базой данных", ex);
            }
        }
    }
}  
