 using System;

namespace FitnessClub.Business.Exceptions
{
    /// <summary>
    /// Исключение, возникающее при ошибках в бизнес-логике
    /// </summary>
    public class BusinessException : Exception
    {
        /// <summary>
        /// Создает новый экземпляр исключения
        /// </summary>
        /// <param name="message">Сообщение об ошибке</param>
        public BusinessException(string message) : base(message)
        {
        }

        /// <summary>
        /// Создает новый экземпляр исключения
        /// </summary>
        /// <param name="message">Сообщение об ошибке</param>
        /// <param name="innerException">Внутреннее исключение</param>
        public BusinessException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public class ValidationException : BusinessException
    {
        public ValidationException(string message) : base(message)
        {
        }
    }

    public class DuplicateDataException : BusinessException
    {
        public DuplicateDataException(string message) : base(message)
        {
        }
    }

    public class BusinessRuleException : BusinessException
    {
        public BusinessRuleException(string message) : base(message)
        {
        }
    }

    public class InvalidOperationBusinessException : BusinessException
    {
        public InvalidOperationBusinessException(string message) : base(message)
        {
        }
    }
} 
