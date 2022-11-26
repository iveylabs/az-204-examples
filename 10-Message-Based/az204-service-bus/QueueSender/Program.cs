using System.Net;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

string connectionString = "";
string queueName = "";
const int numberOfMessages = 3;


// The Service Bus client types are safe to cache and use as a singleton for the lifetime
// of the application, which is best practice when messages are being published or read
// regularly.

// Set the transport type to AmqpWebSockets so that the ServiceBusClient uses the port 443. 
// If you use the default AmqpTcp, you will need to make sure that the ports 5671 and 5672 are open
var clientOptions = new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets };
ServiceBusClient serviceBusClient = new ServiceBusClient(connectionString, clientOptions);
ServiceBusSender serviceBusSender = serviceBusClient.CreateSender(queueName);

// Create a message batch
using ServiceBusMessageBatch messageBatch = await serviceBusSender.CreateMessageBatchAsync();

for(int i = 1; i <= numberOfMessages; i++)
{
    // Try adding a message to the batch
    if(!messageBatch.TryAddMessage(new ServiceBusMessage($"Message #{i}")))
    {
        // If it's too large for the batch
        throw new Exception($"The message {i} is too large to fit in the batch.");
    }
}

try
{
    // Use the sender/producer client to send the match of messages to the queue
    await serviceBusSender.SendMessagesAsync(messageBatch);
    Console.WriteLine($"A batch of {numberOfMessages} messages has been published to the queue.");
}
finally
{
    // Calling DisposeAsync on client types is required to ensure that network resources
    // and other unmanaged objects are properly cleaned up
    await serviceBusSender.DisposeAsync();
    await serviceBusClient.DisposeAsync();
}

Console.WriteLine("Completed. Press any key to exit.");
Console.ReadKey();