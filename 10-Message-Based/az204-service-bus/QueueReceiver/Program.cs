using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

string connectionString = "";
string queueName = "demo-queue";

// Task to handle messages
async Task MessageHandler(ProcessMessageEventArgs args)
{
    string body = args.Message.Body.ToString();
    Console.WriteLine($"Received: {body}");

    // Mark the message as complete, so it gets deleted from the queue
    await args.CompleteMessageAsync(args.Message);
}

// Task to handle errors
Task ErrorHandler(ProcessErrorEventArgs args)
{
    Console.WriteLine(args.Exception.ToString());
    return Task.CompletedTask;
}

ServiceBusClientOptions clientOptions = new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets };
ServiceBusClient serviceBusClient = new ServiceBusClient(connectionString, clientOptions);
ServiceBusProcessor serviceBusProcessor = serviceBusClient.CreateProcessor(queueName, new ServiceBusProcessorOptions());

try
{
    // Add a handler to process messages
    serviceBusProcessor.ProcessMessageAsync += MessageHandler;
    // Add a handler to process errors
    serviceBusProcessor.ProcessErrorAsync += ErrorHandler;

    // Start processing
    await serviceBusProcessor.StartProcessingAsync();

    Console.WriteLine("Wait for a moment and then press any key to end the processing.");
    Console.ReadKey();

    // Stop processing
    Console.WriteLine("\nStopping the receiver...");
    await serviceBusProcessor.StopProcessingAsync();
    Console.WriteLine("Stopped receiving messages.");
}
finally
{
    await serviceBusProcessor.DisposeAsync();
    await serviceBusClient.DisposeAsync();
}