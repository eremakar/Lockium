using Data.Mapping;
using Data.Repository.Helpers;
using Lockium.Data.LockiumDb.Entities;
using Lockium.Models.Dtos;
using Newtonsoft.Json;
using Data.Repository.Helpers;

namespace Lockium.Mappings
{
    public partial class UserMap : MapBase2<User, UserDto, MapOptions>
    {
        private readonly DbMapContext mapContext;

        public UserMap(DbMapContext mapContext)
        {
            this.mapContext = mapContext;
        }

        public override UserDto MapCore(User source, MapOptions? options = null)
        {
            if (source == null)
                return null;

            options = options ?? new MapOptions();

            var result = new UserDto();
            result.Id = source.Id;
            if (options.MapProperties)
            {
                result.UserName = source.UserName;
                result.PasswordHash = source.PasswordHash;
                result.IsActive = source.IsActive;
                result.ProtectFromBruteforceAttempts = source.ProtectFromBruteforceAttempts;
                result.FullName = source.FullName;
                result.PositionName = source.PositionName;
                result.State = source.State;
                result.RegistrationToken = source.RegistrationToken;
                result.BlockExpiration = source.BlockExpiration;
                result.PushToken = source.PushToken;
                result.SignalrToken = source.SignalrToken;
                result.PinCode = source.PinCode;
                result.PinCodeExpiration = source.PinCodeExpiration;
                result.Avatar = source.Avatar;
                result.FailedLoginCount = source.FailedLoginCount;
                result.RefreshToken = source.RefreshToken;
            }
            if (options.MapObjects)
            {
            }
            if (options.MapCollections)
            {
                result.Roles = mapContext.UserRoleMap.Map(source.Roles, options);
            }

            return result;
        }

        public override User ReverseMapCore(UserDto source, MapOptions options = null)
        {
            if (source == null)
                return null;

            options = options ?? new MapOptions();

            var result = new User();
            result.Id = source.Id;
            if (options.MapProperties)
            {
                result.UserName = source.UserName;
                result.PasswordHash = source.PasswordHash;
                result.IsActive = source.IsActive;
                result.ProtectFromBruteforceAttempts = source.ProtectFromBruteforceAttempts;
                result.FullName = source.FullName;
                result.PositionName = source.PositionName;
                result.State = source.State;
                result.RegistrationToken = source.RegistrationToken;
                result.BlockExpiration = source.BlockExpiration.ToUtc();
                result.PushToken = source.PushToken;
                result.SignalrToken = source.SignalrToken;
                result.PinCode = source.PinCode;
                result.PinCodeExpiration = source.PinCodeExpiration.ToUtc();
                if (source.Avatar != null)
                    result.Avatar = JsonConvert.SerializeObject(source.Avatar);
                result.FailedLoginCount = source.FailedLoginCount;
                result.RefreshToken = source.RefreshToken;
            }
            if (options.MapObjects)
            {
            }
            if (options.MapCollections)
            {
                result.Roles = mapContext.UserRoleMap.ReverseMap(source.Roles, options);
            }

            return result;
        }

        public override void MapCore(User source, User destination, MapOptions options = null)
        {
            if (source == null || destination == null)
                return;

            options = options ?? new MapOptions();

            destination.Id = source.Id;
            if (options.MapProperties)
            {
                destination.UserName = source.UserName;
                destination.PasswordHash = source.PasswordHash;
                destination.IsActive = source.IsActive;
                destination.ProtectFromBruteforceAttempts = source.ProtectFromBruteforceAttempts;
                destination.FullName = source.FullName;
                destination.PositionName = source.PositionName;
                destination.State = source.State;
                destination.RegistrationToken = source.RegistrationToken;
                destination.BlockExpiration = source.BlockExpiration;
                destination.PushToken = source.PushToken;
                destination.SignalrToken = source.SignalrToken;
                destination.PinCode = source.PinCode;
                destination.PinCodeExpiration = source.PinCodeExpiration;
                destination.Avatar = JsonHelper.NormalizeSafe(source.Avatar);
                destination.FailedLoginCount = source.FailedLoginCount;
                destination.RefreshToken = source.RefreshToken;
            }
            if (options.MapObjects)
            {
            }
            if (options.MapCollections)
            {
            }

        }
    }
}
