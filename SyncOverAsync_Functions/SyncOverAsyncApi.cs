

using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.OpenApi.Models;
using System.Net;

namespace SyncOverAsync_Functions
{
    public class SyncOverAsyncApi
    {
        public ILogger<SyncOverAsyncApi> Logger { get; }

        public SyncOverAsyncApi(ILogger<SyncOverAsyncApi> logger)
        {
            Logger = logger;
        }

        [FunctionName("Sync-Over-Async-Api-DurableFunction")]
        public static async Task<List<string>> RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            return default;
        }

        [FunctionName("SyncOverAsyncApi_WeatherRequest")]
        [OpenApiOperation(operationId: "GetWeather", tags: new[] { "weather" })]
        //[OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        //[OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Name** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]

        public async Task<HttpResponseMessage> GetWeather([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter)
        {
            return await Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }

    }
}