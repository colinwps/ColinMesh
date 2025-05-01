namespace ColinApp.Auth.Iservices
{
    public interface IAuthServices
    {

        Task<string> GetCaptcha();

    }
}
