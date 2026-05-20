using Lockium.Data.LockiumDb.DatabaseContext;
using Lockium.Models;
using Api.AspNetCore.Models.Configuration;
using Api.AspNetCore.Models.Secure;
using Api.AspNetCore.Services;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Lockium.Services
{
    public class MicroserviceAuthenticationService : JwtTokenAuthenticationService
    {
        private readonly LockiumDbContext db;

        public MicroserviceAuthenticationService(IUserManagementService userManagementService,
            IOptions<TokenManagement> tokenManagement,
            ILogger<MicroserviceAuthenticationService> logger,
            LockiumDbContext db)
            : base(userManagementService, tokenManagement, logger)
        {
            this.db = db;
        }
    }
}
