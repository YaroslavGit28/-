# Этап 3. Разработка моделей данных

## Описание
На этом этапе разработаны классы моделей, представляющие сущности предметной области. Модели инкапсулируют данные и обеспечивают их валидацию.

## Модели

### 1. Member (Член клуба)
```csharp
public class Member
{
    public int Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? Email { get; set; }
    public DateTime JoinDate { get; set; }
}
```

### 2. Membership (Абонемент)
```csharp
public class Membership
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int Duration { get; set; }
    public decimal Price { get; set; }
}
```

### 3. MemberMembership (Привязка абонемента)
```csharp
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
```

## Особенности реализации

1. **Типы данных**
   - Использование nullable типов (string?)
   - Строгая типизация (int, decimal, DateTime)
   - Обязательные поля помечены как required

2. **Навигационные свойства**
   - Связи между сущностями
   - Ленивая загрузка связанных данных
   - Nullable ссылки для опциональных связей

3. **Валидация**
   - Проверка обязательных полей
   - Валидация на уровне бизнес-логики
   - Защита от некорректных данных

4. **Инкапсуляция**
   - Публичные свойства для доступа к данным
   - Приватные поля для хранения
   - Контроль над изменением данных

## Взаимосвязи моделей

```
Member (1) ←→ (N) MemberMembership (N) ←→ (1) Membership
```

- Один член клуба может иметь несколько абонементов
- Один абонемент может быть привязан к разным членам клуба
- MemberMembership связывает Member и Membership

## Использование в приложении

1. **Создание объектов**
```csharp
var member = new Member
{
    FirstName = "Иван",
    LastName = "Петров",
    Email = "ivan@example.com",
    JoinDate = DateTime.Now
};
```

2. **Работа со связями**
```csharp
var memberMembership = new MemberMembership
{
    Member = member,
    Membership = membership,
    StartDate = DateTime.Now,
    EndDate = DateTime.Now.AddDays(30)
};
```

## Преимущества подхода

1. **Типобезопасность**
   - Компилятор проверяет типы
   - Меньше ошибок времени выполнения
   - Улучшенная поддержка IDE

2. **Чистый код**
   - Понятная структура данных
   - Легкость поддержки
   - Хорошая читаемость

3. **Расширяемость**
   - Простота добавления новых свойств
   - Легкость модификации связей
   - Возможность добавления новых моделей 