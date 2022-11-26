using Microsoft.Azure.Cosmos;

ProcessAsync().GetAwaiter().GetResult();

Console.WriteLine("Press ENTER to exit the sample application...");
Console.ReadLine();

static async Task ProcessAsync()
{
    string endpointUri = "";
    string primaryKey = "";

    CosmosClient cosmosClient;
    Database database;
    Container container;

    string databaseId = "az204Database";
    string containerId = "az204Container";

    Console.WriteLine("Beginning operations...\n");

    // Create a new Cosmos client instance
    cosmosClient = new CosmosClient(endpointUri, primaryKey);
    Console.WriteLine("Created new CosmosClient.");

    // Create a new database if it doesn't already exist
    database = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
    Console.WriteLine($"Created database: {database.Id}\nPress ENTER to create the container...");
    Console.ReadLine();

    // Create a new container within the database if it doesn't already exist
    container = await database.CreateContainerIfNotExistsAsync(containerId, "/lastName");
    Console.WriteLine($"Created container: {container.Id}\nPress ENTER to create a new item in the container...");
    Console.ReadLine();

    // Create new objects
    var paul = await CreateItem(container, Guid.NewGuid().ToString(), "Paul", "Ivey", "Brown");
    var another = await CreateItem(container, Guid.NewGuid().ToString(), "Another", "Ivey", "Blue");
    var john = await CreateItem(container, Guid.NewGuid().ToString(), "John", "Wick", "Brown");

    Console.WriteLine($"Paul created in container: {paul.id}.");

    // Read the item using the unique id
    ItemResponse<Entry> response = await  container.ReadItemAsync<Entry>(
        paul.id,
        new PartitionKey(paul.lastName)
    );

    // Serialise response
    Entry item = response.Resource;
    
    Console.WriteLine("Details obtained from reading the item:");
    Console.WriteLine($"ID: {item.id}");
    Console.WriteLine($"Name: {item.firstName} {item.lastName}");
    Console.WriteLine($"Eye colour: {item.eyeColour}");

    // Query using SQL
    var iterator = container.GetItemQueryIterator<Entry>(
        "SELECT * FROM c WHERE c.lastName = 'Ivey'"
    );

    // Go through all results from the query
    while(iterator.HasMoreResults)
    {
        var batch = await iterator.ReadNextAsync();
        foreach(Entry entry in batch)
        {
            Console.WriteLine($"id: {entry.id}\r{entry.firstName} {entry.lastName}\t{entry.eyeColour}");
        }
    }
}

static async Task<Entry> CreateItem(Container container, string id, string first, string last, string colour)
{
    Entry newItem = new(
        id: id,
        firstName: first,
        lastName: last,
        eyeColour: colour
    );
    Entry createdItem = await container.UpsertItemAsync<Entry>(
        item: newItem,
        partitionKey: new PartitionKey(newItem.lastName)
    );
    return createdItem;
}

// Record representing an item in the container
public record Entry(
    string id,
    string firstName,
    string lastName,
    string eyeColour
);
