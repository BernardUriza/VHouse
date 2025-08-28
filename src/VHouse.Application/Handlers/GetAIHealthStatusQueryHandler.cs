using MediatR;
using VHouse.Application.DTOs;
using VHouse.Application.Queries;
using VHouse.Domain.Interfaces;

namespace VHouse.Application.Handlers;

public class GetAIHealthStatusQueryHandler : IRequestHandler<GetAIHealthStatusQuery, AIHealthStatusDto>
{
    private readonly IAIService _aiService;

    public GetAIHealthStatusQueryHandler(IAIService aiService)
    {
        _aiService = aiService;
    }

    public async Task<AIHealthStatusDto> Handle(GetAIHealthStatusQuery request, CancellationToken cancellationToken)
    {
        var healthStatus = await _aiService.GetHealthStatusAsync();

        return new AIHealthStatusDto
        {
            ServiceStatus = healthStatus.ServiceStatus,
            RecommendedProvider = healthStatus.RecommendedProvider,
            FallbackAvailable = healthStatus.FallbackAvailable
        };
    }
}