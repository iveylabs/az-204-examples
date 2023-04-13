
using System;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Processor;
using System.Text.Json;

const string ehubNamespaceConnectionString = "";
const string eventHubName = "iveyhub";
const string blobStorageConnectionString = "";
const string blobContainerName = "checkpoints";

BlobContainerClient storageClient;

// The Event Hubs client types are safe to cache and use as a singleton for the lifetime
// of the application, which is best practice when events are being published or read regularly.        
EventProcessorClient processor;

await ReceiveEvents();

async Task ReceiveEvents()
{
    // Read from the default consumer group: $Default
    string consumerGroup = EventHubConsumerClient.DefaultConsumerGroupName;

    // Create a blob container client that the event processor will use 
    storageClient = new BlobContainerClient(blobStorageConnectionString, blobContainerName);

    // Create an event processor client to process events in the event hub
    processor = new EventProcessorClient(storageClient, consumerGroup, ehubNamespaceConnectionString, eventHubName);

    // Register handlers for processing events and handling errors
    processor.ProcessEventAsync += ProcessEventHandler;
    processor.ProcessErrorAsync += ProcessErrorHandler;

    // Start the processing
    await processor.StartProcessingAsync();

    // Wait for 30 seconds for the events to be processed
    await Task.Delay(TimeSpan.FromSeconds(30));

    // Stop the processing
    await processor.StopProcessingAsync();
}

async Task ProcessEventHandler(ProcessEventArgs eventArgs)
{
    // Write the body of the event to the console window
    Console.WriteLine($"Received event: {Encoding.UTF8.GetString(eventArgs.Data.Body.ToArray())} in partition {eventArgs.Partition.PartitionId}");

    // Update checkpoint in the blob storage so that the app receives only new events the next time it's run
    await eventArgs.UpdateCheckpointAsync(eventArgs.CancellationToken);
    
}

Task ProcessErrorHandler(ProcessErrorEventArgs eventArgs)
{
    // Write details about the error to the console window
    Console.WriteLine($"\tPartition '{ eventArgs.PartitionId}': an unhandled exception was encountered. This was not expected to happen.");
    Console.WriteLine(eventArgs.Exception.Message);
    return Task.CompletedTask;
}