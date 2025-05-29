using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using FitnessClubApp.Models;

namespace FitnessClubApp.Data
{
    public class MemberMembershipRepository : BaseRepository
    {
        private readonly MemberRepository _memberRepository;
        private readonly MembershipRepository _membershipRepository;

        public MemberMembershipRepository(string connectionString) : base(connectionString)
        {
            _memberRepository = new MemberRepository(connectionString);
            _membershipRepository = new MembershipRepository(connectionString);
        }

        public IEnumerable<MemberMembership> GetAllActive()
        {
            var memberships = new List<MemberMembership>();

            try
            {
                using var connection = CreateConnection();
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT mm.*, m.FirstName, m.LastName, m.Email, m.JoinDate,
                           ms.Name, ms.Duration, ms.Price
                    FROM MemberMemberships mm
                    JOIN Members m ON mm.MemberId = m.Id
                    JOIN Memberships ms ON mm.MembershipId = ms.Id
                    WHERE mm.EndDate >= date('now')";

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    memberships.Add(MapFromReader(reader));
                }
            }
            catch (SqliteException ex)
            {
                throw new DatabaseException("Ошибка при получении списка активных абонементов", ex);
            }

            return memberships;
        }

        public IEnumerable<MemberMembership> GetByMemberId(int memberId)
        {
            var memberships = new List<MemberMembership>();

            try
            {
                using var connection = CreateConnection();
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT mm.*, m.FirstName, m.LastName, m.Email, m.JoinDate,
                           ms.Name, ms.Duration, ms.Price
                    FROM MemberMemberships mm
                    JOIN Members m ON mm.MemberId = m.Id
                    JOIN Memberships ms ON mm.MembershipId = ms.Id
                    WHERE mm.MemberId = @MemberId
                    ORDER BY mm.EndDate DESC";
                command.Parameters.AddWithValue("@MemberId", memberId);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    memberships.Add(MapFromReader(reader));
                }
            }
            catch (SqliteException ex)
            {
                throw new DatabaseException($"Ошибка при получении абонементов члена клуба с ID {memberId}", ex);
            }

            return memberships;
        }

        public void AssignMembership(int memberId, int membershipId, DateTime startDate)
        {
            // Проверяем существование члена клуба и абонемента
            var member = _memberRepository.GetById(memberId);
            var membership = _membershipRepository.GetById(membershipId);

            if (member == null)
                throw new InvalidOperationException($"Член клуба с ID {memberId} не найден");
            if (membership == null)
                throw new InvalidOperationException($"Абонемент с ID {membershipId} не найден");

            // Проверяем, нет ли уже активного абонемента
            var hasActive = ExecuteScalar<long>(@"
                SELECT COUNT(*) 
                FROM MemberMemberships 
                WHERE MemberId = @MemberId 
                AND EndDate >= date('now')",
                command => command.Parameters.AddWithValue("@MemberId", memberId)) > 0;

            if (hasActive)
                throw new InvalidOperationException("У члена клуба уже есть активный абонемент");

            var endDate = startDate.AddDays(membership.Duration);

            const string sql = @"
                INSERT INTO MemberMemberships (MemberId, MembershipId, StartDate, EndDate)
                VALUES (@MemberId, @MembershipId, @StartDate, @EndDate)";

            ExecuteCommand(sql, command =>
            {
                command.Parameters.AddWithValue("@MemberId", memberId);
                command.Parameters.AddWithValue("@MembershipId", membershipId);
                command.Parameters.AddWithValue("@StartDate", startDate.ToString("yyyy-MM-dd"));
                command.Parameters.AddWithValue("@EndDate", endDate.ToString("yyyy-MM-dd"));
            });
        }

        private static MemberMembership MapFromReader(SqliteDataReader reader)
        {
            return new MemberMembership
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                MemberId = reader.GetInt32(reader.GetOrdinal("MemberId")),
                MembershipId = reader.GetInt32(reader.GetOrdinal("MembershipId")),
                StartDate = DateTime.Parse(reader.GetString(reader.GetOrdinal("StartDate"))),
                EndDate = DateTime.Parse(reader.GetString(reader.GetOrdinal("EndDate"))),
                Member = new Member
                {
                    Id = reader.GetInt32(reader.GetOrdinal("MemberId")),
                    FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                    LastName = reader.GetString(reader.GetOrdinal("LastName")),
                    Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email")),
                    JoinDate = DateTime.Parse(reader.GetString(reader.GetOrdinal("JoinDate")))
                },
                Membership = new Membership
                {
                    Id = reader.GetInt32(reader.GetOrdinal("MembershipId")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    Duration = reader.GetInt32(reader.GetOrdinal("Duration")),
                    Price = reader.GetDecimal(reader.GetOrdinal("Price"))
                }
            };
        }
    }
} 