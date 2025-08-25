using FluentValidation;
using VHouse;

namespace VHouse.Validators
{
    public class ProductValidator : AbstractValidator<Product>
    {
        public ProductValidator()
        {
            RuleFor(x => x.ProductName)
                .NotEmpty()
                .WithMessage("Product name is required.")
                .Length(2, 200)
                .WithMessage("Product name must be between 2 and 200 characters.");

            RuleFor(x => x.PriceCost)
                .GreaterThan(0)
                .WithMessage("Cost price must be greater than 0.");

            RuleFor(x => x.PriceRetail)
                .GreaterThan(0)
                .WithMessage("Retail price must be greater than 0.")
                .GreaterThanOrEqualTo(x => x.PriceCost)
                .WithMessage("Retail price must be greater than or equal to cost price.");

            RuleFor(x => x.PriceSuggested)
                .GreaterThan(0)
                .WithMessage("Suggested price must be greater than 0.")
                .GreaterThanOrEqualTo(x => x.PriceCost)
                .WithMessage("Suggested price must be greater than or equal to cost price.");

            RuleFor(x => x.PricePublic)
                .GreaterThan(0)
                .WithMessage("Public price must be greater than 0.")
                .GreaterThanOrEqualTo(x => x.PriceCost)
                .WithMessage("Public price must be greater than or equal to cost price.");

            RuleFor(x => x.Emoji)
                .NotEmpty()
                .WithMessage("Product emoji is required.")
                .Length(1, 10)
                .WithMessage("Emoji must be between 1 and 10 characters.");

            RuleFor(x => x.Score)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Score cannot be negative.");
        }
    }
}