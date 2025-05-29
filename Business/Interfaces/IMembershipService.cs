using System;
using System.Collections.Generic;
using FitnessClub.Models;

namespace FitnessClub.Business.Interfaces
{
    /// <summary>
    /// Интерфейс сервиса для работы с абонементами
    /// </summary>
    public interface IMembershipService
    {
        /// <summary>
        /// Получить список всех абонементов
        /// </summary>
        Task<IEnumerable<Membership>> GetAllMembershipsAsync();

        /// <summary>
        /// Получить абонемент по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор абонемента</param>
        Task<Membership?> GetMembershipByIdAsync(int id);

        /// <summary>
        /// Добавить новый абонемент
        /// </summary>
        /// <param name="membership">Данные абонемента</param>
        Task<int> AddMembershipAsync(Membership membership);

        /// <summary>
        /// Обновить данные абонемента
        /// </summary>
        /// <param name="membership">Обновленные данные абонемента</param>
        Task UpdateMembershipAsync(Membership membership);

        /// <summary>
        /// Удалить абонемент
        /// </summary>
        /// <param name="id">Идентификатор абонемента</param>
        Task DeleteMembershipAsync(int id);

        /// <summary>
        /// Получить абонемент по типу
        /// </summary>
        /// <param name="type">Тип абонемента</param>
        Task<Membership?> GetMembershipByTypeAsync(string type);
    }
}  
