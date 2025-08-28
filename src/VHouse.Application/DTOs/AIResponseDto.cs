using VHouse.Domain.Enums;

namespace VHouse.Application.DTOs;

public record AIResponseDto
{
    public string Content { get; init; } = string.Empty;
    public AIProvider UsedProvider { get; init; }
    public string UsedModel { get; init; } = string.Empty;
    public bool IsSuccessful { get; init; }
    public string? ErrorMessage { get; init; }
    public int TokensUsed { get; init; }
    public double ResponseTimeMs { get; init; }
}

public record ImageGenerationDto
{
    public byte[] ImageData { get; init; } = Array.Empty<byte>();
    public string ImageUrl { get; init; } = string.Empty;
    public string RevisedPrompt { get; init; } = string.Empty;
    public AIProvider UsedProvider { get; init; }
    public string UsedModel { get; init; } = string.Empty;
    public bool IsSuccessful { get; init; }
    public string? ErrorMessage { get; init; }
}

public record AIHealthStatusDto
{
    public Dictionary<string, bool> ServiceStatus { get; init; } = new();
    public string RecommendedProvider { get; init; } = string.Empty;
    public bool FallbackAvailable { get; init; }
}