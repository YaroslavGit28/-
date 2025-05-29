 using System;
using System.Collections.Generic;
using FitnessClub.Models;
using FitnessClub.Data;
using FitnessClub.Data.Exceptions;
using FitnessClub.Business.Interfaces;
using FitnessClub.Business.Exceptions;
using FitnessClub.Data.Interfaces;
using System.Threading.Tasks;

namespace FitnessClub.Business.Services
{
    /// <summary>
    /// Сервис для работы с клиентами
    /// </summary>
    public class ClientService : IClientService
    {
        private readonly IRepository<Client> _clientRepository;
        private readonly IRepository<Membership> _membershipRepository;

        /// <summary>
        /// Создает новый экземпляр сервиса клиентов
        /// </summary>
        /// <param name="clientRepository">Репозиторий клиентов</param>
        /// <param name="membershipRepository">Репозиторий абонементов</param>
        public ClientService()
        {
            _clientRepository = new ClientRepository();
            _membershipRepository = new MembershipRepository();
        }

        public Client GetClient(int id)
        {
            try
            {
                var client = _clientRepository.GetById(id);
                if (client == null)
                {
                    throw new BusinessException($"Клиент с ID {id} не найден");
                }
                return client;
            }
            catch (DatabaseException ex)
            {
                throw new BusinessException("Ошибка при получении данных клиента", ex);
            }
        }

        public List<Client> GetAllClients()
        {
            try
            {
                return _clientRepository.GetAll();
            }
            catch (DatabaseException ex)
            {
                throw new BusinessException("Ошибка при получении списка клиентов", ex);
            }
        }

        public List<Client> SearchClients(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                throw new ValidationException("Поисковый запрос не может быть пустым");
            }

            try
            {
                return _clientRepository.SearchByName(searchTerm);
            }
            catch (DatabaseException ex)
            {
                throw new BusinessException("Ошибка при поиске клиентов", ex);
            }
        }

        public int RegisterClient(Client client)
        {
            ValidateClient(client);

            try
            {
                // Проверяем существование абонемента
                var membership = _membershipRepository.GetById(client.MembershipId);
                if (membership == null)
                {
                    throw new ValidationException($"Абонемент с ID {client.MembershipId} не существует");
                }

                if (!membership.IsActive)
                {
                    throw new BusinessRuleException("Нельзя зарегистрировать клиента на неактивный тип абонемента");
                }

                // Проверяем уникальность телефона
                if (!_clientRepository.IsPhoneUnique(client.Phone))
                {
                    throw new DuplicateDataException($"Клиент с телефоном {client.Phone} уже существует");
                }

                // Устанавливаем дату регистрации
                client.JoinDate = DateTime.Now;

                return _clientRepository.Create(client);
            }
            catch (DatabaseException ex)
            {
                throw new BusinessException("Ошибка при регистрации клиента", ex);
            }
        }

        public void UpdateClient(Client client)
        {
            ValidateClient(client);

            try
            {
                // Проверяем существование клиента
                var existingClient = _clientRepository.GetById(client.ClientId);
                if (existingClient == null)
                {
                    throw new BusinessException($"Клиент с ID {client.ClientId} не найден");
                }

                // Проверяем существование абонемента
                var membership = _membershipRepository.GetById(client.MembershipId);
                if (membership == null)
                {
                    throw new ValidationException($"Абонемент с ID {client.MembershipId} не существует");
                }

                // Проверяем уникальность телефона
                if (!_clientRepository.IsPhoneUnique(client.Phone, client.ClientId))
                {
                    throw new DuplicateDataException($"Клиент с телефоном {client.Phone} уже существует");
                }

                // Сохраняем дату регистрации
                client.JoinDate = existingClient.JoinDate;

                _clientRepository.Update(client);
            }
            catch (DatabaseException ex)
            {
                throw new BusinessException("Ошибка при обновлении данных клиента", ex);
            }
        }

        public void DeleteClient(int id)
        {
            try
            {
                var client = _clientRepository.GetById(id);
                if (client == null)
                {
                    throw new BusinessException($"Клиент с ID {id} не найден");
                }

                _clientRepository.Delete(id);
            }
            catch (DatabaseException ex)
            {
                throw new BusinessException("Ошибка при удалении клиента", ex);
            }
        }

        public bool IsPhoneNumberUnique(string phone, int? excludeClientId = null)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                throw new ValidationException("Номер телефона не может быть пустым");
            }

            try
            {
                return _clientRepository.IsPhoneUnique(phone, excludeClientId);
            }
            catch (DatabaseException ex)
            {
                throw new BusinessException("Ошибка при проверке уникальности телефона", ex);
            }
        }

        private void ValidateClient(Client client)
        {
            if (client == null)
            {
                throw new ValidationException("Данные клиента не могут быть пустыми");
            }

            if (string.IsNullOrWhiteSpace(client.FirstName))
            {
                throw new ValidationException("Имя клиента не может быть пустым");
            }

            if (string.IsNullOrWhiteSpace(client.LastName))
            {
                throw new ValidationException("Фамилия клиента не может быть пустой");
            }

            if (string.IsNullOrWhiteSpace(client.Phone))
            {
                throw new ValidationException("Телефон клиента не может быть пустым");
            }

            if (client.MembershipId <= 0)
            {
                throw new ValidationException("Необходимо указать действительный ID абонемента");
            }

            if (client.BirthDate.HasValue && client.BirthDate.Value > DateTime.Now)
            {
                throw new ValidationException("Дата рождения не может быть в будущем");
            }
        }
    }
} 
