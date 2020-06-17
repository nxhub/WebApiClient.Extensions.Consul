namespace WebApiClientCore.Extensions
{
    public class HttpApiConsulOptions : HttpApiOptions
    {
        /// <summary>
        /// 获取或设置 Http 服务在 Consul 中的服务名称，
        /// 例如 service_sms，
        /// 设置了 ServiceName 值, HttpHost 和 HttpHostAttribute 将失效。
        /// </summary>
        public string ConsulServiceName { get; set; }
    }
}
