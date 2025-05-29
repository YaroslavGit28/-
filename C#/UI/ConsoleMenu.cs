using System;
using FitnessClubApp.Business;
using FitnessClubApp.Models;

namespace FitnessClubApp.UI
{
    public class ConsoleMenu
    {
        private readonly IFitnessClubService _fitnessClubService;

        public ConsoleMenu(string connectionString)
        {
            _fitnessClubService = new FitnessClubService(connectionString);
        }

        public void ShowMainMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Фитнес-клуб ===");
                Console.WriteLine("1. Просмотр членов клуба");
                Console.WriteLine("2. Добавить нового члена");
                Console.WriteLine("3. Управление абонементами");
                Console.WriteLine("4. Привязка абонементов");
                Console.WriteLine("5. Отчет о выручке");
                Console.WriteLine("6. Просроченные абонементы");
                Console.WriteLine("0. Выход");

                Console.Write("\nВыберите действие: ");
                var choice = Console.ReadLine();

                try
                {
                    switch (choice)
                    {
                        case "1":
                            ShowMembers();
                            break;
                        case "2":
                            AddNewMember();
                            break;
                        case "3":
                            ManageMemberships();
                            break;
                        case "4":
                            AssignMembership();
                            break;
                        case "5":
                            ShowRevenueReport();
                            break;
                        case "6":
                            ShowExpiredMemberships();
                            break;
                        case "0":
                            return;
                        default:
                            Console.WriteLine("Неверный выбор. Нажмите любую клавишу для продолжения...");
                            Console.ReadKey();
                            break;
                    }
                }
                catch (BusinessException ex)
                {
                    Console.WriteLine($"\nОшибка: {ex.Message}");
                    if (ex.InnerException != null)
                        Console.WriteLine($"Детали: {ex.InnerException.Message}");
                    Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\nНепредвиденная ошибка: {ex.Message}");
                    Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                }
            }
        }

        private void ShowMembers()
        {
            Console.Clear();
            Console.WriteLine("=== Члены клуба ===\n");

            var members = _fitnessClubService.GetAllMembers();
            var hasMembers = false;

            foreach (var member in members)
            {
                hasMembers = true;
                Console.WriteLine($"ID: {member.Id}");
                Console.WriteLine($"Имя: {member.FirstName} {member.LastName}");
                Console.WriteLine($"Email: {member.Email ?? "Не указан"}");
                Console.WriteLine($"Дата регистрации: {member.JoinDate:dd.MM.yyyy}");

                var memberMemberships = _fitnessClubService.GetMemberMemberships(member.Id);
                foreach (var mm in memberMemberships)
                {
                    if (mm.EndDate >= DateTime.Now)
                    {
                        Console.WriteLine($"Активный абонемент: {mm.Membership?.Name}");
                        Console.WriteLine($"Действует до: {mm.EndDate:dd.MM.yyyy}");
                    }
                }
                Console.WriteLine();
            }

            if (!hasMembers)
            {
                Console.WriteLine("Список членов клуба пуст.");
            }

            Console.WriteLine("\nНажмите любую клавишу для возврата в главное меню...");
            Console.ReadKey();
        }

        private void AddNewMember()
        {
            Console.Clear();
            Console.WriteLine("=== Добавление нового члена клуба ===\n");

            Console.Write("Введите имя: ");
            var firstName = Console.ReadLine() ?? "";
            if (string.IsNullOrWhiteSpace(firstName))
            {
                Console.WriteLine("Имя не может быть пустым");
                Console.ReadKey();
                return;
            }

            Console.Write("Введите фамилию: ");
            var lastName = Console.ReadLine() ?? "";
            if (string.IsNullOrWhiteSpace(lastName))
            {
                Console.WriteLine("Фамилия не может быть пустой");
                Console.ReadKey();
                return;
            }

            Console.Write("Введите email (или нажмите Enter для пропуска): ");
            var email = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(email))
                email = null;

            var member = new Member
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                JoinDate = DateTime.Now
            };

            _fitnessClubService.AddMember(member);
            Console.WriteLine("\nЧлен клуба успешно добавлен!");
            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private void ManageMemberships()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Управление абонементами ===\n");
                Console.WriteLine("1. Показать все абонементы");
                Console.WriteLine("2. Добавить абонемент");
                Console.WriteLine("3. Редактировать абонемент");
                Console.WriteLine("4. Удалить абонемент");
                Console.WriteLine("0. Вернуться в главное меню");

                Console.Write("\nВыберите действие: ");
                var choice = Console.ReadLine();

                try
                {
                    switch (choice)
                    {
                        case "1":
                            ShowMemberships();
                            break;
                        case "2":
                            AddMembership();
                            break;
                        case "3":
                            EditMembership();
                            break;
                        case "4":
                            DeleteMembership();
                            break;
                        case "0":
                            return;
                        default:
                            Console.WriteLine("Неверный выбор. Нажмите любую клавишу для продолжения...");
                            Console.ReadKey();
                            break;
                    }
                }
                catch (BusinessException ex)
                {
                    Console.WriteLine($"\nОшибка: {ex.Message}");
                    Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                }
            }
        }

        private void ShowMemberships()
        {
            Console.Clear();
            Console.WriteLine("=== Список абонементов ===\n");

            var memberships = _fitnessClubService.GetAllMemberships();
            var hasMemberships = false;

            foreach (var membership in memberships)
            {
                hasMemberships = true;
                Console.WriteLine($"ID: {membership.Id}");
                Console.WriteLine($"Название: {membership.Name}");
                Console.WriteLine($"Длительность: {membership.Duration} дней");
                Console.WriteLine($"Цена: {membership.Price:C}");
                Console.WriteLine();
            }

            if (!hasMemberships)
            {
                Console.WriteLine("Список абонементов пуст.");
            }

            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private void AddMembership()
        {
            Console.Clear();
            Console.WriteLine("=== Добавление абонемента ===\n");

            Console.Write("Введите название абонемента: ");
            var name = Console.ReadLine() ?? "";
            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("Название не может быть пустым");
                Console.ReadKey();
                return;
            }

            Console.Write("Введите длительность (в днях): ");
            if (!int.TryParse(Console.ReadLine(), out int duration) || duration <= 0)
            {
                Console.WriteLine("Некорректная длительность");
                Console.ReadKey();
                return;
            }

            Console.Write("Введите цену: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal price) || price < 0)
            {
                Console.WriteLine("Некорректная цена");
                Console.ReadKey();
                return;
            }

            var membership = new Membership
            {
                Name = name,
                Duration = duration,
                Price = price
            };

            _fitnessClubService.AddMembership(membership);
            Console.WriteLine("\nАбонемент успешно добавлен!");
            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private void EditMembership()
        {
            Console.Clear();
            Console.WriteLine("=== Редактирование абонемента ===\n");

            ShowMemberships();

            Console.Write("\nВведите ID абонемента для редактирования: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Некорректный ID");
                Console.ReadKey();
                return;
            }

            var membership = _fitnessClubService.GetMembershipById(id);

            Console.Write($"Новое название ({membership.Name}): ");
            var name = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(name))
                membership.Name = name;

            Console.Write($"Новая длительность в днях ({membership.Duration}): ");
            var durationInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(durationInput) && int.TryParse(durationInput, out int duration) && duration > 0)
                membership.Duration = duration;

            Console.Write($"Новая цена ({membership.Price:C}): ");
            var priceInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(priceInput) && decimal.TryParse(priceInput, out decimal price) && price >= 0)
                membership.Price = price;

            _fitnessClubService.UpdateMembership(membership);
            Console.WriteLine("\nАбонемент успешно обновлен!");
            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private void DeleteMembership()
        {
            Console.Clear();
            Console.WriteLine("=== Удаление абонемента ===\n");

            ShowMemberships();

            Console.Write("\nВведите ID абонемента для удаления: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Некорректный ID");
                Console.ReadKey();
                return;
            }

            _fitnessClubService.DeleteMembership(id);
            Console.WriteLine("\nАбонемент успешно удален!");
            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private void AssignMembership()
        {
            Console.Clear();
            Console.WriteLine("=== Привязка абонемента ===\n");

            ShowMembers();
            Console.Write("\nВведите ID члена клуба: ");
            if (!int.TryParse(Console.ReadLine(), out int memberId))
            {
                Console.WriteLine("Некорректный ID члена клуба");
                Console.ReadKey();
                return;
            }

            ShowMemberships();
            Console.Write("\nВведите ID абонемента: ");
            if (!int.TryParse(Console.ReadLine(), out int membershipId))
            {
                Console.WriteLine("Некорректный ID абонемента");
                Console.ReadKey();
                return;
            }

            _fitnessClubService.AssignMembership(memberId, membershipId, DateTime.Now);
            Console.WriteLine("\nАбонемент успешно привязан к члену клуба!");
            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private void ShowRevenueReport()
        {
            Console.Clear();
            Console.WriteLine("=== Отчет о выручке ===\n");

            var startDate = DateTime.Now.AddMonths(-1);
            var endDate = DateTime.Now;

            var revenue = _fitnessClubService.CalculateTotalRevenue(startDate, endDate);
            Console.WriteLine($"Выручка за последний месяц: {revenue:C}");

            var yearStartDate = new DateTime(DateTime.Now.Year, 1, 1);
            revenue = _fitnessClubService.CalculateTotalRevenue(yearStartDate, DateTime.Now);
            Console.WriteLine($"Выручка за текущий год: {revenue:C}");

            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private void ShowExpiredMemberships()
        {
            Console.Clear();
            Console.WriteLine("=== Просроченные и истекающие абонементы ===\n");

            // Показываем просроченные абонементы
            Console.WriteLine("Просроченные абонементы:");
            var expiredMemberships = _fitnessClubService.GetExpiredMemberships();
            bool hasExpired = false;

            foreach (var mm in expiredMemberships)
            {
                hasExpired = true;
                Console.WriteLine($"\nЧлен клуба: {mm.Member?.FirstName} {mm.Member?.LastName}");
                Console.WriteLine($"Абонемент: {mm.Membership?.Name}");
                Console.WriteLine($"Дата окончания: {mm.EndDate:dd.MM.yyyy}");
                Console.WriteLine($"Просрочен на: {(DateTime.Now - mm.EndDate).Days} дней");
            }

            if (!hasExpired)
            {
                Console.WriteLine("Нет просроченных абонементов");
            }

            // Показываем истекающие абонементы
            Console.WriteLine("\nИстекающие абонементы (в течение 7 дней):");
            var expiringMemberships = _fitnessClubService.GetExpiringMemberships(7);
            bool hasExpiring = false;

            foreach (var mm in expiringMemberships)
            {
                hasExpiring = true;
                Console.WriteLine($"\nЧлен клуба: {mm.Member?.FirstName} {mm.Member?.LastName}");
                Console.WriteLine($"Абонемент: {mm.Membership?.Name}");
                Console.WriteLine($"Дата окончания: {mm.EndDate:dd.MM.yyyy}");
                Console.WriteLine($"Осталось дней: {_fitnessClubService.DaysUntilExpiration(mm)}");
            }

            if (!hasExpiring)
            {
                Console.WriteLine("Нет истекающих абонементов");
            }

            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }
    }
}