
using ColinApp.Video.Entities;
using Microsoft.Extensions.Options;
using SIPSorcery.Net;
using SIPSorcery.SIP;
using SIPSorcery.SIP.App;
using System.Net;
using System.Text;
using System.Xml.Linq;

namespace ColinApp.Video.SIPService
{
    public class SipServerService : BackgroundService
    {
        private SIPTransport _sipTransport;
        private readonly GB28181Config _config;

        // 注册设备信息缓存
        private readonly Dictionary<string, SIPEndPoint> _registeredDevices = new();

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
                    var deviceId = sipRequest.Header.From.FromURI.User;
                    Console.WriteLine($"[REGISTER] 设备ID: {deviceId}");

                    _registeredDevices[deviceId] = remoteEndPoint;

                    var registerResp = SIPResponse.GetResponse(sipRequest, SIPResponseStatusCodesEnum.Ok, null);
                    _sipTransport.SendResponseAsync(registerResp);
                    break;

                case SIPMethodsEnum.MESSAGE:
                    string xml = sipRequest.Body;
                    Console.WriteLine($"[MESSAGE XML]:\n{xml}");

                    if (xml.Contains("<CmdType>Catalog</CmdType>"))
                    {
                        HandleCatalogRequest(sipRequest, remoteEndPoint);
                    }
                    else if (xml.Contains("<CmdType>Keepalive</CmdType>"))
                    {
                        Console.WriteLine("[Keepalive] 心跳包已收到。");
                        _sipTransport.SendResponseAsync(SIPResponse.GetResponse(sipRequest, SIPResponseStatusCodesEnum.Ok, null));
                    }
                    else
                    {
                        _sipTransport.SendResponseAsync(SIPResponse.GetResponse(sipRequest, SIPResponseStatusCodesEnum.Ok, null));
                    }
                    break;

                case SIPMethodsEnum.INVITE:
                    var inviteResp = SIPResponse.GetResponse(sipRequest, SIPResponseStatusCodesEnum.Ok, null);
                    inviteResp.Body = "v=0\no=- 0 0 IN IP4 127.0.0.1\ns=Play\nc=IN IP4 192.168.1.106\nt=0 0\nm=video 30000 RTP/AVP 96\na=rtpmap:96 PS/90000";
                    inviteResp.Header.ContentType = "application/sdp";
                    inviteResp.Header.ContentLength = inviteResp.Body.Length;
                    _sipTransport.SendResponseAsync(inviteResp);
                    break;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// 响应设备目录请求
        /// </summary>
        private void HandleCatalogRequest(SIPRequest sipRequest, SIPEndPoint remoteEndPoint)
        {
            var deviceId = sipRequest.Header.From.FromURI.User;

            var xml = new XElement("Response",
                new XElement("CmdType", "Catalog"),
                new XElement("SN", "1"),
                new XElement("DeviceID", deviceId),
                new XElement("SumNum", "1"),
                new XElement("DeviceList",
                    new XElement("Item",
                        new XElement("DeviceID", deviceId),
                        new XElement("Name", "Camera1"),
                        new XElement("Manufacturer", "ColinCam"),
                        new XElement("Model", "CM-1000"),
                        new XElement("Owner", "admin"),
                        new XElement("Parental", "0"),
                        new XElement("CivilCode", "340200"),
                        new XElement("Address", "Test Room"),
                        new XElement("Status", "ON")
                    )
                )
            );

            var xmlString = xml.ToString();
            var response = SIPResponse.GetResponse(sipRequest, SIPResponseStatusCodesEnum.Ok, null);
            response.Body = xmlString;
            response.Header.ContentType = "application/xml";
            response.Header.ContentLength = xmlString.Length;

            _sipTransport.SendResponseAsync(response);
            Console.WriteLine("[Catalog] 已回复设备目录信息");
        }

        /// <summary>
        /// 外部调用此方法，向指定设备发起 INVITE（启动预览）
        /// </summary>
        public async Task<bool> SendInviteAsync(string deviceId)
        {
            if (!_registeredDevices.TryGetValue(deviceId, out var deviceEndpoint))
            {
                Console.WriteLine($"[INVITE] 设备未注册: {deviceId}");
                return false;
            }

            var localSIPURI = SIPURI.ParseSIPURI($"sip:{_config.ServerId}@{_config.ServerIP}");
            var remoteSIPURI = SIPURI.ParseSIPURI($"sip:{deviceId}@{deviceEndpoint.Address}:{deviceEndpoint.Port}");

            var inviteReq = SIPRequest.GetRequest(SIPMethodsEnum.INVITE, remoteSIPURI);
            inviteReq.Header.From = new SIPFromHeader(null, localSIPURI, null);
            inviteReq.Header.To = new SIPToHeader(null, remoteSIPURI, null);
            inviteReq.Header.CSeq = 1;
            inviteReq.Header.CallId = CallProperties.CreateNewCallId();
            inviteReq.Header.Contact = new List<SIPContactHeader>
            {
                new SIPContactHeader(null, localSIPURI)
            };
            inviteReq.Header.Subject = $"{deviceId}:{_config.ServerId}:0:0:0:1";

            inviteReq.Body = "v=0\no=- 0 0 IN IP4 " + _config.MediaServerIP + "\ns=Play\nc=IN IP4 " + _config.MediaServerIP + "\nt=0 0\nm=video 9000 RTP/AVP 96\na=rtpmap:96 PS/90000";
            inviteReq.Header.ContentType = "application/sdp";
            inviteReq.Header.ContentLength = inviteReq.Body.Length;


            //新
            // 设备SIP信息
            var deviceSIP = SIPURI.ParseSIPURI("sip:34020000001320000001@192.168.1.120:5060");

            // 生成 sdp
            string sdp = @"v=0
                  o=- 0 0 IN IP4 192.168.1.106
                  s=Play
                  c=IN IP4 192.168.1.106
                 t=0 0
                 m=video 9000 RTP/AVP 96
                 a=rtpmap:96 PS/90000";

            var inviteRequest = SIPRequest.GetRequest(SIPMethodsEnum.INVITE, deviceSIP);
            inviteRequest.Body = sdp;
            inviteRequest.Header.ContentType = "application/sdp";
            inviteRequest.Header.CSeq = 1;
            await _sipTransport.SendRequestAsync(inviteRequest);
            //新


            //await _sipTransport.SendRequestAsync(deviceEndpoint, inviteReq);
            Console.WriteLine($"[INVITE] 已向设备 {deviceId} 发出预览请求");
            return true;
        }

        public override void Dispose()
        {
            base.Dispose();
            _sipTransport.Shutdown();
        }
    }
}
