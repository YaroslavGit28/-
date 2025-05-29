using System;
using System.Linq;
using System.Threading.Tasks;
using FitnessClub.Business.Interfaces;
using FitnessClub.Models;

namespace FitnessClub.UI
{
    /// <summary>
    /// Консольный пользовательский интерфейс
    /// </summary>
    public class ConsoleUI
    {
        private readonly IClientService _clientService;
        private readonly IMembershipService _membershipService;

        /// <summary>
        /// Создает новый экземпляр консольного интерфейса
        /// </summary>
        /// <param name="clientService">Сервис для работы с клиентами</param>
        /// <param name="membershipService">Сервис для работы с абонементами</param>
        public ConsoleUI(IClientService clientService, IMembershipService membershipService)
        {
            _clientService = clientService;
            _membershipService = membershipService;
        }

        /// <summary>
        /// Запускает главное меню приложения
        /// </summary>
        public async Task RunAsync()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Фитнес-клуб ===");
                Console.WriteLine("1. Клиенты");
                Console.WriteLine("2. Абонементы");
                Console.WriteLine("0. Выход");
                Console.Write("\nВыберите пункт меню: ");

                if (int.TryParse(Console.ReadLine(), out int choice))
                {
                    switch (choice)
                    {
                        case 1:
                            await ShowClientsMenuAsync();
                            break;
                        case 2:
                            await ShowMembershipsMenuAsync();
                            break;
                        case 0:
                            return;
                        default:
                            ShowError("Неверный пункт меню");
                            break;
                    }
                }
                else
                {
                    ShowError("Неверный ввод");
                }
            }
        }

        private async Task ShowClientsMenuAsync()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Управление клиентами ===");
                Console.WriteLine("1. Показать всех клиентов");
                Console.WriteLine("2. Добавить клиента");
                Console.WriteLine("3. Редактировать клиента");
                Console.WriteLine("4. Удалить клиента");
                Console.WriteLine("5. Назначить абонемент");
                Console.WriteLine("0. Назад");
                Console.Write("\nВыберите пункт меню: ");

                if (int.TryParse(Console.ReadLine(), out int choice))
                {
                    switch (choice)
                    {
                        case 1:
                            await ShowAllClientsAsync();
                            break;
                        case 2:
                            await AddClientAsync();
                            break;
                        case 3:
                            await EditClientAsync();
                            break;
                        case 4:
                            await DeleteClientAsync();
                            break;
                        case 5:
                            await AssignMembershipAsync();
                            break;
                        case 0:
                            return;
                        default:
                            ShowError("Неверный пункт меню");
                            break;
                    }
                }
                else
                {
                    ShowError("Неверный ввод");
                }
            }
        }

        private async Task ShowMembershipsMenuAsync()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Управление абонементами ===");
                Console.WriteLine("1. Показать все абонементы");
                Console.WriteLine("2. Добавить абонемент");
                Console.WriteLine("3. Редактировать абонемент");
                Console.WriteLine("4. Удалить абонемент");
                Console.WriteLine("0. Назад");
                Console.Write("\nВыберите пункт меню: ");

                if (int.TryParse(Console.ReadLine(), out int choice))
                {
                    switch (choice)
                    {
                        case 1:
                            await ShowAllMembershipsAsync();
                            break;
                        case 2:
                            await AddMembershipAsync();
                            break;
                        case 3:
                            await EditMembershipAsync();
                            break;
                        case 4:
                            await DeleteMembershipAsync();
                            break;
                        case 0:
                            return;
                        default:
                            ShowError("Неверный пункт меню");
                            break;
                    }
                }
                else
                {
                    ShowError("Неверный ввод");
                }
            }
        }

        private async Task ShowAllClientsAsync()
        {
            Console.Clear();
            Console.WriteLine("=== Список клиентов ===\n");

            var clients = await _clientService.GetAllClientsAsync();
            if (!clients.Any())
            {
                Console.WriteLine("Список клиентов пуст");
            }
            else
            {
                foreach (var client in clients)
                {
                    Console.WriteLine($"ID: {client.ClientId}");
                    Console.WriteLine($"Имя: {client.FullName}");
                    Console.WriteLine($"Дата регистрации: {client.JoinDate:d}");
                    Console.WriteLine($"ID абонемента: {client.MembershipId ?? "нет"}\n");
                }
            }

            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private async Task AddClientAsync()
        {
            Console.Clear();
            Console.WriteLine("=== Добавление клиента ===\n");

            Console.Write("Введите имя: ");
            var firstName = Console.ReadLine() ?? "";

            Console.Write("Введите фамилию: ");
            var lastName = Console.ReadLine() ?? "";

            try
            {
                var client = new Client
                {
                    FirstName = firstName,
                    LastName = lastName,
                    JoinDate = DateTime.Now
                };

                var id = await _clientService.AddClientAsync(client);
                Console.WriteLine($"\nКлиент успешно добавлен (ID: {id})");
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }

            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private async Task EditClientAsync()
        {
            Console.Clear();
            Console.WriteLine("=== Редактирование клиента ===\n");

            Console.Write("Введите ID клиента: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                ShowError("Неверный ID");
                return;
            }

            try
            {
                var client = await _clientService.GetClientByIdAsync(id);
                if (client == null)
                {
                    ShowError($"Клиент с ID {id} не найден");
                    return;
                }

                Console.Write($"Введите имя ({client.FirstName}): ");
                var firstName = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(firstName))
                    client.FirstName = firstName;

                Console.Write($"Введите фамилию ({client.LastName}): ");
                var lastName = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(lastName))
                    client.LastName = lastName;

                await _clientService.UpdateClientAsync(client);
                Console.WriteLine("\nКлиент успешно обновлен");
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }

            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private async Task DeleteClientAsync()
        {
            Console.Clear();
            Console.WriteLine("=== Удаление клиента ===\n");

            Console.Write("Введите ID клиента: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                ShowError("Неверный ID");
                return;
            }

            try
            {
                await _clientService.DeleteClientAsync(id);
                Console.WriteLine("\nКлиент успешно удален");
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }

            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private async Task AssignMembershipAsync()
        {
            Console.Clear();
            Console.WriteLine("=== Назначение абонемента ===\n");

            Console.Write("Введите ID клиента: ");
            if (!int.TryParse(Console.ReadLine(), out int clientId))
            {
                ShowError("Неверный ID клиента");
                return;
            }

            Console.Write("Введите ID абонемента: ");
            if (!int.TryParse(Console.ReadLine(), out int membershipId))
            {
                ShowError("Неверный ID абонемента");
                return;
            }

            try
            {
                await _clientService.AssignMembershipAsync(clientId, membershipId);
                Console.WriteLine("\nАбонемент успешно назначен");
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }

            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private async Task ShowAllMembershipsAsync()
        {
            Console.Clear();
            Console.WriteLine("=== Список абонементов ===\n");

            var memberships = await _membershipService.GetAllMembershipsAsync();
            if (!memberships.Any())
            {
                Console.WriteLine("Список абонементов пуст");
            }
            else
            {
                foreach (var membership in memberships)
                {
                    Console.WriteLine($"ID: {membership.MembershipId}");
                    Console.WriteLine($"Тип: {membership.Type}");
                    Console.WriteLine($"Длительность: {membership.DurationDays} дней");
                    Console.WriteLine($"Стоимость: {membership.Price:C}");
                    Console.WriteLine($"Преимущества: {membership.Benefits ?? "нет"}\n");
                }
            }

            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private async Task AddMembershipAsync()
        {
            Console.Clear();
            Console.WriteLine("=== Добавление абонемента ===\n");

            Console.Write("Введите тип абонемента: ");
            var type = Console.ReadLine() ?? "";

            Console.Write("Введите длительность (дней): ");
            if (!int.TryParse(Console.ReadLine(), out int duration))
            {
                ShowError("Неверная длительность");
                return;
            }

            Console.Write("Введите стоимость: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal price))
            {
                ShowError("Неверная стоимость");
                return;
            }

            Console.Write("Введите преимущества: ");
            var benefits = Console.ReadLine();

            try
            {
                var membership = new Membership
                {
                    Type = type,
                    DurationDays = duration,
                    Price = price,
                    Benefits = benefits
                };

                var id = await _membershipService.AddMembershipAsync(membership);
                Console.WriteLine($"\nАбонемент успешно добавлен (ID: {id})");
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }

            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private async Task EditMembershipAsync()
        {
            Console.Clear();
            Console.WriteLine("=== Редактирование абонемента ===\n");

            Console.Write("Введите ID абонемента: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                ShowError("Неверный ID");
                return;
            }

            try
            {
                var membership = await _membershipService.GetMembershipByIdAsync(id);
                if (membership == null)
                {
                    ShowError($"Абонемент с ID {id} не найден");
                    return;
                }

                Console.Write($"Введите тип абонемента ({membership.Type}): ");
                var type = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(type))
                    membership.Type = type;

                Console.Write($"Введите длительность в днях ({membership.DurationDays}): ");
                var durationInput = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(durationInput) && int.TryParse(durationInput, out int duration))
                    membership.DurationDays = duration;

                Console.Write($"Введите стоимость ({membership.Price:C}): ");
                var priceInput = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(priceInput) && decimal.TryParse(priceInput, out decimal price))
                    membership.Price = price;

                Console.Write($"Введите преимущества ({membership.Benefits}): ");
                var benefits = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(benefits))
                    membership.Benefits = benefits;

                await _membershipService.UpdateMembershipAsync(membership);
                Console.WriteLine("\nАбонемент успешно обновлен");
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }

            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private async Task DeleteMembershipAsync()
        {
            Console.Clear();
            Console.WriteLine("=== Удаление абонемента ===\n");

            Console.Write("Введите ID абонемента: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                ShowError("Неверный ID");
                return;
            }

            try
            {
                await _membershipService.DeleteMembershipAsync(id);
                Console.WriteLine("\nАбонемент успешно удален");
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }

            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private void ShowError(string message)
        {
            Console.WriteLine($"\nОшибка: {message}");
            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }
    }
}  
