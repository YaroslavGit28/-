using System;

namespace FitnessClubApp.Models
{
    public class MemberMembership
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public int MembershipId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Навигационные свойства
        public Member? Member { get; set; }
        public Membership? Membership { get; set; }
    }
} 