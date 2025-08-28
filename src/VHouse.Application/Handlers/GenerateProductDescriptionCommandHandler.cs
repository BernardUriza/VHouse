using MediatR;
using VHouse.Application.Commands;
using VHouse.Application.DTOs;
using VHouse.Domain.Interfaces;
using VHouse.Domain.ValueObjects;
using VHouse.Domain.Enums;

namespace VHouse.Application.Handlers;

public class GenerateProductDescriptionCommandHandler : IRequestHandler<GenerateProductDescriptionCommand, AIResponseDto>
{
    private readonly IAIService _aiService;

    public GenerateProductDescriptionCommandHandler(IAIService aiService)
    {
        _aiService = aiService;
    }

    public async Task<AIResponseDto> Handle(GenerateProductDescriptionCommand request, CancellationToken cancellationToken)
    {
        var prompt = $@"Genera una descripción atractiva y profesional para este producto vegano:

Producto: {request.ProductName}
Precio: ${request.Price:F2}
{(string.IsNullOrEmpty(request.Category) ? "" : $"Categoría: {request.Category}")}

La descripción debe:
- Destacar los beneficios veganos y saludables
- Ser atractiva para clientes conscientes de la salud
- Mencionar características especiales del producto
- Ser concisa pero informativa (máximo 3 párrafos)
- Usar un tono profesional pero amigable

Responde únicamente con la descripción del producto, sin formato adicional.";

        var aiRequest = new AIRequest
        {
            Prompt = prompt,
            MaxTokens = 300,
            Temperature = 0.7,
            PreferredProvider = request.PreferredProvider ?? AIProvider.Claude
        };

        var response = await _aiService.GenerateTextAsync(aiRequest);

        return new AIResponseDto
        {
            Content = response.Content,
            UsedProvider = response.UsedProvider,
            UsedModel = response.UsedModel.ToString(),
            IsSuccessful = response.IsSuccessful,
            ErrorMessage = response.ErrorMessage,
            TokensUsed = response.TokensUsed,
            ResponseTimeMs = response.ResponseTime.TotalMilliseconds
        };
    }
}