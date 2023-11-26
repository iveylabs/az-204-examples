
using Microsoft.Graph;
using Azure.Identity;

// App registration variables
const string _clientId = "";
const string _tenantId = "";

// Configure the interactive browser credential options
var options = new InteractiveBrowserCredentialOptions
{
    ClientId = _clientId,
    TenantId = _tenantId,
    RedirectUri = new Uri("http://localhost")
};

// Obtain a token from the interactive authentication provider
var authProvider = new InteractiveBrowserCredential(options);

// Create a new Graph client with the authentication provider.
GraphServiceClient graphClient = new (authProvider);

// Make a Microsoft Graph API query
var user = await graphClient.Me.GetAsync();

// Customized greeting for the logged-in user
Console.WriteLine($"Hello, {user?.DisplayName}! Your name was obtained from MS Graph.\n");
