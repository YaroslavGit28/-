using System;

namespace FitnessClub.Data.Exceptions
{
    /// <summary>
    /// Исключение, возникающее при работе с базой данных
    /// </summary>
    public class DatabaseException : Exception
    {
        /// <summary>
        /// Создает новый экземпляр исключения
        /// </summary>
        /// <param name="message">Сообщение об ошибке</param>
        public DatabaseException(string message) : base(message)
        {
        }

        /// <summary>
        /// Создает новый экземпляр исключения
        /// </summary>
        /// <param name="message">Сообщение об ошибке</param>
        /// <param name="innerException">Внутреннее исключение</param>
        public DatabaseException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public class EntityNotFoundException : DatabaseException
    {
        public EntityNotFoundException(string entityName, object id) 
            : base($"{entityName} с ID {id} не найден.")
        {
        }
    }

    public class ConnectionException : DatabaseException
    {
        public ConnectionException(string message, Exception innerException) 
            : base($"Ошибка подключения к базе данных: {message}", innerException)
        {
        }
    }

    public class QueryException : DatabaseException
    {
        public QueryException(string message, Exception innerException) 
            : base($"Ошибка выполнения запроса: {message}", innerException)
        {
        }
    }
}  
