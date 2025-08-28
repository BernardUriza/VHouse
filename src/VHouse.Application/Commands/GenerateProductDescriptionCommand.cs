using MediatR;
using VHouse.Application.DTOs;
using VHouse.Domain.Enums;

namespace VHouse.Application.Commands;

public record GenerateProductDescriptionCommand(
    string ProductName,
    decimal Price,
    string? Category = null,
    AIProvider? PreferredProvider = null) : IRequest<AIResponseDto>;