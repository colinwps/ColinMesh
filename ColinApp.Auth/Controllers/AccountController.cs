using ColinApp.Auth.Base;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace ColinApp.Auth.Controllers
{

    [Route("api/[controller]")]
    public class AccountController : BaseApiController
    {
        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest req)
        {
            await Task.Delay(100);

            if (req.Username == "admin" && req.Password == "123456")
            {
                var token = "fake-jwt-token";
                return await OkResponseAsync(new { token }, "登录成功");
            }

            return await FailResponseAsync("用户名或密码错误");
        }
    }
}
