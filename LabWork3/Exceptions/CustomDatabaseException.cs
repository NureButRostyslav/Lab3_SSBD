using System;

namespace StudentManagementApi.Exceptions
{
    // Створення кастомного виключення, яке приймає три аргументи: повідомлення, код помилки та внутрішнє виключення
    public class CustomDatabaseException : Exception
    {
        public int ErrorCode { get; }

        // Конструктор, який приймає повідомлення, код помилки та внутрішнє виключення
        public CustomDatabaseException(string message, int errorCode, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }

        // Конструктор, який приймає тільки повідомлення та код помилки
        public CustomDatabaseException(string message, int errorCode)
            : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}
