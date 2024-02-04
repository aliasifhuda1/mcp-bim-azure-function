using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Business;
using mcp_bim_azure_function.Business;
using System;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace mcp.Function
{
    public class BasicRestCall
    {
        private readonly IConfigurationService _configurationService;

        public BasicRestCall(IConfigurationService configurationService)
        {
            _configurationService = configurationService;
        }

        readonly RestClient restClient = new();
        
        /**
         * This function is triggered by an HTTP request and returns the carbon value of a design multiplied by the area of a shed.
         * @param req The HTTP request
         * @param log The logger
         * @return The carbon value of a design multiplied by the area of a shed
         **/
        [FunctionName("BasicRestCall")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string returnInformation = "";

            LogEnvironmentVariables(log);

            string shedAreaInSquareMeters = req.Query["shedAreaInSquareMeters"];
            if (string.IsNullOrEmpty(shedAreaInSquareMeters))
            {
                return new BadRequestObjectResult("Please pass a shedAreaInSquareMeters on the query string");
            }

            var shedArea = Convert.ToDouble(shedAreaInSquareMeters);

            var url = "https://dev01.api.identity.moata.com/mcp/v1.0/api/projects/1/designs/10911?identityId=268";
            
            var carbonValue = await GetDesignCarbonValueAsync(url);

            returnInformation += "Design Carbon Value: " + carbonValue * shedArea;

            // Upload data to blob
            BlobService blobService = new(_configurationService);
            await blobService.UploadFixedDataAsync();

            // List all blobs
            var allContent = await blobService.ListAllBlobsContentAsync();

            returnInformation += MultiplyWeights(allContent, shedArea);

            return new OkObjectResult(returnInformation);
        }

        /**
         * Logs all environment variables
         * @param log The logger
         **/
        public static void LogEnvironmentVariables(ILogger log)
        {
            foreach (System.Collections.DictionaryEntry variable in System.Environment.GetEnvironmentVariables())
            {
                if(variable.Key.ToString() == "APPSETTING_AzureWebJobsStorage" || 
                   variable.Key.ToString() ==  "APPSETTING_MySecret" || 
                   variable.Key.ToString() == "APPSETTING_WEBSITE_CONTENTAZUREFILECONNECTIONSTRING" || 
                   variable.Key.ToString() == "storageKey" ||
                   variable.Key.ToString() == "storageAccount")
                    continue;
                log.LogInformation($"\n{variable.Key}: {variable.Value}");
            }
        }

        /**
         * Multiplies the weights of all the assets by a given multiplier
         * @param allContent The content of all the blobs
         * @param multiplier The multiplier
         * @return A message with the original and new weights of all the assets
         */
        public static string MultiplyWeights(List<string> allContent, double multiplier)
        {
            string message = "\n";
            allContent.ForEach(c =>
            {
                dynamic asset = JsonConvert.DeserializeObject(c);
                double originalWeight = asset.Weight;
                asset.Weight *= multiplier;
                message += $"\nAsset Utilized: {asset.AssetName}\nOriginal Weight for 1x1 garden shed: {originalWeight} {asset.Unit}\nNew Weight for {multiplier}x{multiplier} garden shed: {asset.Weight} {asset.Unit}\n";
                
            });

            return message;
        }

        /**
         * Gets the carbon value of a design
         * @param url The URL of the design
         * @return The carbon value of a design
         **/
        public async Task<double> GetDesignCarbonValueAsync(string url)
        {
            var response = await restClient.GetAsync(url);
            string totalCarbonValue = await response.Content.ReadAsStringAsync();
            JObject jsonObject = JObject.Parse(totalCarbonValue);
            string designCarbonValue = (string)jsonObject["designCarbonValue"];
            return Convert.ToDouble(designCarbonValue);
        }
    }
}
