using FluentValidation;
using Nop.Plugin.DiscountRules.ShippingCountry.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.DiscountRules.ShippingCountry.Validators
{
    /// <summary>
    /// Represents an <see cref="RequirementModel"/> validator.
    /// </summary>
    public class RequirementModelValidator : BaseNopValidator<RequirementModel>
    {
        public RequirementModelValidator(ILocalizationService localizationService)
        {
            RuleFor(model => model.DiscountId)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("Plugins.DiscountRules.ShippingCountry.Fields.DiscountId.Required"));
            RuleFor(model => model.CountryId)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("Plugins.DiscountRules.ShippingCountry.Fields.CountryId.Required"));
        }
    }
}
