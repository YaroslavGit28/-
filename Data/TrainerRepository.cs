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
    /// Репозиторий для работы с тренерами
    /// </summary>
    public class TrainerRepository : BaseRepository, IRepository<Trainer>
    {
        /// <summary>
        /// Создает новый экземпляр репозитория тренеров
        /// </summary>
        /// <param name="connectionString">Строка подключения к БД</param>
        public TrainerRepository(string connectionString) : base(connectionString)
        {
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Trainer>> GetAllAsync()
        {
            const string sql = @"
                SELECT TrainerId, FirstName, LastName, Specialization
                FROM Trainers";

            var trainers = new List<Trainer>();
            using var reader = await ExecuteReaderAsync(sql);
            
            while (await reader.ReadAsync())
            {
                trainers.Add(new Trainer
                {
                    TrainerId = reader.GetInt32(0),
                    FirstName = reader.GetString(1),
                    LastName = reader.GetString(2),
                    Specialization = reader.IsDBNull(3) ? null : reader.GetString(3)
                });
            }

            return trainers;
        }

        /// <inheritdoc/>
        public async Task<Trainer?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT TrainerId, FirstName, LastName, Specialization
                FROM Trainers
                WHERE TrainerId = @TrainerId";

            using var reader = await ExecuteReaderAsync(sql, 
                new SqliteParameter("@TrainerId", id));

            if (await reader.ReadAsync())
            {
                return new Trainer
                {
                    TrainerId = reader.GetInt32(0),
                    FirstName = reader.GetString(1),
                    LastName = reader.GetString(2),
                    Specialization = reader.IsDBNull(3) ? null : reader.GetString(3)
                };
            }

            return null;
        }

        /// <inheritdoc/>
        public async Task<int> AddAsync(Trainer trainer)
        {
            const string sql = @"
                INSERT INTO Trainers (FirstName, LastName, Specialization)
                VALUES (@FirstName, @LastName, @Specialization);
                SELECT last_insert_rowid();";

            var parameters = new[]
            {
                new SqliteParameter("@FirstName", trainer.FirstName),
                new SqliteParameter("@LastName", trainer.LastName),
                new SqliteParameter("@Specialization", trainer.Specialization ?? (object)DBNull.Value)
            };

            var result = await ExecuteScalarAsync(sql, parameters);
            return Convert.ToInt32(result);
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(Trainer trainer)
        {
            const string sql = @"
                UPDATE Trainers
                SET FirstName = @FirstName,
                    LastName = @LastName,
                    Specialization = @Specialization
                WHERE TrainerId = @TrainerId";

            var parameters = new[]
            {
                new SqliteParameter("@TrainerId", trainer.TrainerId),
                new SqliteParameter("@FirstName", trainer.FirstName),
                new SqliteParameter("@LastName", trainer.LastName),
                new SqliteParameter("@Specialization", trainer.Specialization ?? (object)DBNull.Value)
            };

            await ExecuteNonQueryAsync(sql, parameters);
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(int id)
        {
            const string sql = "DELETE FROM Trainers WHERE TrainerId = @TrainerId";
            await ExecuteNonQueryAsync(sql, new SqliteParameter("@TrainerId", id));
        }

        public Trainer? GetById(int id)
        {
            try
            {
                EnsureConnectionOpen();
                using var command = new SqlCommand(
                    "SELECT * FROM trainers WHERE trainer_id = @Id", _connection, _transaction);
                
                command.Parameters.AddWithValue("@Id", id);
                
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return MapTrainer(reader);
                }
                return null;
            }
            catch (SqlException ex)
            {
                throw new QueryException($"Ошибка при получении тренера с ID {id}", ex);
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Неожиданная ошибка при работе с базой данных", ex);
            }
        }

        public List<Trainer> GetAll()
        {
            var trainers = new List<Trainer>();
            try
            {
                EnsureConnectionOpen();
                using var command = new SqlCommand("SELECT * FROM trainers", _connection, _transaction);
                
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    trainers.Add(MapTrainer(reader));
                }
            }
            catch (SqlException ex)
            {
                throw new QueryException("Ошибка при получении списка тренеров", ex);
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Неожиданная ошибка при работе с базой данных", ex);
            }
            return trainers;
        }

        public int Create(Trainer trainer)
        {
            try
            {
                EnsureConnectionOpen();
                using var command = new SqlCommand(
                    @"INSERT INTO trainers (first_name, last_name, specialization, hire_date, salary, certification) 
                    VALUES (@FirstName, @LastName, @Specialization, @HireDate, @Salary, @Certification);
                    SELECT CAST(SCOPE_IDENTITY() as int)", _connection, _transaction);

                SetParameters(command, trainer);
                
                return (int)command.ExecuteScalar()!;
            }
            catch (SqlException ex)
            {
                throw new QueryException("Ошибка при создании тренера", ex);
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Неожиданная ошибка при работе с базой данных", ex);
            }
        }

        public void Update(Trainer trainer)
        {
            try
            {
                EnsureConnectionOpen();
                using var command = new SqlCommand(
                    @"UPDATE trainers SET 
                    first_name = @FirstName,
                    last_name = @LastName,
                    specialization = @Specialization,
                    hire_date = @HireDate,
                    salary = @Salary,
                    certification = @Certification
                    WHERE trainer_id = @TrainerId", _connection, _transaction);

                command.Parameters.AddWithValue("@TrainerId", trainer.TrainerId);
                SetParameters(command, trainer);
                
                int rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected == 0)
                {
                    throw new EntityNotFoundException("Тренер", trainer.TrainerId);
                }
            }
            catch (SqlException ex)
            {
                throw new QueryException($"Ошибка при обновлении тренера с ID {trainer.TrainerId}", ex);
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
                using var command = new SqlCommand(
                    "DELETE FROM trainers WHERE trainer_id = @Id", _connection, _transaction);
                
                command.Parameters.AddWithValue("@Id", id);
                int rowsAffected = command.ExecuteNonQuery();
                
                if (rowsAffected == 0)
                {
                    throw new EntityNotFoundException("Тренер", id);
                }
            }
            catch (SqlException ex)
            {
                throw new QueryException($"Ошибка при удалении тренера с ID {id}", ex);
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

        private void SetParameters(SqlCommand command, Trainer trainer)
        {
            command.Parameters.AddWithValue("@FirstName", trainer.FirstName);
            command.Parameters.AddWithValue("@LastName", trainer.LastName);
            command.Parameters.AddWithValue("@Specialization", trainer.Specialization);
            command.Parameters.AddWithValue("@HireDate", trainer.HireDate);
            command.Parameters.AddWithValue("@Salary", trainer.Salary);
            command.Parameters.AddWithValue("@Certification", trainer.Certification ?? (object)DBNull.Value);
        }

        private Trainer MapTrainer(SqlDataReader reader)
        {
            return new Trainer
            {
                TrainerId = reader.GetInt32(reader.GetOrdinal("trainer_id")),
                FirstName = reader.GetString(reader.GetOrdinal("first_name")),
                LastName = reader.GetString(reader.GetOrdinal("last_name")),
                Specialization = reader.GetString(reader.GetOrdinal("specialization")),
                HireDate = reader.GetDateTime(reader.GetOrdinal("hire_date")),
                Salary = reader.GetDecimal(reader.GetOrdinal("salary")),
                Certification = reader.IsDBNull(reader.GetOrdinal("certification")) 
                    ? null 
                    : reader.GetString(reader.GetOrdinal("certification"))
            };
        }
    }
} 
