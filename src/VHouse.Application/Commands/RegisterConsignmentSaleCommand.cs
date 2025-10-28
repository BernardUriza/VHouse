using MediatR;
using VHouse.Application.DTOs;
using VHouse.Domain.Entities;
using VHouse.Domain.Exceptions;
using VHouse.Domain.Interfaces;

namespace VHouse.Application.Commands;

public record RegisterConsignmentSaleCommand(
    int ConsignmentId,
    int ConsignmentItemId,
    int QuantitySold,
    decimal UnitPrice,
    string? SaleReference = null,
    string? Notes = null
) : IRequest<int>; // Return sale ID

public class RegisterConsignmentSaleCommandHandler : IRequestHandler<RegisterConsignmentSaleCommand, int>
{
    private readonly IUnitOfWork _unitOfWork;

    public RegisterConsignmentSaleCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(RegisterConsignmentSaleCommand request, CancellationToken cancellationToken)
    {
        // Get consignment
        var consignment = await _unitOfWork.Consignments.GetByIdAsync(request.ConsignmentId);
        if (consignment == null)
            throw new ConsignmentNotFoundException(request.ConsignmentId);

        // Validate consignment status
        if (consignment.Status == ConsignmentStatus.Settled)
            throw new InvalidConsignmentOperationException("Cannot register sales on settled consignment");

        if (consignment.Status == ConsignmentStatus.Expired)
            throw new InvalidConsignmentOperationException("Cannot register sales on expired consignment");

        // Get item
        var item = consignment.ConsignmentItems.FirstOrDefault(i => i.Id == request.ConsignmentItemId);
        if (item == null)
            throw new ConsignmentItemNotFoundException(request.ConsignmentItemId);

        // Validate quantity
        if (request.QuantitySold <= 0)
            throw new InvalidConsignmentOperationException("Quantity sold must be greater than 0");

        if (item.QuantityAvailable < request.QuantitySold)
            throw new InvalidConsignmentOperationException(
                $"Not enough quantity available. Available: {item.QuantityAvailable}, Requested: {request.QuantitySold}");

        // Validate price
        if (request.UnitPrice <= 0)
            throw new InvalidConsignmentOperationException("Unit price must be greater than 0");

        // Calculate amounts
        decimal totalSaleAmount = request.UnitPrice * request.QuantitySold;
        decimal storeAmount = totalSaleAmount * (consignment.StorePercentage / 100);
        decimal bernardAmount = totalSaleAmount * (consignment.BernardPercentage / 100);

        // Create sale
        var sale = new ConsignmentSale
        {
            ConsignmentId = request.ConsignmentId,
            ConsignmentItemId = request.ConsignmentItemId,
            SaleDate = DateTime.UtcNow,
            QuantitySold = request.QuantitySold,
            UnitPrice = request.UnitPrice,
            TotalSaleAmount = totalSaleAmount,
            StoreAmount = storeAmount,
            BernardAmount = bernardAmount,
            SaleReference = request.SaleReference,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        var createdSale = await _unitOfWork.Consignments.AddConsignmentSaleAsync(sale);

        // Update item quantities
        item.QuantitySold += request.QuantitySold;

        // Update consignment totals
        consignment.TotalSold += totalSaleAmount;
        consignment.AmountDueToBernard += bernardAmount;
        consignment.AmountDueToStore += storeAmount;
        consignment.UpdatedAt = DateTime.UtcNow;

        // Update status if all items sold
        var allItemsSold = consignment.ConsignmentItems.All(i => i.QuantityAvailable == 0);
        if (allItemsSold && consignment.Status == ConsignmentStatus.Active)
        {
            consignment.Status = ConsignmentStatus.PartiallySettled;
        }

        await _unitOfWork.Consignments.UpdateAsync(consignment);
        await _unitOfWork.SaveChangesAsync();

        return createdSale.Id;
    }
}
