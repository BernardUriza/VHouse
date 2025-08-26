using VHouse.Interfaces;
using VHouse.Classes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace VHouse.Services;

public class ConversationalAIService
{
    private readonly ILogger<ConversationalAIService> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly Dictionary<string, List<string>> _intentPatterns;
    private readonly Dictionary<string, List<string>> _responses;

    public ConversationalAIService(
        ILogger<ConversationalAIService> logger,
        IConfiguration configuration,
        HttpClient httpClient)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClient = httpClient;
        _intentPatterns = InitializeIntentPatterns();
        _responses = InitializeResponses();
    }

    public async Task<ConversationResponse> ProcessMessageAsync(ConversationMessage message)
    {
        try
        {
            _logger.LogInformation($"Processing message: {message.Text}");
            await Task.Delay(500); // Simulate processing time
            
            // Analyze intent
            var intent = await AnalyzeIntentAsync(message.Text, message.Context);
            
            // Generate response based on intent
            var response = await GenerateResponseAsync(intent, message);
            
            // Extract entities if needed
            var entities = await ExtractEntitiesAsync(message.Text);
            
            return new ConversationResponse
            {
                ResponseId = Guid.NewGuid().ToString(),
                MessageId = message.MessageId,
                Text = response.Text,
                Intent = intent.Intent,
                Confidence = intent.Confidence,
                Entities = entities.Entities,
                Suggestions = GenerateSuggestions(intent.Intent),
                Context = UpdateContext(message.Context, intent, entities),
                ResponseTime = TimeSpan.FromMilliseconds(new Random().Next(300, 800)),
                Timestamp = DateTime.UtcNow,
                RequiresHumanHandoff = intent.Confidence < 0.5,
                SentimentScore = await AnalyzeSentimentAsync(message.Text)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing message: {message.Text}");
            throw;
        }
    }

    public async Task<IntentRecognition> AnalyzeIntentAsync(string text, ConversationContext context)
    {
        try
        {
            await Task.Delay(200);
            
            var normalizedText = text.ToLower().Trim();
            var bestMatch = "general_inquiry";
            var bestScore = 0.0;
            
            foreach (var intentPattern in _intentPatterns)
            {
                var score = CalculateIntentScore(normalizedText, intentPattern.Value);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestMatch = intentPattern.Key;
                }
            }
            
            // Boost confidence based on context
            if (context?.PreviousIntents?.Contains(bestMatch) == true)
            {
                bestScore = Math.Min(1.0, bestScore * 1.2);
            }
            
            return new IntentRecognition
            {
                Intent = bestMatch,
                Confidence = Math.Max(0.3, bestScore), // Minimum confidence
                AlternativeIntents = GetAlternativeIntents(normalizedText, bestMatch),
                IntentParameters = ExtractIntentParameters(normalizedText, bestMatch),
                ContextFactors = AnalyzeContextFactors(context)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error analyzing intent for text: {text}");
            throw;
        }
    }

    public async Task<ResponseGeneration> GenerateResponseAsync(IntentRecognition intent, ConversationMessage message)
    {
        try
        {
            await Task.Delay(300);
            
            var responses = _responses.ContainsKey(intent.Intent) 
                ? _responses[intent.Intent] 
                : _responses["general_inquiry"];
                
            var selectedResponse = responses[new Random().Next(responses.Count)];
            
            // Personalize response based on context
            selectedResponse = PersonalizeResponse(selectedResponse, message.Context);
            
            // Add dynamic content based on intent parameters
            selectedResponse = EnrichResponse(selectedResponse, intent.IntentParameters);
            
            return new ResponseGeneration
            {
                Text = selectedResponse,
                ResponseType = GetResponseType(intent.Intent),
                Confidence = intent.Confidence,
                GenerationMethod = "Rule-based + Template",
                AlternativeResponses = GenerateAlternativeResponses(intent, message, 2),
                RequiresAction = RequiresAction(intent.Intent),
                ActionType = GetActionType(intent.Intent),
                EmotionalTone = DetermineEmotionalTone(intent, message),
                Personalization = GetPersonalizationLevel(message.Context)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error generating response for intent: {intent.Intent}");
            throw;
        }
    }

    public async Task<EntityExtraction> ExtractEntitiesAsync(string text)
    {
        try
        {
            await Task.Delay(150);
            
            var entities = new List<ExtractedEntity>();
            
            // Extract dates
            var dateMatches = Regex.Matches(text, @"\b\d{1,2}[/-]\d{1,2}[/-]\d{2,4}\b");
            foreach (Match match in dateMatches)
            {
                entities.Add(new ExtractedEntity
                {
                    Type = "Date",
                    Value = match.Value,
                    StartPosition = match.Index,
                    EndPosition = match.Index + match.Length,
                    Confidence = 0.95
                });
            }
            
            // Extract phone numbers
            var phoneMatches = Regex.Matches(text, @"\b\d{3}[-.]?\d{3}[-.]?\d{4}\b");
            foreach (Match match in phoneMatches)
            {
                entities.Add(new ExtractedEntity
                {
                    Type = "PhoneNumber",
                    Value = match.Value,
                    StartPosition = match.Index,
                    EndPosition = match.Index + match.Length,
                    Confidence = 0.90
                });
            }
            
            // Extract email addresses
            var emailMatches = Regex.Matches(text, @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b");
            foreach (Match match in emailMatches)
            {
                entities.Add(new ExtractedEntity
                {
                    Type = "Email",
                    Value = match.Value,
                    StartPosition = match.Index,
                    EndPosition = match.Index + match.Length,
                    Confidence = 0.98
                });
            }
            
            // Extract monetary amounts
            var moneyMatches = Regex.Matches(text, @"\$\d{1,3}(?:,\d{3})*(?:\.\d{2})?");
            foreach (Match match in moneyMatches)
            {
                entities.Add(new ExtractedEntity
                {
                    Type = "Money",
                    Value = match.Value,
                    StartPosition = match.Index,
                    EndPosition = match.Index + match.Length,
                    Confidence = 0.92
                });
            }
            
            // Extract product names (simple keyword matching)
            var productKeywords = new[] { "laptop", "phone", "tablet", "computer", "monitor", "keyboard", "mouse" };
            foreach (var keyword in productKeywords)
            {
                if (text.ToLower().Contains(keyword))
                {
                    var index = text.ToLower().IndexOf(keyword);
                    entities.Add(new ExtractedEntity
                    {
                        Type = "Product",
                        Value = keyword,
                        StartPosition = index,
                        EndPosition = index + keyword.Length,
                        Confidence = 0.75
                    });
                }
            }
            
            return new EntityExtraction
            {
                Entities = entities,
                TotalEntitiesFound = entities.Count,
                ProcessingTime = TimeSpan.FromMilliseconds(new Random().Next(100, 200)),
                ExtractionMethod = "Regex + Rule-based"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error extracting entities from text: {text}");
            throw;
        }
    }

    public async Task<SentimentAnalysis> AnalyzeSentimentAsync(string text)
    {
        try
        {
            await Task.Delay(100);
            
            // Simple sentiment analysis based on keywords
            var positiveWords = new[] { "good", "great", "excellent", "love", "happy", "satisfied", "amazing", "wonderful", "fantastic" };
            var negativeWords = new[] { "bad", "terrible", "awful", "hate", "angry", "frustrated", "disappointed", "horrible", "disgusting" };
            
            var words = text.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var positiveCount = words.Count(w => positiveWords.Contains(w));
            var negativeCount = words.Count(w => negativeWords.Contains(w));
            
            var sentiment = "neutral";
            var score = 0.5;
            
            if (positiveCount > negativeCount)
            {
                sentiment = "positive";
                score = 0.5 + (Math.Min(positiveCount, 5) * 0.1);
            }
            else if (negativeCount > positiveCount)
            {
                sentiment = "negative";
                score = 0.5 - (Math.Min(negativeCount, 5) * 0.1);
            }
            
            return new SentimentAnalysis
            {
                Sentiment = sentiment,
                Score = Math.Max(0, Math.Min(1, score)),
                Confidence = 0.7 + (new Random().NextDouble() * 0.2),
                Emotions = new Dictionary<string, double>
                {
                    ["joy"] = sentiment == "positive" ? 0.8 : 0.2,
                    ["anger"] = sentiment == "negative" ? 0.7 : 0.1,
                    ["sadness"] = sentiment == "negative" ? 0.6 : 0.1,
                    ["surprise"] = 0.3,
                    ["fear"] = sentiment == "negative" ? 0.4 : 0.1,
                    ["disgust"] = sentiment == "negative" ? 0.5 : 0.1
                },
                KeyPhrases = ExtractKeyPhrases(text),
                SentimentTrend = "stable"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error analyzing sentiment for text: {text}");
            throw;
        }
    }

    public async Task<ConversationFlow> ManageConversationFlowAsync(ConversationSession session)
    {
        try
        {
            _logger.LogInformation($"Managing conversation flow for session {session.SessionId}");
            await Task.Delay(200);
            
            var currentStep = DetermineCurrentStep(session);
            var nextSteps = GenerateNextSteps(currentStep, session.Context);
            var flowDecision = MakeFlowDecision(session);
            
            return new ConversationFlow
            {
                SessionId = session.SessionId,
                CurrentStep = currentStep,
                NextSteps = nextSteps,
                FlowDecision = flowDecision,
                Context = session.Context,
                IsComplete = IsConversationComplete(session),
                RequiresEscalation = RequiresEscalation(session),
                SuggestedActions = GenerateSuggestedActions(currentStep),
                ConversationState = GetConversationState(session),
                FlowMetrics = new ConversationMetrics
                {
                    TotalTurns = session.MessageHistory?.Count ?? 0,
                    AverageResponseTime = TimeSpan.FromMilliseconds(500),
                    SentimentTrend = "positive",
                    ResolutionProgress = CalculateResolutionProgress(session)
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error managing conversation flow for session {session.SessionId}");
            throw;
        }
    }

    public async Task<List<ConversationSuggestion>> GetResponseSuggestionsAsync(ConversationContext context)
    {
        try
        {
            await Task.Delay(150);
            
            var suggestions = new List<ConversationSuggestion>();
            
            // Generate contextual suggestions
            if (context.CurrentIntent == "product_inquiry")
            {
                suggestions.AddRange(new[]
                {
                    new ConversationSuggestion { Text = "Tell me more about the specifications", Category = "Information", Confidence = 0.9 },
                    new ConversationSuggestion { Text = "What's the price?", Category = "Pricing", Confidence = 0.85 },
                    new ConversationSuggestion { Text = "Is it available in stock?", Category = "Availability", Confidence = 0.8 }
                });
            }
            else if (context.CurrentIntent == "support_request")
            {
                suggestions.AddRange(new[]
                {
                    new ConversationSuggestion { Text = "I need technical support", Category = "Support", Confidence = 0.9 },
                    new ConversationSuggestion { Text = "How do I return this item?", Category = "Returns", Confidence = 0.85 },
                    new ConversationSuggestion { Text = "Can you transfer me to a human agent?", Category = "Escalation", Confidence = 0.75 }
                });
            }
            else
            {
                suggestions.AddRange(new[]
                {
                    new ConversationSuggestion { Text = "Can you help me with my order?", Category = "Order", Confidence = 0.8 },
                    new ConversationSuggestion { Text = "I'm looking for product recommendations", Category = "Recommendation", Confidence = 0.75 },
                    new ConversationSuggestion { Text = "What are your business hours?", Category = "Information", Confidence = 0.7 }
                });
            }
            
            return suggestions.OrderByDescending(s => s.Confidence).Take(5).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting response suggestions");
            throw;
        }
    }

    // Private helper methods
    private Dictionary<string, List<string>> InitializeIntentPatterns()
    {
        return new Dictionary<string, List<string>>
        {
            ["greeting"] = new List<string> { "hello", "hi", "hey", "good morning", "good afternoon", "good evening" },
            ["product_inquiry"] = new List<string> { "product", "item", "buy", "purchase", "price", "cost", "specs", "specifications" },
            ["support_request"] = new List<string> { "help", "support", "problem", "issue", "broken", "not working", "error" },
            ["order_status"] = new List<string> { "order", "delivery", "shipping", "track", "status", "when will", "where is" },
            ["return_request"] = new List<string> { "return", "refund", "exchange", "money back", "cancel order" },
            ["complaint"] = new List<string> { "complain", "disappointed", "terrible", "awful", "bad service", "unsatisfied" },
            ["compliment"] = new List<string> { "great", "excellent", "amazing", "love", "fantastic", "wonderful", "perfect" },
            ["goodbye"] = new List<string> { "bye", "goodbye", "see you", "thanks", "thank you", "that's all" },
            ["general_inquiry"] = new List<string> { "what", "how", "when", "where", "why", "can you", "do you", "information" }
        };
    }

    private Dictionary<string, List<string>> InitializeResponses()
    {
        return new Dictionary<string, List<string>>
        {
            ["greeting"] = new List<string>
            {
                "Hello! How can I help you today?",
                "Hi there! What can I assist you with?",
                "Good to see you! How may I help?",
                "Welcome to VHouse! What brings you here today?"
            },
            ["product_inquiry"] = new List<string>
            {
                "I'd be happy to help you learn more about our products. What specific item are you interested in?",
                "Great! We have an amazing selection. What type of product are you looking for?",
                "I can provide detailed information about any of our products. Which one caught your eye?",
                "Let me help you find the perfect product. What are you shopping for today?"
            },
            ["support_request"] = new List<string>
            {
                "I'm here to help resolve any issues you're experiencing. Can you tell me more about the problem?",
                "I understand you need assistance. What specific issue can I help you with?",
                "No problem! I'm here to help. What seems to be the trouble?",
                "I'd be glad to help troubleshoot. What's going on?"
            },
            ["order_status"] = new List<string>
            {
                "I can help you track your order. Could you please provide your order number?",
                "Let me check on your order status. What's your order number?",
                "I'll be happy to look up your order information. Do you have your order number handy?",
                "Sure! I can help track your order. What's the order number?"
            },
            ["return_request"] = new List<string>
            {
                "I can help you with your return. What item would you like to return and what's the reason?",
                "No problem! I'll guide you through the return process. Which item needs to be returned?",
                "I understand you'd like to return an item. Can you tell me more about it?",
                "I'm here to help with your return. What's the order number for the item you'd like to return?"
            },
            ["complaint"] = new List<string>
            {
                "I'm sorry to hear about your experience. I want to make this right. Can you tell me what happened?",
                "I apologize for any inconvenience. Please share the details so I can help resolve this.",
                "I understand your frustration, and I'm here to help. What can I do to improve your experience?",
                "Thank you for bringing this to my attention. How can I help make this better?"
            },
            ["compliment"] = new List<string>
            {
                "Thank you so much! I'm delighted to hear you're happy with our service.",
                "That's wonderful to hear! We appreciate your positive feedback.",
                "I'm so glad you had a great experience! Is there anything else I can help you with?",
                "Thank you for the kind words! We're thrilled you're satisfied."
            },
            ["goodbye"] = new List<string>
            {
                "Thank you for choosing VHouse! Have a wonderful day!",
                "It was my pleasure helping you today. Take care!",
                "Goodbye! Don't hesitate to reach out if you need anything else.",
                "Have a great day! Thanks for visiting VHouse!"
            },
            ["general_inquiry"] = new List<string>
            {
                "I'd be happy to help answer your question. What would you like to know?",
                "Great question! Let me provide you with the information you need.",
                "I'm here to help with any questions you have. What can I assist you with?",
                "Sure! I'd be glad to help. What information are you looking for?"
            }
        };
    }

    private double CalculateIntentScore(string text, List<string> patterns)
    {
        var score = 0.0;
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var pattern in patterns)
        {
            if (text.Contains(pattern))
            {
                score += 0.8; // Exact phrase match
            }
            else
            {
                var patternWords = pattern.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var matchCount = patternWords.Count(pw => words.Any(w => w.Contains(pw) || pw.Contains(w)));
                score += (double)matchCount / patternWords.Length * 0.5;
            }
        }
        
        return Math.Min(1.0, score / patterns.Count);
    }

    private List<AlternativeIntent> GetAlternativeIntents(string text, string primaryIntent)
    {
        var alternatives = new List<AlternativeIntent>();
        var random = new Random();
        
        foreach (var intentPattern in _intentPatterns)
        {
            if (intentPattern.Key != primaryIntent)
            {
                var score = CalculateIntentScore(text, intentPattern.Value);
                if (score > 0.2)
                {
                    alternatives.Add(new AlternativeIntent
                    {
                        Intent = intentPattern.Key,
                        Confidence = score,
                        Reason = "Keyword similarity"
                    });
                }
            }
        }
        
        return alternatives.OrderByDescending(a => a.Confidence).Take(3).ToList();
    }

    private Dictionary<string, object> ExtractIntentParameters(string text, string intent)
    {
        var parameters = new Dictionary<string, object>();
        
        // Extract parameters based on intent type
        switch (intent)
        {
            case "product_inquiry":
                var productKeywords = new[] { "laptop", "phone", "tablet", "computer" };
                var foundProduct = productKeywords.FirstOrDefault(p => text.ToLower().Contains(p));
                if (foundProduct != null)
                    parameters["product_type"] = foundProduct;
                break;
                
            case "order_status":
                var orderMatch = Regex.Match(text, @"\b\d{6,}\b");
                if (orderMatch.Success)
                    parameters["order_number"] = orderMatch.Value;
                break;
        }
        
        return parameters;
    }

    private List<string> AnalyzeContextFactors(ConversationContext context)
    {
        var factors = new List<string>();
        
        if (context?.PreviousIntents?.Any() == true)
            factors.Add("conversation_history");
            
        if (context?.UserPreferences?.Any() == true)
            factors.Add("user_preferences");
            
        if (context?.CurrentTopic != null)
            factors.Add("topic_continuity");
            
        return factors;
    }

    private string PersonalizeResponse(string response, ConversationContext context)
    {
        if (context?.UserName != null)
        {
            response = response.Replace("Hello!", $"Hello {context.UserName}!");
        }
        
        return response;
    }

    private string EnrichResponse(string response, Dictionary<string, object> parameters)
    {
        foreach (var param in parameters)
        {
            response = response.Replace($"{{{param.Key}}}", param.Value.ToString());
        }
        
        return response;
    }

    private List<string> GenerateSuggestions(string intent)
    {
        return intent switch
        {
            "product_inquiry" => new List<string> { "Show me specifications", "What's the price?", "Is it in stock?" },
            "support_request" => new List<string> { "Technical support", "Return policy", "Contact human agent" },
            "order_status" => new List<string> { "Track my order", "Change delivery address", "Cancel order" },
            _ => new List<string> { "How can you help me?", "Show me products", "Contact support" }
        };
    }

    private ConversationContext UpdateContext(ConversationContext context, IntentRecognition intent, EntityExtraction entities)
    {
        if (context == null)
            context = new ConversationContext();
            
        context.CurrentIntent = intent.Intent;
        context.PreviousIntents = context.PreviousIntents ?? new List<string>();
        
        if (!context.PreviousIntents.Contains(intent.Intent))
            context.PreviousIntents.Add(intent.Intent);
            
        // Update entities
        foreach (var entity in entities.Entities)
        {
            context.ExtractedEntities = context.ExtractedEntities ?? new Dictionary<string, object>();
            context.ExtractedEntities[entity.Type] = entity.Value;
        }
        
        return context;
    }

    private List<string> ExtractKeyPhrases(string text)
    {
        var words = text.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return words.Where(w => w.Length > 3).Distinct().Take(5).ToList();
    }

    private string GetResponseType(string intent)
    {
        return intent switch
        {
            "greeting" => "greeting",
            "goodbye" => "closing",
            "support_request" => "assistance",
            "product_inquiry" => "information",
            _ => "general"
        };
    }

    private List<ResponseGeneration> GenerateAlternativeResponses(IntentRecognition intent, ConversationMessage message, int count)
    {
        var alternatives = new List<ResponseGeneration>();
        var responses = _responses.ContainsKey(intent.Intent) ? _responses[intent.Intent] : _responses["general_inquiry"];
        
        for (int i = 0; i < Math.Min(count, responses.Count); i++)
        {
            alternatives.Add(new ResponseGeneration
            {
                Text = responses[i],
                ResponseType = GetResponseType(intent.Intent),
                Confidence = intent.Confidence * 0.9,
                GenerationMethod = "Template Alternative"
            });
        }
        
        return alternatives;
    }

    private bool RequiresAction(string intent)
    {
        return new[] { "support_request", "order_status", "return_request", "complaint" }.Contains(intent);
    }

    private string GetActionType(string intent)
    {
        return intent switch
        {
            "support_request" => "create_ticket",
            "order_status" => "lookup_order",
            "return_request" => "initiate_return",
            "complaint" => "escalate_to_manager",
            _ => "none"
        };
    }

    private string DetermineEmotionalTone(IntentRecognition intent, ConversationMessage message)
    {
        return intent.Intent switch
        {
            "complaint" => "empathetic",
            "compliment" => "appreciative",
            "support_request" => "helpful",
            "greeting" => "friendly",
            _ => "professional"
        };
    }

    private string GetPersonalizationLevel(ConversationContext context)
    {
        if (context?.UserName != null) return "personalized";
        if (context?.PreviousIntents?.Any() == true) return "contextual";
        return "generic";
    }

    private string DetermineCurrentStep(ConversationSession session)
    {
        if (session.MessageHistory?.Count == 0) return "initial_greeting";
        
        var lastIntent = session.Context?.CurrentIntent;
        return lastIntent switch
        {
            "greeting" => "engagement",
            "product_inquiry" => "information_gathering",
            "support_request" => "problem_solving",
            "order_status" => "order_lookup",
            _ => "general_assistance"
        };
    }

    private List<ConversationStep> GenerateNextSteps(string currentStep, ConversationContext context)
    {
        return currentStep switch
        {
            "initial_greeting" => new List<ConversationStep>
            {
                new ConversationStep { StepName = "identify_intent", Description = "Understand user's primary need" }
            },
            "engagement" => new List<ConversationStep>
            {
                new ConversationStep { StepName = "gather_requirements", Description = "Collect specific information" },
                new ConversationStep { StepName = "provide_options", Description = "Present available solutions" }
            },
            _ => new List<ConversationStep>
            {
                new ConversationStep { StepName = "continue_assistance", Description = "Provide ongoing help" }
            }
        };
    }

    private string MakeFlowDecision(ConversationSession session)
    {
        var context = session.Context;
        
        if (context?.CurrentIntent == "complaint")
            return "escalate_to_human";
        if (context?.CurrentIntent == "goodbye")
            return "end_conversation";
            
        return "continue_conversation";
    }

    private bool IsConversationComplete(ConversationSession session)
    {
        return session.Context?.CurrentIntent == "goodbye" || 
               session.MessageHistory?.Count > 20;
    }

    private bool RequiresEscalation(ConversationSession session)
    {
        return session.Context?.CurrentIntent == "complaint" ||
               (session.MessageHistory?.Count(m => m.Intent == "support_request") ?? 0) > 3;
    }

    private List<SuggestedAction> GenerateSuggestedActions(string currentStep)
    {
        return currentStep switch
        {
            "problem_solving" => new List<SuggestedAction>
            {
                new SuggestedAction { Action = "create_support_ticket", Description = "Create a support ticket" },
                new SuggestedAction { Action = "schedule_callback", Description = "Schedule a callback" }
            },
            "information_gathering" => new List<SuggestedAction>
            {
                new SuggestedAction { Action = "show_products", Description = "Display relevant products" },
                new SuggestedAction { Action = "provide_specs", Description = "Show product specifications" }
            },
            _ => new List<SuggestedAction>()
        };
    }

    private string GetConversationState(ConversationSession session)
    {
        var messageCount = session.MessageHistory?.Count ?? 0;
        
        if (messageCount == 0) return "not_started";
        if (messageCount < 3) return "beginning";
        if (messageCount < 10) return "active";
        if (messageCount < 20) return "extended";
        return "lengthy";
    }

    private double CalculateResolutionProgress(ConversationSession session)
    {
        var intent = session.Context?.CurrentIntent;
        var messageCount = session.MessageHistory?.Count ?? 0;
        
        return intent switch
        {
            "greeting" => 0.2,
            "product_inquiry" => Math.Min(0.8, 0.3 + (messageCount * 0.1)),
            "support_request" => Math.Min(0.9, 0.4 + (messageCount * 0.1)),
            "goodbye" => 1.0,
            _ => Math.Min(0.7, messageCount * 0.1)
        };
    }
}