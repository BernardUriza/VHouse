using MediatR;
using VHouse.Application.DTOs;

namespace VHouse.Application.Queries;

public record GetProductsQuery : IRequest<IEnumerable<ProductDto>>;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, IEnumerable<ProductDto>>
{
    private readonly VHouse.Domain.Interfaces.IUnitOfWork _unitOfWork;

    public GetProductsQueryHandler(VHouse.Domain.Interfaces.IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _unitOfWork.Products.GetAllAsync();
        
        return products.Select(p => new ProductDto
        {
            Id = p.Id,
            ProductName = p.ProductName,
            Emoji = p.Emoji,
            PriceCost = p.PriceCost,
            PriceRetail = p.PriceRetail,
            PriceSuggested = p.PriceSuggested,
            PricePublic = p.PricePublic,
            Description = p.Description,
            StockQuantity = p.StockQuantity,
            IsVegan = p.IsVegan,
            IsActive = p.IsActive
        });
    }
}