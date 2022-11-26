using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

string connectionString = "";
string topicName = "";
const int numberOfMessages = 3;

ServiceBusClient serviceBusClient = new ServiceBusClient(connectionString);
ServiceBusSender serviceBusSender = serviceBusClient.CreateSender(topicName);

// Create a message batch
using ServiceBusMessageBatch messageBatch = await serviceBusSender.CreateMessageBatchAsync();

for(int i = 1; i <= numberOfMessages; i++)
{
    // Try adding a message to the batch
    if(!messageBatch.TryAddMessage(new ServiceBusMessage($"Message #{i}")))
    {
        // If it's too large for the batch
        throw new Exception($"Message #{i} is too large to fit in the batch.");
    }
}

try
{
    // Use the sender/producer client to send the match of messages to the topic
    await serviceBusSender.SendMessagesAsync(messageBatch);
    Console.WriteLine($"A batch of {numberOfMessages} messages has been published to the topic.");
}
finally
{
    await serviceBusSender.DisposeAsync();
    await serviceBusClient.DisposeAsync();
}