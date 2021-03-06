using DurableTask.Core;

namespace SyncOverAsync_Functions;

public class SyncOverAsyncApi
{
    public ILogger<SyncOverAsyncApi> Logger { get; }

    private Random randomGenerator = new Random();
    const string eventName = "weather_response";

    public SyncOverAsyncApi(ILogger<SyncOverAsyncApi> logger) => Logger = logger;

    [FunctionName(nameof(GetWeatherAsync))]
    [OpenApiOperation(operationId: "GetWeatherAsync", tags: new[] { "weather" })]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
    public async Task<HttpResponseMessage> GetWeatherAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestMessage req,
                                                           [DurableClient] IDurableOrchestrationClient starter)
    {
        // Generate a random request Id
        var requestId = randomGenerator.Next(Int32.MaxValue).ToString();

        // Start new orchestration and pass requestId as instance id.
        await starter.StartNewAsync(nameof(RunOrchestrator), requestId);
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

    [FunctionName(nameof(RunOrchestrator))]
    public async Task<string> RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context) => await context.WaitForExternalEvent<string>(eventName);

    [FunctionName(nameof(WeatherResponse))]
    public async Task WeatherResponse([BlobTrigger("weather-results/{name}", Connection = "blobConnection")] Stream myBlob, string name,
                                   [DurableClient] IDurableOrchestrationClient client)
    {
        // Retrieve the requestid (this one looks at the file name)
        var requestId = name.Remove(name.IndexOf('.'));

        // Send notification to the orchestration instance specifying the event completion
        await client.RaiseEventAsync(requestId, eventName, new StreamReader(myBlob).ReadToEnd());
        this.Logger.LogInformation($"Received reply for Request:{name}");
    }

    [FunctionName(nameof(CleanUpOldWeatherResponsesAsync))]
    public async Task CleanUpOldWeatherResponsesAsync([TimerTrigger("0 0 0 * * *")] /* execute every day */ TimerInfo myTimer, [DurableClient] IDurableOrchestrationClient client)
    {
        var result = await client.PurgeInstanceHistoryAsync(DateTime.UtcNow.AddDays(-15), DateTime.UtcNow.AddDays(-1), new List<OrchestrationStatus> { OrchestrationStatus.Completed, OrchestrationStatus.Terminated });
        this.Logger.LogInformation("Cleaned up records: ", result?.InstancesDeleted);
    }
}
