using FluentValidation;
using VHouse.Application.Commands;

namespace VHouse.Application.Validators;

public class RegisterConsignmentSaleCommandValidator : AbstractValidator<RegisterConsignmentSaleCommand>
{
    public RegisterConsignmentSaleCommandValidator()
    {
        RuleFor(x => x.ConsignmentId)
            .GreaterThan(0).WithMessage("Consignment ID must be greater than 0");

        RuleFor(x => x.ConsignmentItemId)
            .GreaterThan(0).WithMessage("Consignment item ID must be greater than 0");

        RuleFor(x => x.QuantitySold)
            .GreaterThan(0).WithMessage("Quantity sold must be greater than 0");

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0).WithMessage("Unit price must be greater than 0");

        RuleFor(x => x.SaleReference)
            .MaximumLength(200).WithMessage("Sale reference cannot exceed 200 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters");
    }
}
