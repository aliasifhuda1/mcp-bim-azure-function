using Microsoft.Extensions.Configuration;

namespace mcp_bim_azure_function.Business
{
    public class ConfigurationService : IConfigurationService
    {
        public IConfiguration Configuration { get; }

        public ConfigurationService()
        {
            Configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();
        }

        public string GetSecret(string key)
        {
            return Configuration[key];
        }
    }
}