using VHouse.Classes;

namespace VHouse.Interfaces
{
    /// <summary>
    /// Service interface for automated backup and disaster recovery operations.
    /// </summary>
    public interface IBackupService
    {
        /// <summary>
        /// Creates a full database backup with compression and encryption.
        /// </summary>
        Task<BackupResult> CreateFullBackupAsync(BackupOptions options);

        /// <summary>
        /// Creates an incremental backup containing only changes since last backup.
        /// </summary>
        Task<BackupResult> CreateIncrementalBackupAsync(BackupOptions options);

        /// <summary>
        /// Restores database from a backup file with validation.
        /// </summary>
        Task<RestoreResult> RestoreFromBackupAsync(RestoreOptions options);

        /// <summary>
        /// Validates backup integrity and recoverability.
        /// </summary>
        Task<BackupValidationResult> ValidateBackupAsync(string backupPath);

        /// <summary>
        /// Gets backup history and status information.
        /// </summary>
        Task<List<BackupRecord>> GetBackupHistoryAsync(DateTime? fromDate = null, DateTime? toDate = null);

        /// <summary>
        /// Schedules automated backups with specified frequency and retention.
        /// </summary>
        Task<bool> ScheduleAutomatedBackupsAsync(BackupSchedule schedule);

        /// <summary>
        /// Performs disaster recovery testing by creating test restore.
        /// </summary>
        Task<DisasterRecoveryTestResult> PerformDisasterRecoveryTestAsync(DisasterRecoveryTestOptions options);

        /// <summary>
        /// Creates point-in-time recovery backup for precise restoration.
        /// </summary>
        Task<BackupResult> CreatePointInTimeBackupAsync(DateTime pointInTime, BackupOptions options);

        /// <summary>
        /// Cleans up old backups based on retention policies.
        /// </summary>
        Task<BackupCleanupResult> CleanupOldBackupsAsync(BackupRetentionPolicy policy);

        /// <summary>
        /// Uploads backup to cloud storage for off-site protection.
        /// </summary>
        Task<CloudBackupResult> UploadBackupToCloudAsync(string backupPath, CloudStorageProvider provider);

        /// <summary>
        /// Downloads backup from cloud storage for restoration.
        /// </summary>
        Task<string> DownloadBackupFromCloudAsync(string cloudBackupId, CloudStorageProvider provider);

        /// <summary>
        /// Gets backup and recovery metrics for monitoring.
        /// </summary>
        Task<BackupMetrics> GetBackupMetricsAsync(DateTime? fromDate = null, DateTime? toDate = null);

        /// <summary>
        /// Creates configuration backup including application settings and certificates.
        /// </summary>
        Task<BackupResult> CreateConfigurationBackupAsync(ConfigurationBackupOptions options);
    }

    /// <summary>
    /// Backup configuration options.
    /// </summary>
    public class BackupOptions
    {
        public string BackupName { get; set; } = $"backup_{DateTime.UtcNow:yyyyMMdd_HHmmss}";
        public string BackupPath { get; set; } = string.Empty;
        public bool CompressBackup { get; set; } = true;
        public bool EncryptBackup { get; set; } = true;
        public string? EncryptionKey { get; set; }
        public BackupType BackupType { get; set; } = BackupType.Full;
        public List<string> TablesToInclude { get; set; } = new();
        public List<string> TablesToExclude { get; set; } = new();
        public int MaxRetryAttempts { get; set; } = 3;
        public TimeSpan CommandTimeout { get; set; } = TimeSpan.FromMinutes(30);
        public bool ValidateBackup { get; set; } = true;
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Backup type enumeration.
    /// </summary>
    public enum BackupType
    {
        Full,
        Incremental,
        Differential,
        PointInTime,
        Configuration
    }

    /// <summary>
    /// Backup operation result.
    /// </summary>
    public class BackupResult
    {
        public bool Success { get; set; }
        public string BackupId { get; set; } = Guid.NewGuid().ToString();
        public string BackupPath { get; set; } = string.Empty;
        public long BackupSizeBytes { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public BackupType BackupType { get; set; }
        public string DatabaseName { get; set; } = string.Empty;
        public string ServerVersion { get; set; } = string.Empty;
        public List<string> IncludedTables { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public List<string> Errors { get; set; } = new();
        public string ChecksumMD5 { get; set; } = string.Empty;
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Database restore options.
    /// </summary>
    public class RestoreOptions
    {
        public string BackupPath { get; set; } = string.Empty;
        public string TargetDatabaseName { get; set; } = string.Empty;
        public bool OverwriteExistingDatabase { get; set; } = false;
        public bool ValidateBeforeRestore { get; set; } = true;
        public bool CreateDatabaseBackupBeforeRestore { get; set; } = true;
        public DateTime? PointInTimeRestore { get; set; }
        public List<string> TablesToRestore { get; set; } = new();
        public string? DecryptionKey { get; set; }
        public int MaxRetryAttempts { get; set; } = 3;
        public TimeSpan CommandTimeout { get; set; } = TimeSpan.FromHours(2);
        public bool PerformConsistencyCheck { get; set; } = true;
    }

    /// <summary>
    /// Database restore result.
    /// </summary>
    public class RestoreResult
    {
        public bool Success { get; set; }
        public string RestoreId { get; set; } = Guid.NewGuid().ToString();
        public string RestoredDatabaseName { get; set; } = string.Empty;
        public string SourceBackupPath { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public long RestoredDataSizeBytes { get; set; }
        public List<string> RestoredTables { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public List<string> Errors { get; set; } = new();
        public bool ConsistencyCheckPassed { get; set; }
        public Dictionary<string, object> ValidationResults { get; set; } = new();
    }

    /// <summary>
    /// Backup validation result.
    /// </summary>
    public class BackupValidationResult
    {
        public bool IsValid { get; set; }
        public bool ChecksumMatches { get; set; }
        public bool CanBeRestored { get; set; }
        public bool EncryptionValid { get; set; }
        public long BackupSizeBytes { get; set; }
        public DateTime BackupDate { get; set; }
        public string DatabaseVersion { get; set; } = string.Empty;
        public List<string> ValidationErrors { get; set; } = new();
        public List<string> ValidationWarnings { get; set; } = new();
        public TimeSpan ValidationDuration { get; set; }
        public Dictionary<string, object> BackupMetadata { get; set; } = new();
    }

    /// <summary>
    /// Backup record for history tracking.
    /// </summary>
    public class BackupRecord
    {
        public string BackupId { get; set; } = string.Empty;
        public string BackupName { get; set; } = string.Empty;
        public BackupType BackupType { get; set; }
        public DateTime CreatedDate { get; set; }
        public string BackupPath { get; set; } = string.Empty;
        public long BackupSizeBytes { get; set; }
        public TimeSpan CreationDuration { get; set; }
        public BackupStatus Status { get; set; }
        public string DatabaseName { get; set; } = string.Empty;
        public bool IsCompressed { get; set; }
        public bool IsEncrypted { get; set; }
        public string ChecksumMD5 { get; set; } = string.Empty;
        public DateTime? LastValidated { get; set; }
        public bool ValidationPassed { get; set; }
        public string? CloudLocation { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Backup status enumeration.
    /// </summary>
    public enum BackupStatus
    {
        InProgress,
        Completed,
        Failed,
        Validating,
        Expired,
        Archived
    }

    /// <summary>
    /// Backup schedule configuration.
    /// </summary>
    public class BackupSchedule
    {
        public string ScheduleId { get; set; } = Guid.NewGuid().ToString();
        public string ScheduleName { get; set; } = string.Empty;
        public BackupFrequency Frequency { get; set; }
        public TimeSpan ScheduledTime { get; set; }
        public List<DayOfWeek> ScheduledDays { get; set; } = new();
        public BackupOptions BackupOptions { get; set; } = new();
        public BackupRetentionPolicy RetentionPolicy { get; set; } = new();
        public bool IsEnabled { get; set; } = true;
        public DateTime NextScheduledRun { get; set; }
        public DateTime? LastRun { get; set; }
        public bool SendNotifications { get; set; } = true;
        public List<string> NotificationEmails { get; set; } = new();
    }

    /// <summary>
    /// Backup frequency options.
    /// </summary>
    public enum BackupFrequency
    {
        Hourly,
        Daily,
        Weekly,
        Monthly,
        Custom
    }

    /// <summary>
    /// Backup retention policy.
    /// </summary>
    public class BackupRetentionPolicy
    {
        public int DailyBackupsToKeep { get; set; } = 7;
        public int WeeklyBackupsToKeep { get; set; } = 4;
        public int MonthlyBackupsToKeep { get; set; } = 12;
        public int YearlyBackupsToKeep { get; set; } = 5;
        public TimeSpan MaxBackupAge { get; set; } = TimeSpan.FromDays(365);
        public long MaxTotalBackupSize { get; set; } = 100L * 1024 * 1024 * 1024; // 100GB
        public bool DeleteExpiredBackups { get; set; } = true;
        public bool ArchiveOldBackups { get; set; } = false;
        public CloudStorageProvider? ArchiveLocation { get; set; }
    }

    /// <summary>
    /// Disaster recovery test options.
    /// </summary>
    public class DisasterRecoveryTestOptions
    {
        public string TestName { get; set; } = $"dr_test_{DateTime.UtcNow:yyyyMMdd_HHmmss}";
        public string BackupPath { get; set; } = string.Empty;
        public string TestDatabaseName { get; set; } = string.Empty;
        public bool PerformDataValidation { get; set; } = true;
        public bool TestApplicationConnectivity { get; set; } = true;
        public List<string> ValidationQueries { get; set; } = new();
        public TimeSpan MaxTestDuration { get; set; } = TimeSpan.FromHours(1);
        public bool CleanupAfterTest { get; set; } = true;
        public Dictionary<string, object> TestParameters { get; set; } = new();
    }

    /// <summary>
    /// Disaster recovery test result.
    /// </summary>
    public class DisasterRecoveryTestResult
    {
        public bool Success { get; set; }
        public string TestId { get; set; } = Guid.NewGuid().ToString();
        public string TestName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public TimeSpan RestoreTime { get; set; }
        public bool RestoreSuccessful { get; set; }
        public bool DataValidationPassed { get; set; }
        public bool ApplicationConnectivityPassed { get; set; }
        public List<ValidationResult> ValidationResults { get; set; } = new();
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public Dictionary<string, object> PerformanceMetrics { get; set; } = new();
        public string Summary { get; set; } = string.Empty;
    }


    /// <summary>
    /// Backup cleanup result.
    /// </summary>
    public class BackupCleanupResult
    {
        public bool Success { get; set; }
        public int BackupsDeleted { get; set; }
        public int BackupsArchived { get; set; }
        public long SpaceFreedBytes { get; set; }
        public TimeSpan CleanupDuration { get; set; }
        public List<string> DeletedBackups { get; set; } = new();
        public List<string> ArchivedBackups { get; set; } = new();
        public List<string> Errors { get; set; } = new();
        public string Summary { get; set; } = string.Empty;
    }

    /// <summary>
    /// Cloud backup result.
    /// </summary>
    public class CloudBackupResult
    {
        public bool Success { get; set; }
        public string CloudBackupId { get; set; } = string.Empty;
        public string CloudLocation { get; set; } = string.Empty;
        public CloudStorageProvider Provider { get; set; }
        public long UploadSizeBytes { get; set; }
        public TimeSpan UploadDuration { get; set; }
        public DateTime UploadTime { get; set; }
        public string ETag { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Cloud storage provider enumeration.
    /// </summary>
    public enum CloudStorageProvider
    {
        AmazonS3,
        AzureBlobStorage,
        GoogleCloudStorage,
        DigitalOceanSpaces,
        BackblazeB2
    }

    /// <summary>
    /// Backup and recovery metrics.
    /// </summary>
    public class BackupMetrics
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public int TotalBackups { get; set; }
        public int SuccessfulBackups { get; set; }
        public int FailedBackups { get; set; }
        public decimal BackupSuccessRate { get; set; }
        public long TotalBackupSizeBytes { get; set; }
        public TimeSpan AverageBackupDuration { get; set; }
        public TimeSpan FastestBackup { get; set; }
        public TimeSpan SlowestBackup { get; set; }
        public Dictionary<BackupType, int> BackupsByType { get; set; } = new();
        public int DisasterRecoveryTests { get; set; }
        public int PassedDisasterRecoveryTests { get; set; }
        public TimeSpan AverageRecoveryTime { get; set; }
        public List<BackupTrend> BackupTrends { get; set; } = new();
    }

    /// <summary>
    /// Backup trend data.
    /// </summary>
    public class BackupTrend
    {
        public DateTime Date { get; set; }
        public int BackupCount { get; set; }
        public long TotalSizeBytes { get; set; }
        public TimeSpan AverageDuration { get; set; }
        public decimal SuccessRate { get; set; }
    }

    /// <summary>
    /// Configuration backup options.
    /// </summary>
    public class ConfigurationBackupOptions
    {
        public string BackupName { get; set; } = $"config_backup_{DateTime.UtcNow:yyyyMMdd_HHmmss}";
        public string BackupPath { get; set; } = string.Empty;
        public bool IncludeApplicationSettings { get; set; } = true;
        public bool IncludeConnectionStrings { get; set; } = true;
        public bool IncludeCertificates { get; set; } = true;
        public bool IncludeEnvironmentVariables { get; set; } = true;
        public bool EncryptSensitiveData { get; set; } = true;
        public List<string> AdditionalPaths { get; set; } = new();
        public List<string> ExcludePatterns { get; set; } = new();
        public bool CompressBackup { get; set; } = true;
    }
}