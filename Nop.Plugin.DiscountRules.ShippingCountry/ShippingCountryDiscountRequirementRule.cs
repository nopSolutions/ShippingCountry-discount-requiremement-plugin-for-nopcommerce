using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Plugins;

namespace Nop.Plugin.DiscountRules.ShippingCountry
{
    public partial class ShippingCountryDiscountRequirementRule : BasePlugin, IDiscountRequirementRule
    {
        #region Fields

        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly IDiscountService _discountService;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IWebHelper _webHelper;

        #endregion

        #region Ctor

        public ShippingCountryDiscountRequirementRule(IActionContextAccessor actionContextAccessor,
            IAddressService addressService,
            ICountryService countryService,
            IDiscountService discountService,
            ILocalizationService localizationService,
            ISettingService settingService,
            IUrlHelperFactory urlHelperFactory,
            IWebHelper webHelper)
        {
            _actionContextAccessor = actionContextAccessor;
            _addressService = addressService;
            _countryService = countryService;
            _discountService = discountService;
            _localizationService = localizationService;
            _settingService = settingService;
            _urlHelperFactory = urlHelperFactory;
            _webHelper = webHelper;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Check discount requirement
        /// </summary>
        /// <param name="request">Object that contains all information required to check the requirement (Current customer, discount, etc)</param>
        /// <returns>Result</returns>
        public DiscountRequirementValidationResult CheckRequirement(DiscountRequirementValidationRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            //invalid by default
            var result = new DiscountRequirementValidationResult();

            if (request.Customer == null)
                return result;

            if (!request.Customer.ShippingAddressId.HasValue)
                return result;

            var shippingCountryId = _settingService.GetSettingByKey<int>(string.Format(DiscountRequirementDefaults.SETTINGS_KEY, request.DiscountRequirementId));
            if (shippingCountryId == 0)
                return result;

            var customerShippingAddress = _addressService.GetAddressById(request.Customer.ShippingAddressId.Value);
            if (customerShippingAddress?.CountryId == null || customerShippingAddress.CountryId == 0)
                return result;

            var customerShippingCountry = _countryService.GetCountryById(customerShippingAddress.CountryId.Value);
            if (customerShippingCountry == null)
                return result;

            result.IsValid = customerShippingCountry.Id == shippingCountryId;

            return result;
        }

        /// <summary>
        /// Get URL for rule configuration
        /// </summary>
        /// <param name="discountId">Discount identifier</param>
        /// <param name="discountRequirementId">Discount requirement identifier (if editing)</param>
        /// <returns>URL</returns>
        public string GetConfigurationUrl(int discountId, int? discountRequirementId)
        {
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

            return urlHelper.Action("Configure", "DiscountRulesShippingCountry",
                new { discountId = discountId, discountRequirementId = discountRequirementId }, _webHelper.CurrentRequestProtocol);
        }

        public override void Install()
        {
            //locales
            _localizationService.AddPluginLocaleResource(new Dictionary<string, string>
            {
                ["Plugins.DiscountRules.ShippingCountry.Fields.SelectCountry"] = "Select country",
                ["Plugins.DiscountRules.ShippingCountry.Fields.Country"] = "Shipping country",
                ["Plugins.DiscountRules.ShippingCountry.Fields.Country.Hint"] = "Select required shipping country.",
                ["Plugins.DiscountRules.ShippingCountry.Fields.DiscountId.Required"] = "Discount is required",
                ["Plugins.DiscountRules.ShippingCountry.Fields.CountryId.Required"] = "Country is required"
            });

            base.Install();
        }

        public override void Uninstall()
        {
            //delete discount requirements is exist
            var discountRequirements = _discountService.GetAllDiscountRequirements()
                .Where(discountRequirement => discountRequirement.DiscountRequirementRuleSystemName == DiscountRequirementDefaults.SYSTEM_NAME);
            foreach (var discountRequirement in discountRequirements)
            {
                _discountService.DeleteDiscountRequirement(discountRequirement, false);
            }

            //locales
            _localizationService.DeletePluginLocaleResources("Plugins.DiscountRules.ShippingCountry");

            base.Uninstall();
        }

        #endregion
    }
}