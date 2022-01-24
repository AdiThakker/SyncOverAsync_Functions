

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

        [FunctionName("SyncOverAsyncApi_Request")]
        public async Task<HttpResponseMessage> HttpStart([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter)
        {
            return default;
        }
    }
}