# Проектирование базы данных

## Схема базы данных

База данных состоит из следующих таблиц:

### 1. Clients (Клиенты)
```sql
CREATE TABLE Clients (
    ClientId INTEGER PRIMARY KEY AUTOINCREMENT,
    FirstName TEXT NOT NULL,
    LastName TEXT NOT NULL,
    JoinDate TEXT NOT NULL,
    MembershipId INTEGER,
    FOREIGN KEY (MembershipId) REFERENCES Memberships(MembershipId)
);
```
- Связь "один-к-одному" с DietPlans (один клиент может иметь один план питания)
- Связь "один-ко-многим" с Visits (один клиент может иметь много посещений)
- Связь "многие-ко-многим" с TrainingSessions через ClientTrainingSessions

### 2. Trainers (Тренеры)
```sql
CREATE TABLE Trainers (
    TrainerId INTEGER PRIMARY KEY AUTOINCREMENT,
    FirstName TEXT NOT NULL,
    LastName TEXT NOT NULL,
    Specialization TEXT
);
```
- Связь "один-ко-многим" с TrainingSessions (один тренер может вести много тренировок)

### 3. Memberships (Абонементы)
```sql
CREATE TABLE Memberships (
    MembershipId INTEGER PRIMARY KEY AUTOINCREMENT,
    Type TEXT UNIQUE NOT NULL,
    DurationDays INTEGER NOT NULL CHECK (DurationDays > 0),
    Price REAL NOT NULL CHECK (Price >= 0),
    Benefits TEXT
);
```
- Связь "один-ко-многим" с Clients (один тип абонемента может быть у многих клиентов)

### 4. TrainingSessions (Тренировки)
```sql
CREATE TABLE TrainingSessions (
    SessionId INTEGER PRIMARY KEY AUTOINCREMENT,
    StartTime TEXT NOT NULL,
    Duration INTEGER NOT NULL CHECK (Duration > 0),
    TrainingType TEXT NOT NULL,
    TrainerId INTEGER,
    GymId INTEGER,
    FOREIGN KEY (TrainerId) REFERENCES Trainers(TrainerId),
    FOREIGN KEY (GymId) REFERENCES Gyms(GymId)
);
```
- Связь "многие-к-одному" с Trainers (много тренировок может вести один тренер)
- Связь "многие-к-одному" с Gyms (много тренировок может проходить в одном зале)
- Связь "многие-ко-многим" с Clients через ClientTrainingSessions

### 5. DietPlans (Планы питания)
```sql
CREATE TABLE DietPlans (
    PlanId INTEGER PRIMARY KEY AUTOINCREMENT,
    StartDate TEXT NOT NULL,
    DailyCalories INTEGER NOT NULL CHECK (DailyCalories > 0),
    ClientId INTEGER UNIQUE,
    FOREIGN KEY (ClientId) REFERENCES Clients(ClientId)
);
```
- Связь "один-к-одному" с Clients (один план питания принадлежит одному клиенту)

### 6. Visits (Посещения)
```sql
CREATE TABLE Visits (
    VisitId INTEGER PRIMARY KEY AUTOINCREMENT,
    ClientId INTEGER,
    VisitTime TEXT NOT NULL,
    Duration INTEGER CHECK (Duration > 0),
    ActivityType TEXT,
    FOREIGN KEY (ClientId) REFERENCES Clients(ClientId)
);
```
- Связь "многие-к-одному" с Clients (много посещений может быть у одного клиента)

### 7. Gyms (Залы)
```sql
CREATE TABLE Gyms (
    GymId INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Location TEXT,
    Capacity INTEGER CHECK (Capacity > 0)
);
```
- Связь "один-ко-многим" с TrainingSessions (в одном зале может проходить много тренировок)

### 8. ClientTrainingSessions (Связь клиентов и тренировок)
```sql
CREATE TABLE ClientTrainingSessions (
    ClientId INTEGER,
    SessionId INTEGER,
    PRIMARY KEY (ClientId, SessionId),
    FOREIGN KEY (ClientId) REFERENCES Clients(ClientId),
    FOREIGN KEY (SessionId) REFERENCES TrainingSessions(SessionId)
);
```
- Таблица для реализации связи "многие-ко-многим" между Clients и TrainingSessions

## Типы связей

1. **Один-к-одному**:
   - Clients <-> DietPlans (один клиент имеет один план питания)

2. **Один-ко-многим**:
   - Memberships -> Clients (один тип абонемента может быть у многих клиентов)
   - Trainers -> TrainingSessions (один тренер может вести много тренировок)
   - Gyms -> TrainingSessions (в одном зале может проходить много тренировок)
   - Clients -> Visits (один клиент может иметь много посещений)

3. **Многие-ко-многим**:
   - Clients <-> TrainingSessions (через таблицу ClientTrainingSessions)

## Индексы

```sql
CREATE INDEX idx_clients_membership ON Clients(MembershipId);
CREATE INDEX idx_trainingsessions_trainer ON TrainingSessions(TrainerId);
CREATE INDEX idx_trainingsessions_gym ON TrainingSessions(GymId);
CREATE INDEX idx_visits_client ON Visits(ClientId);
CREATE INDEX idx_dietplans_client ON DietPlans(ClientId);
```

## Ограничения

1. **Уникальные значения**:
   - Memberships.Type (уникальные типы абонементов)
   - DietPlans.ClientId (один план питания на клиента)

2. **Проверки значений**:
   - Memberships.DurationDays > 0
   - Memberships.Price >= 0
   - TrainingSessions.Duration > 0
   - DietPlans.DailyCalories > 0
   - Visits.Duration > 0
   - Gyms.Capacity > 0

3. **Внешние ключи**:
   - Все связи между таблицами обеспечены внешними ключами

## Примеры запросов

1. Получение информации о клиенте и его текущем абонементе:
```sql
SELECT c.FirstName, c.LastName, m.Type, m.DurationDays, m.Benefits
FROM Clients c
LEFT JOIN Memberships m ON c.MembershipId = m.MembershipId
WHERE c.ClientId = ?;
```

2. Получение расписания тренера на день:
```sql
SELECT ts.StartTime, ts.Duration, ts.TrainingType, g.Name as GymName
FROM TrainingSessions ts
JOIN Gyms g ON ts.GymId = g.GymId
WHERE ts.TrainerId = ? AND date(ts.StartTime) = date(?);
```

3. Подсчет посещений клиента за период:
```sql
SELECT COUNT(*) as VisitCount
FROM Visits
WHERE ClientId = ? AND date(VisitTime) BETWEEN date(?) AND date(?);
```

4. Получение загруженности залов:
```sql
SELECT g.Name, g.Capacity, COUNT(ts.SessionId) as SessionCount
FROM Gyms g
LEFT JOIN TrainingSessions ts ON g.GymId = ts.GymId
WHERE date(ts.StartTime) = date(?)
GROUP BY g.GymId;
``` 