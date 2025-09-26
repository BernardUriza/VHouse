using MediatR;
using VHouse.Application.DTOs;
using VHouse.Domain.Entities;
using VHouse.Domain.Interfaces;

namespace VHouse.Application.Commands;

public record CreatePriceListCommand(
    string Name,
    string Description,
    bool IsDefault = false
) : IRequest<PriceListDto>;

public class CreatePriceListCommandHandler : IRequestHandler<CreatePriceListCommand, PriceListDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreatePriceListCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PriceListDto> Handle(CreatePriceListCommand request, CancellationToken cancellationToken)
    {
        var priceList = new PriceList
        {
            Name = request.Name,
            Description = request.Description,
            IsDefault = request.IsDefault,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.PriceLists.AddAsync(priceList);
        await _unitOfWork.SaveChangesAsync();

        return new PriceListDto
        {
            Id = priceList.Id,
            Name = priceList.Name,
            Description = priceList.Description,
            IsDefault = priceList.IsDefault,
            IsActive = priceList.IsActive,
            CreatedAt = priceList.CreatedAt,
            UpdatedAt = priceList.UpdatedAt
        };
    }
}

public record AddPriceListItemCommand(
    int PriceListId,
    int ProductId,
    decimal CustomPrice,
    decimal DiscountPercentage = 0,
    int MinOrderQuantity = 1
) : IRequest<PriceListItemDto>;

public class AddPriceListItemCommandHandler : IRequestHandler<AddPriceListItemCommand, PriceListItemDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public AddPriceListItemCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PriceListItemDto> Handle(AddPriceListItemCommand request, CancellationToken cancellationToken)
    {
        var priceListItem = new PriceListItem
        {
            PriceListId = request.PriceListId,
            ProductId = request.ProductId,
            CustomPrice = request.CustomPrice,
            DiscountPercentage = request.DiscountPercentage,
            MinOrderQuantity = request.MinOrderQuantity,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.PriceListItems.AddAsync(priceListItem);
        await _unitOfWork.SaveChangesAsync();

        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId);

        return new PriceListItemDto
        {
            Id = priceListItem.Id,
            PriceListId = priceListItem.PriceListId,
            ProductId = priceListItem.ProductId,
            ProductName = product?.ProductName ?? string.Empty,
            CustomPrice = priceListItem.CustomPrice,
            DiscountPercentage = priceListItem.DiscountPercentage,
            MinOrderQuantity = priceListItem.MinOrderQuantity,
            IsActive = priceListItem.IsActive
        };
    }
}

public record AssignPriceListToClientCommand(
    int ClientTenantId,
    int PriceListId
) : IRequest<ClientTenantPriceListDto>;

public class AssignPriceListToClientCommandHandler : IRequestHandler<AssignPriceListToClientCommand, ClientTenantPriceListDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public AssignPriceListToClientCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ClientTenantPriceListDto> Handle(AssignPriceListToClientCommand request, CancellationToken cancellationToken)
    {
        var assignment = new ClientTenantPriceList
        {
            ClientTenantId = request.ClientTenantId,
            PriceListId = request.PriceListId,
            AssignedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _unitOfWork.ClientTenantPriceLists.AddAsync(assignment);
        await _unitOfWork.SaveChangesAsync();

        var client = await _unitOfWork.ClientTenants.GetByIdAsync(request.ClientTenantId);
        var priceList = await _unitOfWork.PriceLists.GetByIdAsync(request.PriceListId);

        return new ClientTenantPriceListDto
        {
            Id = assignment.Id,
            ClientTenantId = assignment.ClientTenantId,
            ClientTenantName = client?.TenantName ?? string.Empty,
            PriceListId = assignment.PriceListId,
            PriceListName = priceList?.Name ?? string.Empty,
            AssignedAt = assignment.AssignedAt,
            IsActive = assignment.IsActive
        };
    }
}