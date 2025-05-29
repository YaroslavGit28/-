# Этап 1. Проектирование базы данных

## Описание
На данном этапе была спроектирована и реализована структура базы данных SQLite для хранения информации о членах клуба, абонементах и их взаимосвязях.

## Структура базы данных

### Таблица Members
Хранит информацию о членах клуба
```sql
CREATE TABLE Members (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    FirstName TEXT NOT NULL,
    LastName TEXT NOT NULL,
    Email TEXT,
    JoinDate TEXT NOT NULL
);
```

### Таблица Memberships
Содержит типы абонементов
```sql
CREATE TABLE Memberships (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Duration INTEGER NOT NULL,
    Price DECIMAL NOT NULL
);
```

### Таблица MemberMemberships
Связывает членов клуба с их абонементами
```sql
CREATE TABLE MemberMemberships (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    MemberId INTEGER NOT NULL,
    MembershipId INTEGER NOT NULL,
    StartDate TEXT NOT NULL,
    EndDate TEXT NOT NULL,
    FOREIGN KEY (MemberId) REFERENCES Members(Id),
    FOREIGN KEY (MembershipId) REFERENCES Memberships(Id)
);
```

## Особенности реализации

1. **Выбор СУБД**
   - Использована SQLite как встраиваемая база данных
   - Не требует установки отдельного сервера
   - Данные хранятся в одном файле

2. **Типы данных**
   - INTEGER для идентификаторов
   - TEXT для строковых данных
   - DECIMAL для денежных значений
   - Даты хранятся в формате TEXT для совместимости

3. **Ограничения**
   - NOT NULL для обязательных полей
   - Внешние ключи для обеспечения целостности данных
   - AUTOINCREMENT для автоматической генерации ID

4. **Связи между таблицами**
   - Members (1) ← → (N) MemberMemberships
   - Memberships (1) ← → (N) MemberMemberships

## Инициализация базы данных

База данных автоматически создается при первом запуске приложения. Скрипт инициализации выполняется в методе `InitializeDatabase()` класса `Program`. 