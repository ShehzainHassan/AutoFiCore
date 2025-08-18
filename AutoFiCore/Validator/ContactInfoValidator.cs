using AutoFiCore.Models;
using FluentValidation;

namespace AutoFiCore.Validator
{
    public class ContactInfoValidator : AbstractValidator<ContactInfo>
    {
        public ContactInfoValidator()
        {
            RuleFor(x => x.FirstName).NotEmpty();
            RuleFor(x => x.LastName).NotEmpty();
            RuleFor(x => x.SelectedOption).NotEmpty();
            RuleFor(x => x.VehicleName).NotEmpty();
            RuleFor(x => x.PostCode).NotEmpty();
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.PhoneNumber).NotEmpty();
            RuleFor(x => x.PreferredContactMethod).NotEmpty();
        }
    }

}
