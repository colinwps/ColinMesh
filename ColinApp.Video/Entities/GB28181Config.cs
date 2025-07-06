namespace ColinApp.Video.Entities
{
    public class GB28181Config
    {
        public string? ServerId { get; set; }         // SIP编号，例如：34020000002000000001
        public string? ServerIP { get; set; }         // 192.168.1.101
        public string? Domain { get; set; }           // SIP域，例如：3402000000
        public string? Password { get; set; }         // 注册密码

        public string? MediaServerIP { get; set; }  // 媒体服务器IP，例如：
    }
}
