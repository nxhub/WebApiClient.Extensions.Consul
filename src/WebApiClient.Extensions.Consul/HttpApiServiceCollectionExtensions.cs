using System;
using System.Net;
using System.Threading.Tasks;
using Consul;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using WebApiClientCore;
using WebApiClientCore.Extensions;
using WebApiClientCore.ResponseCaches;
using WebApiClientCore.Serialization;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HttpApiServiceCollectionExtensions
    {
        public static IHttpClientBuilder AddHttpApiClient<THttpApi>(
               this IServiceCollection services, string consulServiceName)
               where THttpApi : class
        {
            return services.AddHttpApiClient<THttpApi>(
                optins => optins.ConsulServiceName = consulServiceName);
        }

        public static IHttpClientBuilder AddHttpApiClient<THttpApi>(
            this IServiceCollection services,
            Action<HttpApiConsulOptions> configureOptions)
            where THttpApi : class
        {
            return services.AddHttpApiClient<THttpApi>(
                (optins, _) => configureOptions(optins));
        }

        public static IHttpClientBuilder AddHttpApiClient<THttpApi>(
            this IServiceCollection services,
            Action<HttpApiConsulOptions, IServiceProvider> configureOptions)
            where THttpApi : class
        {
            services
                .AddHttpApi()
                .AddOptions<HttpApiConsulOptions>()
                .Configure(configureOptions);

            return services
                .AddHttpClient(typeof(THttpApi).FullName)
                .AddTypedClient((client, provider) =>
                {
                    IOptions<HttpApiConsulOptions> options = provider
                        .GetRequiredService<IOptions<HttpApiConsulOptions>>();

                    provider.ServiceDiscoveryAsync(options.Value).Wait();

                    return HttpApi.Create<THttpApi>(client, provider, options.Value);
                });
        }

        private static async Task ServiceDiscoveryAsync(
            this IServiceProvider provider, HttpApiConsulOptions options)
        {
            IConsulClient consul = provider.GetRequiredService<IConsulClient>();

            if (consul == null)
            {
                throw new InvalidOperationException(
                    "没有找到 Consul 客户端，请使用 service.AddConsulClient(); 注入");
            }

            QueryResult<CatalogService[]> service = await consul.Catalog.Service(options.ConsulServiceName);

            if (service.StatusCode == HttpStatusCode.OK && service.Response.Length > 0)
            {
                Random random = new Random(Environment.TickCount);

                int index = random.Next(service.Response.Length);

                CatalogService catalog = service.Response[index];

                UriBuilder builder = new UriBuilder(catalog.ServiceAddress)
                {
                    Port = catalog.ServicePort,
                };

                options.HttpHost = builder.Uri;
            }
        }

        private static IServiceCollection AddHttpApi(this IServiceCollection services)
        {
            services.AddMemoryCache();
            services.TryAddSingleton<IXmlSerializer, XmlSerializer>();
            services.TryAddSingleton<IJsonSerializer, JsonSerializer>();
            services.TryAddSingleton<IKeyValueSerializer, KeyValueSerializer>();
            services.TryAddSingleton<IResponseCacheProvider, ResponseCacheProvider>();
            return services;
        }
    }
}
