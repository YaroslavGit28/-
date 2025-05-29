using System;

namespace FitnessClubApp.Models
{
    public class Member
    {
        public int Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public string? Email { get; set; }
        public DateTime JoinDate { get; set; }
    }
} 