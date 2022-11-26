using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace Company.Function
{
    public static class HttpTrigger1
    {
        [FunctionName("HttpTrigger1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string userAssignedClientId = "9efd9f2c-84e8-4a8d-8bd4-80a0b4d88875";
            var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions { ManagedIdentityClientId = userAssignedClientId });

            var client = new SecretClient(new Uri("https://piveykv.vault.azure.net/"), credential);

            KeyVaultSecret secret = await client.GetSecretAsync("mysecret");

            string responseMessage = $"Secret value: { secret.Value }";

            return new OkObjectResult(responseMessage);
        }
    }
}
