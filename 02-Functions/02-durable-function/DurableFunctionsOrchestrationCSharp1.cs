using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace Iveylabs.Function
{
    public static class DurableFunctionsOrchestrationCSharp1
    {
        // Client/starter function
        [FunctionName("StarterFunction")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            // Call the orchestrator function
            string instanceId = await starter.StartNewAsync("OrchestratorFunction", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            // Create URL to check status
            return starter.CreateCheckStatusResponse(req, instanceId);
        }

        // Orchestrator function
        [FunctionName("OrchestratorFunction")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            // Counter entity
            var entityId = new EntityId(nameof(Counter), "myCounter");
            await context.CallEntityAsync(entityId, "RESET");
            int i = 0;

            var outputs = new List<string>();
            
            // Replace "SayHello" with the name of your Durable Activity Function.
            outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), "Tokyo"));
            i++;
            await context.CallEntityAsync(entityId, "ADD", 1);

            outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), "Seattle"));
            i++;
            await context.CallEntityAsync(entityId, "ADD", 1);

            outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), "London"));
            i++;
            await context.CallEntityAsync(entityId, "ADD", 1);

            // Get the current state value from the entity function
            int currentState = await context.CallEntityAsync<int>(entityId, "GET");

            log.LogInformation($"Orchestrator thinks i = {i} and state value = {currentState}");

            // If state isn't 3, clear outputs
            if(currentState != i)
            {
                log.LogInformation("Clearing outputs.");
                outputs.Clear();
            }

            // returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]
            return outputs;
        }

        // Entity function
        [FunctionName("Counter")]
        public static async Task Counter([EntityTrigger] IDurableEntityContext ctx, ILogger log)
        {
            switch(ctx.OperationName.ToLowerInvariant())
            {
                case "add":
                    ctx.SetState(ctx.GetState<int>() + ctx.GetInput<int>());
                    log.LogInformation($"Added {ctx.GetInput<int>().ToString()} to state. New state value is {ctx.GetState<int>().ToString()}.");
                    break;
                case "reset":
                    ctx.SetState(0);
                    log.LogInformation($"State reset.");
                    break;
                case "get":
                    log.LogInformation($"Returning state value of {ctx.GetState<int>()}.");
                    ctx.Return(ctx.GetState<int>());
                    break;
            }
        }

        // Activity function
        [FunctionName(nameof(SayHello))]
        public static string SayHello([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation($"Saying hello to {name}.");
            return $"Hello {name}!";
        }
    }
}