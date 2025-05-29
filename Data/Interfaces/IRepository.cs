namespace FitnessClub.Data.Interfaces;

/// <summary>
/// Базовый интерфейс репозитория для работы с сущностями
/// </summary>
/// <typeparam name="T">Тип сущности</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Получить все записи
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// Получить запись по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор записи</param>
    Task<T?> GetByIdAsync(int id);

    /// <summary>
    /// Добавить новую запись
    /// </summary>
    /// <param name="entity">Новая сущность</param>
    Task<int> AddAsync(T entity);

    /// <summary>
    /// Обновить существующую запись
    /// </summary>
    /// <param name="entity">Обновленная сущность</param>
    Task UpdateAsync(T entity);

    /// <summary>
    /// Удалить запись
    /// </summary>
    /// <param name="id">Идентификатор записи</param>
    Task DeleteAsync(int id);
} 