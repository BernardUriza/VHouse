using System.ComponentModel.DataAnnotations;

namespace VHouse.Classes;

// Temporary AI Ethics models to resolve compilation errors
public class BiasAnalysisRequest
{
    public string ModelId { get; set; } = string.Empty;
    public string DatasetName { get; set; } = string.Empty;
    public List<string> ProtectedAttributes { get; set; } = new();
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public class BiasAnalysisReport
{
    public string AnalysisId { get; set; } = string.Empty;
    public string ModelId { get; set; } = string.Empty;
    public bool HasBias { get; set; }
    public Dictionary<string, double> BiasMetrics { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class FairnessAuditRequest
{
    public string ModelId { get; set; } = string.Empty;
    public string AuditType { get; set; } = string.Empty;
    public Dictionary<string, object> Criteria { get; set; } = new();
}

public class FairnessAuditReport
{
    public string AuditId { get; set; } = string.Empty;
    public string ModelId { get; set; } = string.Empty;
    public bool PassedAudit { get; set; }
    public Dictionary<string, object> Results { get; set; } = new();
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}

public class ExplainabilityRequest
{
    public string ModelId { get; set; } = string.Empty;
    public string PredictionId { get; set; } = string.Empty;
    public Dictionary<string, object> InputData { get; set; } = new();
}

public class TransparencyRequest
{
    public string ModelId { get; set; } = string.Empty;
    public List<string> RequiredDetails { get; set; } = new();
}

public class TransparencyReport
{
    public string ReportId { get; set; } = string.Empty;
    public string ModelId { get; set; } = string.Empty;
    public Dictionary<string, object> ModelDetails { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class EthicsGuideline
{
    public string GuidelineId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}

public class EthicsComplianceRequest
{
    public string ModelId { get; set; } = string.Empty;
    public List<string> Guidelines { get; set; } = new();
}

public class EthicsComplianceCheck
{
    public string CheckId { get; set; } = string.Empty;
    public string ModelId { get; set; } = string.Empty;
    public bool IsCompliant { get; set; }
    public List<string> Violations { get; set; } = new();
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
}

public class MitigationStrategy
{
    public string StrategyId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Steps { get; set; } = new();
}

public class ComplianceStatus
{
    public string StatusId { get; set; } = string.Empty;
    public string ModelId { get; set; } = string.Empty;
    public bool IsCompliant { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<string> Issues { get; set; } = new();
    public DateTime LastChecked { get; set; } = DateTime.UtcNow;
}

public class BiasTestCase
{
    public string TestId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, object> TestData { get; set; } = new();
    public bool Passed { get; set; }
}

public class AuditFinding
{
    public string FindingId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
    public DateTime FoundAt { get; set; } = DateTime.UtcNow;
}

public class ContributionInfo
{
    public string FeatureName { get; set; } = string.Empty;
    public double Contribution { get; set; }
    public string Direction { get; set; } = string.Empty;
    public double Confidence { get; set; }
}

public class CounterfactualExample
{
    public string ExampleId { get; set; } = string.Empty;
    public Dictionary<string, object> OriginalInput { get; set; } = new();
    public Dictionary<string, object> ModifiedInput { get; set; } = new();
    public string OriginalPrediction { get; set; } = string.Empty;
    public string ModifiedPrediction { get; set; } = string.Empty;
    public double Distance { get; set; }
}