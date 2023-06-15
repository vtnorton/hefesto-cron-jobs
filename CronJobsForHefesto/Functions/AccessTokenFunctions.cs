using CronJobsForHefesto.Services;
using Microsoft.Extensions.Logging;

namespace CronJobsForHefesto.Functions
{
    public class AccessTokenFunctions
    {
        private readonly ILogger _logger;
        private readonly IHttpClientFactory _clientFactory;

        public AccessTokenFunctions(IHttpClientFactory clientFactory, ILogger logger)
        {
            _logger = logger;
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
                _logger.LogInformation("✅ Instagram Token refreshed");
            else
                _logger.LogError($"⚠️ Instagram Token failed to refresh: {response.StatusCode}");
        }
    }
}
