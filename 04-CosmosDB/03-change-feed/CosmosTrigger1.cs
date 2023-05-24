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
        private static readonly string _databaseId = "ToDoList";
        private static readonly string _containerId = "Items2";
        private static CosmosClient cosmosClient = new CosmosClient(_connectionString);

        [FunctionName("CosmosTrigger1")]
        public static async Task Run([CosmosDBTrigger(
            databaseName: "ToDoList",
            collectionName: "Items",
            ConnectionStringSetting = "iveycosmos_DOCUMENTDB",
            LeaseCollectionName = "leases",
            CreateLeaseCollectionIfNotExists = true)]IReadOnlyList<Document> input, ILogger log)
        {
            var container2 = cosmosClient.GetContainer(_databaseId, _containerId);

            foreach(Document doc in input)
            {
                log.LogInformation("Pushed doc into container2");
                log.LogInformation($"doc: {doc}");
                try
                {
                    await container2.CreateItemAsync<Document>(doc);
                }
                catch(Exception e)
                {
                    log.LogInformation($"Exception pushing dock into container2: {e}");
                }
            }
        }
    }
}
