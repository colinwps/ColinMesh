namespace ColinApp.Auth.Entities.Common
{
    /// <summary>
    /// 验证码结果
    /// </summary>
    public class CaptchaResult
    {
        public string? CaptchaId { get; set; }
        public string? Base64Image { get; set; }
    }
}
