
namespace Lockium.Models.Dtos
{
    public partial class UserDto
    {
        public int Id { get; set; }
        /// <summary>
        /// Имя пользователя
        /// </summary>
        public string? UserName { get; set; }
        /// <summary>
        /// Хеш пароля
        /// </summary>
        public string? PasswordHash { get; set; }
        /// <summary>
        /// Активен
        /// </summary>
        public bool IsActive { get; set; }
        /// <summary>
        /// Количество попыток входа
        /// </summary>
        public int ProtectFromBruteforceAttempts { get; set; }
        /// <summary>
        /// ФИО
        /// </summary>
        public string? FullName { get; set; }
        /// <summary>
        /// Название должности
        /// </summary>
        public string? PositionName { get; set; }
        /// <summary>
        /// Статус: 1 - ожидает кода регистрации, 2 - зарегистрирован, 3 - ввести пин код
        /// </summary>
        public int State { get; set; }
        /// <summary>
        /// Код регистрации
        /// </summary>
        public string? RegistrationToken { get; set; }
        /// <summary>
        /// Время истечения блокировки
        /// </summary>
        public DateTime BlockExpiration { get; set; }
        /// <summary>
        /// Пуш токен
        /// </summary>
        public string? PushToken { get; set; }
        /// <summary>
        /// Signalr токен
        /// </summary>
        public string? SignalrToken { get; set; }
        /// <summary>
        /// Пин код
        /// </summary>
        public string? PinCode { get; set; }
        /// <summary>
        /// Время истечения пин кода
        /// </summary>
        public DateTime PinCodeExpiration { get; set; }
        /// <summary>
        /// S3 файл аватара
        /// </summary>
        public object? Avatar { get; set; }
        /// <summary>
        /// Защита от взлома
        /// </summary>
        public int FailedLoginCount { get; set; }
        /// <summary>
        /// Рефреш токен
        /// </summary>
        public string? RefreshToken { get; set; }

        public List<UserRoleDto>? Roles { get; set; }
    }
}
