namespace VHouse.Interfaces
{
    /// <summary>
    /// Service for managing background jobs and tasks.
    /// </summary>
    public interface IBackgroundJobService
    {
        /// <summary>
        /// Enqueues a background job for execution.
        /// </summary>
        void EnqueueJob<T>(string jobName, T jobData, TimeSpan? delay = null);

        /// <summary>
        /// Schedules a recurring job.
        /// </summary>
        void ScheduleRecurringJob(string jobName, Func<Task> job, TimeSpan interval);

        /// <summary>
        /// Cancels a scheduled job.
        /// </summary>
        void CancelJob(string jobName);

        /// <summary>
        /// Gets job status.
        /// </summary>
        Task<string> GetJobStatusAsync(string jobName);
    }

    /// <summary>
    /// Background job data structure.
    /// </summary>
    public class BackgroundJob
    {
        public string JobName { get; set; } = string.Empty;
        public object? JobData { get; set; }
        public DateTime ScheduledTime { get; set; }
        public DateTime? ExecutedTime { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Running, Completed, Failed
        public string? ErrorMessage { get; set; }
        public int RetryCount { get; set; } = 0;
        public int MaxRetries { get; set; } = 3;
    }
}