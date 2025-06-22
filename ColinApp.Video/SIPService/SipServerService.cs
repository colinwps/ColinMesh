
using ColinApp.Video.Entities;
using Microsoft.Extensions.Options;
using SIPSorcery.SIP;
using System.Net;

namespace ColinApp.Video.SIPService
{
    public class SipServerService : BackgroundService
    {
        private SIPTransport _sipTransport;

        private readonly GB28181Config _config;

        public SipServerService(IOptions<GB28181Config> options)
        {
            _config = options.Value;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _sipTransport = new SIPTransport();

            var localIP = IPAddress.Parse(_config.ServerIP);
            _sipTransport.AddSIPChannel(new SIPUDPChannel(new IPEndPoint(localIP, 8819)));

            _sipTransport.SIPTransportRequestReceived += OnSIPRequestReceived;

            Console.WriteLine("SIP Server started on port 8819...");
            return Task.CompletedTask;
        }

        private Task OnSIPRequestReceived(SIPEndPoint localEndPoint, SIPEndPoint remoteEndPoint, SIPRequest sipRequest)
        {
            Console.WriteLine($"[SIP] 收到 {sipRequest.Method} 请求");

            switch (sipRequest.Method)
            {
                case SIPMethodsEnum.REGISTER:
                    var registerResp = SIPResponse.GetResponse(sipRequest, SIPResponseStatusCodesEnum.Ok, null);
                    _sipTransport.SendResponseAsync(registerResp); // Updated to use SendResponseAsync
                    break;

                case SIPMethodsEnum.MESSAGE:
                    string xml = sipRequest.Body;
                    Console.WriteLine($"[MESSAGE XML]:\n{xml}");
                    _sipTransport.SendResponseAsync(SIPResponse.GetResponse(sipRequest, SIPResponseStatusCodesEnum.Ok, null)); // Updated to use SendResponseAsync
                    break;

                case SIPMethodsEnum.INVITE:
                    var inviteResp = SIPResponse.GetResponse(sipRequest, SIPResponseStatusCodesEnum.Ok, null);
                    inviteResp.Body = "v=0\no=- 0 0 IN IP4 127.0.0.1\ns=Play\nc=IN IP4 192.168.1.100\nt=0 0\nm=video 30000 RTP/AVP 96\na=rtpmap:96 PS/90000";
                    inviteResp.Header.ContentType = "application/sdp";
                    inviteResp.Header.ContentLength = inviteResp.Body.Length;
                    _sipTransport.SendResponseAsync(inviteResp); // Updated to use SendResponseAsync
                    break;
            }

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            base.Dispose();
            _sipTransport.Shutdown();
        }
    }
}
