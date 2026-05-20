using Api.AspNetCore.Models.Secure;
using Api.AspNetCore.Services;
using Data.Repository.Helpers;

namespace Lockium.Services.Auth
{
    public class MicroserviceUserManagementService : UserManagementService
    {
        private readonly IUserService userService;
        private readonly IRoleService roleService;
        private readonly ILogger<MicroserviceUserManagementService> logger;

        public MicroserviceUserManagementService(IUserService userService, IRoleService roleService, ILogger<MicroserviceUserManagementService> logger)
        {
            this.userService = userService;
            this.roleService = roleService;
            this.logger = logger;
        }

        public override async Task<JwtToken> CheckAttempts(string username, string isValidMessage)
        {
            return new JwtToken();
        }

        public override async Task<User?> GetUserByLogin(string login)
        {
            var original = await userService.FindByUserName(login);

            if (original == null)
                return null;

            return new User
            {
                Id = original.Id.ToString(),
                Login = original.UserName
            };
        }

        public override async Task<IEnumerable<UserRole>> GetRoles(string login)
        {
            var original = await roleService.ByUserName(login);

            if (original == null)
                return new List<UserRole>();

            return new List<UserRole> { new UserRole { Name = original.Code } };
        }

        public override async Task<IUserManagementService.IsValidResult> IsValidUser(string username, string password, List<string> allowedRoles = null)
        {
            var original = await userService.FindByUserName(username);

            if (original == null)
                return IUserManagementService.IsValidResult.InvalidLoginOrPassword;

            try
            {
                var result = password == CryptHelper.DecryptString(original.PasswordHash);

                if (!result)
                    return IUserManagementService.IsValidResult.InvalidLoginOrPassword;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Decrypt");
                return IUserManagementService.IsValidResult.Error;
            }

            if (!original.IsActive)
                return IUserManagementService.IsValidResult.NotActive;

            return IUserManagementService.IsValidResult.Success;
        }

        public override async Task SetRefreshToken(string username, string refreshToken)
        {
        }

        public override async Task<IUserManagementService.IsValidRefreshTokenResult> IsValidRefreshToken(string username, string refreshToken)
        {
            return IUserManagementService.IsValidRefreshTokenResult.Success;
        }
    }
}
