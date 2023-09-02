using CronJobsForHefesto.Models;
using Microsoft.Extensions.Logging;
using Notion.Client;

namespace CronJobsForHefesto.Services
{
    internal class NotionService
    {
        private readonly ILogger _logger;
        private NotionClient _notionClient;
        private readonly string _databaseId;

        public NotionService(ILogger logger)
        {
            _logger = logger;
            _databaseId = Environment.GetEnvironmentVariable("NOTION_DATABASE_ID");
            _notionClient = NotionClientFactory.Create(new ClientOptions
            {
                AuthToken = Environment.GetEnvironmentVariable("NOTION_AUTH_TOKEN")
            });
        }

        public async Task<List<Schedule>> GetEventsAsync()
        {
            var eventsList = new List<Schedule>();
            var selectEventFilter = new MultiSelectFilter("Type", contains: "Evento");

            var selectEnjoyFilter = new SelectFilter("Status-evento", equal: "Enjoy");
            var selectMarketingFilter = new SelectFilter("Status-evento", equal: "Criação/Marketing/Etc...");
            var selectTwitchLivesFilter = new SelectFilter("Status-evento", equal: "Twitch-lives");

            var queryEventInMarketingParams = new DatabasesQueryParameters
            {
                Filter = new CompoundFilter(
                    and: new List<Filter> { selectEventFilter, selectMarketingFilter }
                ),
                PageSize = 50
            };
            var queryEventInEnjoyParams = new DatabasesQueryParameters
            {
                Filter = new CompoundFilter(
                    and: new List<Filter> { selectEventFilter, selectEnjoyFilter }
                ),
                PageSize = 50
            };
            var queryEventInTwitchParams = new DatabasesQueryParameters
            {
                Filter = new CompoundFilter(
                    and: new List<Filter> { selectEventFilter, selectTwitchLivesFilter }
                ),
                PageSize = 50
            };

            var eventsInMarketing = await _notionClient.Databases.QueryAsync(_databaseId, queryEventInMarketingParams);
            var eventsInEnjoy = await _notionClient.Databases.QueryAsync(_databaseId, queryEventInEnjoyParams);
            var eventsInTwitch = await _notionClient.Databases.QueryAsync(_databaseId, queryEventInTwitchParams);

            List<Page> events = new List<Page>();
            events.AddRange(eventsInMarketing.Results.ToList());
            events.AddRange(eventsInEnjoy.Results.ToList());
            events.AddRange(eventsInTwitch.Results.ToList());

            foreach (dynamic item in events)
            {
                try
                {
                    string? description = "Evento privado";
                    string? link = "Evento privado";
                    string? coverUrl = string.Empty;

                    var types = item.Properties["Type"]?.MultiSelect as List<SelectOption>;
                    bool isPrivate = types.Any(x => x.Name == "Private");

                    if (!isPrivate)
                    {
                        description = item.Properties["Observação"]?.RichText[0]?.PlainText;
                        link = item.Properties["Link"]?.Url;
                        coverUrl = GetCoverURL(item);
                    }

                    var startDate = item.Properties["Date"].Date?.Start;
                    var endDate = item.Properties["Date"].Date?.End;

                    eventsList.Add(new Schedule()
                    {
                        Name = item.Properties["Name"].Title[0].PlainText,
                        Description = description,
                        Link = link,
                        IsPrivate = isPrivate,
                        StartDate = DateTime.Parse(startDate),
                        EndDate = DateTime.Parse(endDate),
                        CoverUrl = coverUrl,
                    });
                }
                catch (Exception)
                {
                }

            }

            return eventsList;
        }

        public async Task<List<PageCover>> GetPageCoversAsync(bool showStorageCovers = true)
        {
            var pagesCovers = new List<PageCover>();

            var selectPostFilter = new MultiSelectFilter("Type", contains: "Post");
            var selectChangelogFilter = new MultiSelectFilter("Type", contains: "Changelog");

            var selectQueryPoster = new SelectFilter("Status-post", equal: "Query");
            var selectFollowUpPoster = new SelectFilter("Status-post", equal: "Follow-up");
            var selectPublishedPoster = new SelectFilter("Status-post", equal: "Published");
            var statusFilter = new List<Filter> { selectQueryPoster, selectFollowUpPoster, selectPublishedPoster };

            var queryPostParams = new DatabasesQueryParameters
            {
                Filter = new CompoundFilter(
                    or: statusFilter,
                    and: new List<Filter> { selectPostFilter }
                ),
                PageSize = 250,
                Sorts = new List<Sort> { new()
                {
                    Property = "Date",
                    Direction = Direction.Descending
                } }
            };

            var queryChangelogParams = new DatabasesQueryParameters
            {
                Filter = new CompoundFilter(
                    and: new List<Filter> { selectChangelogFilter }
                ),
                PageSize = 250,
                Sorts = new List<Sort> { new()
                {
                    Property = "Date",
                    Direction = Direction.Descending
                } }
            };

            var posts = await _notionClient.Databases.QueryAsync(_databaseId, queryPostParams);
            var changelogs = await _notionClient.Databases.QueryAsync(_databaseId, queryChangelogParams);

            foreach (var item in posts.Results)
            {
                if (item.Cover != null)
                {
                    pagesCovers.Add(new PageCover()
                    {
                        PageId = item.Id,
                        PageLink = item.Url,
                        PhotoURL = GetCoverURL(item)
                    });
                    Console.WriteLine("Imported Notion page: " + item.Id);
                }

            }

            foreach (var item in changelogs.Results)
            {
                if (item.Cover != null)
                {
                    pagesCovers.Add(new PageCover()
                    {
                        PageId = item.Id,
                        PageLink = item.Url,
                        PhotoURL = GetCoverURL(item)
                    });
                    Console.WriteLine("Imported Notion page: " + item.Id);
                }

            }

            if (showStorageCovers)
                return pagesCovers.ToList();

            return pagesCovers.Where(x => !x.IsAlreadyStoraged).ToList();
        }

        private string GetCoverURL(dynamic item)
        {
            if (item.Cover is ExternalFile externalCover && externalCover.External.Url != null)
                return externalCover.External.Url;

            if (item.Cover is UploadedFile coverFile && coverFile.File.Url != null)
                return item.Cover.File.Url;

            return "";
        }

        public void UpdatePageCover(List<PageCover> pageCovers)
        {
            foreach (var item in pageCovers)
            {
                UpdatePageCover(item);
            }
        }

        private void UpdatePageCover(PageCover pageCover)
        {
            var external = new ExternalFile.Info()
            {
                Url = pageCover.PhotoURL
            };
            var cover = new ExternalFile()
            {
                External = external
            };
            var toUpdate = new PagesUpdateParameters()
            {
                Cover = cover
            };
            _notionClient.Pages.UpdateAsync(pageCover.PageId, toUpdate);
            _logger.LogInformation("▶️ Updated page: " + pageCover.PageId + " (" + pageCover.PageLink + ")");
        }
    }
}