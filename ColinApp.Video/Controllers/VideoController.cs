using ColinApp.Video.Base;
using ColinApp.Video.SIPService;
using Microsoft.AspNetCore.Mvc;


namespace ColinApp.Video.Controllers
{
    [Route("api/[controller]")]
    public class VideoController : BaseApiController
    {
        private readonly SipServerService _sipServerService;
        public VideoController(SipServerService sipServerService) 
        {
            _sipServerService = sipServerService;
        }

        [HttpGet]
        [Route("invite")]
        public async Task<IActionResult> Invite(string deviceId, string channelId)
        {
            // 这里可以添加处理邀请逻辑的代码
            // 例如，发送SIP INVITE请求到指定设备和通道
            Console.WriteLine($"邀请设备 {deviceId} 的通道 {channelId}");

            await _sipServerService.SendInviteAsync(deviceId);

            return Ok(new { message = "邀请已发送" });
        }



    }
}
