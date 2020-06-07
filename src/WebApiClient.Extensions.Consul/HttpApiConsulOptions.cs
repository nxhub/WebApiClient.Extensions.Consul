using System;

namespace WebApiClientCore.Extensions
{
    public class HttpApiConsulOptions<THttpApi> : HttpApiOptions<THttpApi>
    {
        /// <summary>
        /// 获取或设置 Http 服务在 Consul 中的服务名称，
        /// 例如 service_sms，
        /// 设置了 ServiceName 值, HttpHost 和 HttpHostAttribute 将失效。
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// 获取或设置 Http 服务的请求路径
        /// 例如 /api/orders
        /// </summary>
        public string ServicePath { get; set; }

        /// <summary>
        /// 获取或设置 Consul 客户端地址，
        /// 例如 http://172.16.0.8:8500。
        /// </summary>
        public Uri ConculAddress { get; set; }

        public string Datacenter { get; set; }
    }
}
