using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApiClientCore.Attributes;

namespace WebApiClient.Extensions.Consul.Sample
{
    public interface ITestApi
    {
        [HttpGet("/api/values")]
        Task<string[]> Get();
    }
}
