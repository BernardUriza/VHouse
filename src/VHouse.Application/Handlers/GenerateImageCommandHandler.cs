using MediatR;
using VHouse.Application.Commands;
using VHouse.Application.DTOs;
using VHouse.Domain.Interfaces;
using VHouse.Domain.ValueObjects;
using VHouse.Domain.Enums;

namespace VHouse.Application.Handlers;

public class GenerateImageCommandHandler : IRequestHandler<GenerateImageCommand, ImageGenerationDto>
{
    private readonly IAIService _aiService;

    public GenerateImageCommandHandler(IAIService aiService)
    {
        _aiService = aiService;
    }

    public async Task<ImageGenerationDto> Handle(GenerateImageCommand request, CancellationToken cancellationToken)
    {
        var enhancedPrompt = $@"{request.Prompt}

Estilo: Fotografía profesional de alimentos veganos
Iluminación: Natural y cálida
Composición: Limpia y minimalista
Colores: Vibrantes y naturales
Calidad: Alta resolución para uso comercial
{(string.IsNullOrEmpty(request.Style) ? "" : $"Estilo adicional: {request.Style}")}";

        var imageRequest = new ImageGenerationRequest
        {
            Prompt = enhancedPrompt,
            PreferredProvider = request.PreferredProvider ?? AIProvider.OpenAI,
            Style = request.Style ?? "natural"
        };

        var response = await _aiService.GenerateImageAsync(imageRequest);

        return new ImageGenerationDto
        {
            ImageData = response.ImageData,
            ImageUrl = response.ImageUrl,
            RevisedPrompt = response.RevisedPrompt,
            UsedProvider = response.UsedProvider,
            UsedModel = response.UsedModel.ToString(),
            IsSuccessful = response.IsSuccessful,
            ErrorMessage = response.ErrorMessage
        };
    }
}