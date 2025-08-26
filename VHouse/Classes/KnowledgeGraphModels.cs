using System.ComponentModel.DataAnnotations;

namespace VHouse.Classes;

// Knowledge Graph models to resolve compilation errors
public class KnowledgeGraph
{
    public string GraphId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<KnowledgeEntity> Entities { get; set; } = new();
    public List<KnowledgeRelationship> Relationships { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public string Version { get; set; } = "1.0";
    public int EntityCount => Entities?.Count ?? 0;
    public int RelationshipCount => Relationships?.Count ?? 0;
    public string Status { get; set; } = "Active";
    public List<string> Tags { get; set; } = new();
}
public class KnowledgeEntity
{
    public string EntityId { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public Dictionary<string, object> Properties { get; set; } = new();
    public List<string> Categories { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public double Confidence { get; set; } = 1.0;
    public string Source { get; set; } = string.Empty;
}

public class KnowledgeRelationship
{
    public string RelationshipId { get; set; } = string.Empty;
    public string SourceEntityId { get; set; } = string.Empty;
    public string TargetEntityId { get; set; } = string.Empty;
    public string RelationType { get; set; } = string.Empty;
    public double Strength { get; set; } = 1.0;
    public Dictionary<string, object> Properties { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public string Source { get; set; } = string.Empty;
    public bool IsDirected { get; set; } = true;
    public string Type { get; set; } = string.Empty;
    public double Confidence { get; set; } = 1.0;
}

public class Community
{
    public string CommunityId { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<string> EntityIds { get; set; } = new();
    public List<string> Members { get; set; } = new();
    public Dictionary<string, double> CentralityScores { get; set; } = new();
    public double Modularity { get; set; }
    public double Cohesion { get; set; }
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
    public string DetectionAlgorithm { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class SemanticSearchResult
{
    public string ResultId { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public List<SearchMatch> Matches { get; set; } = new();
    public DateTime SearchedAt { get; set; } = DateTime.UtcNow;
    public TimeSpan ProcessingTime { get; set; }
    public Dictionary<string, object> SearchMetadata { get; set; } = new();
}

public class SearchMatch
{
    public string EntityId { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public double RelevanceScore { get; set; }
    public Dictionary<string, object> MatchDetails { get; set; } = new();
    public List<string> MatchedFields { get; set; } = new();
}

public class GraphTraversal
{
    public string TraversalId { get; set; } = string.Empty;
    public string StartEntityId { get; set; } = string.Empty;
    public string EndEntityId { get; set; } = string.Empty;
    public List<GraphPath> Paths { get; set; } = new();
    public DateTime TraversedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> TraversalParameters { get; set; } = new();
}

public class GraphPath
{
    public string PathId { get; set; } = string.Empty;
    public List<string> EntityIds { get; set; } = new();
    public List<string> RelationshipIds { get; set; } = new();
    public double PathWeight { get; set; }
    public int PathLength { get; set; }
    public Dictionary<string, object> PathProperties { get; set; } = new();
}

public class GraphCluster
{
    public string ClusterId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<string> EntityIds { get; set; } = new();
    public Dictionary<string, object> ClusterProperties { get; set; } = new();
    public double Cohesion { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string ClusteringAlgorithm { get; set; } = string.Empty;
}

public class GraphMetrics
{
    public string MetricsId { get; set; } = string.Empty;
    public int TotalEntities { get; set; }
    public int TotalRelationships { get; set; }
    public double AverageConnectivity { get; set; }
    public double GraphDensity { get; set; }
    public Dictionary<string, double> CentralityMeasures { get; set; } = new();
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}

public class KnowledgeGraphQuery
{
    public string QueryId { get; set; } = string.Empty;
    public string QueryText { get; set; } = string.Empty;
    public string QueryType { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int MaxResults { get; set; } = 100;
}

public class KnowledgeGraphUpdate
{
    public string UpdateId { get; set; } = string.Empty;
    public string UpdateType { get; set; } = string.Empty; // CREATE, UPDATE, DELETE
    public string TargetType { get; set; } = string.Empty; // ENTITY, RELATIONSHIP
    public string TargetId { get; set; } = string.Empty;
    public Dictionary<string, object> UpdateData { get; set; } = new();
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string UpdatedBy { get; set; } = string.Empty;
}

public class GraphAnalytics
{
    public string AnalyticsId { get; set; } = string.Empty;
    public string AnalysisType { get; set; } = string.Empty;
    public Dictionary<string, object> Results { get; set; } = new();
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
    public TimeSpan ProcessingTime { get; set; }
    public Dictionary<string, object> AnalysisParameters { get; set; } = new();
}

public class EntitySimilarity
{
    public string SimilarityId { get; set; } = string.Empty;
    public string EntityId1 { get; set; } = string.Empty;
    public string EntityId2 { get; set; } = string.Empty;
    public double SimilarityScore { get; set; }
    public string SimilarityMethod { get; set; } = string.Empty;
    public Dictionary<string, object> SimilarityDetails { get; set; } = new();
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}

public class GraphVisualization
{
    public string VisualizationId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Layout { get; set; } = string.Empty;
    public Dictionary<string, object> LayoutParameters { get; set; } = new();
    public List<VisualNode> Nodes { get; set; } = new();
    public List<VisualEdge> Edges { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class VisualNode
{
    public string NodeId { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public double X { get; set; }
    public double Y { get; set; }
    public string Color { get; set; } = "#000000";
    public double Size { get; set; } = 10;
    public Dictionary<string, object> Attributes { get; set; } = new();
}

public class VisualEdge
{
    public string EdgeId { get; set; } = string.Empty;
    public string SourceNodeId { get; set; } = string.Empty;
    public string TargetNodeId { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Color { get; set; } = "#808080";
    public double Width { get; set; } = 1.0;
    public Dictionary<string, object> Attributes { get; set; } = new();
}

public class KnowledgeExtraction
{
    public string ExtractionId { get; set; } = string.Empty;
    public string SourceDocument { get; set; } = string.Empty;
    public List<KnowledgeExtractedEntity> ExtractedEntities { get; set; } = new();
    public List<ExtractedRelationship> ExtractedRelationships { get; set; } = new();
    public DateTime ExtractedAt { get; set; } = DateTime.UtcNow;
    public string ExtractionMethod { get; set; } = string.Empty;
    public double ConfidenceThreshold { get; set; } = 0.8;
}

public class KnowledgeExtractedEntity
{
    public string Text { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public int StartPosition { get; set; }
    public int EndPosition { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new();
}

public class ExtractedRelationship
{
    public string SubjectEntity { get; set; } = string.Empty;
    public string Predicate { get; set; } = string.Empty;
    public string ObjectEntity { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new();
}

public class GraphImportResult
{
    public string ImportId { get; set; } = string.Empty;
    public int EntitiesImported { get; set; }
    public int RelationshipsImported { get; set; }
    public List<string> Errors { get; set; } = new();
    public TimeSpan ImportDuration { get; set; }
    public DateTime ImportedAt { get; set; } = DateTime.UtcNow;
    public string ImportSource { get; set; } = string.Empty;
}

public class GraphExportResult
{
    public string ExportId { get; set; } = string.Empty;
    public string ExportFormat { get; set; } = string.Empty;
    public string ExportPath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public int EntitiesExported { get; set; }
    public int RelationshipsExported { get; set; }
    public DateTime ExportedAt { get; set; } = DateTime.UtcNow;
}

// Additional classes for KnowledgeGraphService
public class EntityExtractionConfig
{
    public string ConfigId { get; set; } = string.Empty;
    public string ExtractionMethod { get; set; } = string.Empty;
    public double ConfidenceThreshold { get; set; } = 0.8;
    public List<string> EntityTypes { get; set; } = new();
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public class KnowledgeGraphRequest
{
    public string RequestId { get; set; } = string.Empty;
    public string RequestType { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
}

public class KnowledgeGraphConstruction
{
    public string ConstructionId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int EntitiesProcessed { get; set; }
    public int RelationshipsCreated { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class EntityLinkingConfig
{
    public string ConfigId { get; set; } = string.Empty;
    public string LinkingMethod { get; set; } = string.Empty;
    public double SimilarityThreshold { get; set; } = 0.7;
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public class EntityLinkingResult
{
    public string ResultId { get; set; } = string.Empty;
    public List<EntityLink> Links { get; set; } = new();
    public DateTime LinkedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class EntityLink
{
    public string LinkId { get; set; } = string.Empty;
    public string SourceEntityId { get; set; } = string.Empty;
    public string TargetEntityId { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public string LinkType { get; set; } = string.Empty;
}

public class GraphQueryRequest
{
    public string QueryId { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public string QueryLanguage { get; set; } = "CYPHER";
    public Dictionary<string, object> Parameters { get; set; } = new();
    public int MaxResults { get; set; } = 1000;
}

public class SemanticSearchRequest
{
    public string SearchId { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public string SearchType { get; set; } = string.Empty;
    public int MaxResults { get; set; } = 100;
    public Dictionary<string, object> Filters { get; set; } = new();
}

public class SemanticSearch
{
    public string SearchId { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public List<SearchMatch> Results { get; set; } = new();
    public DateTime SearchedAt { get; set; } = DateTime.UtcNow;
    public TimeSpan ProcessingTime { get; set; }
}

public class QueryResult
{
    public string ResultId { get; set; } = string.Empty;
    public List<Dictionary<string, object>> Rows { get; set; } = new();
    public List<string> Columns { get; set; } = new();
    public int TotalResults { get; set; }
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
}

public class SemanticAnswer
{
    public string AnswerId { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public List<string> SupportingEntities { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}