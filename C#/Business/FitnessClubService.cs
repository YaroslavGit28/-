using System;
using System.Collections.Generic;
using System.Linq;
using FitnessClubApp.Data;
using FitnessClubApp.Models;

namespace FitnessClubApp.Business
{
    public class FitnessClubService : IFitnessClubService
    {
        private readonly MemberRepository _memberRepository;
        private readonly MembershipRepository _membershipRepository;
        private readonly MemberMembershipRepository _memberMembershipRepository;

        public FitnessClubService(string connectionString)
        {
            _memberRepository = new MemberRepository(connectionString);
            _membershipRepository = new MembershipRepository(connectionString);
            _memberMembershipRepository = new MemberMembershipRepository(connectionString);
        }

        // Реализация методов для работы с членами клуба
        public IEnumerable<Member> GetAllMembers()
        {
            try
            {
                return _memberRepository.GetAll();
            }
            catch (DatabaseException ex)
            {
                throw new BusinessException("Ошибка при получении списка членов клуба", ex);
            }
        }

        public Member GetMemberById(int id)
        {
            try
            {
                var member = _memberRepository.GetById(id);
                if (member == null)
                    throw new BusinessException($"Член клуба с ID {id} не найден");
                return member;
            }
            catch (DatabaseException ex)
            {
                throw new BusinessException($"Ошибка при получении члена клуба с ID {id}", ex);
            }
        }

        public void AddMember(Member member)
        {
            ValidateMember(member);
            try
            {
                _memberRepository.Add(member);
            }
            catch (DatabaseException ex)
            {
                throw new BusinessException("Ошибка при добавлении члена клуба", ex);
            }
        }

        public void UpdateMember(Member member)
        {
            ValidateMember(member);
            try
            {
                _memberRepository.Update(member);
            }
            catch (DatabaseException ex)
            {
                throw new BusinessException("Ошибка при обновлении данных члена клуба", ex);
            }
        }

        public void DeleteMember(int id)
        {
            if (HasActiveMembership(id))
                throw new BusinessException("Нельзя удалить члена клуба с активным абонементом");

            try
            {
                _memberRepository.Delete(id);
            }
            catch (DatabaseException ex)
            {
                throw new BusinessException($"Ошибка при удалении члена клуба с ID {id}", ex);
            }
        }

        // Реализация методов для работы с абонементами
        public IEnumerable<Membership> GetAllMemberships()
        {
            try
            {
                return _membershipRepository.GetAll();
            }
            catch (DatabaseException ex)
            {
                throw new BusinessException("Ошибка при получении списка абонементов", ex);
            }
        }

        public Membership GetMembershipById(int id)
        {
            try
            {
                var membership = _membershipRepository.GetById(id);
                if (membership == null)
                    throw new BusinessException($"Абонемент с ID {id} не найден");
                return membership;
            }
            catch (DatabaseException ex)
            {
                throw new BusinessException($"Ошибка при получении абонемента с ID {id}", ex);
            }
        }

        public void AddMembership(Membership membership)
        {
            ValidateMembership(membership);
            try
            {
                _membershipRepository.Add(membership);
            }
            catch (DatabaseException ex)
            {
                throw new BusinessException("Ошибка при добавлении абонемента", ex);
            }
        }

        public void UpdateMembership(Membership membership)
        {
            ValidateMembership(membership);
            try
            {
                _membershipRepository.Update(membership);
            }
            catch (DatabaseException ex)
            {
                throw new BusinessException("Ошибка при обновлении абонемента", ex);
            }
        }

        public void DeleteMembership(int id)
        {
            try
            {
                _membershipRepository.Delete(id);
            }
            catch (InvalidOperationException ex)
            {
                throw new BusinessException(ex.Message);
            }
            catch (DatabaseException ex)
            {
                throw new BusinessException($"Ошибка при удалении абонемента с ID {id}", ex);
            }
        }

        // Реализация методов для работы с привязками абонементов
        public IEnumerable<MemberMembership> GetActiveMemberships()
        {
            try
            {
                return _memberMembershipRepository.GetAllActive();
            }
            catch (DatabaseException ex)
            {
                throw new BusinessException("Ошибка при получении активных абонементов", ex);
            }
        }

        public IEnumerable<MemberMembership> GetMemberMemberships(int memberId)
        {
            try
            {
                return _memberMembershipRepository.GetByMemberId(memberId);
            }
            catch (DatabaseException ex)
            {
                throw new BusinessException($"Ошибка при получении абонементов члена клуба с ID {memberId}", ex);
            }
        }

        public void AssignMembership(int memberId, int membershipId, DateTime startDate)
        {
            if (HasActiveMembership(memberId))
                throw new BusinessException("У члена клуба уже есть активный абонемент");

            try
            {
                _memberMembershipRepository.AssignMembership(memberId, membershipId, startDate);
            }
            catch (InvalidOperationException ex)
            {
                throw new BusinessException(ex.Message);
            }
            catch (DatabaseException ex)
            {
                throw new BusinessException("Ошибка при привязке абонемента", ex);
            }
        }

        // Реализация бизнес-операций
        public bool HasActiveMembership(int memberId)
        {
            try
            {
                return GetMemberMemberships(memberId)
                    .Any(mm => mm.EndDate >= DateTime.Now);
            }
            catch (DatabaseException ex)
            {
                throw new BusinessException($"Ошибка при проверке активных абонементов члена клуба с ID {memberId}", ex);
            }
        }

        public DateTime CalculateMembershipEndDate(int membershipId, DateTime startDate)
        {
            var membership = GetMembershipById(membershipId);
            return startDate.AddDays(membership.Duration);
        }

        public decimal CalculateTotalRevenue(DateTime startDate, DateTime endDate)
        {
            try
            {
                var activeMemberships = GetActiveMemberships()
                    .Where(mm => mm.StartDate >= startDate && mm.StartDate <= endDate);

                return activeMemberships.Sum(mm => mm.Membership?.Price ?? 0);
            }
            catch (DatabaseException ex)
            {
                throw new BusinessException("Ошибка при расчете общей выручки", ex);
            }
        }

        // Реализация методов для работы с просроченными абонементами
        public IEnumerable<MemberMembership> GetExpiredMemberships()
        {
            try
            {
                return _memberMembershipRepository.GetAllActive()
                    .Where(mm => mm.EndDate < DateTime.Now)
                    .OrderBy(mm => mm.EndDate);
            }
            catch (DatabaseException ex)
            {
                throw new BusinessException("Ошибка при получении просроченных абонементов", ex);
            }
        }

        public IEnumerable<MemberMembership> GetExpiringMemberships(int daysThreshold)
        {
            if (daysThreshold < 0)
                throw new BusinessException("Количество дней должно быть положительным числом");

            try
            {
                var thresholdDate = DateTime.Now.AddDays(daysThreshold);
                return _memberMembershipRepository.GetAllActive()
                    .Where(mm => mm.EndDate >= DateTime.Now && mm.EndDate <= thresholdDate)
                    .OrderBy(mm => mm.EndDate);
            }
            catch (DatabaseException ex)
            {
                throw new BusinessException("Ошибка при получении истекающих абонементов", ex);
            }
        }

        public bool IsExpired(MemberMembership membership)
        {
            if (membership == null)
                throw new BusinessException("Абонемент не может быть null");

            return membership.EndDate < DateTime.Now;
        }

        public int DaysUntilExpiration(MemberMembership membership)
        {
            if (membership == null)
                throw new BusinessException("Абонемент не может быть null");

            var daysLeft = (membership.EndDate - DateTime.Now).Days;
            return daysLeft < 0 ? 0 : daysLeft;
        }

        // Вспомогательные методы валидации
        private void ValidateMember(Member member)
        {
            if (member == null)
                throw new BusinessException("Член клуба не может быть null");

            if (string.IsNullOrWhiteSpace(member.FirstName))
                throw new BusinessException("Имя члена клуба не может быть пустым");

            if (string.IsNullOrWhiteSpace(member.LastName))
                throw new BusinessException("Фамилия члена клуба не может быть пустой");

            if (member.JoinDate > DateTime.Now)
                throw new BusinessException("Дата регистрации не может быть в будущем");
        }

        private void ValidateMembership(Membership membership)
        {
            if (membership == null)
                throw new BusinessException("Абонемент не может быть null");

            if (string.IsNullOrWhiteSpace(membership.Name))
                throw new BusinessException("Название абонемента не может быть пустым");

            if (membership.Duration <= 0)
                throw new BusinessException("Длительность абонемента должна быть положительным числом");

            if (membership.Price < 0)
                throw new BusinessException("Цена абонемента не может быть отрицательной");
        }
    }
} 