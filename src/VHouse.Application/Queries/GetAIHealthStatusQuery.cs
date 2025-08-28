using MediatR;
using VHouse.Application.DTOs;

namespace VHouse.Application.Queries;

public record GetAIHealthStatusQuery : IRequest<AIHealthStatusDto>;