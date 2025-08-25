using System.Collections.Concurrent;
using VHouse.Interfaces;

namespace VHouse.Services
{
    /// <summary>
    /// Simple in-memory background job service for development.
    /// In production, consider using Hangfire, Quartz.NET, or Azure Service Bus.
    /// </summary>
    public class BackgroundJobService : IBackgroundJobService, IHostedService, IDisposable
    {
        private readonly ILogger<BackgroundJobService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly ConcurrentQueue<BackgroundJob> _jobQueue = new();
        private readonly ConcurrentDictionary<string, BackgroundJob> _jobs = new();
        private readonly ConcurrentDictionary<string, Timer> _recurringJobs = new();
        private Timer? _processingTimer;

        public BackgroundJobService(ILogger<BackgroundJobService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public void EnqueueJob<T>(string jobName, T jobData, TimeSpan? delay = null)
        {
            var job = new BackgroundJob
            {
                JobName = jobName,
                JobData = jobData,
                ScheduledTime = DateTime.UtcNow.Add(delay ?? TimeSpan.Zero)
            };

            _jobs[jobName] = job;
            _jobQueue.Enqueue(job);
            
            _logger.LogInformation("Job {JobName} enqueued for execution at {ScheduledTime}", 
                jobName, job.ScheduledTime);
        }

        public void ScheduleRecurringJob(string jobName, Func<Task> job, TimeSpan interval)
        {
            var timer = new Timer(async _ =>
            {
                try
                {
                    _logger.LogDebug("Executing recurring job: {JobName}", jobName);
                    await job();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing recurring job: {JobName}", jobName);
                }
            }, null, TimeSpan.Zero, interval);

            _recurringJobs[jobName] = timer;
            _logger.LogInformation("Recurring job {JobName} scheduled with interval {Interval}", 
                jobName, interval);
        }

        public void CancelJob(string jobName)
        {
            if (_jobs.TryRemove(jobName, out var job))
            {
                job.Status = "Cancelled";
                _logger.LogInformation("Job {JobName} cancelled", jobName);
            }

            if (_recurringJobs.TryRemove(jobName, out var timer))
            {
                timer.Dispose();
                _logger.LogInformation("Recurring job {JobName} cancelled", jobName);
            }
        }

        public Task<string> GetJobStatusAsync(string jobName)
        {
            if (_jobs.TryGetValue(jobName, out var job))
            {
                return Task.FromResult(job.Status);
            }
            return Task.FromResult("Not Found");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Background Job Service starting...");
            
            // Start the job processing timer
            _processingTimer = new Timer(ProcessJobs, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
            
            // Schedule some system maintenance jobs
            ScheduleSystemJobs();
            
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Background Job Service stopping...");
            
            _processingTimer?.Change(Timeout.Infinite, 0);
            
            // Dispose all recurring job timers
            foreach (var timer in _recurringJobs.Values)
            {
                timer.Dispose();
            }
            _recurringJobs.Clear();
            
            return Task.CompletedTask;
        }

        private void ScheduleSystemJobs()
        {
            // Cache cleanup every 30 minutes
            ScheduleRecurringJob("cache-cleanup", async () =>
            {
                using var scope = _serviceProvider.CreateScope();
                var caching = scope.ServiceProvider.GetService<ICachingService>();
                if (caching != null)
                {
                    _logger.LogDebug("Running cache cleanup...");
                    // Implement cache cleanup logic here
                }
            }, TimeSpan.FromMinutes(30));

            // Database maintenance every 6 hours
            ScheduleRecurringJob("db-maintenance", async () =>
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                
                _logger.LogDebug("Running database maintenance...");
                // Implement database cleanup/optimization here
            }, TimeSpan.FromHours(6));
        }

        private async void ProcessJobs(object? state)
        {
            var currentTime = DateTime.UtcNow;
            var processedJobs = new List<BackgroundJob>();

            while (_jobQueue.TryDequeue(out var job))
            {
                if (job.ScheduledTime <= currentTime && job.Status == "Pending")
                {
                    await ProcessSingleJob(job);
                    processedJobs.Add(job);
                }
                else if (job.ScheduledTime > currentTime)
                {
                    // Put the job back in the queue if not ready
                    _jobQueue.Enqueue(job);
                    break;
                }
            }

            // Re-enqueue failed jobs that can be retried
            foreach (var job in processedJobs.Where(j => j.Status == "Failed" && j.RetryCount < j.MaxRetries))
            {
                job.RetryCount++;
                job.Status = "Pending";
                job.ScheduledTime = DateTime.UtcNow.AddSeconds(30 * job.RetryCount); // Exponential backoff
                _jobQueue.Enqueue(job);
            }
        }

        private async Task ProcessSingleJob(BackgroundJob job)
        {
            try
            {
                job.Status = "Running";
                job.ExecutedTime = DateTime.UtcNow;

                _logger.LogDebug("Processing job: {JobName}", job.JobName);

                using var scope = _serviceProvider.CreateScope();

                // Execute job based on job name
                await ExecuteJobLogic(job, scope.ServiceProvider);

                job.Status = "Completed";
                _logger.LogDebug("Job {JobName} completed successfully", job.JobName);
            }
            catch (Exception ex)
            {
                job.Status = "Failed";
                job.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Job {JobName} failed: {ErrorMessage}", job.JobName, ex.Message);
            }
        }

        private async Task ExecuteJobLogic(BackgroundJob job, IServiceProvider serviceProvider)
        {
            switch (job.JobName)
            {
                case "inventory-update":
                    await ProcessInventoryUpdate(job, serviceProvider);
                    break;
                case "email-notification":
                    await ProcessEmailNotification(job, serviceProvider);
                    break;
                case "data-export":
                    await ProcessDataExport(job, serviceProvider);
                    break;
                default:
                    _logger.LogWarning("Unknown job type: {JobName}", job.JobName);
                    break;
            }
        }

        private async Task ProcessInventoryUpdate(BackgroundJob job, IServiceProvider serviceProvider)
        {
            // Implement inventory update logic
            await Task.Delay(100); // Simulate work
        }

        private async Task ProcessEmailNotification(BackgroundJob job, IServiceProvider serviceProvider)
        {
            // Implement email notification logic
            await Task.Delay(100); // Simulate work
        }

        private async Task ProcessDataExport(BackgroundJob job, IServiceProvider serviceProvider)
        {
            // Implement data export logic
            await Task.Delay(100); // Simulate work
        }

        public void Dispose()
        {
            _processingTimer?.Dispose();
            foreach (var timer in _recurringJobs.Values)
            {
                timer.Dispose();
            }
        }
    }
}