using MediatR;
using VHouse.Application.DTOs;
using VHouse.Domain.Enums;

namespace VHouse.Application.Commands;

public record GenerateImageCommand(
    string Prompt,
    string? Style = null,
    AIProvider? PreferredProvider = null) : IRequest<ImageGenerationDto>;