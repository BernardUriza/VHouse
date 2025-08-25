using FluentValidation;
using VHouse.Classes;

namespace VHouse.Validators
{
    public class OrderValidator : AbstractValidator<Order>
    {
        public OrderValidator()
        {
            RuleFor(x => x.OrderDate)
                .NotEmpty()
                .WithMessage("Order date is required.")
                .LessThanOrEqualTo(DateTime.UtcNow)
                .WithMessage("Order date cannot be in the future.");

            RuleFor(x => x.DeliveryDate)
                .NotEmpty()
                .WithMessage("Delivery date is required.")
                .GreaterThanOrEqualTo(x => x.OrderDate)
                .WithMessage("Delivery date must be after or equal to order date.");

            RuleFor(x => x.PriceType)
                .NotEmpty()
                .WithMessage("Price type is required.")
                .Must(x => new[] { "Cost", "Retail", "Suggested", "Public" }.Contains(x))
                .WithMessage("Price type must be one of: Cost, Retail, Suggested, Public.");

            RuleFor(x => x.TotalAmount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Total amount cannot be negative.");

            RuleFor(x => x.Discount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Discount cannot be negative.")
                .LessThanOrEqualTo(x => x.TotalAmount)
                .WithMessage("Discount cannot be greater than total amount.");

            RuleFor(x => x.ShippingCost)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Shipping cost cannot be negative.");

            RuleFor(x => x.Items)
                .NotEmpty()
                .WithMessage("Order must contain at least one item.");

            RuleForEach(x => x.Items).SetValidator(new OrderItemValidator());
        }
    }

    public class OrderItemValidator : AbstractValidator<OrderItem>
    {
        public OrderItemValidator()
        {
            RuleFor(x => x.ProductId)
                .GreaterThan(0)
                .WithMessage("Valid product ID is required.");

            RuleFor(x => x.ProductName)
                .NotEmpty()
                .WithMessage("Product name is required.")
                .Length(2, 200)
                .WithMessage("Product name must be between 2 and 200 characters.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage("Quantity must be greater than 0.");

            RuleFor(x => x.Price)
                .GreaterThan(0)
                .WithMessage("Price must be greater than 0.");
        }
    }
}