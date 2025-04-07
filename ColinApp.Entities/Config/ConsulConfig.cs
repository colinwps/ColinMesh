using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColinApp.Entities.Config
{
    /// <summary>
    /// Consul 配置类
    /// </summary>
    public class ConsulConfig
    {
        /// <summary>
        /// 地址
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// 服务名
        /// </summary>
        public string? ServiceName { get; set; }

        /// <summary>
        /// 服务编号
        /// </summary>
        public string? ServiceId { get; set; }

        /// <summary>
        /// 服务地址
        /// </summary>
        public string? ServiceAddress { get; set; }

        /// <summary>
        /// 服务编号
        /// </summary>
        public int? ServicePort { get; set; }
    }
}
