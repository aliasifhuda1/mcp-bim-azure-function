using Microsoft.Extensions.Configuration;

namespace mcp_bim_azure_function.Business
{
    public interface IConfigurationService
    {
        IConfiguration Configuration { get; }

        string GetSecret(string key);
    }
}