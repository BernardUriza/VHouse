using FluentValidation;
using VHouse.Application.Commands;

namespace VHouse.Application.Validators;

public class SettleConsignmentCommandValidator : AbstractValidator<SettleConsignmentCommand>
{
    public SettleConsignmentCommandValidator()
    {
        RuleFor(x => x.ConsignmentId)
            .GreaterThan(0).WithMessage("Consignment ID must be greater than 0");

        RuleFor(x => x.SettlementNotes)
            .MaximumLength(1000).WithMessage("Settlement notes cannot exceed 1000 characters");
    }
}
