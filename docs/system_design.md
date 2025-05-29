# Система управления фитнес-клубом

## Описание проблемы
Система управления фитнес-клубом предназначена для автоматизации основных бизнес-процессов фитнес-клуба, включая управление клиентами, тренерами, абонементами, тренировками и посещениями. Система решает следующие проблемы:
- Учет клиентов и их абонементов
- Планирование тренировок и распределение нагрузки на тренеров
- Контроль посещаемости и использования услуг
- Управление диетическими планами клиентов
- Мониторинг загруженности залов

## Ключевые функции
1. Управление клиентами:
   - Регистрация новых клиентов
   - Продление абонементов
   - Отслеживание посещений
   - Назначение персональных тренировок

2. Управление тренерами:
   - Учет специализации и квалификации
   - Составление расписания работы
   - Назначение на тренировки

3. Управление абонементами:
   - Различные типы абонементов
   - Учет срока действия
   - Контроль оплаты

4. Управление тренировками:
   - Планирование групповых занятий
   - Запись на персональные тренировки
   - Контроль загруженности залов

5. Управление диетическими планами:
   - Создание индивидуальных планов питания
   - Отслеживание прогресса

## UML-диаграммы

### 1. Диаграмма вариантов использования (Use Case Diagram)
```plantuml
@startuml
left to right direction
actor "Клиент" as Client
actor "Тренер" as Trainer
actor "Администратор" as Admin

rectangle "Система управления фитнес-клубом" {
  usecase "Регистрация" as UC1
  usecase "Покупка абонемента" as UC2
  usecase "Запись на тренировку" as UC3
  usecase "Просмотр расписания" as UC4
  usecase "Получение диет-плана" as UC5
  usecase "Проведение тренировки" as UC6
  usecase "Составление программы" as UC7
  usecase "Управление клиентами" as UC8
  usecase "Управление тренерами" as UC9
  usecase "Управление абонементами" as UC10
}

Client --> UC1
Client --> UC2
Client --> UC3
Client --> UC4
Client --> UC5
Trainer --> UC6
Trainer --> UC7
Trainer --> UC4
Admin --> UC8
Admin --> UC9
Admin --> UC10
@enduml
```

### 2. Диаграмма классов (Class Diagram)
```plantuml
@startuml
class Client {
  +ClientId: int
  +FirstName: string
  +LastName: string
  +JoinDate: DateTime
  +GetCurrentMembership(): Membership
  +AddVisit(visit: Visit): void
}

class Trainer {
  +TrainerId: int
  +FirstName: string
  +LastName: string
  +Specialization: string
  +GetSchedule(): List<TrainingSession>
}

class Membership {
  +MembershipId: int
  +Type: string
  +DurationDays: int
  +Price: decimal
  +Benefits: string
  +IsValid(): bool
}

class TrainingSession {
  +SessionId: int
  +StartTime: DateTime
  +Duration: int
  +TrainingType: string
  +GetTrainer(): Trainer
  +GetGym(): Gym
}

class DietPlan {
  +PlanId: int
  +StartDate: DateTime
  +DailyCalories: int
  +GetClient(): Client
}

class Visit {
  +VisitId: int
  +VisitTime: DateTime
  +Duration: int
  +ActivityType: string
  +GetClient(): Client
}

class Gym {
  +GymId: int
  +Name: string
  +Location: string
  +Capacity: int
  +GetCurrentOccupancy(): int
}

Client "1" -- "0..1" Membership
Client "1" -- "*" Visit
Client "1" -- "0..1" DietPlan
Trainer "1" -- "*" TrainingSession
TrainingSession "*" -- "1" Gym
@enduml
```

### 3. Диаграмма последовательностей (Sequence Diagram)
```plantuml
@startuml
actor Client
participant "UI" as UI
participant "ClientService" as CS
participant "MembershipService" as MS
participant "Database" as DB

Client -> UI: Запрос на покупку абонемента
activate UI

UI -> CS: ValidateClient(clientId)
activate CS
CS -> DB: GetClient(clientId)
DB --> CS: ClientData
CS --> UI: ClientValidated
deactivate CS

UI -> MS: PurchaseMembership(clientId, membershipType)
activate MS
MS -> DB: GetMembershipType(type)
DB --> MS: MembershipData
MS -> DB: CreateMembership(clientId, membershipData)
DB --> MS: Success
MS --> UI: MembershipCreated
deactivate MS

UI --> Client: Подтверждение покупки
deactivate UI
@enduml
```

## Обоснование выбора диаграмм

1. **Диаграмма вариантов использования** выбрана для наглядного представления всех возможных действий пользователей системы. Она помогает понять, какие функции должны быть реализованы для каждой роли пользователя.

2. **Диаграмма классов** показывает структуру системы, включая все основные сущности, их атрибуты и методы, а также связи между ними. Это ключевая диаграмма для понимания архитектуры приложения и его дальнейшей реализации.

3. **Диаграмма последовательностей** демонстрирует взаимодействие между компонентами системы на примере типичного сценария использования - покупки абонемента. Она помогает понять, как различные слои приложения взаимодействуют друг с другом.

## Архитектурные решения

1. **Трехслойная архитектура**:
   - Слой представления (UI)
   - Бизнес-слой (Services)
   - Слой данных (Repositories)

2. **Паттерны проектирования**:
   - Repository Pattern для работы с базой данных
   - Service Pattern для бизнес-логики
   - Factory Pattern для создания объектов

3. **Принципы SOLID**:
   - Single Responsibility Principle: каждый класс отвечает за одну конкретную функциональность
   - Open/Closed Principle: возможность расширения функционала без изменения существующего кода
   - Interface Segregation: использование специализированных интерфейсов
   - Dependency Inversion: зависимость от абстракций, а не от конкретных реализаций 