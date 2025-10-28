using MediatR;
using VHouse.Domain.Entities;
using VHouse.Domain.Exceptions;
using VHouse.Domain.Interfaces;

namespace VHouse.Application.Commands;

public record SettleConsignmentCommand(
    int ConsignmentId,
    string? SettlementNotes = null
) : IRequest<bool>;

public class SettleConsignmentCommandHandler : IRequestHandler<SettleConsignmentCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public SettleConsignmentCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(SettleConsignmentCommand request, CancellationToken cancellationToken)
    {
        var consignment = await _unitOfWork.Consignments.GetByIdForSettlementAsync(request.ConsignmentId);

        if (consignment == null)
            throw new ConsignmentNotFoundException(request.ConsignmentId);

        if (consignment.Status == ConsignmentStatus.Settled)
            throw new InvalidConsignmentOperationException("Consignment is already settled");

        // Update status
        consignment.Status = ConsignmentStatus.Settled;
        consignment.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.SettlementNotes))
        {
            consignment.Notes = string.IsNullOrWhiteSpace(consignment.Notes)
                ? $"Liquidación: {request.SettlementNotes}"
                : $"{consignment.Notes}\n\nLiquidación: {request.SettlementNotes}";
        }

        await _unitOfWork.Consignments.UpdateAsync(consignment);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}
