using System;
using System.Collections.Generic;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;
using System.Threading.Tasks;

namespace Iveylabs.Function
{
    public static class CosmosTrigger1
    {
        private static readonly string _connectionString = "";
        private static readonly string _databaseId = "ToDo";
        private static readonly string _containerId = "CopyTasks";
        private static readonly CosmosClient cosmosClient = new(_connectionString);

        [FunctionName("CosmosTrigger1")]
        public static async Task Run([CosmosDBTrigger(
            databaseName: "ToDo",
            collectionName: "Tasks",
            ConnectionStringSetting = "cosmos_DOCUMENTDB",
/* The lease container acts as state storage and coordinates processing the change feed across multiple workers.
* The lease container can be stored in the same account as the monitored container or in a separate account.*/
            LeaseCollectionName = "leases",
            CreateLeaseCollectionIfNotExists = true)]IReadOnlyList<Document> input, ILogger log)
        {
            var container2 = cosmosClient.GetContainer(_databaseId, _containerId);

            foreach(Document doc in input)
            {
                log.LogInformation("Pushed doc into container");
                log.LogInformation($"doc: {doc}");
                try
                {
                    await container2.CreateItemAsync<Document>(doc);
                }
                catch(Exception e)
                {
                    log.LogInformation($"Exception pushing doc into container: {e}");
                }
            }
        }
    }
}
