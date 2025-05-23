﻿namespace ColinApp.Auth.Entities.Common
{
    public class LoginResponse
    {
        /// <summary>
        /// JWT访问令牌
        /// </summary>
        public string? AccessToken { get; set; }

        /// <summary>
        /// 刷新令牌
        /// </summary>
        public string? RefreshToken { get; set; }

        /// <summary>
        /// Token有效期（单位：秒）
        /// </summary>
        public long ExpiresIn { get; set; }

        /// <summary>
        /// 登录用户名称（可选）
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// 用户Id（可选）
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// 用户角色
        /// </summary>
        public string? UserRole { get; set; }

        /// <summary>
        /// 角色名称
        /// </summary>
        public string? UserRoleName { get; set; }

        /// <summary>
        /// 用户头像
        /// </summary>
        public string? UserAvatar { get; set; }
    }
}
