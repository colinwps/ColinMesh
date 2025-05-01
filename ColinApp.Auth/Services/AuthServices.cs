using ColinApp.Auth.Iservices;
using ColinApp.Common.Captcha;

namespace ColinApp.Auth.Services
{
    public class AuthServices : IAuthServices
    {
        private readonly ILogger<AuthServices> _logger;

        


        public AuthServices()
        {
        }

        public async Task<string> GetCaptcha()
        {
            try
            {
                var (code, base64Image) = CaptchaHelper.GenerateCaptchaImage();
                var captchaId = Guid.NewGuid().ToString();
                return captchaId;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
