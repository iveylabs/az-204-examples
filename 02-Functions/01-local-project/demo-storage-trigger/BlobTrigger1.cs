using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace Iveylabs.Function
{
    public class BlobTrigger1
    {
        [FunctionName("BlobTrigger1")]
        [return: Queue("demo-queue")]
        public string Run(
            [BlobTrigger("images/{name}", Connection = "AzureWebJobsStorage")]Stream myBlob, string name, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
            return $"New file {name} needs to be reviewed.";
        }
    }
}