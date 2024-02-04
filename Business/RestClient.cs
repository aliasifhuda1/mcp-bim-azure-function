using mcp_bim_azure_function.Business;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Business
{
    public class RestClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfigurationService _configurationService;

        public RestClient()
        {
            _httpClient = new HttpClient();
            _configurationService = new ConfigurationService();
        }

        /**
         * This function sends a POST request to the specified URL with the specified content.
         * @param url The URL to send the POST request to
         * @param content The content to send in the POST request
         * @return The response from the POST request
         **/
        public async Task<HttpResponseMessage> PostAsync<T>(string url, T content)
        {
            var jsonContent = JsonConvert.SerializeObject(content);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var tokenService = new TokenService(_configurationService);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await tokenService.GetToken());

            return await _httpClient.PostAsync(url, httpContent);
        }

        /**
         * This function sends a GET request to the specified URL.
         * @param url The URL to send the GET request to
         * @return The response from the GET request
         **/
        public async Task<HttpResponseMessage> GetAsync(string url)
        {
            var tokenService = new TokenService(_configurationService);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await tokenService.GetToken());

            return await _httpClient.GetAsync(url);
        }

    }
}
