using System;

namespace FitnessClub.Models
{
    /// <summary>
    /// Представляет клиента фитнес-клуба
    /// </summary>
    public class Client
    {
        /// <summary>
        /// Идентификатор клиента
        /// </summary>
        public int ClientId { get; set; }

        /// <summary>
        /// Имя клиента
        /// </summary>
        public required string FirstName { get; set; }

        /// <summary>
        /// Фамилия клиента
        /// </summary>
        public required string LastName { get; set; }

        /// <summary>
        /// Дата регистрации
        /// </summary>
        public DateTime JoinDate { get; set; }

        /// <summary>
        /// Идентификатор текущего абонемента
        /// </summary>
        public int? MembershipId { get; set; }

        /// <summary>
        /// Полное имя клиента
        /// </summary>
        public string FullName => $"{LastName} {FirstName}";

        // Navigation property
        public Membership? Membership { get; set; }
    }
}  
