using ColinApp.Auth.Entities.Common;
using ColinApp.Common.BaseController;

namespace ColinApp.Auth.Iservices
{
    public interface IAuthServices
    {

        Task<ApiResponse<CaptchaResult>> GetCaptcha();

        Task<bool> Verify(string captchaId, string inputCode);

        Task<ApiResponse<LoginResponse>> Login(LoginRequest loginRequest);

        Task<ApiResponse<LoginResponse>> RefreshToken(string refreshToken);

    }
}
