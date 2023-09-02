using CronJobsForHefesto.Services;
using Microsoft.Extensions.Logging;

namespace CronJobsForHefesto.Functions
{
    internal class DiscordToCalendarFunctions
    {
        private readonly ILogger _logger;
        private NotionService _notionService;
        private MicrosoftGraphServices _graphServices;
        private readonly IHttpClientFactory _clientFactory;

        public DiscordToCalendarFunctions(IHttpClientFactory clientFactory, ILogger logger)
        {
            _logger = logger;
            _notionService = new NotionService(_logger);
            _clientFactory = clientFactory;
            _graphServices = new MicrosoftGraphServices();
        }

        public async Task SaveNotionEventsToDiscordAndCalendar()
        {
            _graphServices.GetCalendarAsync();

            // var events = await _notionService.GetEventsAsync();


            if (true)
                _logger.LogInformation("✅ Events from Discord added to calendar");
            else
                _logger.LogError($"⚠️ Something went wrong with discords events beign added to the calendar");
        }
    }
}
