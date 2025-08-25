using FluentValidation;
using GloboClima.API.DTOs;

namespace GloboClima.API.Validators
{
    public class FavoriteInputValidator : AbstractValidator<FavoriteInput>
    {
        public FavoriteInputValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

            RuleFor(x => x.Type)
                .NotEmpty().WithMessage("Type is required")
                .Must(BeValidType).WithMessage("Type must be either 'city' or 'country'");
        }

        private bool BeValidType(string type)
        {
            return type == "city" || type == "country";
        }
    }
}
