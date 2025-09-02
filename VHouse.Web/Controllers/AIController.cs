using MediatR;
using Microsoft.AspNetCore.Mvc;
using VHouse.Application.Commands;
using VHouse.Application.Queries;
using VHouse.Domain.Enums;
using VHouse.Domain.Interfaces;
using VHouse.Domain.ValueObjects;

namespace VHouse.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AIController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IAIService _aiService;

    public AIController(IMediator mediator, IAIService aiService)
    {
        _mediator = mediator;
        _aiService = aiService;
    }

    [HttpPost("generate-description")]
    public async Task<IActionResult> GenerateProductDescription([FromBody] GenerateDescriptionRequest request)
    {
        var command = new GenerateProductDescriptionCommand(
            request.ProductName,
            request.Price,
            request.Category,
            request.PreferredProvider);

        var result = await _mediator.Send(command);

        if (!result.IsSuccessful)
        {
            return BadRequest(new { error = result.ErrorMessage });
        }

        return Ok(result);
    }

    [HttpPost("generate-image")]
    public async Task<IActionResult> GenerateImage([FromBody] GenerateImageRequest request)
    {
        var command = new GenerateImageCommand(
            request.Prompt,
            request.Style,
            request.PreferredProvider);

        var result = await _mediator.Send(command);

        if (!result.IsSuccessful)
        {
            return BadRequest(new { error = result.ErrorMessage });
        }

        return Ok(result);
    }

    [HttpGet("health")]
    public async Task<IActionResult> GetHealthStatus()
    {
        var query = new GetAIHealthStatusQuery();
        var result = await _mediator.Send(query);

        return Ok(result);
    }

    [HttpPost("process-enhanced-order")]
    public Task<IActionResult> ProcessEnhancedOrder([FromBody] ProcessEnhancedOrderRequest request)
    {
        // Implementar usando DirectAI injection en lugar de MediatR para demo rápido
        // En producción, crear Commands/Handlers específicos
        return Task.FromResult<IActionResult>(BadRequest(new { error = "Endpoint pendiente - usar AIService directamente" }));
    }

    [HttpPost("validate-availability")]
    public Task<IActionResult> ValidateAvailability([FromBody] ValidateAvailabilityRequest request)
    {
        return Task.FromResult<IActionResult>(BadRequest(new { error = "Endpoint pendiente - usar AIService directamente" }));
    }

    [HttpPost("generate-alternatives")]
    public Task<IActionResult> GenerateAlternatives([FromBody] GenerateAlternativesRequest request)
    {
        return Task.FromResult<IActionResult>(BadRequest(new { error = "Endpoint pendiente - usar AIService directamente" }));
    }

    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] ChatRequest request)
    {
        try
        {
            var aiRequest = new AIRequest
            {
                Prompt = request.UserMessage,
                SystemMessage = request.SystemMessage ?? "Eres un asistente especializado en productos veganos.",
                PreferredProvider = AIProvider.Claude,
                MaxTokens = 500,
                Temperature = 0.7
            };

            var response = await _aiService.GenerateTextAsync(aiRequest);
            
            if (response.IsSuccessful)
            {
                return Ok(new ChatResponse(
                    response.Content,
                    true,
                    response.UsedProvider.ToString()
                ));
            }
            else
            {
                return Ok(new ChatResponse(
                    "🌱 Disculpa, tengo problemas técnicos momentáneos. ¿Puedo ayudarte con información básica sobre nuestros productos veganos?",
                    false,
                    null,
                    response.ErrorMessage
                ));
            }
        }
        catch (Exception ex)
        {
            return Ok(new ChatResponse(
                "🌱 Lo siento, hay un problema temporal. ¿Te interesa algún producto específico?",
                false,
                null,
                ex.Message
            ));
        }
    }
}

public record GenerateDescriptionRequest(
    string ProductName,
    decimal Price,
    string? Category = null,
    AIProvider? PreferredProvider = null);

public record GenerateImageRequest(
    string Prompt,
    string? Style = null,
    AIProvider? PreferredProvider = null);

public record ProcessEnhancedOrderRequest(
    string CatalogJson,
    string CustomerInput);

public record ValidateAvailabilityRequest(
    List<object> OrderItems,
    string Context);

public record GenerateAlternativesRequest(
    List<int> UnavailableProductIds,
    string AvailableProductsJson);

public record ChatRequest(
    string UserMessage,
    string? SystemMessage = null);

public record ChatResponse(
    string Content,
    bool IsSuccessful,
    string? Provider = null,
    string? Error = null);