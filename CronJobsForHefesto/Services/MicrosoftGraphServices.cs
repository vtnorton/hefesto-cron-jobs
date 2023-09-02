using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client;
using System.Net.Http.Headers;

namespace CronJobsForHefesto.Services
{
    internal class MicrosoftGraphServices
    {
        private GraphServiceClient _graphServiceClient;
        private string _userToken;

        public MicrosoftGraphServices()
        {

            _userToken = Environment.GetEnvironmentVariable("MICROSOFT_GRAPH_TOKEN");
            _graphServiceClient = new GraphServiceClient(new DelegateAuthenticationProvider((requestMessage) =>
            {
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _userToken);
                return Task.CompletedTask;
            }));
        }

        public async Task GetCalendarAsync()
        {
            //https://learn.microsoft.com/en-us/graph/auth/
            //https://learn.microsoft.com/en-us/graph/auth-v2-user?tabs=http
            //https://learn.microsoft.com/en-us/graph/auth-v2-service?tabs=curl
            //https://learn.microsoft.com/en-us/graph/auth-v2-service?tabs=http

            var confidentialClientApplication = ConfidentialClientApplicationBuilder
                .Create("")
                .WithClientSecret("")
                .WithAuthority(new Uri($"https://login.microsoftonline.com/TENAT_ID"))
                .Build();

            var authResult = await confidentialClientApplication
                .AcquireTokenForClient(new string[] { "https://graph.microsoft.com/.default" })
                .ExecuteAsync();

            _graphServiceClient = new GraphServiceClient(new DelegateAuthenticationProvider((requestMessage) =>
            {
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _userToken);
                return Task.CompletedTask;
            }));


            var calendars = _graphServiceClient.Me.Calendars;
        }
    }
}
