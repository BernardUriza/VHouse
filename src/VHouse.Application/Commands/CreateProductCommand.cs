using MediatR;
using VHouse.Application.DTOs;
using VHouse.Domain.Entities;
using VHouse.Domain.Interfaces;

namespace VHouse.Application.Commands;

public record CreateProductCommand(
    string ProductName,
    string Emoji,
    decimal PriceCost,
    decimal PriceRetail,
    decimal PriceSuggested,
    decimal PricePublic,
    string Description,
    int StockQuantity,
    bool IsVegan = true
) : IRequest<ProductDto>;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            ProductName = request.ProductName,
            Emoji = request.Emoji,
            PriceCost = request.PriceCost,
            PriceRetail = request.PriceRetail,
            PriceSuggested = request.PriceSuggested,
            PricePublic = request.PricePublic,
            Description = request.Description,
            StockQuantity = request.StockQuantity,
            IsVegan = request.IsVegan,
            IsActive = true
        };

        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

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