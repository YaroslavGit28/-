
## Описание проекта

Система управления библиотекой - это консольное приложение на C# с использованием PostgreSQL для учета книг, читателей и операций выдачи/возврата.

## Структура базы данных

### Основные таблицы

1. **books** - информация о книгах
   - `book_id` (PK) - уникальный идентификатор
   - `isbn` - международный номер
   - `title` - название книги
   - `year_published` - год издания
   - `publisher_id` (FK) - ссылка на издателя

2. **authors** - авторы книг
   - `author_id` (PK)
   - `first_name`, `last_name` - имя и фамилия
   - `birth_date` - дата рождения

3. **book_copies** - экземпляры книг
   - `copy_id` (PK)
   - `book_id` (FK) - ссылка на книгу
   - `status` - статус (доступен, выдан и т.д.)

4. **readers** - зарегистрированные читатели
   - `reader_id` (PK)
   - `first_name`, `last_name`
   - `email`, `phone`

### Связи между таблицами

- **Один-ко-многим**:
  - Книга → Экземпляры (1 книга → много экземпляров)
  - Издатель → Книги (1 издатель → много книг)

- **Многие-ко-многим**:
  - Книги ↔ Авторы (через book_authors)
  - Книги ↔ Жанры (через book_genres)

- **Один-к-одному**:
  - Читатель ↔ Библиотечная карта

## Установка и настройка

1. Установите PostgreSQL (версия 12+)
2. Создайте базу данных:
   ```bash
   createdb library_db
   ```
3. Загрузите схему:
   ```bash
   psql -d library_db -f database/01_schema.sql
   ```
4. Заполните тестовыми данными:
   ```bash
   psql -d library_db -f database/02_data.sql
   ```

## Примеры запросов

### Поиск доступных книг по автору
```sql
SELECT b.title, a.last_name 
FROM books b
JOIN book_authors ba ON b.book_id = ba.book_id
JOIN authors a ON ba.author_id = a.author_id
WHERE a.last_name = 'Азимов';
```

### Количество книг в каждом жанре
```sql
SELECT g.name, COUNT(bg.book_id) 
FROM genres g
LEFT JOIN book_genres bg ON g.genre_id = bg.genre_id
GROUP BY g.name;
```

## Структура репозитория

```
/library-management-system
│
├── database/               # Скрипты базы данных
│   ├── 01_schema.sql       # Создание структуры
│   ├── 02_data.sql        # Тестовые данные
│   └── 03_indexes.sql     # Оптимизационные индексы
│
├── src/                    # Исходный код приложения
│   ├── Models/             # Классы сущностей
│   ├── Services/           # Бизнес-логика
│   └── Program.cs          # Точка входа
│
├── docs/                   # Документация
│   └── ER-diagram.png      # Диаграмма отношений
│
└── README.md               # Этот файл
```

