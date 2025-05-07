namespace ColinApp.Auth.Utils
{
    public static class PasswordHelper
    {
        /// <summary>
        /// 生成密码哈希
        /// </summary>
        /// <param name="password">明文密码</param>
        /// <returns>哈希密码</returns>
        public static string HashPassword(string password)
        {
            // 使用 BCrypt 哈希算法
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        /// <summary>
        /// 验证密码
        /// </summary>
        /// <param name="password">明文密码</param>
        /// <param name="hashedPassword">哈希密码</param>
        /// <returns>验证结果</returns>
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}
