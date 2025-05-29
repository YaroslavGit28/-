using System;
using System.Collections.Generic;
using FitnessClub.Models;
using FitnessClub.Data;
using FitnessClub.Data.Exceptions;
using FitnessClub.Business.Interfaces;
using FitnessClub.Business.Exceptions;
using FitnessClub.Data.Interfaces;
using System.Threading.Tasks;
using System.Linq;

namespace FitnessClub.Business.Services
{
    /// <summary>
    /// Сервис для работы с абонементами
    /// </summary>
    public class MembershipService : IMembershipService
    {
        private readonly IRepository<Membership> _membershipRepository;
        private const decimal BASE_PRICE_PER_DAY = 100m;
        private const decimal ACCESS_LEVEL_MULTIPLIER = 1.5m;

        /// <summary>
        /// Создает новый экземпляр сервиса абонементов
        /// </summary>
        /// <param name="membershipRepository">Репозиторий абонементов</param>
        public MembershipService(IRepository<Membership> membershipRepository)
        {
            _membershipRepository = membershipRepository;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Membership>> GetAllMembershipsAsync()
        {
            return await _membershipRepository.GetAllAsync();
        }

        /// <inheritdoc/>
        public async Task<Membership?> GetMembershipByIdAsync(int id)
        {
            return await _membershipRepository.GetByIdAsync(id);
        }

        /// <inheritdoc/>
        public async Task<int> AddMembershipAsync(Membership membership)
        {
            if (string.IsNullOrWhiteSpace(membership.Type))
                throw new BusinessException("Тип абонемента не может быть пустым");

            if (membership.DurationDays <= 0)
                throw new BusinessException("Длительность абонемента должна быть положительным числом");

            if (membership.Price < 0)
                throw new BusinessException("Стоимость абонемента не может быть отрицательной");

            var existingMembership = await GetMembershipByTypeAsync(membership.Type);
            if (existingMembership != null)
                throw new BusinessException($"Абонемент с типом '{membership.Type}' уже существует");

            return await _membershipRepository.AddAsync(membership);
        }

        /// <inheritdoc/>
        public async Task UpdateMembershipAsync(Membership membership)
        {
            if (string.IsNullOrWhiteSpace(membership.Type))
                throw new BusinessException("Тип абонемента не может быть пустым");

            if (membership.DurationDays <= 0)
                throw new BusinessException("Длительность абонемента должна быть положительным числом");

            if (membership.Price < 0)
                throw new BusinessException("Стоимость абонемента не может быть отрицательной");

            var existingMembership = await _membershipRepository.GetByIdAsync(membership.MembershipId);
            if (existingMembership == null)
                throw new BusinessException($"Абонемент с ID {membership.MembershipId} не найден");

            var duplicateType = await GetMembershipByTypeAsync(membership.Type);
            if (duplicateType != null && duplicateType.MembershipId != membership.MembershipId)
                throw new BusinessException($"Абонемент с типом '{membership.Type}' уже существует");

            await _membershipRepository.UpdateAsync(membership);
        }

        /// <inheritdoc/>
        public async Task DeleteMembershipAsync(int id)
        {
            var membership = await _membershipRepository.GetByIdAsync(id);
            if (membership == null)
                throw new BusinessException($"Абонемент с ID {id} не найден");

            await _membershipRepository.DeleteAsync(id);
        }

        /// <inheritdoc/>
        public async Task<Membership?> GetMembershipByTypeAsync(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
                throw new BusinessException("Тип абонемента не может быть пустым");

            var memberships = await _membershipRepository.GetAllAsync();
            return memberships.FirstOrDefault(m => m.Type.Equals(type, StringComparison.OrdinalIgnoreCase));
        }

        public List<Membership> GetAllMemberships()
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

        public List<Membership> GetActiveMemberships()
        {
            try
            {
                return _membershipRepository.GetActiveTypes();
            }
            catch (DatabaseException ex)
            {
                throw new BusinessException("Ошибка при получении списка активных абонементов", ex);
            }
        }

        public int CreateMembership(Membership membership)
        {
            ValidateMembership(membership);

            try
            {
                // Проверяем уникальность типа абонемента
                if (!_membershipRepository.IsTypeUnique(membership.Type))
                {
                    throw new DuplicateDataException($"Абонемент с типом '{membership.Type}' уже существует");
                }

                // Рассчитываем цену, если она не установлена
                if (membership.Price <= 0)
                {
                    membership.Price = CalculatePrice(membership.DurationDays, membership.AccessLevel);
                }

                return _membershipRepository.Create(membership);
            }
            catch (DatabaseException ex)
            {
                throw new BusinessException("Ошибка при создании абонемента", ex);
            }
        }

        public void UpdateMembership(Membership membership)
        {
            ValidateMembership(membership);

            try
            {
                // Проверяем существование абонемента
                var existingMembership = _membershipRepository.GetById(membership.MembershipId);
                if (existingMembership == null)
                {
                    throw new BusinessException($"Абонемент с ID {membership.MembershipId} не найден");
                }

                // Проверяем уникальность типа абонемента
                if (!_membershipRepository.IsTypeUnique(membership.Type, membership.MembershipId))
                {
                    throw new DuplicateDataException($"Абонемент с типом '{membership.Type}' уже существует");
                }

                // Рассчитываем цену, если она не установлена
                if (membership.Price <= 0)
                {
                    membership.Price = CalculatePrice(membership.DurationDays, membership.AccessLevel);
                }

                _membershipRepository.Update(membership);
            }
            catch (DatabaseException ex)
            {
                throw new BusinessException("Ошибка при обновлении абонемента", ex);
            }
        }

        public void DeactivateMembership(int id)
        {
            try
            {
                var membership = _membershipRepository.GetById(id);
                if (membership == null)
                {
                    throw new BusinessException($"Абонемент с ID {id} не найден");
                }

                if (!membership.IsActive)
                {
                    throw new BusinessRuleException("Абонемент уже деактивирован");
                }

                _membershipRepository.Delete(id); // В репозитории это реализовано как soft delete
            }
            catch (DatabaseException ex)
            {
                throw new BusinessException("Ошибка при деактивации абонемента", ex);
            }
        }

        public bool IsMembershipTypeUnique(string type, int? excludeMembershipId = null)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                throw new ValidationException("Тип абонемента не может быть пустым");
            }

            try
            {
                return _membershipRepository.IsTypeUnique(type, excludeMembershipId);
            }
            catch (DatabaseException ex)
            {
                throw new BusinessException("Ошибка при проверке уникальности типа абонемента", ex);
            }
        }

        public decimal CalculatePrice(int durationDays, int accessLevel)
        {
            if (durationDays <= 0)
            {
                throw new ValidationException("Длительность абонемента должна быть больше 0 дней");
            }

            if (accessLevel < 1 || accessLevel > 3)
            {
                throw new ValidationException("Уровень доступа должен быть от 1 до 3");
            }

            // Базовая цена за день * количество дней * множитель уровня доступа
            decimal price = BASE_PRICE_PER_DAY * durationDays;
            
            // Применяем множитель в зависимости от уровня доступа
            price *= 1 + ((accessLevel - 1) * ACCESS_LEVEL_MULTIPLIER);

            // Скидка за длительность
            if (durationDays >= 180) // Полгода
            {
                price *= 0.9m; // 10% скидка
            }
            else if (durationDays >= 90) // 3 месяца
            {
                price *= 0.95m; // 5% скидка
            }

            return Math.Round(price, 2);
        }

        private void ValidateMembership(Membership membership)
        {
            if (membership == null)
            {
                throw new ValidationException("Данные абонемента не могут быть пустыми");
            }

            if (string.IsNullOrWhiteSpace(membership.Type))
            {
                throw new ValidationException("Тип абонемента не может быть пустым");
            }

            if (membership.DurationDays <= 0)
            {
                throw new ValidationException("Длительность абонемента должна быть больше 0 дней");
            }

            if (membership.AccessLevel < 1 || membership.AccessLevel > 3)
            {
                throw new ValidationException("Уровень доступа должен быть от 1 до 3");
            }

            if (membership.Price < 0)
            {
                throw new ValidationException("Цена не может быть отрицательной");
            }
        }
    }
}  
