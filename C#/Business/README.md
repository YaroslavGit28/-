# Этап 5. Реализация бизнес-логики

## Описание
На этом этапе реализована бизнес-логика приложения, которая обеспечивает выполнение бизнес-правил и координирует работу между слоем представления и слоем данных.

## Основные компоненты

### 1. Интерфейс IFitnessClubService
```csharp
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
    
    // Бизнес-операции
    bool HasActiveMembership(int memberId);
    DateTime CalculateMembershipEndDate(int membershipId, DateTime startDate);
    decimal CalculateTotalRevenue(DateTime startDate, DateTime endDate);
}
```

### 2. Класс FitnessClubService
Реализует бизнес-логику и координирует работу с репозиториями.

### 3. Класс BusinessException
Обрабатывает ошибки бизнес-логики.

## Бизнес-правила

1. **Управление членами клуба**
   - Нельзя удалить члена клуба с активным абонементом
   - Имя и фамилия обязательны
   - Дата регистрации не может быть в будущем

2. **Управление абонементами**
   - Цена должна быть положительной
   - Длительность должна быть больше нуля
   - Нельзя удалить абонемент с активными привязками

3. **Привязка абонементов**
   - У члена клуба может быть только один активный абонемент
   - Дата начала не может быть в прошлом
   - Автоматический расчет даты окончания

## Особенности реализации

1. **Валидация данных**
```csharp
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
```

2. **Обработка ошибок**
```csharp
try
{
    _memberRepository.Add(member);
}
catch (DatabaseException ex)
{
    throw new BusinessException("Ошибка при добавлении члена клуба", ex);
}
```

3. **Бизнес-операции**
```csharp
public decimal CalculateTotalRevenue(DateTime startDate, DateTime endDate)
{
    var activeMemberships = GetActiveMemberships()
        .Where(mm => mm.StartDate >= startDate && mm.StartDate <= endDate);

    return activeMemberships.Sum(mm => mm.Membership?.Price ?? 0);
}
```

## Преимущества подхода

1. **Изоляция бизнес-логики**
   - Отделение от UI и доступа к данным
   - Централизованная валидация
   - Единое место для бизнес-правил

2. **Улучшенная поддерживаемость**
   - Легкость модификации правил
   - Понятная структура кода
   - Простота тестирования

3. **Безопасность**
   - Контроль доступа к данным
   - Проверка всех операций
   - Защита от некорректных действий

4. **Расширяемость**
   - Простота добавления новых правил
   - Легкость внедрения новых функций
   - Гибкость в модификации логики 