namespace CronJobsForHefesto.Models
{
    internal class Schedule
    {
        public bool IsPrivate { get; set; }
        public string? Name { get; set; }
        public string? CoverUrl { get; set; }
        public string? Description { get; set; }
        public string? Link { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
