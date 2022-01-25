

using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.OpenApi.Models;
using System.Net;

namespace SyncOverAsync_Functions
{
    public class SyncOverAsyncApi
    {
        public ILogger<SyncOverAsyncApi> Logger { get; }        
        
        private Random randomGenerator = new Random();
        private const string OrchestrationFunctionName = "Sync-Over-Async-Api-DurableFunction";
        const string OrchestrationComplete = "weather_async_response";

        public SyncOverAsyncApi(ILogger<SyncOverAsyncApi> logger)
        {
            Logger = logger;
        }
        
        [FunctionName("SyncOverAsyncApi_WeatherRequest")]
        [OpenApiOperation(operationId: "GetWeatherAsync", tags: new[] { "weather" })]
        //[OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        //[OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Name** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]

        public async Task<HttpResponseMessage> GetWeather([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter)
        {
            var requestId = randomGenerator.Next(Int32.MaxValue).ToString();

            // Start new orchestration
            await starter.StartNewAsync(OrchestrationFunctionName, requestId.ToString());
            this.Logger.LogInformation($"Started orchestration for {requestId}");

            // Wait for orchestration to complete or timeout to occur
            var completion = await starter.WaitForCompletionOrCreateCheckStatusResponseAsync(req, requestId.ToString(), TimeSpan.FromSeconds(60));
            if (completion.StatusCode != HttpStatusCode.OK)
            {
                await starter.TerminateAsync(requestId, "Timeout Occured"); // Log additional context (if any)
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }

            return completion;
        }

        [FunctionName(OrchestrationFunctionName)]
        public async Task<string> RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context) => await context.WaitForExternalEvent<string>(OrchestrationComplete);
    
        [FunctionName("SyncOverAsyncApi_WeatherReply")]
        public async Task WeatherReply([BlobTrigger("devstoreaccount1/weather-results/{name}", Connection = "blobConnection")] Stream myBlob, string name,
                                       [DurableClient] IDurableOrchestrationClient client)
        {
            var requestId = name;
            await client.RaiseEventAsync(requestId, OrchestrationComplete, new StreamReader(myBlob).ReadToEnd());
            this.Logger.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
        }
    }
}