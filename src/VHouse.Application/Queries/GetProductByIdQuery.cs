using MediatR;
using VHouse.Application.DTOs;

namespace VHouse.Application.Queries;

public record GetProductByIdQuery(int Id) : IRequest<ProductDto?>;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    private readonly VHouse.Domain.Interfaces.IUnitOfWork _unitOfWork;

    public GetProductByIdQueryHandler(VHouse.Domain.Interfaces.IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.Id);

        if (product == null)
        {
            return null;
        }

        return new ProductDto
        {
            Id = product.Id,
            ProductName = product.ProductName,
            Emoji = product.Emoji,
            PriceCost = product.PriceCost,
            PriceRetail = product.PriceRetail,
            PriceSuggested = product.PriceSuggested,
            PricePublic = product.PricePublic,
            Description = product.Description,
            StockQuantity = product.StockQuantity,
            IsVegan = product.IsVegan,
            IsActive = product.IsActive
        };
    }
}
