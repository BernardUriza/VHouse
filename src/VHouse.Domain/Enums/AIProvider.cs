namespace VHouse.Domain.Enums;

public enum AIProvider
{
    None = 0,
    Claude = 1,
    OpenAI = 2,
    Fallback = 3
}

public enum AIModel
{
    None = 0,
    // Claude models
    Claude3Opus = 1,
    Claude3Sonnet = 2,
    Claude3Haiku = 3,
    
    // OpenAI models
    GPT4 = 101,
    GPT35Turbo = 102,
    GPT4Turbo = 103,
    
    // DALL-E models
    DallE3 = 201,
    DallE2 = 202
}