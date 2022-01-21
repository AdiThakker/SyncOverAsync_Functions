using Microsoft.Azure.Functions.Extensions.DependencyInjection;

namespace SyncOverAsync_Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            throw new NotImplementedException();    
        }
    }
}
