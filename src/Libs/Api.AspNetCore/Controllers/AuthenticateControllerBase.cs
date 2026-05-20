using System;
using System.Threading.Tasks;
using Api.AspNetCore.Models.Secure;
using Api.AspNetCore.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.AspNetCore.Controllers
{
    public abstract class AuthenticateControllerBase : ControllerBase
    {
        private readonly IAuthenticateService authService;
        public AuthenticateControllerBase(IAuthenticateService authService)
        {
            this.authService = authService;
        }

        public virtual async Task<ActionResult> RequestTokenAsync(JwtTokenRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var token = await authService.IsAuthenticated(request);
            if (token != null)
            {
                if (!string.IsNullOrEmpty(token.ErrorCode))
                    return Unauthorized(token.ErrorCode);

                return Ok(token);
            }

            return BadRequest("Invalid Request");
        }

        public virtual async Task<ActionResult> RequestRefreshTokenAsync(RefreshTokenRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var token = await authService.RefreshTokenAsync(request);
            if (token == null)
                return Unauthorized("Access denied");
            if (!string.IsNullOrEmpty(token.ErrorCode))
                return Unauthorized(token.ErrorCode);

            return Ok(token);
        }
    }
}
