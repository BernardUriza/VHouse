using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VHouse.Classes;

namespace VHouse.Interfaces
{
    public enum DeploymentTarget
    {
        Development,
        Staging,
        Production
    }
    
    public interface IAIOrchestrationService
    {
        // Model Training & Management
        Task<ModelTrainingResult> TrainModelAsync(ModelConfig config, TrainingData data);
        Task<ModelTrainingResult> AutoTrainModelAsync(AutoMLConfig config);
        Task<PredictionResult> ExecutePredictionAsync(string modelId, PredictionInput input);
        Task<ModelPerformance> EvaluateModelAsync(string modelId, ValidationData data);
        Task<bool> DeployModelAsync(string modelId, DeploymentTarget target);
        
        // Model Lifecycle
        Task<List<MLModel>> GetModelsAsync();
        Task<MLModel> GetModelAsync(string modelId);
        Task<bool> DeleteModelAsync(string modelId);
        Task<ModelVersion> CreateModelVersionAsync(string modelId, ModelConfig config);
        Task<List<ModelVersion>> GetModelVersionsAsync(string modelId);
        
        // Experiment Management
        Task<Experiment> CreateExperimentAsync(ExperimentConfig config);
        Task<ExperimentRun> RunExperimentAsync(string experimentId, ExperimentParameters parameters);
        Task<List<Experiment>> GetExperimentsAsync();
        Task<ExperimentResults> GetExperimentResultsAsync(string experimentId);
        
        // Model Monitoring
        Task<ModelMonitoring> GetModelMonitoringAsync(string modelId);
        Task<DataDrift> DetectDataDriftAsync(string modelId);
        Task<BiasAnalysis> DetectModelBiasAsync(string modelId);
        Task<bool> SetModelAlertsAsync(string modelId, List<ModelAlert> alerts);
        
        // Pipeline Management
        Task<MLPipeline> CreatePipelineAsync(PipelineConfig config);
        Task<PipelineExecution> ExecutePipelineAsync(string pipelineId);
        Task<List<MLPipeline>> GetPipelinesAsync();
        Task<PipelineStatus> GetPipelineStatusAsync(string pipelineId);
    }
}