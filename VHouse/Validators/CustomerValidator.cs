using FluentValidation;
using VHouse.Classes;

namespace VHouse.Validators
{
    public class CustomerValidator : AbstractValidator<Customer>
    {
        public CustomerValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty()
                .WithMessage("Full name is required.")
                .Length(2, 100)
                .WithMessage("Full name must be between 2 and 100 characters.");

            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email is required.")
                .EmailAddress()
                .WithMessage("Invalid email format.")
                .MaximumLength(255)
                .WithMessage("Email must not exceed 255 characters.");

            RuleFor(x => x.Phone)
                .NotEmpty()
                .WithMessage("Phone number is required.")
                .Matches(@"^[\+]?[1-9][\d]{0,15}$")
                .WithMessage("Invalid phone number format.");

            RuleFor(x => x.Address)
                .NotEmpty()
                .WithMessage("Address is required.")
                .Length(5, 255)
                .WithMessage("Address must be between 5 and 255 characters.");
        }
    }
}