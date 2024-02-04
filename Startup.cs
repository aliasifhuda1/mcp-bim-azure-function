using mcp.function;
using mcp_bim_azure_function.Business;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

  [assembly: FunctionsStartup(typeof(Startup))]
namespace mcp.function
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();
        }
    }
}
