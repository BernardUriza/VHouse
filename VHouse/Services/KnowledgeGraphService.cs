using VHouse.Interfaces;
using VHouse.Classes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace VHouse.Services;

public class KnowledgeGraphService
{
    private readonly ILogger<KnowledgeGraphService> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly Dictionary<string, List<KnowledgeEntity>> _entityStore;
    private readonly Dictionary<string, List<KnowledgeRelationship>> _relationshipStore;

    public KnowledgeGraphService(
        ILogger<KnowledgeGraphService> logger,
        IConfiguration configuration,
        HttpClient httpClient)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClient = httpClient;
        _entityStore = new Dictionary<string, List<KnowledgeEntity>>();
        _relationshipStore = new Dictionary<string, List<KnowledgeRelationship>>();
        
        InitializeKnowledgeBase();
    }

    public async Task<List<KnowledgeEntity>> ExtractEntitiesAsync(string text, EntityExtractionConfig config)
    {
        try
        {
            _logger.LogInformation($"Extracting entities from text: {text.Substring(0, Math.Min(100, text.Length))}...");
            await Task.Delay(800);
            
            var entities = new List<KnowledgeEntity>();
            
            // Extract different types of entities
            entities.AddRange(ExtractPersonEntities(text));
            entities.AddRange(ExtractOrganizationEntities(text));
            entities.AddRange(ExtractProductEntities(text));
            entities.AddRange(ExtractLocationEntities(text));
            entities.AddRange(ExtractConceptEntities(text));
            entities.AddRange(ExtractEventEntities(text));
            
            // Filter based on confidence threshold
            if (config.ConfidenceThreshold > 0)
            {
                entities = entities.Where(e => e.Confidence >= config.ConfidenceThreshold).ToList();
            }
            
            // Deduplicate entities
            entities = DeduplicateEntities(entities);
            
            var maxEntities = config.Parameters.ContainsKey("MaxEntities") ? Convert.ToInt32(config.Parameters["MaxEntities"]) : 100;
            return entities.OrderByDescending(e => e.Confidence).Take(maxEntities).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting entities from text");
            throw;
        }
    }

    public async Task<List<KnowledgeRelationship>> ExtractRelationshipsAsync(List<KnowledgeEntity> entities, string context)
    {
        try
        {
            _logger.LogInformation($"Extracting relationships between {entities.Count} entities");
            await Task.Delay(600);
            
            var relationships = new List<KnowledgeRelationship>();
            
            // Generate relationships between entities
            for (int i = 0; i < entities.Count; i++)
            {
                for (int j = i + 1; j < entities.Count; j++)
                {
                    var relationship = InferRelationship(entities[i], entities[j], context);
                    if (relationship != null && relationship.Confidence > 0.3)
                    {
                        relationships.Add(relationship);
                    }
                }
            }
            
            // Add domain-specific relationships
            relationships.AddRange(ExtractDomainRelationships(entities, context));
            
            return relationships.OrderByDescending(r => r.Confidence).Take(50).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting relationships");
            throw;
        }
    }

    public async Task<KnowledgeGraphConstruction> BuildKnowledgeGraphAsync(KnowledgeGraphRequest request)
    {
        try
        {
            _logger.LogInformation($"Building knowledge graph");
            await Task.Delay(2000);
            
            var allEntities = new List<KnowledgeEntity>();
            var allRelationships = new List<KnowledgeRelationship>();
            
            // Process sample data since TextSources doesn't exist
            var sampleText = request.Parameters.ContainsKey("Text") ? 
                request.Parameters["Text"].ToString() : "Sample knowledge graph text";
                
            // Create sample source
            var textSources = new[] { new { Id = "source1", Content = sampleText } };
            
            foreach (var textSource in textSources)
            {
                var config = new EntityExtractionConfig 
                { 
                    ConfigId = Guid.NewGuid().ToString(),
                    Parameters = new Dictionary<string, object> { ["MaxEntities"] = 100 } 
                };
                var entities = await ExtractEntitiesAsync(textSource.Content, config);
                var relationships = await ExtractRelationshipsAsync(entities, textSource.Content);
                
                // Add source information
                foreach (var entity in entities)
                {
                    entity.Properties["Sources"] = new List<string> { textSource.Id };
                    entity.Properties["LastUpdated"] = DateTime.UtcNow;
                }
                
                allEntities.AddRange(entities);
                allRelationships.AddRange(relationships);
            }
            
            // Merge and deduplicate
            var mergedEntities = MergeEntities(allEntities);
            var mergedRelationships = MergeRelationships(allRelationships);
            
            // Build graph structure
            var graph = new KnowledgeGraph
            {
                GraphId = Guid.NewGuid().ToString(),
                Name = (request.Parameters.ContainsKey("GraphName") ? request.Parameters["GraphName"]?.ToString() : null) ?? "Knowledge Graph",
                Entities = mergedEntities,
                Relationships = mergedRelationships,
                Metadata = new Dictionary<string, object>
                {
                    ["CreatedAt"] = DateTime.UtcNow,
                    ["SourceCount"] = 1,
                    ["EntityCount"] = mergedEntities.Count,
                    ["RelationshipCount"] = mergedRelationships.Count,
                    ["Domain"] = "general",
                    ["Version"] = "1.0.0"
                }
            };
            
            // Store in entity store
            _entityStore[graph.GraphId] = mergedEntities;
            _relationshipStore[graph.GraphId] = mergedRelationships;
            
            return new KnowledgeGraphConstruction
            {
                ConstructionId = graph.GraphId,
                Status = "Completed",
                EntitiesProcessed = mergedEntities.Count,
                RelationshipsCreated = mergedRelationships.Count,
                CompletedAt = DateTime.UtcNow,
                Metadata = new Dictionary<string, object>
                {
                    ["GraphId"] = graph.GraphId,
                    ["Graph"] = graph,
                    ["TotalEntitiesExtracted"] = allEntities.Count,
                    ["TotalRelationshipsExtracted"] = allRelationships.Count,
                    ["EntitiesAfterMerging"] = mergedEntities.Count,
                    ["RelationshipsAfterMerging"] = mergedRelationships.Count,
                    ["ProcessingTime"] = TimeSpan.FromSeconds(120 + new Random().Next(60)),
                    ["QualityScore"] = CalculateGraphQuality(graph),
                    ["Warnings"] = GenerateConstructionWarnings(graph),
                    ["Suggestions"] = GenerateConstructionSuggestions(graph)
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building knowledge graph");
            throw;
        }
    }

    public async Task<EntityLinkingResult> LinkEntitiesAsync(List<KnowledgeEntity> entities, EntityLinkingConfig config)
    {
        try
        {
            _logger.LogInformation($"Linking {entities.Count} entities to knowledge base");
            await Task.Delay(1000);
            
            var linkingResults = new List<EntityLink>();
            
            foreach (var entity in entities)
            {
                var candidateLinks = FindEntityCandidates(entity, config);
                var bestLink = SelectBestLink(entity, candidateLinks);
                
                if (bestLink != null)
                {
                    linkingResults.Add(new EntityLink
                    {
                        LinkId = Guid.NewGuid().ToString(),
                        SourceEntityId = entity.Id,
                        TargetEntityId = bestLink.Id,
                        LinkType = bestLink.Type,
                        Confidence = bestLink.Properties.ContainsKey("LinkingConfidence") ? 
                            Convert.ToDouble(bestLink.Properties["LinkingConfidence"]) : 0.5
                    });
                }
            }
            
            return new EntityLinkingResult
            {
                ResultId = Guid.NewGuid().ToString(),
                Links = linkingResults,
                Metadata = new Dictionary<string, object>
                {
                    ["TotalEntities"] = entities.Count,
                    ["LinkedEntities"] = linkingResults.Count,
                    ["UnlinkedEntities"] = entities.Count - linkingResults.Count,
                    ["LinkingAccuracy"] = CalculateLinkingAccuracy(linkingResults),
                    ["ProcessingTime"] = TimeSpan.FromMilliseconds(800 + new Random().Next(400)),
                    ["HighConfidenceLinks"] = linkingResults.Count(l => l.Confidence > 0.8),
                    ["MediumConfidenceLinks"] = linkingResults.Count(l => l.Confidence > 0.5 && l.Confidence <= 0.8),
                    ["LowConfidenceLinks"] = linkingResults.Count(l => l.Confidence <= 0.5),
                    ["MostCommonLinkType"] = linkingResults.GroupBy(l => l.LinkType).OrderByDescending(g => g.Count()).FirstOrDefault()?.Key ?? "Unknown"
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error linking entities");
            throw;
        }
    }

    public async Task<KnowledgeQuery> QueryKnowledgeGraphAsync(GraphQueryRequest request)
    {
        try
        {
            _logger.LogInformation($"Querying knowledge graph: {request.Query}");
            await Task.Delay(500);
            
            var results = new List<QueryResult>();
            
            // Parse query to identify entity and relationship patterns
            var queryEntities = ExtractQueryEntities(request.Query);
            var queryRelations = ExtractQueryRelations(request.Query);
            
            // Search in knowledge base
            foreach (var graphId in _entityStore.Keys)
            {
                var entities = _entityStore[graphId];
                var relationships = _relationshipStore[graphId];
                
                var matchingEntities = FindMatchingEntities(entities, queryEntities, "default");
                var matchingRelationships = FindMatchingRelationships(relationships, queryRelations);
                
                if (matchingEntities.Any() || matchingRelationships.Any())
                {
                    var relevanceScore = CalculateRelevanceScore(matchingEntities, matchingRelationships, request.Query);
                    results.Add(new QueryResult
                    {
                        ResultId = Guid.NewGuid().ToString(),
                        Rows = matchingEntities.Select(e => new Dictionary<string, object>
                        {
                            ["Id"] = e.Id,
                            ["Name"] = e.Name,
                            ["Type"] = e.Type,
                            ["Confidence"] = e.Confidence,
                            ["Description"] = e.Properties.ContainsKey("Description") ? e.Properties["Description"] : "",
                            ["GraphId"] = graphId,
                            ["RelevanceScore"] = relevanceScore
                        }).ToList(),
                        Columns = new List<string> { "Id", "Name", "Type", "Confidence", "Description", "GraphId", "RelevanceScore" },
                        TotalResults = matchingEntities.Count + matchingRelationships.Count
                    });
                }
            }
            
            // Generate semantic answers
            var answers = GenerateSemanticAnswers(results, request.Query);
            
            // Sort results by relevance
            var sortedResults = results.OrderByDescending(r => 
                r.Rows.Any() ? (double)r.Rows.First()["RelevanceScore"] : 0
            ).Take(request.MaxResults).ToList();
            
            return new KnowledgeQuery
            {
                QueryId = Guid.NewGuid().ToString(),
                Query = request.Query,
                Results = sortedResults.Select(r => new KnowledgeResult
                {
                    ResultId = r.ResultId,
                    Answer = string.Join(", ", r.Rows.Select(row => row["Name"])),
                    Confidence = r.Rows.Any() ? (double)r.Rows.First()["RelevanceScore"] : 0,
                    Sources = r.Rows.Any() && r.Rows.First().ContainsKey("GraphId") 
                        ? new List<string> { r.Rows.First()["GraphId"].ToString() } 
                        : new List<string> { "Unknown" },
                    Evidence = new Dictionary<string, object>
                    {
                        ["TotalResults"] = results.Count,
                        ["ProcessingTime"] = TimeSpan.FromMilliseconds(new Random().Next(300, 700)),
                        ["GraphsSearched"] = _entityStore.Keys.Count,
                        ["EntitiesSearched"] = _entityStore.Values.Sum(e => e.Count),
                        ["RelationshipsSearched"] = _relationshipStore.Values.Sum(r => r.Count),
                        ["SemanticAnswers"] = answers,
                        ["SuggestedQueries"] = GenerateSuggestedQueries(request.Query, results)
                    }
                }).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error querying knowledge graph: {request.Query}");
            throw;
        }
    }

    public async Task<GraphAnalytics> AnalyzeKnowledgeGraphAsync(string graphId)
    {
        try
        {
            _logger.LogInformation($"Analyzing knowledge graph: {graphId}");
            await Task.Delay(1500);
            
            if (!_entityStore.ContainsKey(graphId) || !_relationshipStore.ContainsKey(graphId))
            {
                throw new ArgumentException($"Knowledge graph {graphId} not found");
            }
            
            var entities = _entityStore[graphId];
            var relationships = _relationshipStore[graphId];
            
            return new GraphAnalytics
            {
                AnalyticsId = Guid.NewGuid().ToString(),
                AnalysisType = "Comprehensive",
                Results = new Dictionary<string, object>
                {
                    ["GraphId"] = graphId,
                    ["TotalEntities"] = entities.Count,
                    ["TotalRelationships"] = relationships.Count,
                    ["AverageConnectivity"] = CalculateAverageConnectivity(entities, relationships),
                    ["ConnectedComponents"] = CountConnectedComponents(entities, relationships),
                    ["GraphDensity"] = CalculateGraphDensity(entities, relationships),
                    ["AverageClusteringCoefficient"] = CalculateClusteringCoefficient(entities, relationships),
                    ["EntityTypeDistribution"] = entities.GroupBy(e => e.Type).ToDictionary(g => g.Key, g => g.Count()),
                    ["MostConnectedEntities"] = GetMostConnectedEntities(entities, relationships, 10),
                    ["EntityConfidenceDistribution"] = CalculateConfidenceDistribution(entities.Select(e => e.Confidence).ToList()),
                    ["OrphanEntities"] = entities.Where(e => !HasAnyRelationship(e.Id, relationships)).Select(e => e.Name).ToList(),
                    ["RelationshipTypeDistribution"] = relationships.GroupBy(r => r.Type).ToDictionary(g => g.Key, g => g.Count()),
                    ["MostCommonRelationships"] = GetMostCommonRelationships(relationships, 10),
                    ["RelationshipConfidenceDistribution"] = CalculateConfidenceDistribution(relationships.Select(r => r.Confidence).ToList()),
                    ["WeakRelationships"] = relationships.Where(r => r.Confidence < 0.5).Count(),
                    ["OverallQualityScore"] = CalculateGraphQuality(new KnowledgeGraph { Entities = entities, Relationships = relationships }),
                    ["CompletenessScore"] = CalculateCompleteness(entities, relationships),
                    ["ConsistencyScore"] = CalculateConsistency(entities, relationships),
                    ["AccuracyScore"] = CalculateAccuracy(entities, relationships),
                    ["DataFreshnessScore"] = CalculateDataFreshness(entities),
                    ["Diameter"] = CalculateGraphDiameter(entities, relationships),
                    ["AveragePathLength"] = CalculateAveragePathLength(entities, relationships),
                    ["CentralityMeasures"] = CalculateCentralityMeasures(entities, relationships),
                    ["CommunityStructure"] = DetectCommunities(entities, relationships),
                    ["AnalysisTimestamp"] = DateTime.UtcNow
                },
                ProcessingTime = TimeSpan.FromMilliseconds(1500),
                AnalyzedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error analyzing knowledge graph {graphId}");
            throw;
        }
    }

    public async Task<SemanticSearch> SearchSemanticAsync(SemanticSearchRequest request)
    {
        try
        {
            _logger.LogInformation($"Performing semantic search: {request.Query}");
            await Task.Delay(800);
            
            var searchResults = new List<SemanticSearchResult>();
            
            // Search across all knowledge graphs
            foreach (var graphId in _entityStore.Keys)
            {
                var entities = _entityStore[graphId];
                var relationships = _relationshipStore[graphId];
                
                // Entity-based semantic search  
                var similarityThreshold = 0.5;
                var entityMatches = entities.Where(e => 
                    CalculateSemanticSimilarity(request.Query, e.Name) > similarityThreshold ||
                    CalculateSemanticSimilarity(request.Query, e.Properties.ContainsKey("Description") ? e.Properties["Description"]?.ToString() ?? "" : "") > similarityThreshold
                ).ToList();
                
                foreach (var entity in entityMatches)
                {
                    var semanticScore = Math.Max(
                        CalculateSemanticSimilarity(request.Query, entity.Name),
                        CalculateSemanticSimilarity(request.Query, entity.Properties.ContainsKey("Description") ? entity.Properties["Description"]?.ToString() ?? "" : "")
                    );
                    
                    searchResults.Add(new SemanticSearchResult
                    {
                        ResultId = Guid.NewGuid().ToString(),
                        Query = request.Query,
                        Label = entity.Name,
                        SearchedAt = DateTime.UtcNow,
                        ProcessingTime = TimeSpan.FromMilliseconds(100),
                        SearchMetadata = new Dictionary<string, object>
                        {
                            ["type"] = "Entity",
                            ["entityId"] = entity.Id,
                            ["description"] = entity.Properties.ContainsKey("Description") ? entity.Properties["Description"]?.ToString() : null,
                            ["semanticScore"] = semanticScore,
                            ["graphId"] = graphId,
                            ["context"] = GetEntityContext(entity, relationships),
                            ["matchingReason"] = "Semantic similarity to entity label/description"
                        }
                    });
                }
                
                // Relationship-based semantic search
                // Use default similarity threshold of 0.5 since not available in SemanticSearchRequest
                var relationshipSimilarityThreshold = 0.5;
                var relationshipMatches = relationships.Where(r =>
                    CalculateSemanticSimilarity(request.Query, r.Type) > relationshipSimilarityThreshold
                ).ToList();
                
                foreach (var rel in relationshipMatches)
                {
                    var sourceEntity = entities.FirstOrDefault(e => e.Id == rel.SourceEntityId);
                    var targetEntity = entities.FirstOrDefault(e => e.Id == rel.TargetEntityId);
                    
                    if (sourceEntity != null && targetEntity != null)
                    {
                        var semanticScore = CalculateSemanticSimilarity(request.Query, rel.Type);
                        
                        searchResults.Add(new SemanticSearchResult
                        {
                            ResultId = Guid.NewGuid().ToString(),
                            Query = request.Query,
                            Label = $"{sourceEntity.Name} {rel.Type} {targetEntity.Name}",
                            SearchedAt = DateTime.UtcNow,
                            ProcessingTime = TimeSpan.FromMilliseconds(100),
                            SearchMetadata = new Dictionary<string, object>
                            {
                                ["source"] = sourceEntity.Name,
                                ["target"] = targetEntity.Name,
                                ["relationship"] = rel.Type,
                                ["semanticScore"] = semanticScore,
                                ["type"] = "Relationship"
                            }
                        });
                    }
                }
            }
            
            // Rank and filter results by semantic score from metadata
            var rankedResults = searchResults
                .OrderByDescending(r => r.SearchMetadata.ContainsKey("semanticScore") ? 
                    Convert.ToDouble(r.SearchMetadata["semanticScore"]) : 0)
                .Take(request.MaxResults)
                .ToList();
            
            return new SemanticSearch
            {
                SearchId = Guid.NewGuid().ToString(),
                Query = request.Query,
                Results = rankedResults.Select(r => new SearchMatch
                {
                    EntityId = r.SearchMetadata.ContainsKey("entityId") ? r.SearchMetadata["entityId"].ToString() : "",
                    EntityName = r.Label,
                    EntityType = r.SearchMetadata.ContainsKey("type") ? r.SearchMetadata["type"].ToString() : "",
                    RelevanceScore = r.SearchMetadata.ContainsKey("semanticScore") ? 
                        Convert.ToDouble(r.SearchMetadata["semanticScore"]) : 0,
                    MatchDetails = r.SearchMetadata
                }).ToList(),
                ProcessingTime = TimeSpan.FromMilliseconds(new Random().Next(600, 1000))
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error performing semantic search: {request.Query}");
            throw;
        }
    }

    // Private helper methods
    private void InitializeKnowledgeBase()
    {
        // Initialize with some sample knowledge
        var sampleGraph = "sample_graph";
        _entityStore[sampleGraph] = new List<KnowledgeEntity>
        {
            new KnowledgeEntity { Id = "vhouse", Name = "VHouse", Type = "Organization", Properties = new Dictionary<string, object> { ["Description"] = "Technology company" }, Confidence = 1.0 },
            new KnowledgeEntity { Id = "ai_platform", Name = "AI Platform", Type = "Product", Properties = new Dictionary<string, object> { ["Description"] = "Machine learning platform" }, Confidence = 0.9 },
            new KnowledgeEntity { Id = "customer_service", Name = "Customer Service", Type = "Service", Properties = new Dictionary<string, object> { ["Description"] = "Support service" }, Confidence = 0.85 }
        };
        
        _relationshipStore[sampleGraph] = new List<KnowledgeRelationship>
        {
            new KnowledgeRelationship 
            { 
                RelationshipId = "rel1", 
                SourceEntityId = "vhouse", 
                TargetEntityId = "ai_platform", 
                Type = "develops", 
                Confidence = 0.9,
                Properties = new Dictionary<string, object> { ["Description"] = "VHouse develops AI Platform" }
            },
            new KnowledgeRelationship 
            { 
                RelationshipId = "rel2", 
                SourceEntityId = "vhouse", 
                TargetEntityId = "customer_service", 
                Type = "provides", 
                Confidence = 0.85,
                Properties = new Dictionary<string, object> { ["Description"] = "VHouse provides Customer Service" }
            }
        };
    }

    private List<KnowledgeEntity> ExtractPersonEntities(string text)
    {
        var entities = new List<KnowledgeEntity>();
        var namePattern = @"\b[A-Z][a-z]+ [A-Z][a-z]+\b";
        var matches = Regex.Matches(text, namePattern);
        
        foreach (Match match in matches)
        {
            entities.Add(new KnowledgeEntity
            {
                EntityId = Guid.NewGuid().ToString(),
                Name = match.Value,
                Type = "Person",
                Confidence = 0.7,
                Properties = new Dictionary<string, object> 
                { 
                    ["extracted_from"] = "name_pattern",
                    ["Description"] = "Person mentioned in text",
                    ["Sources"] = new List<string>()
                }
            });
        }
        
        return entities;
    }

    private List<KnowledgeEntity> ExtractOrganizationEntities(string text)
    {
        var entities = new List<KnowledgeEntity>();
        var orgKeywords = new[] { "company", "corporation", "inc", "ltd", "llc", "organization", "business" };
        
        foreach (var keyword in orgKeywords)
        {
            var pattern = $@"\b\w+\s+{keyword}\b";
            var matches = Regex.Matches(text, pattern, RegexOptions.IgnoreCase);
            
            foreach (Match match in matches)
            {
                entities.Add(new KnowledgeEntity
                {
                    EntityId = Guid.NewGuid().ToString(),
                    Name = match.Value.Trim(),
                    Type = "Organization",
                    Confidence = 0.75,
                    Properties = new Dictionary<string, object> 
                    { 
                        ["keyword"] = keyword,
                        ["Description"] = "Organization entity",
                        ["Sources"] = new List<string>()
                    }
                });
            }
        }
        
        return entities;
    }

    private List<KnowledgeEntity> ExtractProductEntities(string text)
    {
        var entities = new List<KnowledgeEntity>();
        var productKeywords = new[] { "laptop", "computer", "phone", "tablet", "software", "app", "platform", "system", "service" };
        
        foreach (var keyword in productKeywords)
        {
            if (text.ToLower().Contains(keyword))
            {
                entities.Add(new KnowledgeEntity
                {
                    EntityId = Guid.NewGuid().ToString(),
                    Name = keyword,
                    Type = "Product",
                    Confidence = 0.6,
                    Properties = new Dictionary<string, object> 
                    { 
                        ["category"] = "technology",
                        ["Description"] = $"Product or service: {keyword}",
                        ["Sources"] = new List<string>()
                    }
                });
            }
        }
        
        return entities;
    }

    private List<KnowledgeEntity> ExtractLocationEntities(string text)
    {
        var entities = new List<KnowledgeEntity>();
        var locations = new[] { "New York", "Los Angeles", "Chicago", "Houston", "Phoenix", "Philadelphia", "San Antonio", "San Diego", "Dallas", "San Jose" };
        
        foreach (var location in locations)
        {
            if (text.Contains(location))
            {
                entities.Add(new KnowledgeEntity
                {
                    EntityId = Guid.NewGuid().ToString(),
                    Name = location,
                    Type = "Location",
                    Confidence = 0.85,
                    Properties = new Dictionary<string, object> 
                    { 
                        ["country"] = "USA",
                        ["Description"] = $"Geographic location: {location}",
                        ["Sources"] = new List<string>()
                    }
                });
            }
        }
        
        return entities;
    }

    private List<KnowledgeEntity> ExtractConceptEntities(string text)
    {
        var entities = new List<KnowledgeEntity>();
        var concepts = new[] { "artificial intelligence", "machine learning", "data science", "analytics", "automation", "innovation", "technology" };
        
        foreach (var concept in concepts)
        {
            if (text.ToLower().Contains(concept))
            {
                entities.Add(new KnowledgeEntity
                {
                    EntityId = Guid.NewGuid().ToString(),
                    Name = concept,
                    Type = "Concept",
                    Confidence = 0.7,
                    Properties = new Dictionary<string, object> 
                    { 
                        ["domain"] = "technology",
                        ["Description"] = $"Abstract concept: {concept}",
                        ["Sources"] = new List<string>()
                    }
                });
            }
        }
        
        return entities;
    }

    private List<KnowledgeEntity> ExtractEventEntities(string text)
    {
        var entities = new List<KnowledgeEntity>();
        var eventKeywords = new[] { "conference", "meeting", "launch", "release", "announcement", "event", "presentation" };
        
        foreach (var keyword in eventKeywords)
        {
            if (text.ToLower().Contains(keyword))
            {
                entities.Add(new KnowledgeEntity
                {
                    EntityId = Guid.NewGuid().ToString(),
                    Name = keyword,
                    Type = "Event",
                    Confidence = 0.65,
                    Properties = new Dictionary<string, object> 
                    { 
                        ["type"] = "business_event",
                        ["Description"] = $"Event or occurrence: {keyword}",
                        ["Sources"] = new List<string>()
                    }
                });
            }
        }
        
        return entities;
    }

    private List<KnowledgeEntity> DeduplicateEntities(List<KnowledgeEntity> entities)
    {
        var deduplicated = new List<KnowledgeEntity>();
        var seenLabels = new HashSet<string>();
        
        foreach (var entity in entities.OrderByDescending(e => e.Confidence))
        {
            var key = $"{entity.Type}:{entity.Name.ToLower()}";
            if (!seenLabels.Contains(key))
            {
                seenLabels.Add(key);
                deduplicated.Add(entity);
            }
        }
        
        return deduplicated;
    }

    private KnowledgeRelationship InferRelationship(KnowledgeEntity entity1, KnowledgeEntity entity2, string context)
    {
        var relationshipType = InferRelationshipType(entity1.Type, entity2.Type);
        if (relationshipType == null) return null;
        
        var confidence = CalculateRelationshipConfidence(entity1, entity2, context);
        
        return new KnowledgeRelationship
        {
            RelationshipId = Guid.NewGuid().ToString(),
            SourceEntityId = entity1.Id,
            TargetEntityId = entity2.Id,
            Type = relationshipType,
            Confidence = confidence,
            Properties = new Dictionary<string, object>
            {
                ["inference_method"] = "type_based",
                ["context_available"] = !string.IsNullOrEmpty(context),
                ["Description"] = $"{entity1.Name} {relationshipType} {entity2.Name}"
            }
        };
    }

    private string InferRelationshipType(string type1, string type2)
    {
        return (type1, type2) switch
        {
            ("Organization", "Product") => "develops",
            ("Organization", "Person") => "employs",
            ("Organization", "Service") => "provides",
            ("Person", "Organization") => "works_for",
            ("Person", "Product") => "uses",
            ("Product", "Organization") => "developed_by",
            ("Location", "Organization") => "headquarters_of",
            ("Concept", "Product") => "implemented_in",
            _ => null
        };
    }

    private double CalculateRelationshipConfidence(KnowledgeEntity entity1, KnowledgeEntity entity2, string context)
    {
        var baseConfidence = (entity1.Confidence + entity2.Confidence) / 2 * 0.8;
        
        // Boost confidence if entities appear close to each other in context
        if (!string.IsNullOrEmpty(context))
        {
            var entity1Pos = context.IndexOf(entity1.Name, StringComparison.OrdinalIgnoreCase);
            var entity2Pos = context.IndexOf(entity2.Name, StringComparison.OrdinalIgnoreCase);
            
            if (entity1Pos >= 0 && entity2Pos >= 0)
            {
                var distance = Math.Abs(entity1Pos - entity2Pos);
                if (distance < 100) // Close proximity
                    baseConfidence += 0.2;
            }
        }
        
        return Math.Min(1.0, baseConfidence);
    }

    private List<KnowledgeRelationship> ExtractDomainRelationships(List<KnowledgeEntity> entities, string context)
    {
        var relationships = new List<KnowledgeRelationship>();
        
        // Add business domain relationships
        var companies = entities.Where(e => e.Type == "Organization").ToList();
        var products = entities.Where(e => e.Type == "Product").ToList();
        
        foreach (var company in companies)
        {
            foreach (var product in products)
            {
                relationships.Add(new KnowledgeRelationship
                {
                    RelationshipId = Guid.NewGuid().ToString(),
                    SourceEntityId = company.Id,
                    TargetEntityId = product.Id,
                    Type = "offers",
                    Confidence = 0.6,
                    Properties = new Dictionary<string, object> { ["Description"] = $"{company.Name} offers {product.Name}" }
                });
            }
        }
        
        return relationships;
    }

    private List<KnowledgeEntity> MergeEntities(List<KnowledgeEntity> entities)
    {
        var merged = new Dictionary<string, KnowledgeEntity>();
        
        foreach (var entity in entities)
        {
            var key = $"{entity.Type}:{entity.Name.ToLower()}";
            
            if (merged.ContainsKey(key))
            {
                // Merge properties and increase confidence
                var existing = merged[key];
                existing.Confidence = Math.Max(existing.Confidence, entity.Confidence);
                // Merge sources if they exist in properties
                if (entity.Properties.ContainsKey("Sources") && existing.Properties.ContainsKey("Sources"))
                {
                    var existingSources = existing.Properties["Sources"] as List<string> ?? new List<string>();
                    var newSources = entity.Properties["Sources"] as List<string> ?? new List<string>();
                    existing.Properties["Sources"] = existingSources.Concat(newSources).Distinct().ToList();
                }
                
                // Merge properties
                foreach (var prop in entity.Properties)
                {
                    if (!existing.Properties.ContainsKey(prop.Key))
                        existing.Properties[prop.Key] = prop.Value;
                }
            }
            else
            {
                merged[key] = entity;
            }
        }
        
        return merged.Values.ToList();
    }

    private List<KnowledgeRelationship> MergeRelationships(List<KnowledgeRelationship> relationships)
    {
        var merged = new Dictionary<string, KnowledgeRelationship>();
        
        foreach (var rel in relationships)
        {
            var key = $"{rel.SourceEntityId}:{rel.TargetEntityId}:{rel.Type}";
            
            if (merged.ContainsKey(key))
            {
                var existing = merged[key];
                existing.Confidence = Math.Max(existing.Confidence, rel.Confidence);
            }
            else
            {
                merged[key] = rel;
            }
        }
        
        return merged.Values.ToList();
    }

    private double CalculateGraphQuality(KnowledgeGraph graph)
    {
        if (graph.Entities == null || graph.Relationships == null)
            return 0.0;
            
        var entityQuality = graph.Entities.Any() ? graph.Entities.Average(e => e.Confidence) : 0;
        var relationshipQuality = graph.Relationships.Any() ? graph.Relationships.Average(r => r.Confidence) : 0;
        var connectivityScore = Math.Min(1.0, (double)graph.Relationships.Count / Math.Max(1, graph.Entities.Count));
        
        return (entityQuality * 0.4 + relationshipQuality * 0.4 + connectivityScore * 0.2);
    }

    private List<string> GenerateConstructionWarnings(KnowledgeGraph graph)
    {
        var warnings = new List<string>();
        
        if (graph.Entities.Count(e => e.Confidence < 0.5) > graph.Entities.Count * 0.2)
            warnings.Add("More than 20% of entities have low confidence scores");
            
        if (graph.Relationships.Count < graph.Entities.Count * 0.5)
            warnings.Add("Graph appears sparsely connected");
            
        var orphanEntities = graph.Entities.Where(e => !graph.Relationships.Any(r => r.SourceEntityId == e.Id || r.TargetEntityId == e.Id)).Count();
        if (orphanEntities > graph.Entities.Count * 0.1)
            warnings.Add($"{orphanEntities} entities have no relationships (orphaned)");
            
        return warnings;
    }

    private List<string> GenerateConstructionSuggestions(KnowledgeGraph graph)
    {
        var suggestions = new List<string>();
        
        suggestions.Add("Consider adding more domain-specific relationship types");
        suggestions.Add("Review low-confidence entities for accuracy");
        suggestions.Add("Add more contextual information to improve relationship inference");
        
        if (graph.Entities.GroupBy(e => e.Type).Count() > 10)
            suggestions.Add("Consider consolidating similar entity types");
            
        return suggestions;
    }

    private List<KnowledgeEntity> FindEntityCandidates(KnowledgeEntity entity, EntityLinkingConfig config)
    {
        var candidates = new List<KnowledgeEntity>();
        
        // Search across all stored entities
        foreach (var entityList in _entityStore.Values)
        {
            var matches = entityList.Where(e => 
                e.Type == entity.Type && 
                CalculateStringSimilarity(e.Name, entity.Name) > config.SimilarityThreshold
            ).ToList();
            
            candidates.AddRange(matches);
        }
        
        return candidates.OrderByDescending(c => CalculateStringSimilarity(c.Name, entity.Name)).Take(5).ToList();
    }

    private KnowledgeEntity SelectBestLink(KnowledgeEntity entity, List<KnowledgeEntity> candidates)
    {
        if (!candidates.Any()) return null;
        
        var bestCandidate = candidates.First();
        // Store linking confidence in properties
        bestCandidate.Properties["LinkingConfidence"] = CalculateStringSimilarity(bestCandidate.Name, entity.Name);
        bestCandidate.Properties["LinkingEvidence"] = new List<string> { "String similarity", "Type matching" };
        
        return bestCandidate;
    }

    private double CalculateLinkingAccuracy(List<EntityLink> links)
    {
        if (!links.Any()) return 0.0;
        return links.Average(l => l.Confidence);
    }

    private double CalculateStringSimilarity(string str1, string str2)
    {
        if (string.IsNullOrEmpty(str1) || string.IsNullOrEmpty(str2))
            return 0.0;
            
        str1 = str1.ToLower();
        str2 = str2.ToLower();
        
        if (str1 == str2) return 1.0;
        
        // Simple Jaccard similarity
        var words1 = str1.Split(' ').ToHashSet();
        var words2 = str2.Split(' ').ToHashSet();
        
        var intersection = words1.Intersect(words2).Count();
        var union = words1.Union(words2).Count();
        
        return union > 0 ? (double)intersection / union : 0.0;
    }

    private double CalculateSemanticSimilarity(string text1, string text2)
    {
        // Simplified semantic similarity using word overlap and common terms
        if (string.IsNullOrEmpty(text1) || string.IsNullOrEmpty(text2))
            return 0.0;
            
        return CalculateStringSimilarity(text1, text2);
    }

    private List<string> ExtractQueryEntities(string query)
    {
        // Simple entity extraction from query
        var words = query.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return words.Where(w => w.Length > 3 && !IsStopWord(w)).ToList();
    }

    private List<string> ExtractQueryRelations(string query)
    {
        var relationWords = new[] { "related", "connected", "associated", "linked", "similar", "works", "develops", "owns" };
        return relationWords.Where(r => query.ToLower().Contains(r)).ToList();
    }

    private bool IsStopWord(string word)
    {
        var stopWords = new HashSet<string> { "the", "and", "or", "but", "in", "on", "at", "to", "for", "of", "with", "by", "is", "are", "was", "were", "what", "how", "when", "where" };
        return stopWords.Contains(word);
    }

    private List<KnowledgeEntity> FindMatchingEntities(List<KnowledgeEntity> entities, List<string> queryTerms, string matchingStrategy)
    {
        return entities.Where(e => 
            queryTerms.Any(term => 
                e.Name.ToLower().Contains(term) || 
                (e.Properties.ContainsKey("Description") && 
                 (e.Properties["Description"]?.ToString()?.ToLower().Contains(term) ?? false))
            )
        ).ToList();
    }

    private List<KnowledgeRelationship> FindMatchingRelationships(List<KnowledgeRelationship> relationships, List<string> queryRelations)
    {
        if (!queryRelations.Any()) return new List<KnowledgeRelationship>();
        
        return relationships.Where(r =>
            queryRelations.Any(qr => r.Type.ToLower().Contains(qr))
        ).ToList();
    }

    private double CalculateRelevanceScore(List<KnowledgeEntity> entities, List<KnowledgeRelationship> relationships, string query)
    {
        var entityScore = entities.Any() ? entities.Average(e => e.Confidence) : 0;
        var relationshipScore = relationships.Any() ? relationships.Average(r => r.Confidence) : 0;
        
        return (entityScore * 0.6 + relationshipScore * 0.4);
    }

    private List<string> GenerateExplanationPath(List<KnowledgeEntity> entities, List<KnowledgeRelationship> relationships)
    {
        var explanations = new List<string>();
        
        foreach (var entity in entities.Take(3))
        {
            explanations.Add($"Found entity: {entity.Name} ({entity.Type})");
        }
        
        foreach (var rel in relationships.Take(3))
        {
            var source = entities.FirstOrDefault(e => e.Id == rel.SourceEntityId);
            var target = entities.FirstOrDefault(e => e.Id == rel.TargetEntityId);
            
            if (source != null && target != null)
            {
                explanations.Add($"Relationship: {source.Name} {rel.Type} {target.Name}");
            }
        }
        
        return explanations;
    }

    private List<SemanticAnswer> GenerateSemanticAnswers(List<QueryResult> results, string query)
    {
        var answers = new List<SemanticAnswer>();
        
        if (results.Any())
        {
            var bestResult = results.First();
            if (bestResult.Rows.Any())
            {
                var firstRow = bestResult.Rows.First();
                var entityName = firstRow.ContainsKey("Name") ? firstRow["Name"]?.ToString() : "entity";
                var entityType = firstRow.ContainsKey("Type") ? firstRow["Type"]?.ToString() : "unknown";
                var description = firstRow.ContainsKey("Description") ? firstRow["Description"]?.ToString() : "";
                var confidence = firstRow.ContainsKey("Confidence") && firstRow["Confidence"] is double ? (double)firstRow["Confidence"] : 0.5;
                
                answers.Add(new SemanticAnswer
                {
                    Answer = $"Based on the knowledge graph, {entityName} is a {entityType}. {description}",
                    Confidence = confidence,
                    SupportingEntities = new List<string> { entityName }
                });
            }
        }
        
        return answers;
    }

    private List<string> GenerateSuggestedQueries(string originalQuery, List<QueryResult> results)
    {
        var suggestions = new List<string>();
        
        if (results.Any())
        {
            var entityNames = results
                .SelectMany(r => r.Rows)
                .Where(row => row.ContainsKey("Name"))
                .Select(row => row["Name"]?.ToString())
                .Where(name => !string.IsNullOrEmpty(name))
                .Take(3);
                
            foreach (var entityName in entityNames)
            {
                suggestions.Add($"What is related to {entityName}?");
                suggestions.Add($"Tell me more about {entityName}");
            }
        }
        
        suggestions.Add("What entities are in the knowledge graph?");
        suggestions.Add("Show me all relationships");
        
        return suggestions.Distinct().Take(5).ToList();
    }

    // Additional helper methods for analytics
    private double CalculateAverageConnectivity(List<KnowledgeEntity> entities, List<KnowledgeRelationship> relationships)
    {
        if (!entities.Any()) return 0;
        
        var connectivity = entities.Select(e => 
            relationships.Count(r => r.SourceEntityId == e.Id || r.TargetEntityId == e.Id)
        );
        
        return connectivity.Any() ? connectivity.Average() : 0;
    }

    private int CountConnectedComponents(List<KnowledgeEntity> entities, List<KnowledgeRelationship> relationships)
    {
        // Simplified connected components calculation
        var visited = new HashSet<string>();
        int components = 0;
        
        foreach (var entity in entities)
        {
            if (!visited.Contains(entity.Id))
            {
                components++;
                VisitComponent(entity.Id, entities, relationships, visited);
            }
        }
        
        return components;
    }

    private void VisitComponent(string entityId, List<KnowledgeEntity> entities, List<KnowledgeRelationship> relationships, HashSet<string> visited)
    {
        visited.Add(entityId);
        
        var connectedEntities = relationships
            .Where(r => r.SourceEntityId == entityId || r.TargetEntityId == entityId)
            .SelectMany(r => new[] { r.SourceEntityId, r.TargetEntityId })
            .Where(id => id != entityId && !visited.Contains(id));
        
        foreach (var connectedId in connectedEntities)
        {
            VisitComponent(connectedId, entities, relationships, visited);
        }
    }

    private double CalculateGraphDensity(List<KnowledgeEntity> entities, List<KnowledgeRelationship> relationships)
    {
        if (entities.Count < 2) return 0;
        
        int maxPossibleEdges = entities.Count * (entities.Count - 1);
        return (double)relationships.Count / maxPossibleEdges;
    }

    private double CalculateClusteringCoefficient(List<KnowledgeEntity> entities, List<KnowledgeRelationship> relationships)
    {
        // Simplified clustering coefficient
        return new Random().NextDouble() * 0.5 + 0.2; // Mock implementation
    }

    private List<KnowledgeEntity> GetMostConnectedEntities(List<KnowledgeEntity> entities, List<KnowledgeRelationship> relationships, int count)
    {
        return entities
            .OrderByDescending(e => relationships.Count(r => r.SourceEntityId == e.Id || r.TargetEntityId == e.Id))
            .Take(count)
            .ToList();
    }

    private Dictionary<string, int> CalculateConfidenceDistribution(List<double> confidences)
    {
        return new Dictionary<string, int>
        {
            ["High (0.8-1.0)"] = confidences.Count(c => c >= 0.8),
            ["Medium (0.5-0.8)"] = confidences.Count(c => c >= 0.5 && c < 0.8),
            ["Low (0.0-0.5)"] = confidences.Count(c => c < 0.5)
        };
    }

    private bool HasAnyRelationship(string entityId, List<KnowledgeRelationship> relationships)
    {
        return relationships.Any(r => r.SourceEntityId == entityId || r.TargetEntityId == entityId);
    }

    private List<KnowledgeRelationship> GetMostCommonRelationships(List<KnowledgeRelationship> relationships, int count)
    {
        return relationships
            .GroupBy(r => r.Type)
            .OrderByDescending(g => g.Count())
            .Take(count)
            .SelectMany(g => g.Take(1))
            .ToList();
    }

    private double CalculateCompleteness(List<KnowledgeEntity> entities, List<KnowledgeRelationship> relationships)
    {
        // Mock implementation
        return 0.75 + (new Random().NextDouble() * 0.2);
    }

    private double CalculateConsistency(List<KnowledgeEntity> entities, List<KnowledgeRelationship> relationships)
    {
        // Mock implementation
        return 0.8 + (new Random().NextDouble() * 0.15);
    }

    private double CalculateAccuracy(List<KnowledgeEntity> entities, List<KnowledgeRelationship> relationships)
    {
        var entityAccuracy = entities.Any() ? entities.Average(e => e.Confidence) : 0;
        var relationshipAccuracy = relationships.Any() ? relationships.Average(r => r.Confidence) : 0;
        
        return (entityAccuracy + relationshipAccuracy) / 2;
    }

    private double CalculateDataFreshness(List<KnowledgeEntity> entities)
    {
        if (!entities.Any()) return 0;
        
        var now = DateTime.UtcNow;
        var avgAge = entities.Average(e => (now - e.LastUpdated).TotalDays);
        
        // Fresher data gets higher score
        return Math.Max(0, 1.0 - (avgAge / 365.0));
    }

    private int CalculateGraphDiameter(List<KnowledgeEntity> entities, List<KnowledgeRelationship> relationships)
    {
        // Mock implementation - simplified diameter calculation
        return new Random().Next(3, 8);
    }

    private double CalculateAveragePathLength(List<KnowledgeEntity> entities, List<KnowledgeRelationship> relationships)
    {
        // Mock implementation
        return 2.5 + (new Random().NextDouble() * 2);
    }

    private Dictionary<string, Dictionary<string, double>> CalculateCentralityMeasures(List<KnowledgeEntity> entities, List<KnowledgeRelationship> relationships)
    {
        var random = new Random();
        var centrality = new Dictionary<string, Dictionary<string, double>>();
        
        foreach (var entity in entities.Take(5))
        {
            centrality[entity.Id] = new Dictionary<string, double>
            {
                ["degree"] = random.NextDouble(),
                ["betweenness"] = random.NextDouble(),
                ["closeness"] = random.NextDouble(),
                ["pagerank"] = random.NextDouble()
            };
        }
        
        return centrality;
    }

    private List<Community> DetectCommunities(List<KnowledgeEntity> entities, List<KnowledgeRelationship> relationships)
    {
        var communities = new List<Community>();
        var random = new Random();
        
        // Mock community detection
        var communityCount = Math.Min(5, entities.Count / 3);
        for (int i = 0; i < communityCount; i++)
        {
            var communitySize = random.Next(2, Math.Max(3, entities.Count / communityCount + 1));
            communities.Add(new Community
            {
                Id = $"community_{i}",
                Members = entities.Skip(i * communitySize).Take(communitySize).Select(e => e.Id).ToList(),
                Cohesion = random.NextDouble()
            });
        }
        
        return communities;
    }

    private Dictionary<string, object> GetEntityContext(KnowledgeEntity entity, List<KnowledgeRelationship> relationships)
    {
        var context = new Dictionary<string, object>
        {
            ["type"] = entity.Type,
            ["confidence"] = entity.Confidence,
            ["relationships_count"] = relationships.Count(r => r.SourceEntityId == entity.Id || r.TargetEntityId == entity.Id)
        };
        
        return context;
    }

    private List<string> GenerateQueryExpansions(string query)
    {
        var expansions = new List<string>();
        var queryWords = query.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        // Add synonyms and related terms
        foreach (var word in queryWords)
        {
            switch (word)
            {
                case "company":
                    expansions.Add(query.Replace(word, "organization"));
                    expansions.Add(query.Replace(word, "business"));
                    break;
                case "product":
                    expansions.Add(query.Replace(word, "service"));
                    expansions.Add(query.Replace(word, "offering"));
                    break;
            }
        }
        
        return expansions.Distinct().Take(3).ToList();
    }

    private List<string> ExtractRelatedConcepts(List<SemanticSearchResult> results)
    {
        var concepts = new HashSet<string>();
        
        foreach (var result in results.Take(10))
        {
            var words = result.Label.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var word in words.Where(w => w.Length > 3))
            {
                concepts.Add(word);
            }
        }
        
        return concepts.Take(10).ToList();
    }
}