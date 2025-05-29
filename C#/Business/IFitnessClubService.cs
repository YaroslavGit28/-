using System;
using System.Collections.Generic;
using FitnessClubApp.Models;

namespace FitnessClubApp.Business
{
    public interface IFitnessClubService
    {
        // Методы для работы с членами клуба
        IEnumerable<Member> GetAllMembers();
        Member GetMemberById(int id);
        void AddMember(Member member);
        void UpdateMember(Member member);
        void DeleteMember(int id);
        
        // Методы для работы с абонементами
        IEnumerable<Membership> GetAllMemberships();
        Membership GetMembershipById(int id);
        void AddMembership(Membership membership);
        void UpdateMembership(Membership membership);
        void DeleteMembership(int id);
        
        // Методы для работы с привязками абонементов
        IEnumerable<MemberMembership> GetActiveMemberships();
        IEnumerable<MemberMembership> GetMemberMemberships(int memberId);
        void AssignMembership(int memberId, int membershipId, DateTime startDate);
        
        // Методы для работы с просроченными абонементами
        IEnumerable<MemberMembership> GetExpiredMemberships();
        IEnumerable<MemberMembership> GetExpiringMemberships(int daysThreshold);
        bool IsExpired(MemberMembership membership);
        int DaysUntilExpiration(MemberMembership membership);
        
        // Бизнес-операции
        bool HasActiveMembership(int memberId);
        DateTime CalculateMembershipEndDate(int membershipId, DateTime startDate);
        decimal CalculateTotalRevenue(DateTime startDate, DateTime endDate);
    }
} 