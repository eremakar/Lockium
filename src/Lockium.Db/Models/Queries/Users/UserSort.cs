using Data.Repository;
using Lockium.Data.LockiumDb.Entities;

namespace Lockium.Models.Queries.Users
{
    public partial class UserSort : SortBase<User>
    {
        public SortOperand? Id { get; set; }
        /// <summary>
        /// Имя пользователя
        /// </summary>
        public SortOperand? UserName { get; set; }
        /// <summary>
        /// Хеш пароля
        /// </summary>
        public SortOperand? PasswordHash { get; set; }
        /// <summary>
        /// Активен
        /// </summary>
        public SortOperand? IsActive { get; set; }
        /// <summary>
        /// Количество попыток входа
        /// </summary>
        public SortOperand? ProtectFromBruteforceAttempts { get; set; }
        /// <summary>
        /// ФИО
        /// </summary>
        public SortOperand? FullName { get; set; }
        /// <summary>
        /// Название должности
        /// </summary>
        public SortOperand? PositionName { get; set; }
        /// <summary>
        /// Статус: 1 - ожидает кода регистрации, 2 - зарегистрирован, 3 - ввести пин код
        /// </summary>
        public SortOperand? State { get; set; }
        /// <summary>
        /// Код регистрации
        /// </summary>
        public SortOperand? RegistrationToken { get; set; }
        /// <summary>
        /// Время истечения блокировки
        /// </summary>
        public SortOperand? BlockExpiration { get; set; }
        /// <summary>
        /// Пуш токен
        /// </summary>
        public SortOperand? PushToken { get; set; }
        /// <summary>
        /// Signalr токен
        /// </summary>
        public SortOperand? SignalrToken { get; set; }
        /// <summary>
        /// Пин код
        /// </summary>
        public SortOperand? PinCode { get; set; }
        /// <summary>
        /// Время истечения пин кода
        /// </summary>
        public SortOperand? PinCodeExpiration { get; set; }
        /// <summary>
        /// S3 файл аватара
        /// </summary>
        public SortOperand? Avatar { get; set; }
        /// <summary>
        /// Защита от взлома
        /// </summary>
        public SortOperand? FailedLoginCount { get; set; }
        /// <summary>
        /// Рефреш токен
        /// </summary>
        public SortOperand? RefreshToken { get; set; }
    }
}
