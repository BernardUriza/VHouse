using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VHouse.Interfaces
{
    public interface IDataWarehouseService
    {
        // ETL Operations
        Task<ETLResult> ExtractTransformLoadAsync(ETLJobDefinition jobDefinition);
        Task<List<ETLJob>> GetActiveETLJobsAsync();
        Task<bool> CancelETLJobAsync(string jobId);
        
        // Data Marts
        Task<DataMart> CreateDataMartAsync(DataMartDefinition definition);
        Task<List<DataMart>> GetDataMartsAsync();
        Task<bool> RefreshDataMartAsync(string dataMartId);
        
        // Historical Data
        Task<HistoricalDataSet> GetHistoricalDataAsync(HistoricalDataQuery query);
        Task<AggregatedData> GetAggregatedDataAsync(AggregationRequest request);
        Task<bool> ArchiveDataAsync(ArchiveRequest request);
        
        // Data Quality
        Task<DataQualityReport> RunDataQualityChecksAsync();
        Task<List<DataIssue>> IdentifyDataIssuesAsync();
        Task<bool> CleanDataAsync(DataCleaningRequest request);
    }

    public class ETLJobDefinition
    {
        public string JobId { get; set; }
        public string Name { get; set; }
        public string SourceConnection { get; set; }
        public string DestinationConnection { get; set; }
        public List<TransformationRule> Transformations { get; set; }
        public Schedule Schedule { get; set; }
    }

    public class ETLResult
    {
        public string JobId { get; set; }
        public bool Success { get; set; }
        public int RecordsProcessed { get; set; }
        public TimeSpan ProcessingTime { get; set; }
        public List<string> Errors { get; set; }
    }

    public class DataMart
    {
        public string DataMartId { get; set; }
        public string Name { get; set; }
        public string Subject { get; set; }
        public DateTime LastRefresh { get; set; }
        public Dictionary<string, object> Schema { get; set; }
    }

    // Additional supporting classes
    public class ETLJob { }
    public class DataMartDefinition { }
    public class HistoricalDataSet { }
    public class HistoricalDataQuery { }
    public class AggregatedData { }
    public class AggregationRequest { }
    public class ArchiveRequest { }
    public class DataQualityReport { }
    public class DataIssue { }
    public class DataCleaningRequest { }
    public class TransformationRule { }
    public class Schedule { }
}