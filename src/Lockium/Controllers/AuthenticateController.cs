using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Api.AspNetCore.Controllers;
using Api.AspNetCore.Models.Secure;
using Api.AspNetCore.Services;

namespace Lockium.Controllers
{
    [ApiController]
    public class AuthenticateController : AuthenticateControllerBase
    {
        public AuthenticateController(IAuthenticateService authService)
            : base(authService)
        {
        }

        /// <summary>
        /// Request to get the JWT Token
        /// </summary>
        // <remarks>
        // ### Пример получения JWT Token
        // POST **api/v1/authenticate**
        // <!--
        // <code>
        // 
        // {
        //   "username": "admin",
        //   "password": "admin"
        // }
        // 
        // </code>
        // -->
        // </remarks>
        // <param name="request">User login & pass</param>
        // <returns>JWT Token</returns>
        // <response code="200">OK</response>
        // <response code="400">Login error (invalid login data)</response>
        [AllowAnonymous]
        [HttpPost]
        [Route("/api/v1/authenticate")]
        [ProducesResponseType(typeof(JwtToken), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        public override Task<ActionResult> RequestTokenAsync(JwtTokenRequest request)
        {
            return base.RequestTokenAsync(request);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("/api/v1/refreshToken")]
        public override Task<ActionResult> RequestRefreshTokenAsync(RefreshTokenRequest request)
        {
            return base.RequestRefreshTokenAsync(request);
        }
    }
}
