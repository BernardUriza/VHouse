using FluentValidation;
using VHouse.Application.Commands;
using VHouse.Domain.Interfaces;

namespace VHouse.Application.Validators;

public class CreateConsignmentCommandValidator : AbstractValidator<CreateConsignmentCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateConsignmentCommandValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

        RuleFor(x => x.ClientTenantId)
            .GreaterThan(0).WithMessage("Client tenant ID must be greater than 0")
            .MustAsync(ClientTenantExists).WithMessage("Client tenant not found");

        RuleFor(x => x.StorePercentage)
            .GreaterThanOrEqualTo(0).WithMessage("Store percentage must be >= 0")
            .LessThanOrEqualTo(100).WithMessage("Store percentage must be <= 100");

        RuleFor(x => x.BernardPercentage)
            .GreaterThanOrEqualTo(0).WithMessage("Bernard percentage must be >= 0")
            .LessThanOrEqualTo(100).WithMessage("Bernard percentage must be <= 100");

        RuleFor(x => x)
            .Must(PercentagesSum100)
            .WithMessage("Store and Bernard percentages must sum to 100%");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Must provide at least one item")
            .Must(items => items.All(i => i.QuantityConsigned > 0))
            .WithMessage("All items must have positive quantities")
            .Must(items => items.All(i => i.CostPrice > 0))
            .WithMessage("All items must have positive cost prices")
            .Must(items => items.All(i => i.RetailPrice > 0))
            .WithMessage("All items must have positive retail prices")
            .Must(items => items.All(i => i.RetailPrice >= i.CostPrice))
            .WithMessage("Retail price must be greater than or equal to cost price");

        RuleFor(x => x.ExpiryDate)
            .Must((command, expiryDate) => !expiryDate.HasValue || expiryDate.Value > DateTime.UtcNow)
            .WithMessage("Expiry date must be in the future");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters");

        RuleFor(x => x.Terms)
            .NotEmpty().WithMessage("Terms are required")
            .MaximumLength(1000).WithMessage("Terms cannot exceed 1000 characters");
    }

    private bool PercentagesSum100(CreateConsignmentCommand command)
    {
        return Math.Abs(command.StorePercentage + command.BernardPercentage - 100) < 0.01m;
    }

    private async Task<bool> ClientTenantExists(int clientTenantId, CancellationToken cancellationToken)
    {
        var tenant = await _unitOfWork.ClientTenants.GetByIdAsync(clientTenantId);
        return tenant != null;
    }
}
