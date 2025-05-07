namespace ColinApp.Auth.Entities.Common
{
    /// <summary>
    /// 登录类
    /// </summary>
    public class LoginRequest
    {
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? CaptchaId { get; set; }
        public string? CaptchaCode { get; set; }
    }
}
