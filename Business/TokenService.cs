using mcp_bim_azure_function.Business;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Business
{
    public class TokenService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfigurationService _configurationService;

        public TokenService(IConfigurationService configurationService)
        {
            _configurationService = configurationService;
            _httpClient = new HttpClient();
        }

        public async Task<string> GetToken()
        {
            var payload = new
            {
                refresh_token = _configurationService.GetSecret("refreshToken"),
                policy = "B2C_1_MottMac_DEV"
            };

            var jsonContent = JsonConvert.SerializeObject(payload);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var url = "https://dev01.api.identity.moata.com/v2.5/api/oAuth2/RefreshToken";

            var result = await _httpClient.PostAsync(url, httpContent);
            var responseContent = await result.Content.ReadAsStringAsync();

            var parsedContent = JsonConvert.DeserializeObject<JObject>(responseContent);
            var accessToken = parsedContent["access_token"].ToString();

            return accessToken;
        }
    }
}
