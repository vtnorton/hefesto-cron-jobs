using CronJobsForHefesto.Services;

namespace CronJobsForHefesto.Functions
{
    public class AccessTokenFunctions
    {
        private readonly IHttpClientFactory _clientFactory;

        public AccessTokenFunctions(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task RefreshInstagramToken()
        {
            var accessToken = Environment.GetEnvironmentVariable("INSTAGRAM_ACCESS_TOKEN");

            var apiUrl = $"https://graph.instagram.com/refresh_access_token?grant_type=ig_refresh_token&access_token={accessToken}";

            var httpClient = _clientFactory.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);
            var response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
                Console.WriteLine("✅ Instagram Token refreshed");
            else
                Console.WriteLine($"⚠️ Instagram Token failed to refresh: {response.StatusCode}");
        }
    }
}
