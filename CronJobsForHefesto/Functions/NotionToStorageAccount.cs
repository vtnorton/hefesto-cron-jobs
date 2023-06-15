﻿using CronJobsForHefesto.Services;
using Microsoft.Extensions.Logging;
using System;

namespace CronJobsForHefesto.Functions
{
    public class NotionToStorageAccount
    {
        private readonly ILogger _logger;
        private NotionService _notionService;
        private StorageAccountService _storageAccountService;

        public NotionToStorageAccount(IHttpClientFactory clientFactory, ILogger logger) {
            _logger = logger;
            _notionService = new NotionService(_logger);
            _storageAccountService = new StorageAccountService(clientFactory);
        }

        public async Task ExcuteActionAsync()
        {
            var originalCovers = await _notionService.GetPageCoversAsync(false);
            if (originalCovers.Count == 0)
            {
                _logger.LogInformation("✅ No image was found to be replaced");
                
            }else
            {
                var newPathCovers = await _storageAccountService.UploadToStorageAccountAsync(originalCovers);
                _notionService.UpdatePageCover(newPathCovers);
            }
        }
    }
}
