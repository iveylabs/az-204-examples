
using System.Text;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using System.Text.Json;

const string connectionString = "";
const string eventHubName = "iveyhub";
const int numOfEvents = 2000;

// The Event Hubs client types are safe to cache and use as a singleton for the lifetime
// of the application, which is best practice when events are being published or read regularly.
EventHubProducerClient producerClient;
await SendEvents();

async Task SendEvents()
{
    // Create a producer client that you can use to send events to an event hub
    producerClient = new EventHubProducerClient(connectionString, eventHubName);

    // Create a batch of events 
    using EventDataBatch eventBatch = await producerClient.CreateBatchAsync();

    for (int i = 1; i <= numOfEvents; i++)
    {
        CustomEvent myEvent = new();

        var rpm = new Random();

        if(i <= numOfEvents/3)
        {
            myEvent.WarehouseId = "Warehouse1";
            myEvent.ConveyorId = "Conveyor1.1";
            myEvent.RPM = rpm.Next(0, 700);
        }
        else if(i > numOfEvents/3 && i < numOfEvents/6)
        {
            myEvent.WarehouseId = "Warehouse1";
            myEvent.ConveyorId = "Conveyor1.2";
            myEvent.RPM = rpm.Next(0, 700);
        }
        else
        {
            myEvent.WarehouseId = "Warehouse2";
            myEvent.ConveyorId = "Conveyor2.1";
            myEvent.RPM = rpm.Next(0, 700);
        }

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        string jsonString = JsonSerializer.Serialize(myEvent, options);

        EventData eventData = new EventData(Encoding.UTF8.GetBytes(jsonString));

        if (! eventBatch.TryAdd(eventData))
        {
            // if it is too large for the batch
            throw new Exception($"Event {i} is too large for the batch and cannot be sent.");
        }
    }
    
    try
    {
        // Use the producer client to send the batch of events to the event hub
        await producerClient.SendAsync(eventBatch);
        Console.WriteLine($"A batch of {numOfEvents} events has been published.");
    }

    finally
    {
        await producerClient.DisposeAsync();
    }
}
