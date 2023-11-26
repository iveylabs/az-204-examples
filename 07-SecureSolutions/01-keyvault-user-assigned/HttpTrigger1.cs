
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Core;

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

            string userAssignedClientId = "";
            string secretName = "";
            Uri vaultUri = new("");
            
            TokenCredential credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions { ManagedIdentityClientId = userAssignedClientId });

            try
            {
                var client = new SecretClient(vaultUri, credential);

                KeyVaultSecret secret = await client.GetSecretAsync(secretName);

                string responseMessage = $"Secret value: { secret.Value }";

                return new OkObjectResult(responseMessage);
            }
            catch (Azure.RequestFailedException) // Because of an annoying known issue
            {
                credential = new AzureCliCredential();

                var client = new SecretClient(vaultUri, credential);

                KeyVaultSecret secret = await client.GetSecretAsync(secretName);

                string responseMessage = $"Secret value: { secret.Value }";

                return new OkObjectResult(responseMessage);
            }
        }
    }
}
