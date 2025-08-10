
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

            try
            {
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
                        inviteResp.Body = $@"v=0
o=- 0 0 IN IP4 {_config.MediaServerIP}
s=Play
c=IN IP4 {_config.MediaServerIP}
t=0 0
m=video 10000 RTP/AVP 96
a=rtpmap:96 PS/90000
a=recvonly";
                        inviteResp.Header.ContentType = "application/sdp";
                        inviteResp.Header.ContentLength = inviteResp.Body.Length;
                        _sipTransport.SendResponseAsync(inviteResp);
                        Console.WriteLine("[INVITE] 回复设备 INVITE 请求");
                        break;

                    case SIPMethodsEnum.ACK:
                        Console.WriteLine("[ACK] 收到设备确认，推流会话建立");
                        // 可选：解析INVITE响应的SDP（需缓存），确认摄像头推流端口
                        break;

                    default:
                        Console.WriteLine($"[SIP] 未处理的方法: {sipRequest.Method}");
                        _sipTransport.SendResponseAsync(SIPResponse.GetResponse(sipRequest, SIPResponseStatusCodesEnum.MethodNotAllowed, null));
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SIP] 处理请求失败: {ex.Message}");
                _sipTransport.SendResponseAsync(SIPResponse.GetResponse(sipRequest, SIPResponseStatusCodesEnum.InternalServerError, null));
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

            try
            {
                // ZLMediaKit媒体服务器配置
                var mediaServerIP = _config.MediaServerIP; // 例如 "192.168.1.106"
                var mediaServerPort = 30000; // 确保在ZLMediaKit的RTP端口范围内（检查config.ini中的[rtp]配置）

                // 构造SDP，指定ZLMediaKit接收流的IP和端口
                string sdp = $@"v=0
o=- 0 0 IN IP4 {mediaServerIP}
s=Play
c=IN IP4 {mediaServerIP}
t=0 0
m=video {mediaServerPort} RTP/AVP 96
a=rtpmap:96 PS/90000
a=sendonly";

                // 设备SIP URI
                var deviceSIP = SIPURI.ParseSIPURI($"sip:{deviceId}@{deviceEndpoint.Address}:{deviceEndpoint.Port}");
                var localSIPURI = SIPURI.ParseSIPURI($"sip:{_config.ServerId}@{_config.ServerIP}");

                // 创建INVITE请求
                var inviteRequest = SIPRequest.GetRequest(SIPMethodsEnum.INVITE, deviceSIP);
                inviteRequest.Header.From = new SIPFromHeader(null, localSIPURI, null);
                inviteRequest.Header.To = new SIPToHeader(null, deviceSIP, null);
                inviteRequest.Header.CSeq = 1;
                inviteRequest.Header.CallId = CallProperties.CreateNewCallId();
                inviteRequest.Header.Contact = new List<SIPContactHeader>
        {
            new SIPContactHeader(null, localSIPURI)
        };
                inviteRequest.Header.Subject = $"{deviceId}:0"; // GB28181格式，调整为ZLMediaKit需要的流ID
                inviteRequest.Body = sdp;
                inviteRequest.Header.ContentType = "application/sdp";
                inviteRequest.Header.ContentLength = sdp.Length;

                // 发送INVITE请求
                await _sipTransport.SendRequestAsync(deviceEndpoint, inviteRequest);
                Console.WriteLine($"[INVITE] 已向设备 {deviceId} 发送推流请求，目标 {mediaServerIP}:{mediaServerPort}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[INVITE] 发送推流请求失败: {ex.Message}");
                return false;
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            _sipTransport.Shutdown();
        }
    }
}
