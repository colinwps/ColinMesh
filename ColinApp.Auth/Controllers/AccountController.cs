using ColinApp.Auth.Base;
using ColinApp.Auth.Entities.Common;
using ColinApp.Auth.Iservices;
using Microsoft.AspNetCore.Mvc;

namespace ColinApp.Auth.Controllers
{

    [Route("api/[controller]")]
    public class AccountController : BaseApiController
    {
        private readonly ILogger<AccountController> _logger;

        private readonly IAuthServices _authServices;

        public AccountController(ILogger<AccountController> logger, IAuthServices authServices)
        {
            _logger = logger;
            _authServices = authServices;
        }

        [HttpGet]
        [Route("captcha")]
        public async Task<IActionResult> GetCaptchaAsync()
        {
            var result = await _authServices.GetCaptcha();
            return Ok(result);
        }


        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest loginRequest)
        {
            var result = await _authServices.Login(loginRequest);

            return Ok(result);
        }
    }
}
