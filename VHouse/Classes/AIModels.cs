using System.ComponentModel.DataAnnotations;

namespace VHouse.Classes;

// AI Orchestration Models
public class ModelConfig
{
    public string ModelName { get; set; } = string.Empty;
    public string Algorithm { get; set; } = string.Empty;
    public int MaxEpochs { get; set; }
    public double LearningRate { get; set; }
    public Dictionary<string, object> Hyperparameters { get; set; } = new();
}

public class TrainingData
{
    public string DatasetPath { get; set; } = string.Empty;
    public int SampleCount { get; set; }
    public int FeatureCount { get; set; }
    public Dictionary<string, object> Features { get; set; } = new();
}

public class ModelTrainingResult
{
    public string ModelId { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
    public double TrainingAccuracy { get; set; }
    public double ValidationAccuracy { get; set; }
    public TimeSpan TrainingTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public Dictionary<string, double> Metrics { get; set; } = new();
}

public class PredictionInput
{
    public Dictionary<string, object> Features { get; set; } = new();
    public string InputType { get; set; } = string.Empty;
}

public class PredictionResult
{
    public string ModelId { get; set; } = string.Empty;
    public object Prediction { get; set; } = new();
    public double Confidence { get; set; }
    public TimeSpan ProcessingTime { get; set; }
    public Dictionary<string, double> ClassProbabilities { get; set; } = new();
}

public class ValidationData
{
    public string DatasetPath { get; set; } = string.Empty;
    public int SampleCount { get; set; }
    public Dictionary<string, object> TestSamples { get; set; } = new();
}

public class ModelPerformance
{
    public string ModelId { get; set; } = string.Empty;
    public double Accuracy { get; set; }
    public double Precision { get; set; }
    public double Recall { get; set; }
    public double F1Score { get; set; }
    public int[,] ConfusionMatrix { get; set; } = new int[0,0];
    public DateTime EvaluationDate { get; set; }
}

public class DeploymentConfig
{
    public string Environment { get; set; } = string.Empty;
    public int CpuCores { get; set; }
    public int MemoryGB { get; set; }
    public int StorageGB { get; set; }
}

public class ModelDeployment
{
    public string DeploymentId { get; set; } = string.Empty;
    public string ModelId { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public DateTime DeployedAt { get; set; }
    public DeploymentResources Resources { get; set; } = new();
}

public class DeploymentResources
{
    public int CpuCores { get; set; }
    public int MemoryGB { get; set; }
    public int StorageGB { get; set; }
}

public class ModelMonitoring
{
    public string ModelId { get; set; } = string.Empty;
    public string HealthStatus { get; set; } = string.Empty;
    public int RequestsPerSecond { get; set; }
    public TimeSpan LatencyP95 { get; set; }
    public double ErrorRate { get; set; }
    public double DriftScore { get; set; }
    public DateTime LastUpdated { get; set; }
    public Dictionary<string, object> Metrics { get; set; } = new();
}

public class ModelVersion
{
    public string VersionId { get; set; } = string.Empty;
    public string ModelId { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public double Accuracy { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Changes { get; set; } = string.Empty;
    public string TrainingDataset { get; set; } = string.Empty;
    public Dictionary<string, double> TrainingMetrics { get; set; } = new();
    public long ModelSizeBytes { get; set; }
    public List<string> Dependencies { get; set; } = new();
}

public class AutoMLConfig
{
    public string TaskType { get; set; } = string.Empty;
    public TimeSpan MaxTrainingTime { get; set; }
    public int MaxModels { get; set; }
}

public class AutoMLResult
{
    public ModelCandidate BestModel { get; set; } = new();
    public List<ModelCandidate> AllCandidates { get; set; } = new();
    public TimeSpan ExecutionTime { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class ModelCandidate
{
    public string Algorithm { get; set; } = string.Empty;
    public double Score { get; set; }
    public TimeSpan TrainingTime { get; set; }
    public Dictionary<string, object> Hyperparameters { get; set; } = new();
    public double CrossValidationScore { get; set; }
    public Dictionary<string, double> FeatureImportance { get; set; } = new();
}

public class TuningConfig
{
    public int MaxTrials { get; set; }
    public TimeSpan MaxTime { get; set; }
    public Dictionary<string, object> SearchSpace { get; set; } = new();
}

public class HyperparameterTuning
{
    public string ModelId { get; set; } = string.Empty;
    public Dictionary<string, object> BestParameters { get; set; } = new();
    public double BestScore { get; set; }
    public int TotalTrials { get; set; }
    public TimeSpan ExecutionTime { get; set; }
}

public class DriftDetectionConfig
{
    public string Method { get; set; } = string.Empty;
    public double Threshold { get; set; }
}

public class DataDrift
{
    public string ModelId { get; set; } = string.Empty;
    public double DriftScore { get; set; }
    public string DetectionMethod { get; set; } = string.Empty;
    public List<FeatureDrift> Features { get; set; } = new();
    public DateTime DetectedAt { get; set; }
    public string Severity { get; set; } = string.Empty;
}

public class FeatureDrift
{
    public string FeatureName { get; set; } = string.Empty;
    public double DriftScore { get; set; }
}

public class BiasConfig
{
    public List<string> ProtectedAttributes { get; set; } = new();
    public string FairnessMetric { get; set; } = string.Empty;
}

public class BiasAnalysis
{
    public string ModelId { get; set; } = string.Empty;
    public double OverallBiasScore { get; set; }
    public List<AttributeBias> ProtectedAttributes { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public DateTime AnalyzedAt { get; set; }
}

public class AttributeBias
{
    public string Attribute { get; set; } = string.Empty;
    public double BiasScore { get; set; }
}

public class ExplainabilityInput
{
    public Dictionary<string, object> Features { get; set; } = new();
    public string PredictionId { get; set; } = string.Empty;
}

public class ExplainabilityReport
{
    public string ModelId { get; set; } = string.Empty;
    public Dictionary<string, double> FeatureImportance { get; set; } = new();
    public Dictionary<string, double> ShapValues { get; set; } = new();
    public string LocalExplanation { get; set; } = string.Empty;
    public string GlobalExplanation { get; set; } = string.Empty;
}

public class ABTestConfig
{
    public TimeSpan Duration { get; set; }
    public int SampleSize { get; set; }
    public string MetricName { get; set; } = string.Empty;
}

public class ABTestResult
{
    public string TestId { get; set; } = string.Empty;
    public ABTestMetrics ModelA { get; set; } = new();
    public ABTestMetrics ModelB { get; set; } = new();
    public double StatisticalSignificance { get; set; }
    public string Winner { get; set; } = string.Empty;
    public TimeSpan TestDuration { get; set; }
    public int SampleSize { get; set; }
}

public class ABTestMetrics
{
    public string ModelId { get; set; } = string.Empty;
    public double Accuracy { get; set; }
    public TimeSpan LatencyP95 { get; set; }
    public double ErrorRate { get; set; }
}

public class MLPipelineConfig
{
    public string PipelineName { get; set; } = string.Empty;
    public List<string> Steps { get; set; } = new();
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public class PipelineExecution
{
    public string PipelineId { get; set; } = string.Empty;
    public string PipelineName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public List<PipelineStep> Steps { get; set; } = new();
    public Dictionary<string, string> Artifacts { get; set; } = new();
}

public class PipelineStep
{
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
}

public class MLModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public double Accuracy { get; set; }
    public string Version { get; set; } = string.Empty;
    public DateTime DeployedAt { get; set; }
    public DateTime LastPrediction { get; set; }
}

// Computer Vision Models
public class ImageAnalysis
{
    public int Width { get; set; }
    public int Height { get; set; }
    public string Format { get; set; } = string.Empty;
    public long Size { get; set; }
    public List<ColorInfo> Colors { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public string Description { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public TimeSpan ProcessingTime { get; set; }
}

public class ColorInfo
{
    public string Color { get; set; } = string.Empty;
    public double Percentage { get; set; }
}

public class ObjectDetectionConfig
{
    public string ModelId { get; set; } = string.Empty;
    public double ConfidenceThreshold { get; set; }
}

public class ObjectDetection
{
    public List<DetectedObject> Objects { get; set; } = new();
    public int TotalObjects { get; set; }
    public TimeSpan ProcessingTime { get; set; }
    public string ModelUsed { get; set; } = string.Empty;
}

public class DetectedObject
{
    public string Label { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public BoundingBox BoundingBox { get; set; } = new();
}

public class BoundingBox
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}

public class FaceAnalysis
{
    public List<DetectedFace> Faces { get; set; } = new();
    public int TotalFaces { get; set; }
    public TimeSpan ProcessingTime { get; set; }
}

public class DetectedFace
{
    public BoundingBox BoundingBox { get; set; } = new();
    public double Confidence { get; set; }
    public int Age { get; set; }
    public string Gender { get; set; } = string.Empty;
    public Dictionary<string, double> Emotions { get; set; } = new();
    public List<FaceLandmark> Landmarks { get; set; } = new();
}

public class FaceLandmark
{
    public string Type { get; set; } = string.Empty;
    public int X { get; set; }
    public int Y { get; set; }
}

public class TextExtraction
{
    public string ExtractedText { get; set; } = string.Empty;
    public List<TextBlock> TextBlocks { get; set; } = new();
    public string Language { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public TimeSpan ProcessingTime { get; set; }
}

public class TextBlock
{
    public string Text { get; set; } = string.Empty;
    public BoundingBox BoundingBox { get; set; } = new();
    public double Confidence { get; set; }
}

public class ImageClassification
{
    public string PredictedClass { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public Dictionary<string, double> ClassProbabilities { get; set; } = new();
    public string ModelId { get; set; } = string.Empty;
    public TimeSpan ProcessingTime { get; set; }
}

public class ProductRecognition
{
    public string ProductName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string SubCategory { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public List<SimilarProduct> SimilarProducts { get; set; } = new();
    public decimal EstimatedPrice { get; set; }
    public TimeSpan ProcessingTime { get; set; }
}

public class SimilarProduct
{
    public string Name { get; set; } = string.Empty;
    public double Similarity { get; set; }
}

public class QualityStandards
{
    public double MinScore { get; set; }
    public List<string> RequiredChecks { get; set; } = new();
}

public class QualityInspection
{
    public double OverallScore { get; set; }
    public string QualityGrade { get; set; } = string.Empty;
    public List<QualityDefect> Defects { get; set; } = new();
    public Dictionary<string, double> QualityMetrics { get; set; } = new();
    public bool PassedStandards { get; set; }
    public TimeSpan ProcessingTime { get; set; }
}

public class QualityDefect
{
    public string Type { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public BoundingBox Location { get; set; } = new();
    public double Confidence { get; set; }
}

public class EnhancementSettings
{
    public bool AutoContrast { get; set; }
    public bool NoiseReduction { get; set; }
    public double Sharpening { get; set; }
}

public class ImageEnhancement
{
    public byte[] EnhancedImageData { get; set; } = Array.Empty<byte>();
    public List<string> EnhancementApplied { get; set; } = new();
    public double QualityImprovement { get; set; }
    public TimeSpan ProcessingTime { get; set; }
}

public class SegmentationConfig
{
    public string ModelId { get; set; } = string.Empty;
    public int NumClasses { get; set; }
}

public class ImageSegmentation
{
    public List<ImageSegment> Segments { get; set; } = new();
    public int TotalSegments { get; set; }
    public string ModelUsed { get; set; } = string.Empty;
    public TimeSpan ProcessingTime { get; set; }
}

public class ImageSegment
{
    public string Label { get; set; } = string.Empty;
    public byte[] Mask { get; set; } = Array.Empty<byte>();
    public double Area { get; set; }
    public double Confidence { get; set; }
}

public class BackgroundRemoval
{
    public byte[] ProcessedImageData { get; set; } = Array.Empty<byte>();
    public byte[] BackgroundMask { get; set; } = Array.Empty<byte>();
    public double EdgeQuality { get; set; }
    public TimeSpan ProcessingTime { get; set; }
}

public class VideoAnalysis
{
    public TimeSpan Duration { get; set; }
    public double FrameRate { get; set; }
    public string Resolution { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public long Size { get; set; }
    public List<VideoScene> Scenes { get; set; } = new();
    public List<VideoKeyFrame> KeyFrames { get; set; } = new();
    public TimeSpan ProcessingTime { get; set; }
}

public class VideoScene
{
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class VideoKeyFrame
{
    public TimeSpan Timestamp { get; set; }
    public byte[] FrameData { get; set; } = Array.Empty<byte>();
    public string Description { get; set; } = string.Empty;
}

public class ActionRecognition
{
    public List<RecognizedAction> Actions { get; set; } = new();
    public int TotalActions { get; set; }
    public TimeSpan ProcessingTime { get; set; }
}

public class RecognizedAction
{
    public string Action { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public double Confidence { get; set; }
}

public class VideoSummarization
{
    public byte[] SummaryVideoData { get; set; } = Array.Empty<byte>();
    public TimeSpan OriginalDuration { get; set; }
    public TimeSpan SummaryDuration { get; set; }
    public List<VideoMoment> KeyMoments { get; set; } = new();
    public double CompressionRatio { get; set; }
    public TimeSpan ProcessingTime { get; set; }
}

public class VideoMoment
{
    public TimeSpan Timestamp { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class ImageSearchConfig
{
    public double SimilarityThreshold { get; set; }
    public int MaxResults { get; set; }
}

public class SimilaritySearch
{
    public string QueryImageHash { get; set; } = string.Empty;
    public List<SimilarImage> SimilarImages { get; set; } = new();
    public string SearchMethod { get; set; } = string.Empty;
    public TimeSpan ProcessingTime { get; set; }
}

public class SimilarImage
{
    public string ImageId { get; set; } = string.Empty;
    public double Similarity { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public class ImageGenerationPrompt
{
    public string Description { get; set; } = string.Empty;
    public string Style { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public int? Seed { get; set; }
}

public class ImageGeneration
{
    public byte[] GeneratedImageData { get; set; } = Array.Empty<byte>();
    public string Prompt { get; set; } = string.Empty;
    public string Style { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public int Seed { get; set; }
    public TimeSpan ProcessingTime { get; set; }
}

public class StyleTransfer
{
    public byte[] StyledImageData { get; set; } = Array.Empty<byte>();
    public double StyleStrength { get; set; }
    public double ContentPreservation { get; set; }
    public TimeSpan ProcessingTime { get; set; }
}

public class ImageComparison
{
    public double SimilarityScore { get; set; }
    public double StructuralSimilarity { get; set; }
    public double ColorSimilarity { get; set; }
    public double TextureSimilarity { get; set; }
    public string ComparisonMethod { get; set; } = string.Empty;
    public List<ImageDifference> Differences { get; set; } = new();
    public TimeSpan ProcessingTime { get; set; }
}

public class ImageDifference
{
    public string Type { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public BoundingBox Location { get; set; } = new();
}

public class BatchProcessingConfig
{
    public string ProcessingType { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public class BatchProcessingResult
{
    public int TotalImages { get; set; }
    public int ProcessedSuccessfully { get; set; }
    public int Failed { get; set; }
    public TimeSpan TotalProcessingTime { get; set; }
    public TimeSpan AverageProcessingTime { get; set; }
    public List<BatchImageResult> Results { get; set; } = new();
}

public class BatchImageResult
{
    public string ImageId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public object Result { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

public class BatchVideoConfig
{
    public string ProcessingType { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public class VideoProcessingResult
{
    public int TotalVideos { get; set; }
    public int ProcessedSuccessfully { get; set; }
    public int Failed { get; set; }
    public TimeSpan TotalProcessingTime { get; set; }
    public TimeSpan AverageProcessingTime { get; set; }
    public List<BatchVideoResult> Results { get; set; } = new();
}

public class BatchVideoResult
{
    public string VideoId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public object Result { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

public class CVModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public double Accuracy { get; set; }
}

public class CVModelInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public double Accuracy { get; set; }
    public string InputSize { get; set; } = string.Empty;
    public int OutputClasses { get; set; }
    public string TrainingDataset { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
}

public class CVTrainingData
{
    public string DatasetPath { get; set; } = string.Empty;
    public int ImageCount { get; set; }
    public List<string> Classes { get; set; } = new();
}

public class CVTrainingConfig
{
    public string ModelName { get; set; } = string.Empty;
    public int MaxEpochs { get; set; }
    public double LearningRate { get; set; }
    public int BatchSize { get; set; }
}

public class CVModelTraining
{
    public string ModelId { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public double TrainingAccuracy { get; set; }
    public double ValidationAccuracy { get; set; }
    public TimeSpan TrainingTime { get; set; }
    public int EpochsCompleted { get; set; }
    public double FinalLoss { get; set; }
}

public class CVPerformanceMetrics
{
    public int TotalRequests { get; set; }
    public TimeSpan AverageLatency { get; set; }
    public double SuccessRate { get; set; }
    public double ErrorRate { get; set; }
    public int ThroughputPerSecond { get; set; }
    public TimeSpan PeakLatency { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class CVUsageStats
{
    public object Period { get; set; } = new();
    public int TotalRequests { get; set; }
    public int UniqueUsers { get; set; }
    public string MostUsedFeature { get; set; } = string.Empty;
    public int AverageRequestSize { get; set; }
    public int TotalDataProcessed { get; set; }
    public Dictionary<string, decimal> CostAnalysis { get; set; } = new();
}

// NLP and Conversational AI Models
public class IntentRecognition
{
    public string Intent { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public List<AlternativeIntent> AlternativeIntents { get; set; } = new();
    public Dictionary<string, object> IntentParameters { get; set; } = new();
    public List<string> ContextFactors { get; set; } = new();
}

public class AlternativeIntent
{
    public string Intent { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class SentimentAnalysis
{
    public string Sentiment { get; set; } = string.Empty;
    public double Score { get; set; }
    public double Confidence { get; set; }
    public Dictionary<string, double> Emotions { get; set; } = new();
    public List<string> KeyPhrases { get; set; } = new();
    public string SentimentTrend { get; set; } = string.Empty;
}

public class EntityExtraction
{
    public List<ExtractedEntity> Entities { get; set; } = new();
    public int TotalEntitiesFound { get; set; }
    public TimeSpan ProcessingTime { get; set; }
    public string ExtractionMethod { get; set; } = string.Empty;
}

public class ExtractedEntity
{
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int StartPosition { get; set; }
    public int EndPosition { get; set; }
    public double Confidence { get; set; }
}

public class ConversationMessage
{
    public string MessageId { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public ConversationContext Context { get; set; } = new();
    public DateTime Timestamp { get; set; }
    public string Intent { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
}

public class ConversationContext
{
    public string? UserName { get; set; }
    public string? CurrentIntent { get; set; }
    public string? CurrentTopic { get; set; }
    public List<string>? PreviousIntents { get; set; }
    public Dictionary<string, object>? UserPreferences { get; set; }
    public Dictionary<string, object>? ExtractedEntities { get; set; }
}

public class ConversationResponse
{
    public string ResponseId { get; set; } = string.Empty;
    public string MessageId { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string Intent { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public List<ExtractedEntity> Entities { get; set; } = new();
    public List<string> Suggestions { get; set; } = new();
    public ConversationContext Context { get; set; } = new();
    public TimeSpan ResponseTime { get; set; }
    public DateTime Timestamp { get; set; }
    public bool RequiresHumanHandoff { get; set; }
    public SentimentAnalysis SentimentScore { get; set; } = new();
}

public class ResponseGeneration
{
    public string Text { get; set; } = string.Empty;
    public string ResponseType { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public string GenerationMethod { get; set; } = string.Empty;
    public List<ResponseGeneration> AlternativeResponses { get; set; } = new();
    public bool RequiresAction { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string EmotionalTone { get; set; } = string.Empty;
    public string Personalization { get; set; } = string.Empty;
}

public class ConversationSession
{
    public string SessionId { get; set; } = string.Empty;
    public ConversationContext Context { get; set; } = new();
    public List<ConversationMessage>? MessageHistory { get; set; }
    public DateTime StartedAt { get; set; }
}

public class ConversationFlow
{
    public string SessionId { get; set; } = string.Empty;
    public string CurrentStep { get; set; } = string.Empty;
    public List<ConversationStep> NextSteps { get; set; } = new();
    public string FlowDecision { get; set; } = string.Empty;
    public ConversationContext Context { get; set; } = new();
    public bool IsComplete { get; set; }
    public bool RequiresEscalation { get; set; }
    public List<SuggestedAction> SuggestedActions { get; set; } = new();
    public string ConversationState { get; set; } = string.Empty;
    public ConversationMetrics FlowMetrics { get; set; } = new();
}

public class ConversationStep
{
    public string StepName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class SuggestedAction
{
    public string Action { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class ConversationMetrics
{
    public int TotalTurns { get; set; }
    public TimeSpan AverageResponseTime { get; set; }
    public string SentimentTrend { get; set; } = string.Empty;
    public double ResolutionProgress { get; set; }
}

public class ConversationSuggestion
{
    public string Text { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public double Confidence { get; set; }
}

// Recommendation Models
public class RecommendationRequest
{
    public string UserId { get; set; } = string.Empty;
    public string ItemType { get; set; } = string.Empty;
    public int MaxRecommendations { get; set; }
    public RecommendationContext Context { get; set; } = new();
}

public class RecommendationContext
{
    public Dictionary<string, object> UserPreferences { get; set; } = new();
    public List<string> RecentInteractions { get; set; } = new();
    public string Location { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

// ProductRecommendations class defined in IRecommendationService.cs

public class RecommendedItem
{
    public string ItemId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public double RecommendationScore { get; set; }
    public List<string> ReasonCodes { get; set; } = new();
    public string ImageUrl { get; set; } = string.Empty;
}

public class CustomerSegmentation
{
    public string UserId { get; set; } = string.Empty;
    public string Segment { get; set; } = string.Empty;
    public double SegmentConfidence { get; set; }
    public Dictionary<string, double> SegmentScores { get; set; } = new();
    public List<string> SegmentCharacteristics { get; set; } = new();
}

public class SegmentationCriteria
{
    public List<string> Features { get; set; } = new();
    public string SegmentationMethod { get; set; } = string.Empty;
    public int NumberOfSegments { get; set; }
}

// Removed duplicate ABTestConfig and ABTestResult classes - they exist elsewhere

// Training Models
public class TrainingJobRequest
{
    public string ModelName { get; set; } = string.Empty;
    public string Algorithm { get; set; } = string.Empty;
    public string DatasetPath { get; set; } = string.Empty;
    public Dictionary<string, object> Hyperparameters { get; set; } = new();
    public int MaxEpochs { get; set; }
    public double LearningRate { get; set; }
}

public class TrainingJob
{
    public string JobId { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public double Progress { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? CompletionTime { get; set; }
    public TimeSpan ElapsedTime { get; set; }
    public Dictionary<string, double> CurrentMetrics { get; set; } = new();
}

public class AutoMLJobRequest
{
    public string TaskType { get; set; } = string.Empty;
    public string DatasetPath { get; set; } = string.Empty;
    public TimeSpan MaxTrainingTime { get; set; }
    public int MaxModels { get; set; }
    public string TargetMetric { get; set; } = string.Empty;
}

public class AutoMLJobResult
{
    public string JobId { get; set; } = string.Empty;
    public string BestModelId { get; set; } = string.Empty;
    public double BestScore { get; set; }
    public List<ModelCandidate> AllModels { get; set; } = new();
    public TimeSpan TotalTime { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class HyperparameterTuningRequest
{
    public string ModelId { get; set; } = string.Empty;
    public Dictionary<string, object> SearchSpace { get; set; } = new();
    public int MaxTrials { get; set; }
    public TimeSpan MaxTime { get; set; }
    public string OptimizationGoal { get; set; } = string.Empty;
}

public class HyperparameterTuningResult
{
    public string TuningId { get; set; } = string.Empty;
    public Dictionary<string, object> BestParameters { get; set; } = new();
    public double BestScore { get; set; }
    public int TotalTrials { get; set; }
    public List<TrialResult> Trials { get; set; } = new();
}

public class TrialResult
{
    public int TrialNumber { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
    public double Score { get; set; }
    public TimeSpan Duration { get; set; }
}

public class DatasetValidationRequest
{
    public string DatasetPath { get; set; } = string.Empty;
    public List<string> ValidationChecks { get; set; } = new();
    public Dictionary<string, object> ValidationRules { get; set; } = new();
}

public class DatasetValidation
{
    public string DatasetId { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public List<string> Issues { get; set; } = new();
    public Dictionary<string, object> Statistics { get; set; } = new();
    public DateTime ValidatedAt { get; set; }
}

public class FeatureEngineeringRequest
{
    public string DatasetPath { get; set; } = string.Empty;
    public List<string> FeatureTransformations { get; set; } = new();
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public class FeatureEngineeringResult
{
    public string ResultId { get; set; } = string.Empty;
    public string ProcessedDatasetPath { get; set; } = string.Empty;
    public List<string> GeneratedFeatures { get; set; } = new();
    public Dictionary<string, object> FeatureStatistics { get; set; } = new();
    public TimeSpan ProcessingTime { get; set; }
}

public class ModelEvaluationRequest
{
    public string ModelId { get; set; } = string.Empty;
    public string TestDataPath { get; set; } = string.Empty;
    public List<string> Metrics { get; set; } = new();
}

public class ModelEvaluationResult
{
    public string EvaluationId { get; set; } = string.Empty;
    public string ModelId { get; set; } = string.Empty;
    public Dictionary<string, double> Metrics { get; set; } = new();
    public int[,] ConfusionMatrix { get; set; } = new int[0,0];
    public DateTime EvaluatedAt { get; set; }
}

// Helper classes for training
public class ParameterRange
{
    public object MinValue { get; set; } = new();
    public object MaxValue { get; set; } = new();
    public string Distribution { get; set; } = string.Empty;
}

public class DataValidationIssue
{
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Column { get; set; } = string.Empty;
}

public class TransformationStep
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public class ClassMetrics
{
    public string ClassName { get; set; } = string.Empty;
    public double Precision { get; set; }
    public double Recall { get; set; }
    public double F1Score { get; set; }
    public int Support { get; set; }
}

// Inference Models
public class InferenceRequest
{
    public string ModelId { get; set; } = string.Empty;
    public Dictionary<string, object> InputData { get; set; } = new();
    public string RequestId { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public class InferenceResult
{
    public string RequestId { get; set; } = string.Empty;
    public string ModelId { get; set; } = string.Empty;
    public object Prediction { get; set; } = new();
    public double Confidence { get; set; }
    public TimeSpan ProcessingTime { get; set; }
    public Dictionary<string, double> Probabilities { get; set; } = new();
    public DateTime Timestamp { get; set; }
}

public class BatchInferenceRequest
{
    public string ModelId { get; set; } = string.Empty;
    public List<Dictionary<string, object>> BatchInputs { get; set; } = new();
    public string BatchId { get; set; } = string.Empty;
}

public class BatchInferenceResult
{
    public string BatchId { get; set; } = string.Empty;
    public List<InferenceResult> Results { get; set; } = new();
    public int TotalRequests { get; set; }
    public int SuccessfulRequests { get; set; }
    public TimeSpan TotalProcessingTime { get; set; }
}

public class StreamingInferenceRequest
{
    public string ModelId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public Dictionary<string, object> StreamConfig { get; set; } = new();
}

public class StreamingInferenceSession
{
    public string SessionId { get; set; } = string.Empty;
    public string ModelId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public int ProcessedItems { get; set; }
    public double AverageLatency { get; set; }
}

public class ModelDeploymentRequest
{
    public string ModelId { get; set; } = string.Empty;
    public string TargetEnvironment { get; set; } = string.Empty;
    public DeploymentConfig Configuration { get; set; } = new();
    public Dictionary<string, object> DeploymentParams { get; set; } = new();
}

public class ModelEndpoint
{
    public string EndpointId { get; set; } = string.Empty;
    public string ModelId { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime DeployedAt { get; set; }
    public EndpointMetrics Metrics { get; set; } = new();
}

public class EndpointMetrics
{
    public int RequestCount { get; set; }
    public TimeSpan AverageLatency { get; set; }
    public double ErrorRate { get; set; }
    public DateTime LastRequest { get; set; }
}

public class EndpointHealth
{
    public string EndpointId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public TimeSpan ResponseTime { get; set; }
    public DateTime LastCheck { get; set; }
    public List<string> Issues { get; set; } = new();
}

public class InferenceMetrics
{
    public string ModelId { get; set; } = string.Empty;
    public int TotalRequests { get; set; }
    public TimeSpan AverageLatency { get; set; }
    public double ThroughputPerSecond { get; set; }
    public double ErrorRate { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
}

public class LoadTestConfig
{
    public string EndpointId { get; set; } = string.Empty;
    public int ConcurrentUsers { get; set; }
    public int RequestsPerSecond { get; set; }
    public TimeSpan Duration { get; set; }
    public Dictionary<string, object> TestData { get; set; } = new();
}

public class LoadTestResult
{
    public string TestId { get; set; } = string.Empty;
    public string EndpointId { get; set; } = string.Empty;
    public int TotalRequests { get; set; }
    public int SuccessfulRequests { get; set; }
    public TimeSpan AverageLatency { get; set; }
    public TimeSpan MaxLatency { get; set; }
    public double RequestsPerSecond { get; set; }
    public Dictionary<int, int> ResponseCodes { get; set; } = new();
}

public class InferenceSession
{
    public string SessionId { get; set; } = string.Empty;
    public string ModelId { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public int RequestCount { get; set; }
    public Dictionary<string, object> SessionData { get; set; } = new();
}

public class HourlyStat
{
    public DateTime Hour { get; set; }
    public int RequestCount { get; set; }
    public TimeSpan AverageLatency { get; set; }
    public double ErrorRate { get; set; }
}

// MLModel Service Models  
public class ModelRegistry
{
    public string RegistryId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<RegisteredModel> Models { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class RegisteredModel
{
    public string ModelId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; }
}

public class ModelRegistration
{
    public string ModelId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
    public List<string> Tags { get; set; } = new();
}

public class ModelVersionRequest
{
    public string ModelId { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, object> Changes { get; set; } = new();
}

public class ModelComparison
{
    public string ComparisonId { get; set; } = string.Empty;
    public List<string> ModelIds { get; set; } = new();
    public Dictionary<string, Dictionary<string, double>> Metrics { get; set; } = new();
    public string BestModel { get; set; } = string.Empty;
    public DateTime ComparedAt { get; set; }
}

public class ModelLineage
{
    public string ModelId { get; set; } = string.Empty;
    public string ParentModelId { get; set; } = string.Empty;
    public List<string> Dependencies { get; set; } = new();
    public Dictionary<string, object> Provenance { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class ModelBackupConfig
{
    public string ModelId { get; set; } = string.Empty;
    public string BackupLocation { get; set; } = string.Empty;
    public bool IncludeWeights { get; set; }
    public bool IncludeMetadata { get; set; }
    public Dictionary<string, object> BackupOptions { get; set; } = new();
}

public class ModelBackup
{
    public string BackupId { get; set; } = string.Empty;
    public string ModelId { get; set; } = string.Empty;
    public string BackupLocation { get; set; } = string.Empty;
    public long BackupSize { get; set; }
    public DateTime BackupTime { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class ModelRestoreConfig
{
    public string BackupId { get; set; } = string.Empty;
    public string TargetLocation { get; set; } = string.Empty;
    public Dictionary<string, object> RestoreOptions { get; set; } = new();
}

public class ModelRestore
{
    public string RestoreId { get; set; } = string.Empty;
    public string BackupId { get; set; } = string.Empty;
    public string ModelId { get; set; } = string.Empty;
    public DateTime RestoreTime { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class ModelExportConfig
{
    public string ModelId { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public string TargetPlatform { get; set; } = string.Empty;
    public Dictionary<string, object> ExportOptions { get; set; } = new();
}

public class ModelExport
{
    public string ExportId { get; set; } = string.Empty;
    public string ModelId { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public string ExportPath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime ExportTime { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class ModelImportConfig
{
    public string SourcePath { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
    public Dictionary<string, object> ImportOptions { get; set; } = new();
}

public class ModelImport
{
    public string ImportId { get; set; } = string.Empty;
    public string ModelId { get; set; } = string.Empty;
    public string SourcePath { get; set; } = string.Empty;
    public DateTime ImportTime { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class AccuracyDataPoint
{
    public DateTime Timestamp { get; set; }
    public double Accuracy { get; set; }
    public string DatasetUsed { get; set; } = string.Empty;
}

public class LatencyDataPoint
{
    public DateTime Timestamp { get; set; }
    public TimeSpan Latency { get; set; }
    public int RequestCount { get; set; }
}

// Experiment Management Models
public class Experiment
{
    public string ExperimentId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<ExperimentRun> Runs { get; set; } = new();
    public string Owner { get; set; } = string.Empty;
    public Dictionary<string, object> Tags { get; set; } = new();
}

public class ExperimentConfig
{
    public string ExperimentName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Objective { get; set; } = string.Empty;
    public List<ModelConfig> ModelConfigs { get; set; } = new();
    public TrainingData Data { get; set; } = new();
    public Dictionary<string, object> Tags { get; set; } = new();
}

public class ExperimentParameters
{
    public Dictionary<string, object> Hyperparameters { get; set; } = new();
    public string DataVersion { get; set; } = string.Empty;
    public Dictionary<string, object> Environment { get; set; } = new();
}

public class ExperimentRun
{
    public string RunId { get; set; } = string.Empty;
    public string ExperimentId { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public ExperimentParameters Parameters { get; set; } = new();
    public Dictionary<string, double> Metrics { get; set; } = new();
    public List<string> Artifacts { get; set; } = new();
    public Dictionary<string, object> Logs { get; set; } = new();
}

public class ExperimentResults
{
    public string ExperimentId { get; set; } = string.Empty;
    public List<ExperimentRun> Runs { get; set; } = new();
    public ExperimentRun BestRun { get; set; } = new();
    public string OptimizationMetric { get; set; } = string.Empty;
    public Dictionary<string, object> Insights { get; set; } = new();
}

// Pipeline Models
public class MLPipeline
{
    public string PipelineId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<PipelineStep> Steps { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
}

public class PipelineConfig
{
    public string PipelineName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<PipelineStepConfig> Steps { get; set; } = new();
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public class PipelineStepConfig
{
    public string StepName { get; set; } = string.Empty;
    public string StepType { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public class PipelineStatus
{
    public string PipelineId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Progress { get; set; }
    public DateTime LastUpdated { get; set; }
    public List<PipelineStepStatus> StepStatuses { get; set; } = new();
}

public class PipelineStepStatus
{
    public string StepName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
}

// Model Alert Models
public class ModelAlert
{
    public string AlertId { get; set; } = string.Empty;
    public string AlertType { get; set; } = string.Empty;
    public string Condition { get; set; } = string.Empty;
    public double Threshold { get; set; }
    public List<string> Recipients { get; set; } = new();
    public bool IsActive { get; set; } = true;
}

// NLP Models
public class TextClassification
{
    public string ClassificationId { get; set; } = string.Empty;
    public string PredictedClass { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public Dictionary<string, double> ClassProbabilities { get; set; } = new();
    public List<string> Categories { get; set; } = new();
    public DateTime ClassifiedAt { get; set; } = DateTime.UtcNow;
}

public class LanguageDetection
{
    public string DetectionId { get; set; } = string.Empty;
    public string DetectedLanguage { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public Dictionary<string, double> LanguageProbabilities { get; set; } = new();
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
}

public class KeywordExtraction
{
    public string ExtractionId { get; set; } = string.Empty;
    public List<ExtractedKeyword> Keywords { get; set; } = new();
    public int TotalKeywords { get; set; }
    public DateTime ExtractedAt { get; set; } = DateTime.UtcNow;
}

public class ExtractedKeyword
{
    public string Keyword { get; set; } = string.Empty;
    public double Relevance { get; set; }
    public int Frequency { get; set; }
    public string Category { get; set; } = string.Empty;
}

public class TextSummarization
{
    public string SummaryId { get; set; } = string.Empty;
    public string OriginalText { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public int OriginalLength { get; set; }
    public int SummaryLength { get; set; }
    public double CompressionRatio { get; set; }
    public List<string> KeySentences { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class TopicModeling
{
    public string ModelingId { get; set; } = string.Empty;
    public List<Topic> Topics { get; set; } = new();
    public int NumberOfTopics { get; set; }
    public Dictionary<string, object> ModelParameters { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class Topic
{
    public string TopicId { get; set; } = string.Empty;
    public string TopicName { get; set; } = string.Empty;
    public List<string> Keywords { get; set; } = new();
    public Dictionary<string, double> WordDistribution { get; set; } = new();
    public double Coherence { get; set; }
}

public class SemanticSimilarity
{
    public string SimilarityId { get; set; } = string.Empty;
    public string Text1 { get; set; } = string.Empty;
    public string Text2 { get; set; } = string.Empty;
    public double SimilarityScore { get; set; }
    public string SimilarityMethod { get; set; } = string.Empty;
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}

public class TextGeneration
{
    public string GenerationId { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public string GeneratedText { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class TextGenerationRequest
{
    public string Prompt { get; set; } = string.Empty;
    public int MaxLength { get; set; } = 100;
    public double Temperature { get; set; } = 0.7;
    public double TopP { get; set; } = 0.9;
    public int TopK { get; set; } = 50;
    public string Model { get; set; } = string.Empty;
    public Dictionary<string, object> AdditionalParams { get; set; } = new();
}

public class QuestionAnswering
{
    public string AnswerId { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public string Context { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public int StartIndex { get; set; }
    public int EndIndex { get; set; }
    public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;
}

// Document Processing Models
public class NLPDocument
{
    public string DocumentId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class DocumentAnalysis
{
    public string AnalysisId { get; set; } = string.Empty;
    public string DocumentId { get; set; } = string.Empty;
    public int WordCount { get; set; }
    public int SentenceCount { get; set; }
    public int ParagraphCount { get; set; }
    public Dictionary<string, int> KeywordFrequency { get; set; } = new();
    public string DetectedLanguage { get; set; } = string.Empty;
    public double ReadabilityScore { get; set; }
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
}

public class DocumentSummarization
{
    public string SummarizationId { get; set; } = string.Empty;
    public string DocumentId { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public List<string> KeyPoints { get; set; } = new();
    public double CompressionRatio { get; set; }
    public DateTime SummarizedAt { get; set; } = DateTime.UtcNow;
}

public class DocumentClassification
{
    public string ClassificationId { get; set; } = string.Empty;
    public string DocumentId { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public Dictionary<string, double> CategoryProbabilities { get; set; } = new();
    public DateTime ClassifiedAt { get; set; } = DateTime.UtcNow;
}

public class ExtractionRules
{
    public string RulesId { get; set; } = string.Empty;
    public List<ExtractionRule> Rules { get; set; } = new();
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public class ExtractionRule
{
    public string RuleName { get; set; } = string.Empty;
    public string Pattern { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public Dictionary<string, object> Options { get; set; } = new();
}

public class InformationExtraction
{
    public string ExtractionId { get; set; } = string.Empty;
    public string DocumentId { get; set; } = string.Empty;
    public List<ExtractedInformation> ExtractedItems { get; set; } = new();
    public DateTime ExtractedAt { get; set; } = DateTime.UtcNow;
}

public class ExtractedInformation
{
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public int StartPosition { get; set; }
    public int EndPosition { get; set; }
    public Dictionary<string, object> Context { get; set; } = new();
}

// Knowledge Management Models
public class EntityRelationship
{
    public string RelationshipId { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Predicate { get; set; } = string.Empty;
    public string Object { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public Dictionary<string, object> Context { get; set; } = new();
}

public class ConceptExtraction
{
    public string ExtractionId { get; set; } = string.Empty;
    public List<ExtractedConcept> Concepts { get; set; } = new();
    public DateTime ExtractedAt { get; set; } = DateTime.UtcNow;
}

public class ExtractedConcept
{
    public string ConceptName { get; set; } = string.Empty;
    public string ConceptType { get; set; } = string.Empty;
    public double Relevance { get; set; }
    public List<string> RelatedTerms { get; set; } = new();
    public Dictionary<string, object> Properties { get; set; } = new();
}

public class KnowledgeQuery
{
    public string QueryId { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public List<KnowledgeResult> Results { get; set; } = new();
    public DateTime QueryTime { get; set; } = DateTime.UtcNow;
}

public class KnowledgeResult
{
    public string ResultId { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public List<string> Sources { get; set; } = new();
    public Dictionary<string, object> Evidence { get; set; } = new();
}

// Training Models
public class ConversationTrainingData
{
    public string DatasetId { get; set; } = string.Empty;
    public List<ConversationExample> Examples { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class ConversationExample
{
    public string ExampleId { get; set; } = string.Empty;
    public string UserMessage { get; set; } = string.Empty;
    public string BotResponse { get; set; } = string.Empty;
    public string Intent { get; set; } = string.Empty;
    public Dictionary<string, object> Entities { get; set; } = new();
    public ConversationContext Context { get; set; } = new();
}

public class IntentTrainingData
{
    public string DatasetId { get; set; } = string.Empty;
    public List<IntentExample> Examples { get; set; } = new();
    public List<string> IntentLabels { get; set; } = new();
}

public class IntentExample
{
    public string Text { get; set; } = string.Empty;
    public string Intent { get; set; } = string.Empty;
    public Dictionary<string, object> Entities { get; set; } = new();
}

public class ConversationModel
{
    public string ModelId { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
    public string ModelType { get; set; } = string.Empty;
    public DateTime TrainedAt { get; set; }
    public Dictionary<string, double> Metrics { get; set; } = new();
    public string Status { get; set; } = string.Empty;
}

public class IntentModel
{
    public string ModelId { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
    public List<string> SupportedIntents { get; set; } = new();
    public DateTime TrainedAt { get; set; }
    public Dictionary<string, double> Metrics { get; set; } = new();
    public string Status { get; set; } = string.Empty;
}

// Additional ML Model classes
public class ModelMetrics
{
    public double Accuracy { get; set; }
    public double Precision { get; set; }
    public double Recall { get; set; }
    public double F1Score { get; set; }
    public double AUC { get; set; }
    public double MAE { get; set; } // Mean Absolute Error
    public double MSE { get; set; } // Mean Squared Error
    public double RMSE { get; set; } // Root Mean Squared Error
    public double R2Score { get; set; }
    public Dictionary<string, double> CustomMetrics { get; set; } = new();
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}

public class PredictiveProductRecommendations
{
    public string RecommendationId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public List<RecommendedItem> RecommendedProducts { get; set; } = new();
    public string RecommendationAlgorithm { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> PersonalizationFactors { get; set; } = new();
}

public class ResourceRequirements
{
    public int CpuCores { get; set; }
    public int MemoryGB { get; set; }
    public int StorageGB { get; set; }
    public string InstanceType { get; set; } = string.Empty;
    public Dictionary<string, object> CustomRequirements { get; set; } = new();
}

public class HealthCheck
{
    public string CheckId { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CheckTime { get; set; } = DateTime.UtcNow;
    public TimeSpan ResponseTime { get; set; }
    public Dictionary<string, object> Details { get; set; } = new();
    public List<string> Issues { get; set; } = new();
}

// Recommendation System Models
public class ProductRecommendations
{
    public string RecommendationId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public List<RecommendedItem> Products { get; set; } = new();
    public string RecommendationType { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Context { get; set; } = new();
}

public class TrendingRequest
{
    public string Category { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int Count { get; set; } = 10;
    public Dictionary<string, object> Filters { get; set; } = new();
}

public class CustomerProfile
{
    public string CustomerId { get; set; } = string.Empty;
    public Dictionary<string, object> Demographics { get; set; } = new();
    public Dictionary<string, object> Preferences { get; set; } = new();
    public List<string> InterestCategories { get; set; } = new();
    public Dictionary<string, double> BehaviorScores { get; set; } = new();
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

public class CustomerInsights
{
    public string CustomerId { get; set; } = string.Empty;
    public Dictionary<string, object> PurchasePatterns { get; set; } = new();
    public Dictionary<string, object> BrowsingBehavior { get; set; } = new();
    public List<string> PreferredCategories { get; set; } = new();
    public decimal AverageOrderValue { get; set; }
    public int TotalOrders { get; set; }
    public DateTime LastPurchase { get; set; }
}

public class CustomerCluster
{
    public string ClusterId { get; set; } = string.Empty;
    public string ClusterName { get; set; } = string.Empty;
    public List<string> CustomerIds { get; set; } = new();
    public Dictionary<string, object> Characteristics { get; set; } = new();
    public int Size { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class CollaborativeRecommendations
{
    public string UserId { get; set; } = string.Empty;
    public List<RecommendedItem> Items { get; set; } = new();
    public string Algorithm { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class UserSimilarity
{
    public string UserId { get; set; } = string.Empty;
    public List<SimilarUser> SimilarUsers { get; set; } = new();
    public string SimilarityMethod { get; set; } = string.Empty;
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}

public class SimilarUser
{
    public string UserId { get; set; } = string.Empty;
    public double SimilarityScore { get; set; }
    public Dictionary<string, object> SharedAttributes { get; set; } = new();
}

public class ItemSimilarity
{
    public string ItemId { get; set; } = string.Empty;
    public List<SimilarItem> SimilarItems { get; set; } = new();
    public string SimilarityMethod { get; set; } = string.Empty;
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}

public class SimilarItem
{
    public string ItemId { get; set; } = string.Empty;
    public double SimilarityScore { get; set; }
    public Dictionary<string, object> SharedFeatures { get; set; } = new();
}

public class InteractionMatrix
{
    public string MatrixId { get; set; } = string.Empty;
    public List<string> UserIds { get; set; } = new();
    public List<string> ItemIds { get; set; } = new();
    public Dictionary<string, Dictionary<string, double>> Interactions { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class MatrixFactorizationResult
{
    public string ResultId { get; set; } = string.Empty;
    public Dictionary<string, double[]> UserFactors { get; set; } = new();
    public Dictionary<string, double[]> ItemFactors { get; set; } = new();
    public double RMSEScore { get; set; }
    public int Factors { get; set; }
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}

public class ContentBasedRecommendations
{
    public string UserId { get; set; } = string.Empty;
    public List<RecommendedItem> Items { get; set; } = new();
    public string ContentMethod { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class ContentRequest
{
    public string UserId { get; set; } = string.Empty;
    public List<string> PreferredCategories { get; set; } = new();
    public Dictionary<string, object> ContentFilters { get; set; } = new();
    public int Count { get; set; } = 10;
}

public class ItemProfile
{
    public string ItemId { get; set; } = string.Empty;
    public Dictionary<string, object> Features { get; set; } = new();
    public List<string> Categories { get; set; } = new();
    public Dictionary<string, double> AttributeScores { get; set; } = new();
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

public class UserProfile
{
    public string UserId { get; set; } = string.Empty;
    public Dictionary<string, double> CategoryPreferences { get; set; } = new();
    public Dictionary<string, object> ProfileFeatures { get; set; } = new();
    public List<string> RecentInteractions { get; set; } = new();
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

public class FeatureSimilarity
{
    public string Item1Id { get; set; } = string.Empty;
    public string Item2Id { get; set; } = string.Empty;
    public double SimilarityScore { get; set; }
    public Dictionary<string, double> FeatureContributions { get; set; } = new();
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}

public class RealTimeRecommendations
{
    public string SessionId { get; set; } = string.Empty;
    public List<RecommendedItem> Items { get; set; } = new();
    public string RecommendationReason { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public TimeSpan ResponseTime { get; set; }
}

public class RealTimeContext
{
    public string UserId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public List<string> RecentViews { get; set; } = new();
    public List<string> CartItems { get; set; } = new();
    public Dictionary<string, object> CurrentContext { get; set; } = new();
}

public class UserInteraction
{
    public string UserId { get; set; } = string.Empty;
    public string ItemId { get; set; } = string.Empty;
    public string InteractionType { get; set; } = string.Empty;
    public double Rating { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Context { get; set; } = new();
}

public class SessionRecommendations
{
    public string SessionId { get; set; } = string.Empty;
    public List<RecommendedItem> Items { get; set; } = new();
    public Dictionary<string, object> SessionData { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class BrowsingSession
{
    public string SessionId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public List<PageView> PageViews { get; set; } = new();
    public TimeSpan Duration { get; set; }
    public DateTime StartTime { get; set; }
    public Dictionary<string, object> SessionMetadata { get; set; } = new();
}

public class PageView
{
    public string PageUrl { get; set; } = string.Empty;
    public string ItemId { get; set; } = string.Empty;
    public TimeSpan TimeSpent { get; set; }
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object> ViewData { get; set; } = new();
}

public class BehaviorAnalysis
{
    public string SessionId { get; set; } = string.Empty;
    public Dictionary<string, object> BehaviorPatterns { get; set; } = new();
    public List<string> InterestSignals { get; set; } = new();
    public double EngagementScore { get; set; }
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
}

public class RecommendationModelConfig
{
    public string ModelName { get; set; } = string.Empty;
    public string Algorithm { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
    public string TrainingDataset { get; set; } = string.Empty;
    public TimeSpan TrainingDuration { get; set; }
}

public class UserBehaviorData
{
    public string DatasetId { get; set; } = string.Empty;
    public List<UserInteraction> Interactions { get; set; } = new();
    public List<UserProfile> UserProfiles { get; set; } = new();
    public List<ItemProfile> ItemProfiles { get; set; } = new();
    public DateTime DataCutoff { get; set; }
}

public class ModelEvaluation
{
    public string EvaluationId { get; set; } = string.Empty;
    public string ModelId { get; set; } = string.Empty;
    public Dictionary<string, double> Metrics { get; set; } = new();
    public double Precision { get; set; }
    public double Recall { get; set; }
    public double F1Score { get; set; }
    public double Coverage { get; set; }
    public DateTime EvaluatedAt { get; set; } = DateTime.UtcNow;
}

public class TestData
{
    public string DatasetId { get; set; } = string.Empty;
    public List<UserInteraction> TestInteractions { get; set; } = new();
    public int TestUsers { get; set; }
    public int TestItems { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class BusinessRule
{
    public string RuleId { get; set; } = string.Empty;
    public string RuleName { get; set; } = string.Empty;
    public string RuleType { get; set; } = string.Empty;
    public Dictionary<string, object> Conditions { get; set; } = new();
    public Dictionary<string, object> Actions { get; set; } = new();
    public bool IsActive { get; set; } = true;
}

public class RecommendationFiltering
{
    public List<RecommendedItem> FilteredItems { get; set; } = new();
    public List<string> AppliedFilters { get; set; } = new();
    public int OriginalCount { get; set; }
    public int FilteredCount { get; set; }
    public DateTime FilteredAt { get; set; } = DateTime.UtcNow;
}

public class FilterCriteria
{
    public List<string> ExcludeCategories { get; set; } = new();
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public List<string> RequiredAttributes { get; set; } = new();
    public Dictionary<string, object> CustomFilters { get; set; } = new();
}

public class DiversityAnalysis
{
    public string AnalysisId { get; set; } = string.Empty;
    public double CategoryDiversity { get; set; }
    public double PriceDiversity { get; set; }
    public double BrandDiversity { get; set; }
    public double OverallDiversityScore { get; set; }
    public Dictionary<string, int> CategoryDistribution { get; set; } = new();
}

public class ExplanableRecommendations
{
    public List<ExplainedRecommendation> Recommendations { get; set; } = new();
    public string ExplanationMethod { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class ExplainedRecommendation
{
    public RecommendedItem Item { get; set; } = new();
    public string Explanation { get; set; } = string.Empty;
    public List<string> ReasonCodes { get; set; } = new();
    public double ConfidenceScore { get; set; }
}

public class ExplanationRequest
{
    public string UserId { get; set; } = string.Empty;
    public List<string> ItemIds { get; set; } = new();
    public string ExplanationType { get; set; } = string.Empty;
    public Dictionary<string, object> Context { get; set; } = new();
}

public class RecommendationAnalytics
{
    public string AnalyticsId { get; set; } = string.Empty;
    public Dictionary<string, double> PerformanceMetrics { get; set; } = new();
    public Dictionary<string, int> RecommendationCounts { get; set; } = new();
    public double AverageClickThroughRate { get; set; }
    public double AverageConversionRate { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class ConversionMetrics
{
    public string MetricsId { get; set; } = string.Empty;
    public double ConversionRate { get; set; }
    public double ClickThroughRate { get; set; }
    public decimal RevenuePerRecommendation { get; set; }
    public int TotalRecommendations { get; set; }
    public int TotalConversions { get; set; }
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}

public class ConversionAnalysisRequest
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string ModelId { get; set; } = string.Empty;
    public List<string> UserSegments { get; set; } = new();
}

public class ClickThroughAnalysis
{
    public string AnalysisId { get; set; } = string.Empty;
    public string ModelId { get; set; } = string.Empty;
    public double OverallCTR { get; set; }
    public Dictionary<string, double> CTRByCategory { get; set; } = new();
    public Dictionary<string, double> CTRByPosition { get; set; } = new();
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
}

public class RevenueImpact
{
    public string ImpactId { get; set; } = string.Empty;
    public decimal TotalRevenue { get; set; }
    public decimal RevenueFromRecommendations { get; set; }
    public double RevenuePercentage { get; set; }
    public decimal AverageOrderValue { get; set; }
    public Dictionary<string, decimal> RevenueByCategory { get; set; } = new();
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}

public class RevenueAnalysisRequest
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string ModelId { get; set; } = string.Empty;
    public List<string> ProductCategories { get; set; } = new();
}

// Prediction Service Models
public class DemandForecast
{
    public string ProductId { get; set; } = string.Empty;
    public List<DailyDemand> Predictions { get; set; } = new();
    public double ConfidenceLevel { get; set; }
    public Dictionary<string, object> Factors { get; set; } = new();
}

public class DailyDemand
{
    public DateTime Date { get; set; }
    public double PredictedDemand { get; set; }
    public double ConfidenceInterval { get; set; }
}

public class InventoryForecast
{
    public string ProductId { get; set; } = string.Empty;
    public DateTime ForecastDate { get; set; }
    public int PredictedStockLevel { get; set; }
    public int RecommendedOrderQuantity { get; set; }
    public DateTime ReorderDate { get; set; }
    public double ConfidenceScore { get; set; }
}

public class SeasonalForecast
{
    public string Category { get; set; } = string.Empty;
    public List<SeasonalTrend> Trends { get; set; } = new();
    public Dictionary<string, double> SeasonalFactors { get; set; } = new();
    public double Accuracy { get; set; }
}

public class SeasonalTrend
{
    public string Period { get; set; } = string.Empty;
    public double TrendFactor { get; set; }
    public double ConfidenceLevel { get; set; }
}

public class SalesPrediction
{
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public double PredictedSales { get; set; }
    public double ConfidenceInterval { get; set; }
    public List<SalesFactor> ContributingFactors { get; set; } = new();
}

public class SalesFactor
{
    public string FactorName { get; set; } = string.Empty;
    public double Impact { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class RevenueProjection
{
    public DateTime ProjectionDate { get; set; }
    public decimal ProjectedRevenue { get; set; }
    public decimal ConfidenceInterval { get; set; }
    public Dictionary<string, decimal> RevenueBySegment { get; set; } = new();
    public List<string> Assumptions { get; set; } = new();
}

public class ProjectionParameters
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string ProjectionType { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public class CustomerLifetimeValue
{
    public string CustomerId { get; set; } = string.Empty;
    public decimal PredictedLTV { get; set; }
    public decimal CurrentValue { get; set; }
    public int PredictedLifespanDays { get; set; }
    public double ConfidenceScore { get; set; }
    public List<string> ValueDrivers { get; set; } = new();
}

public class ChurnPrediction
{
    public string CustomerId { get; set; } = string.Empty;
    public double ChurnProbability { get; set; }
    public List<string> RiskFactors { get; set; } = new();
    public List<string> RetentionStrategies { get; set; } = new();
    public DateTime PredictionDate { get; set; }
    public string RiskLevel { get; set; } = string.Empty;
}

public class NextPurchasePrediction
{
    public string CustomerId { get; set; } = string.Empty;
    public DateTime PredictedPurchaseDate { get; set; }
    public List<string> PredictedProducts { get; set; } = new();
    public decimal PredictedOrderValue { get; set; }
    public double ConfidenceScore { get; set; }
}

public class OptimalPrice
{
    public string ProductId { get; set; } = string.Empty;
    public decimal RecommendedPrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal PotentialRevenueIncrease { get; set; }
    public double DemandElasticity { get; set; }
    public string PricingStrategy { get; set; } = string.Empty;
}

public class PriceElasticity
{
    public string ProductId { get; set; } = string.Empty;
    public double ElasticityCoefficient { get; set; }
    public string ElasticityCategory { get; set; } = string.Empty;
    public Dictionary<decimal, double> PriceDemandCurve { get; set; } = new();
}

public class PromotionEffectiveness
{
    public string PromotionId { get; set; } = string.Empty;
    public double PredictedLiftPercent { get; set; }
    public decimal PredictedRevenue { get; set; }
    public decimal EstimatedCost { get; set; }
    public decimal ROI { get; set; }
    public Dictionary<string, object> Metrics { get; set; } = new();
}

public class PromotionScenario
{
    public string PromotionType { get; set; } = string.Empty;
    public decimal DiscountPercent { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<string> TargetProducts { get; set; } = new();
    public List<string> TargetCustomerSegments { get; set; } = new();
}

public class ModelStatus
{
    public string ModelId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime LastTraining { get; set; }
    public DateTime LastPrediction { get; set; }
    public double Accuracy { get; set; }
    public string Version { get; set; } = string.Empty;
}

public class UpdateParameters
{
    public Dictionary<string, object> Parameters { get; set; } = new();
    public bool RetrainModel { get; set; }
    public string UpdateReason { get; set; } = string.Empty;
}

public class PredictionModel
{
    public string ModelId { get; set; } = string.Empty;
    public string ModelType { get; set; } = string.Empty;
    public DateTime TrainedDate { get; set; }
    public double Accuracy { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
}