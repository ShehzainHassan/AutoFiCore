using AutoFiCore.Models;
using FluentValidation;

namespace AutoFiCore.Validator
{
    /// <summary>
    /// Provides validation rules for the <see cref="ContactInfo"/> model.
    /// </summary>
    public class ContactInfoValidator : AbstractValidator<ContactInfo>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContactInfoValidator"/> class
        /// and defines validation rules for contact information fields.
        /// </summary>
        public ContactInfoValidator()
        {
            RuleFor(x => x.FirstName).NotEmpty().WithMessage("First name is required.");
            RuleFor(x => x.LastName).NotEmpty().WithMessage("Last name is required.");
            RuleFor(x => x.SelectedOption).NotEmpty().WithMessage("Selected option is required.");
            RuleFor(x => x.VehicleName).NotEmpty().WithMessage("Vehicle name is required.");
            RuleFor(x => x.PostCode).NotEmpty().WithMessage("Post code is required.");
            RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required.").EmailAddress().WithMessage("Invalid email format.");
            RuleFor(x => x.PhoneNumber).NotEmpty().WithMessage("Phone number is required.");
            RuleFor(x => x.PreferredContactMethod).NotEmpty().WithMessage("Preferred contact method is required.");
        }
    }
}