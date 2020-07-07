# WebApiClient.Extensions.Consul
使用 [Consul](https://github.com/G-Research/consuldotnet) 对 [WebApiClientCore](https://github.com/dotnetcore/WebApiClient) 进行扩展，提供服务发现功能。

## 安装
```xml
<PackageReference Include="WebApiClient.Extensions.Consul" Version="0.0.3" />
```

## 使用
```cs
public void ConfigureServices(IServiceCollection services)
{
    // ...

    // 注入 Consul 客户端
    services.AddConsulClient();

    // 注入 HttpApi 客户端
    services.AddHttpApiClient<ITestApi>("consul_service");
}
```
