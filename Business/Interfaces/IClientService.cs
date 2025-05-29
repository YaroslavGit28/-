using System;
using System.Collections.Generic;
using FitnessClub.Models;

namespace FitnessClub.Business.Interfaces
{
    /// <summary>
    /// Интерфейс сервиса для работы с клиентами
    /// </summary>
    public interface IClientService
    {
        /// <summary>
        /// Получить список всех клиентов
        /// </summary>
        Task<IEnumerable<Client>> GetAllClientsAsync();

        /// <summary>
        /// Получить клиента по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор клиента</param>
        Task<Client?> GetClientByIdAsync(int id);

        /// <summary>
        /// Добавить нового клиента
        /// </summary>
        /// <param name="client">Данные клиента</param>
        Task<int> AddClientAsync(Client client);

        /// <summary>
        /// Обновить данные клиента
        /// </summary>
        /// <param name="client">Обновленные данные клиента</param>
        Task UpdateClientAsync(Client client);

        /// <summary>
        /// Удалить клиента
        /// </summary>
        /// <param name="id">Идентификатор клиента</param>
        Task DeleteClientAsync(int id);

        /// <summary>
        /// Назначить абонемент клиенту
        /// </summary>
        /// <param name="clientId">Идентификатор клиента</param>
        /// <param name="membershipId">Идентификатор абонемента</param>
        Task AssignMembershipAsync(int clientId, int membershipId);

        List<Client> SearchClients(string searchTerm);
        bool IsPhoneNumberUnique(string phone, int? excludeClientId = null);
    }
}  
