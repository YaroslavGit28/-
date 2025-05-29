-- Создание таблиц для системы управления фитнес-клубом

-- Таблица абонементов (должна быть создана первой, так как на нее ссылаются другие таблицы)
CREATE TABLE Memberships (
    MembershipId INTEGER PRIMARY KEY AUTOINCREMENT,
    Type TEXT UNIQUE NOT NULL,
    DurationDays INTEGER NOT NULL CHECK (DurationDays > 0),
    Price REAL NOT NULL CHECK (Price >= 0),
    Benefits TEXT
);

-- Таблица клиентов
CREATE TABLE Clients (
    ClientId INTEGER PRIMARY KEY AUTOINCREMENT,
    FirstName TEXT NOT NULL,
    LastName TEXT NOT NULL,
    JoinDate TEXT NOT NULL,
    MembershipId INTEGER,
    FOREIGN KEY (MembershipId) REFERENCES Memberships(MembershipId)
);

-- Таблица тренеров
CREATE TABLE Trainers (
    TrainerId INTEGER PRIMARY KEY AUTOINCREMENT,
    FirstName TEXT NOT NULL,
    LastName TEXT NOT NULL,
    Specialization TEXT
);

-- Таблица залов
CREATE TABLE Gyms (
    GymId INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Location TEXT,
    Capacity INTEGER CHECK (Capacity > 0)
);

-- Таблица тренировок
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

-- Таблица планов питания
CREATE TABLE DietPlans (
    PlanId INTEGER PRIMARY KEY AUTOINCREMENT,
    StartDate TEXT NOT NULL,
    DailyCalories INTEGER NOT NULL CHECK (DailyCalories > 0),
    ClientId INTEGER UNIQUE,
    FOREIGN KEY (ClientId) REFERENCES Clients(ClientId)
);

-- Таблица посещений
CREATE TABLE Visits (
    VisitId INTEGER PRIMARY KEY AUTOINCREMENT,
    ClientId INTEGER,
    VisitTime TEXT NOT NULL,
    Duration INTEGER CHECK (Duration > 0),
    ActivityType TEXT,
    FOREIGN KEY (ClientId) REFERENCES Clients(ClientId)
);

-- Таблица связи клиентов и тренировок (для связи многие-ко-многим)
CREATE TABLE ClientTrainingSessions (
    ClientId INTEGER,
    SessionId INTEGER,
    PRIMARY KEY (ClientId, SessionId),
    FOREIGN KEY (ClientId) REFERENCES Clients(ClientId),
    FOREIGN KEY (SessionId) REFERENCES TrainingSessions(SessionId)
);

-- Создание индексов для оптимизации запросов
CREATE INDEX idx_clients_membership ON Clients(MembershipId);
CREATE INDEX idx_trainingsessions_trainer ON TrainingSessions(TrainerId);
CREATE INDEX idx_trainingsessions_gym ON TrainingSessions(GymId);
CREATE INDEX idx_visits_client ON Visits(ClientId);
CREATE INDEX idx_dietplans_client ON DietPlans(ClientId);

-- Вставка тестовых данных

-- Абонементы
INSERT INTO Memberships (Type, DurationDays, Price, Benefits) VALUES
('Базовый', 30, 2000.00, 'Доступ к тренажерному залу'),
('Стандарт', 30, 3000.00, 'Доступ к тренажерному залу и групповым занятиям'),
('Премиум', 30, 5000.00, 'Полный доступ ко всем услугам');

-- Тренеры
INSERT INTO Trainers (FirstName, LastName, Specialization) VALUES
('Иван', 'Петров', 'Силовые тренировки'),
('Мария', 'Иванова', 'Йога'),
('Алексей', 'Сидоров', 'Кроссфит');

-- Залы
INSERT INTO Gyms (Name, Location, Capacity) VALUES
('Тренажерный зал', 'Первый этаж', 50),
('Зал групповых занятий', 'Второй этаж', 30),
('Зал кроссфита', 'Первый этаж', 20);

-- Клиенты
INSERT INTO Clients (FirstName, LastName, JoinDate, MembershipId) VALUES
('Анна', 'Смирнова', '2024-01-01', 1),
('Павел', 'Козлов', '2024-01-02', 2),
('Елена', 'Морозова', '2024-01-03', 3);

-- Тренировки
INSERT INTO TrainingSessions (StartTime, Duration, TrainingType, TrainerId, GymId) VALUES
('2024-01-15 10:00:00', 60, 'Силовая тренировка', 1, 1),
('2024-01-15 11:00:00', 60, 'Йога', 2, 2),
('2024-01-15 12:00:00', 45, 'Кроссфит', 3, 3);

-- Планы питания
INSERT INTO DietPlans (StartDate, DailyCalories, ClientId) VALUES
('2024-01-01', 2000, 1),
('2024-01-02', 2500, 2),
('2024-01-03', 1800, 3);

-- Посещения
INSERT INTO Visits (ClientId, VisitTime, Duration, ActivityType) VALUES
(1, '2024-01-15 10:00:00', 60, 'Тренажерный зал'),
(2, '2024-01-15 11:00:00', 60, 'Групповое занятие'),
(3, '2024-01-15 12:00:00', 45, 'Кроссфит');

-- Связи клиентов и тренировок
INSERT INTO ClientTrainingSessions (ClientId, SessionId) VALUES
(1, 1),
(2, 2),
(3, 3); 