using Azure;
using Azure.Data.Tables;

namespace GiftOfTheGivers.Functions.Models
{
    // What the web app sends once an employee posts a project update.
    public class ProjectUpdateLogRequest
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string? PostedByName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Table Storage entity. PartitionKey groups rows by project so per-project
    // history can be queried cheaply; RowKey is reverse-chronological (ticks
    // subtracted from DateTime.MaxValue) so the newest entries sort first.
    public class ProjectUpdateLogEntity : ITableEntity
    {
        public string PartitionKey { get; set; } = string.Empty;
        public string RowKey { get; set; } = string.Empty;
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public string ProjectName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string? PostedByName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
