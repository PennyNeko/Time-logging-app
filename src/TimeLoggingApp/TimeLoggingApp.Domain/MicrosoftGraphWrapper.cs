using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using static System.Formats.Asn1.AsnWriter;

namespace TimeLoggingApp.Domain
{
    public class MicrosoftGraphWrapper
    {
        public GraphServiceClient Authenitcate()
        {
            var scopes = new[] { "Calendars.Read" };
            var tenantId = "common";

            // Value from app registration
            var clientId = "[CLIENT_ID_GOES_HERE]";

            // using Azure.Identity;
            var options = new InteractiveBrowserCredentialOptions
            {
                TenantId = tenantId,
                ClientId = clientId,
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
                // MUST be http://localhost or http://localhost:PORT
                // See https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/System-Browser-on-.Net-Core
                RedirectUri = new Uri("http://localhost:47923"),
            };

            // https://learn.microsoft.com/dotnet/api/azure.identity.interactivebrowsercredential
            var interactiveCredential = new InteractiveBrowserCredential(options);

            return new GraphServiceClient(interactiveCredential, scopes);
        }

        private GraphServiceClient GetIntegratedWindowsProvider(string clientId, string tenantId, string[] scopes)
        {
            //Using integrated windows provider
            var pca = PublicClientApplicationBuilder
                .Create(clientId)
                .WithTenantId(tenantId)
                .Build();

            var authProvider = new DelegateAuthenticationProvider(async (request) => {
                // Use Microsoft.Identity.Client to retrieve token
                var result = await pca.AcquireTokenByIntegratedWindowsAuth(scopes).ExecuteAsync();

                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.AccessToken);
            });

            return new GraphServiceClient(authProvider);
        }

        public async Task<string> GetFirstEventOfDay()
        {
            var client = Authenitcate();
            var events = await client.Me.Calendar.CalendarView.Request(
                new List<QueryOption>() { 
                    new QueryOption("startDateTime", DateTime.Today.Date.ToString("s")),
                    new QueryOption("endDateTime", DateTime.Today.AddDays(1).Date.ToString("s")) })
                .GetAsync();
            return events.First().Subject;
        }

    }
}