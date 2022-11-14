using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

Console.WriteLine("Azure Blob storage exercise\n");
ProcessAsync().GetAwaiter().GetResult();

Console.WriteLine("Press ENTER to exit the sample application...");
Console.ReadLine();

static async Task ProcessAsync()
{
    string storageConnectionString = "";
    string containerName = "az204storagedemo";
    string localPath = ".\\data";
    string fileName = "az204demo.txt";
    string localFilePath = Path.Combine(localPath, fileName);
    string downloadedFilePath = localFilePath.Replace(".txt", "_DOWNLOADED.txt");
    
    // Create a client that can authenticate with a connection string
    Console.WriteLine("Creating BlobServiceClient...");
    BlobServiceClient blobServiceClient = new BlobServiceClient(storageConnectionString);

    // Create the container and return a BlobContainerClient object
    Console.WriteLine("Creating BlobContainerClient...");
    BlobContainerClient containerClient = await blobServiceClient.CreateBlobContainerAsync(containerName);
    Console.WriteLine($"A container named '{containerName}' has been created. ");
    Console.WriteLine("Press ENTER to create and upload a file to the container...");
    Console.ReadLine();

    // Write text to the file
    await File.WriteAllTextAsync(localFilePath, "Hello, World! This is my new demo file.");

    //Get a reference to the blob and create it if it doesn't already exist
    BlobClient blobClient = containerClient.GetBlobClient(fileName);

    Console.WriteLine($"Uploading {fileName} as blob: \n\t{blobClient.Uri}");

    // Open the file and upload its data
    using FileStream uploadFileStream = File.OpenRead(localFilePath);
    await blobClient.UploadAsync(uploadFileStream, true);
    uploadFileStream.Close();

    Console.WriteLine("The file was uploaded. Press ENTER to list all blobs in the container...");
    Console.ReadLine();

    // List all blobs in the container
    Console.WriteLine("Listing blobs...");
    await foreach(BlobItem blobItem in containerClient.GetBlobsAsync())
    {
        Console.WriteLine($"\t{blobItem.Name}");
    }

    Console.WriteLine("Press ENTER to list some properties of the container...");
    Console.ReadLine();

    // Get some container properties and write them to console
    var properties = await containerClient.GetPropertiesAsync();
    Console.WriteLine($"Properties of container {containerClient.Uri}:");
    Console.WriteLine($"\tETag: {properties.Value.ETag}");
    Console.WriteLine($"\tLast modified time in UTC: {properties.Value.LastModified}:");

    Console.WriteLine("Press ENTER to set some metadata on the container...");
    Console.ReadLine();

    IDictionary<string, string> metadata = new Dictionary<string, string>();

    // Add some metadata to the Dictionary
    metadata.Add("docType", "textDocuments");
    metadata.Add("category", "demo");

    // Set the metadata
    await containerClient.SetMetadataAsync(metadata);

    Console.WriteLine("Metadata set. Press ENTER to retrieve the container's metadata...");
    Console.ReadLine();

    // Get the container's metadata and write to console
    properties = await containerClient.GetPropertiesAsync();
    Console.WriteLine("Container metadata:");
    foreach(var metadataItem in properties.Value.Metadata)
    {
        Console.WriteLine($"\tKey: {metadataItem.Key}");
        Console.WriteLine($"\tValue: {metadataItem.Value}");
    }

    Console.WriteLine("Press ENTER to download the file with an altered file name...");
    Console.ReadLine();

    Console.WriteLine($"Downloading blob to\n\t{downloadedFilePath}");
    
    // Download the blob locally
    BlobDownloadInfo downloadInfo = await blobClient.DownloadAsync();

    using (FileStream downloadFileStream = File.OpenWrite(downloadedFilePath))
    {
        await downloadInfo.Content.CopyToAsync(downloadFileStream);
        downloadFileStream.Close();
    }
    Console.WriteLine("File downloaded. Press ENTER to delete the container and local files.");
    Console.ReadLine();

    // Delete the container and clean up local files
    Console.WriteLine("\n\nDeleting the container...");
    await containerClient.DeleteAsync();

    Console.WriteLine("Deleting the local source and downloaded files...");
    File.Delete(localFilePath);
    File.Delete(downloadedFilePath);
    Console.WriteLine("Clean up completed.");
}