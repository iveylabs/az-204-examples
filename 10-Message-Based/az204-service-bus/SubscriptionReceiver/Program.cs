using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

string connectionString = "";
string topicName = "";
string subscriptionName = "";

async Task MessageHandler(ProcessMessageEventArgs args)
{
    string body = args.Message.Body.ToString();
    Console.WriteLine($"Received {body} from subscription {subscriptionName}.");

    // Mark the message as complete, removing it from the subscription
    await args.CompleteMessageAsync(args.Message);
}

Task ErrorHandler(ProcessErrorEventArgs args)
{
    Console.WriteLine(args.Exception.ToString());
    return Task.CompletedTask;
}

ServiceBusClient serviceBusClient = new ServiceBusClient(connectionString);
ServiceBusProcessor serviceBusProcessor = serviceBusClient.CreateProcessor(topicName, subscriptionName, new ServiceBusProcessorOptions());

try
{
    // Add handler to process messages
    serviceBusProcessor.ProcessMessageAsync += MessageHandler;
    // Add handler to process errors
    serviceBusProcessor.ProcessErrorAsync += ErrorHandler;

    // Start processing
    await serviceBusProcessor.StartProcessingAsync();

    Console.WriteLine("Wait for a moment and then press any key to end processing.");
    Console.ReadKey();

    // Stop processing
    Console.WriteLine("\nStopping receiver...");
    await serviceBusProcessor.StopProcessingAsync();
    Console.WriteLine("Stopped receiving messages.");
}
finally
{
    await serviceBusProcessor.DisposeAsync();
    await serviceBusClient.DisposeAsync();
}