using MediatR;
using VHouse.Application.DTOs;
using VHouse.Domain.Interfaces;

namespace VHouse.Application.Commands;

public record UpdateProductCommand(
    int Id,
    string ProductName,
    string Emoji,
    decimal PriceCost,
    decimal PriceRetail,
    decimal PriceSuggested,
    decimal PricePublic,
    string Description,
    int StockQuantity,
    bool IsActive
) : IRequest<ProductDto>;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProductCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ProductDto> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.Id);

        if (product == null)
        {
            throw new KeyNotFoundException($"Product with ID {request.Id} not found");
        }

        product.ProductName = request.ProductName;
        product.Emoji = request.Emoji;
        product.PriceCost = request.PriceCost;
        product.PriceRetail = request.PriceRetail;
        product.PriceSuggested = request.PriceSuggested;
        product.PricePublic = request.PricePublic;
        product.Description = request.Description;
        product.StockQuantity = request.StockQuantity;
        product.IsActive = request.IsActive;

        _unitOfWork.Products.Update(product);
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