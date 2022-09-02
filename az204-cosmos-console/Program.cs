using Microsoft.Azure.Cosmos;

ProcessAsync().GetAwaiter().GetResult();

Console.WriteLine("Press ENTER to exit the sample application...");
Console.ReadLine();

static async Task ProcessAsync()
{
    string EndpointUri = "";
    string PrimaryKey = "";

    CosmosClient cosmosClient;
    Database database;
    Container container;

    string databaseId = "az204Database";
    string containerId = "az204Container";

    Console.WriteLine("Beginning operations...\n");

    // Create a new Cosmos client instance
    cosmosClient = new CosmosClient(EndpointUri, PrimaryKey);
    Console.WriteLine("Created new CosmosClient.");

    // Create a new database if it doesn't already exist
    database = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
    Console.WriteLine($"Created database: {database.Id}\nPress ENTER to create the container...");
    Console.ReadLine();

    // Create a new container within the database if it doesn't already exist
    container = await database.CreateContainerIfNotExistsAsync(containerId, "/lastName");
    Console.WriteLine($"Created container: {container.Id}\nPress ENTER to create a new item in the container...");
    Console.ReadLine();

    string paulGuid = Guid.NewGuid().ToString();

    // Create a new object and upset (create or replace) to the container
    Entry newItem = new(
        id: paulGuid,
        firstName: "Paul",
        lastName: "Ivey",
        eyeColour: "Brown"
    );
    Entry createdItem = await container.UpsertItemAsync<Entry>(
        item: newItem,
        partitionKey: new PartitionKey(newItem.lastName)
    );

    Console.WriteLine($"New item created in container: {createdItem.id}.");    
}

// Record representing an item in the container
public record Entry(
    string id,
    string firstName,
    string lastName,
    string eyeColour
);