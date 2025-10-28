using MediatR;
using VHouse.Application.Common;
using VHouse.Application.DTOs;
using VHouse.Domain.Interfaces;

namespace VHouse.Application.Queries;

public record GetConsignmentByIdQuery(int Id) : IRequest<ConsignmentDto?>;

public class GetConsignmentByIdQueryHandler : IRequestHandler<GetConsignmentByIdQuery, ConsignmentDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetConsignmentByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ConsignmentDto?> Handle(GetConsignmentByIdQuery request, CancellationToken cancellationToken)
    {
        var consignment = await _unitOfWork.Consignments.GetByIdWithAllDetailsAsync(request.Id);

        if (consignment == null)
            return null;

        return ConsignmentMapper.ToDto(consignment, includeDetails: true);
    }
}
