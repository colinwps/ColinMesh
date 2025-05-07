using ColinApp.Auth.Entities.Common;
using ColinApp.Auth.Iservices;
using ColinApp.Auth.Models.System;
using ColinApp.Auth.Utils;
using ColinApp.Common.BaseController;
using ColinApp.Common.Cache;
using ColinApp.Common.Captcha;
using ColinApp.Common.IUnitOfWork;
using System.Linq;

namespace ColinApp.Auth.Services
{
    public class AuthServices : IAuthServices
    {
        private readonly ILogger<AuthServices> _logger;

        private readonly IUnitOfWork _uow;

        private readonly RedisService _readis;

        private readonly JwtService _jwtService;


        public AuthServices(ILogger<AuthServices> logger, IUnitOfWork uow, RedisService readis, JwtService jwtService)
        {
            _logger = logger;
            _uow = uow;
            _readis = readis;
            _jwtService = jwtService;
        }

        #region 获取验证码
        /// <summary>
        /// 获取验证码
        /// </summary>
        /// <returns></returns>
        public async Task<ApiResponse<CaptchaResult>> GetCaptcha()
        {
            try
            {
                var (code, base64Image) = CaptchaHelper.GenerateCaptchaImage();
                var captchaId = Guid.NewGuid().ToString();

                await _readis.SetStringAsync(captchaId, code, TimeSpan.FromSeconds(120));

                CaptchaResult captchaResult = new CaptchaResult
                {
                    CaptchaId = captchaId,
                    Base64Image = base64Image
                };

                return ApiResponse<CaptchaResult>.Success(captchaResult);

            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion

        #region 验证码判断
        /// <summary>
        /// 验证码判断
        /// </summary>
        /// <param name="captchaId"></param>
        /// <param name="inputCode"></param>
        /// <returns></returns>
        public async Task<bool> Verify(string captchaId, string inputCode)
        {
            var code = await _readis.GetStringAsync(captchaId);
            if (string.IsNullOrEmpty(code))
            {
                return false;
            }
            if (code.Equals(inputCode, StringComparison.OrdinalIgnoreCase))
            {
                await _readis.RemoveAsync(captchaId);
                return true;
            }
            return false;
        }
        #endregion


        #region 登录

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="loginRequest">登录实体类</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<ApiResponse<LoginResponse>> Login(LoginRequest loginRequest)
        {
            try
            {
                string p = PasswordHelper.HashPassword(loginRequest.Password);

                // 1. 验证验证码
                var isValid = await Verify(loginRequest.CaptchaId, loginRequest.CaptchaCode);
                if (!isValid)
                {
                    return ApiResponse<LoginResponse>.Fail("验证码错误");
                }

                var loginUser = await _uow.Repository<User>().FindSingleByConditionAsync(x => x.UserLoginName == loginRequest.UserName);

                if (loginUser == null)
                {
                    return ApiResponse<LoginResponse>.Fail("用户名或密码错误");
                }

                bool isValidPassword = BCrypt.Net.BCrypt.Verify(loginRequest.Password, loginUser.PasswordHash);

                if (!isValidPassword)
                {
                    return ApiResponse<LoginResponse>.Fail("用户名或密码错误");
                }
                // 2. 生成 JWT Token
                var token = _jwtService.GenerateToken(loginUser.UserId);
                var refreshToken = _jwtService.GenerateRefreshToken();

                await _uow.Repository<RefreshToken>().AddAsync(refreshToken);
                await _uow.SaveChangesAsync();

                LoginResponse loginResponse = new LoginResponse
                {
                    AccessToken = token,
                    RefreshToken = refreshToken.Token,
                    ExpiresIn = 7200,
                    UserName = loginUser.UserName,
                    UserId = loginUser.UserId
                };
                return ApiResponse<LoginResponse>.Success(loginResponse);


            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion


        #region 刷新Token
        /// <summary>
        /// 刷新Token
        /// </summary>
        /// <param name="refreshToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task<ApiResponse<LoginResponse>> RefreshToken(string refreshToken)
        {
            throw new NotImplementedException();
        }
        #endregion


    }
}
