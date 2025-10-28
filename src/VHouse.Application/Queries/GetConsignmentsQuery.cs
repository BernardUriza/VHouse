using MediatR;
using VHouse.Application.Common;
using VHouse.Application.DTOs;
using VHouse.Domain.Interfaces;

namespace VHouse.Application.Queries;

public record GetConsignmentsQuery : IRequest<List<ConsignmentDto>>;

public class GetConsignmentsQueryHandler : IRequestHandler<GetConsignmentsQuery, List<ConsignmentDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetConsignmentsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<ConsignmentDto>> Handle(GetConsignmentsQuery request, CancellationToken cancellationToken)
    {
        var consignments = await _unitOfWork.Consignments.GetAllAsync();
        return consignments.Select(c => ConsignmentMapper.ToDto(c, includeDetails: true)).ToList();
    }
}
