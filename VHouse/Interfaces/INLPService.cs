using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VHouse.Classes;

namespace VHouse.Interfaces
{
    public interface INLPService
    {
        // Intent Recognition & Classification
        Task<IntentRecognition> AnalyzeIntentAsync(string text, string context);
        Task<ConversationFlow> GenerateResponseAsync(ConversationContext context);
        Task<ConversationResponse> ProcessChatMessageAsync(ConversationMessage message);
        Task<ConversationContext> ManageDialogStateAsync(string sessionId, Dictionary<string, object> input);
        
        // Text Analysis & Understanding
        Task<SentimentAnalysis> AnalyzeSentimentAsync(string text);
        Task<EntityExtraction> ExtractEntitiesAsync(string text);
        Task<TextClassification> ClassifyTextAsync(string text, List<string> categories);
        Task<LanguageDetection> DetectLanguageAsync(string text);
        Task<KeywordExtraction> ExtractKeywordsAsync(string text);
        
        // Advanced NLP Features
        Task<TextSummarization> SummarizeTextAsync(string text, int maxSentences);
        Task<TopicModeling> AnalyzeTopicsAsync(List<string> documents);
        Task<SemanticSimilarity> CalculateSimilarityAsync(string text1, string text2);
        Task<TextGeneration> GenerateTextAsync(TextGenerationRequest request);
        Task<QuestionAnswering> AnswerQuestionAsync(string question, string context);
        
        // Document Processing
        Task<DocumentAnalysis> AnalyzeDocumentAsync(NLPDocument document);
        Task<DocumentSummarization> SummarizeDocumentAsync(NLPDocument document);
        Task<DocumentClassification> ClassifyDocumentAsync(NLPDocument document);
        Task<InformationExtraction> ExtractInformationAsync(NLPDocument document, ExtractionRules rules);
        
        // Knowledge Management
        Task<KnowledgeGraph> BuildKnowledgeGraphAsync(List<string> documents);
        Task<EntityRelationship> ExtractRelationshipsAsync(string text);
        Task<ConceptExtraction> ExtractConceptsAsync(string text);
        Task<KnowledgeQuery> QueryKnowledgeBaseAsync(string query);
        
        // Conversational AI Training
        Task<ConversationModel> TrainConversationModelAsync(ConversationTrainingData data);
        Task<IntentModel> TrainIntentModelAsync(IntentTrainingData data);
        Task<bool> UpdateConversationModelAsync(string modelId, List<ConversationExample> examples);
    }
}