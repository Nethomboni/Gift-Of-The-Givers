using System.Net;
using System.Text.Json;
using Azure.Data.Tables;
using GiftOfTheGivers.Functions.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GiftOfTheGivers.Functions.Functions
{
    // A.1/A.2 - HTTP-triggered function. The web app calls this whenever an
    // Employee posts a project update, and the function writes a durable
    // record of it into Azure Table Storage (Azurite locally, a real Storage
    // Account once deployed) - independent of the web app's own in-memory
    // EF Core database, which resets on every restart.
    public class LogProjectUpdateFunction
    {
        private readonly ILogger _logger;
        private readonly TableClient _tableClient;

        public LogProjectUpdateFunction(ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            _logger = loggerFactory.CreateLogger<LogProjectUpdateFunction>();

            var connectionString = configuration["StorageConnectionString"] ?? "UseDevelopmentStorage=true";
            var tableName = configuration["ProjectUpdatesTableName"] ?? "ProjectUpdateLogs";

            var serviceClient = new TableServiceClient(connectionString);
            _tableClient = serviceClient.GetTableClient(tableName);
            _tableClient.CreateIfNotExists();
        }

        [Function("LogProjectUpdate")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "projectupdates/log")]
            HttpRequestData req)
        {
            _logger.LogInformation("LogProjectUpdate triggered.");

            ProjectUpdateLogRequest? update;
            try
            {
                update = await JsonSerializer.DeserializeAsync<ProjectUpdateLogRequest>(
                    req.Body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (JsonException)
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Request body must be valid JSON.");
                return badRequest;
            }

            if (update is null || string.IsNullOrWhiteSpace(update.Title))
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("A project update Title is required.");
                return badRequest;
            }

            // RowKey descends by ticks so the newest update for a project is
            // always first when querying a partition in ascending RowKey order.
            var rowKey = (DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks).ToString("d19");

            var entity = new ProjectUpdateLogEntity
            {
                PartitionKey = update.ProjectId.ToString(),
                RowKey = rowKey,
                ProjectName = update.ProjectName,
                Title = update.Title,
                Description = update.Description,
                Location = update.Location,
                PostedByName = update.PostedByName,
                CreatedAt = update.CreatedAt == default ? DateTime.UtcNow : update.CreatedAt
            };

            await _tableClient.AddEntityAsync(entity);

            _logger.LogInformation(
                "Logged update '{Title}' for project {ProjectId} to Azure Table Storage.",
                update.Title, update.ProjectId);

            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteAsJsonAsync(new { logged = true, partitionKey = entity.PartitionKey, rowKey = entity.RowKey });
            return response;
        }
    }
}
