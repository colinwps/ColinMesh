using Consul;

namespace ColinApp.Gateway.ReverseProxy
{
    public static class ConsulServiceDiscovery
    {
        public static async Task<List<string>> GetServiceAddressesAsync(string serviceName)
        {
            using var client = new ConsulClient();
            var result = await client.Health.Service(serviceName, tag: null, passingOnly: true);
            return result.Response
                .Select(s => $"http://{s.Service.Address}:{s.Service.Port}")
                .ToList();
        }
    }
}
