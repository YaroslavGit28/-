-- �������� ������ ��� ������� ���������� ������-������

-- ������� ����������� (������ ���� ������� ������, ��� ��� �� ��� ��������� ������ �������)
CREATE TABLE IF NOT EXISTS Members (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    FirstName TEXT NOT NULL,
    LastName TEXT NOT NULL,
    Email TEXT UNIQUE,
    JoinDate DATE NOT NULL
);

CREATE TABLE IF NOT EXISTS Memberships (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Duration INTEGER NOT NULL,
    Price DECIMAL(10,2) NOT NULL
);

CREATE TABLE IF NOT EXISTS MemberMemberships (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    MemberId INTEGER NOT NULL,
    MembershipId INTEGER NOT NULL,
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    FOREIGN KEY (MemberId) REFERENCES Members(Id),
    FOREIGN KEY (MembershipId) REFERENCES Memberships(Id)
);

--  
CREATE TABLE Clients (
    ClientId INTEGER PRIMARY KEY AUTOINCREMENT,
    FirstName TEXT NOT NULL,
    LastName TEXT NOT NULL,
    JoinDate TEXT NOT NULL,
    MembershipId INTEGER,
    FOREIGN KEY (MembershipId) REFERENCES Memberships(MembershipId)
);

-- ������� ��������
CREATE TABLE Trainers (
    TrainerId INTEGER PRIMARY KEY AUTOINCREMENT,
    FirstName TEXT NOT NULL,
    LastName TEXT NOT NULL,
    Specialization TEXT
);

-- ������� �����
CREATE TABLE Gyms (
    GymId INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Location TEXT,
    Capacity INTEGER CHECK (Capacity > 0)
);

-- ������� ����������
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

-- ������� ������ �������
CREATE TABLE DietPlans (
    PlanId INTEGER PRIMARY KEY AUTOINCREMENT,
    StartDate TEXT NOT NULL,
    DailyCalories INTEGER NOT NULL CHECK (DailyCalories > 0),
    ClientId INTEGER UNIQUE,
    FOREIGN KEY (ClientId) REFERENCES Clients(ClientId)
);

-- ������� ���������
CREATE TABLE Visits (
    VisitId INTEGER PRIMARY KEY AUTOINCREMENT,
    ClientId INTEGER,
    VisitTime TEXT NOT NULL,
    Duration INTEGER CHECK (Duration > 0),
    ActivityType TEXT,
    FOREIGN KEY (ClientId) REFERENCES Clients(ClientId)
);

-- ������� ����� �������� � ���������� (��� ����� ������-��-������)
CREATE TABLE ClientTrainingSessions (
    ClientId INTEGER,
    SessionId INTEGER,
    PRIMARY KEY (ClientId, SessionId),
    FOREIGN KEY (ClientId) REFERENCES Clients(ClientId),
    FOREIGN KEY (SessionId) REFERENCES TrainingSessions(SessionId)
);

-- �������� �������� ��� ����������� ��������
CREATE INDEX idx_clients_membership ON Clients(MembershipId);
CREATE INDEX idx_trainingsessions_trainer ON TrainingSessions(TrainerId);
CREATE INDEX idx_trainingsessions_gym ON TrainingSessions(GymId);
CREATE INDEX idx_visits_client ON Visits(ClientId);
CREATE INDEX idx_dietplans_client ON DietPlans(ClientId);

-- ������� �������� ������

-- ����������
INSERT INTO Memberships (Type, DurationDays, Price, Benefits) VALUES
('�������', 30, 2000.00, '������ � ������������ ����'),
('��������', 30, 3000.00, '������ � ������������ ���� � ��������� ��������'),
('�������', 30, 5000.00, '������ ������ �� ���� �������');

-- �������
INSERT INTO Trainers (FirstName, LastName, Specialization) VALUES
('����', '������', '������� ����������'),
('�����', '�������', '����'),
('�������', '�������', '��������');

-- ����
INSERT INTO Gyms (Name, Location, Capacity) VALUES
('����������� ���', '������ ����', 50),
('��� ��������� �������', '������ ����', 30),
('��� ���������', '������ ����', 20);

-- �������
INSERT INTO Clients (FirstName, LastName, JoinDate, MembershipId) VALUES
('����', '��������', '2024-01-01', 1),
('�����', '������', '2024-01-02', 2),
('�����', '��������', '2024-01-03', 3);

-- ����������
INSERT INTO TrainingSessions (StartTime, Duration, TrainingType, TrainerId, GymId) VALUES
('2024-01-15 10:00:00', 60, '������� ����������', 1, 1),
('2024-01-15 11:00:00', 60, '����', 2, 2),
('2024-01-15 12:00:00', 45, '��������', 3, 3);

-- ����� �������
INSERT INTO DietPlans (StartDate, DailyCalories, ClientId) VALUES
('2024-01-01', 2000, 1),
('2024-01-02', 2500, 2),
('2024-01-03', 1800, 3);

-- ���������
INSERT INTO Visits (ClientId, VisitTime, Duration, ActivityType) VALUES
(1, '2024-01-15 10:00:00', 60, '����������� ���'),
(2, '2024-01-15 11:00:00', 60, '��������� �������'),
(3, '2024-01-15 12:00:00', 45, '��������');

-- ����� �������� � ����������
INSERT INTO ClientTrainingSessions (ClientId, SessionId) VALUES
(1, 1),
(2, 2),
(3, 3); 