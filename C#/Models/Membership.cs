using System;

namespace FitnessClubApp.Models
{
    public class Membership
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public int Duration { get; set; }
        public decimal Price { get; set; }
    }
} 