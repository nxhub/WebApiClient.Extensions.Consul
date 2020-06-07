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
            this IServiceCollection services,
            Action<HttpApiConsulOptions<THttpApi>> configureOptions)
            where THttpApi : class
        {
            return services.AddHttpApiClient<THttpApi>((optins, _) => configureOptions(optins));
        }

        public static IHttpClientBuilder AddHttpApiClient<THttpApi>(
            this IServiceCollection services,
            Action<HttpApiConsulOptions<THttpApi>, IServiceProvider> configureOptions)
            where THttpApi : class
        {
            services
                .AddHttpApi()
                .AddOptions<HttpApiConsulOptions<THttpApi>>()
                .Configure(configureOptions);

            return services
                .AddHttpClient(typeof(THttpApi).FullName)
                .AddTypedClient((client, serviceProvider) =>
                {
                    IOptions<HttpApiConsulOptions<THttpApi>> options = serviceProvider
                        .GetRequiredService<IOptions<HttpApiConsulOptions<THttpApi>>>();

                    ServiceDiscoveryAsync(options.Value).Wait();

                    return HttpApi.Create<THttpApi>(client, serviceProvider, options.Value);
                });
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

        private static async Task ServiceDiscoveryAsync<THttpApi>(HttpApiConsulOptions<THttpApi> options)
        {
            if (options.ConculAddress != null && !string.IsNullOrWhiteSpace(options.ServiceName))
            {
                ConsulClient consul = new ConsulClient(config =>
                {
                    config.Address = options.ConculAddress;

                    config.Datacenter = options.Datacenter;
                });

                QueryResult<CatalogService[]> service = await consul.Catalog.Service(options.ServiceName);

                if (service.StatusCode == HttpStatusCode.OK && service.Response.Length > 0)
                {
                    Random random = new Random(Environment.TickCount);

                    int index = random.Next(service.Response.Length);

                    CatalogService catalog = service.Response[index];

                    UriBuilder builder = new UriBuilder(catalog.ServiceAddress)
                    {
                        Port = catalog.ServicePort,
                        Path = options.ServicePath,
                    };

                    options.HttpHost = builder.Uri;
                }
            }
        }
    }
}
