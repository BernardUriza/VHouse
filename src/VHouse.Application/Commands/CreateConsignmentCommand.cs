using MediatR;
using VHouse.Application.DTOs;
using VHouse.Domain.Entities;
using VHouse.Domain.Interfaces;

namespace VHouse.Application.Commands;

public record CreateConsignmentItemRequest(
    int ProductId,
    int QuantityConsigned,
    decimal CostPrice,
    decimal RetailPrice,
    string? Notes = null
);

public record CreateConsignmentCommand(
    int ClientTenantId,
    DateTime? ExpiryDate,
    string Notes,
    string Terms,
    decimal StorePercentage,
    decimal BernardPercentage,
    List<CreateConsignmentItemRequest> Items
) : IRequest<int>; // Return only ID, not full DTO

public class CreateConsignmentCommandHandler : IRequestHandler<CreateConsignmentCommand, int>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateConsignmentCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(CreateConsignmentCommand request, CancellationToken cancellationToken)
    {
        // Validations
        if (request.Items == null || !request.Items.Any())
            throw new InvalidOperationException("Debe agregar al menos un producto");

        if (request.Items.Any(i => i.QuantityConsigned <= 0))
            throw new InvalidOperationException("Cantidad debe ser mayor a 0");

        // Generate consignment number
        var year = DateTime.UtcNow.Year;
        var count = await _unitOfWork.Consignments.GetCountAsync();
        var consignmentNumber = $"CONS-{year}-{(count + 1):D3}";

        // Calculate totals
        decimal totalCost = request.Items.Sum(i => i.CostPrice * i.QuantityConsigned);
        decimal totalRetail = request.Items.Sum(i => i.RetailPrice * i.QuantityConsigned);

        var consignment = new Consignment
        {
            ConsignmentNumber = consignmentNumber,
            ClientTenantId = request.ClientTenantId,
            ConsignmentDate = DateTime.UtcNow,
            ExpiryDate = request.ExpiryDate,
            Status = ConsignmentStatus.Active,
            Notes = request.Notes,
            Terms = request.Terms,
            TotalValueAtCost = totalCost,
            TotalValueAtRetail = totalRetail,
            StorePercentage = request.StorePercentage,
            BernardPercentage = request.BernardPercentage,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _unitOfWork.Consignments.AddAsync(consignment);

        // Add items
        var items = request.Items.Select(itemRequest => new ConsignmentItem
        {
            ConsignmentId = created.Id,
            ProductId = itemRequest.ProductId,
            QuantityConsigned = itemRequest.QuantityConsigned,
            QuantitySold = 0,
            QuantityReturned = 0,
            CostPrice = itemRequest.CostPrice,
            RetailPrice = itemRequest.RetailPrice,
            Notes = itemRequest.Notes,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        await _unitOfWork.Consignments.AddConsignmentItemsAsync(items);
        await _unitOfWork.SaveChangesAsync();

        return created.Id;
    }
}
