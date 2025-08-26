using VHouse.Classes;

namespace VHouse.Interfaces;

public interface IComputerVisionService
{
    // Image Analysis
    Task<ImageAnalysis> AnalyzeImageAsync(byte[] imageData);
    Task<ImageAnalysis> AnalyzeImageAsync(string imageUrl);
    Task<ObjectDetection> DetectObjectsAsync(byte[] imageData, ObjectDetectionConfig config);
    Task<FaceAnalysis> AnalyzeFacesAsync(byte[] imageData);
    Task<TextExtraction> ExtractTextAsync(byte[] imageData);
    
    // Image Classification
    Task<ImageClassification> ClassifyImageAsync(byte[] imageData, string modelId);
    Task<ProductRecognition> RecognizeProductAsync(byte[] imageData);
    Task<QualityInspection> InspectQualityAsync(byte[] imageData, QualityStandards standards);
    
    // Image Processing
    Task<ImageEnhancement> EnhanceImageAsync(byte[] imageData, EnhancementSettings settings);
    Task<ImageSegmentation> SegmentImageAsync(byte[] imageData, SegmentationConfig config);
    Task<BackgroundRemoval> RemoveBackgroundAsync(byte[] imageData);
    
    // Video Analysis
    Task<VideoAnalysis> AnalyzeVideoAsync(byte[] videoData);
    Task<ActionRecognition> RecognizeActionsAsync(byte[] videoData);
    Task<VideoSummarization> SummarizeVideoAsync(byte[] videoData, TimeSpan maxDuration);
    
    // Advanced Features
    Task<SimilaritySearch> FindSimilarImagesAsync(byte[] queryImage, ImageSearchConfig config);
    Task<ImageGeneration> GenerateImageAsync(ImageGenerationPrompt prompt);
    Task<StyleTransfer> TransferStyleAsync(byte[] contentImage, byte[] styleImage);
    Task<ImageComparison> CompareImagesAsync(byte[] image1, byte[] image2);
    
    // Batch Processing
    Task<BatchProcessingResult> ProcessImagesBatchAsync(List<byte[]> images, BatchProcessingConfig config);
    Task<VideoProcessingResult> ProcessVideosBatchAsync(List<byte[]> videos, BatchVideoConfig config);
    
    // Model Management
    Task<List<CVModel>> GetAvailableModelsAsync();
    Task<CVModelInfo> GetModelInfoAsync(string modelId);
    Task<CVModelTraining> TrainCustomModelAsync(CVTrainingData trainingData, CVTrainingConfig config);
    
    // Performance Monitoring
    Task<CVPerformanceMetrics> GetPerformanceMetricsAsync();
    Task<CVUsageStats> GetUsageStatisticsAsync(DateTime from, DateTime to);
}