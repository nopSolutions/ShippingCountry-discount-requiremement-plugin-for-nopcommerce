using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public async Task<DiscountRequirementValidationResult> CheckRequirementAsync(DiscountRequirementValidationRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            //invalid by default
            var result = new DiscountRequirementValidationResult();

            if (request.Customer == null)
                return result;

            if (!request.Customer.ShippingAddressId.HasValue)
                return result;

            var shippingCountryId = await _settingService.GetSettingByKeyAsync<int>(string.Format(DiscountRequirementDefaults.SETTINGS_KEY, request.DiscountRequirementId));
            if (shippingCountryId == 0)
                return result;

            var customerShippingAddress = await _addressService.GetAddressByIdAsync(request.Customer.ShippingAddressId.Value);
            var customerShippingCountryId = customerShippingAddress?.CountryId != null
                ? (await _countryService.GetCountryByIdAsync(customerShippingAddress.CountryId.Value))?.Id
                : 0;

            result.IsValid = customerShippingCountryId == shippingCountryId;

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
                new { discountId = discountId, discountRequirementId = discountRequirementId }, _webHelper.GetCurrentRequestProtocol());
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task InstallAsync()
        {
            //locales
            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Plugins.DiscountRules.ShippingCountry.Fields.SelectCountry"] = "Select country",
                ["Plugins.DiscountRules.ShippingCountry.Fields.Country"] = "Shipping country",
                ["Plugins.DiscountRules.ShippingCountry.Fields.Country.Hint"] = "Select required shipping country.",
                ["Plugins.DiscountRules.ShippingCountry.Fields.DiscountId.Required"] = "Discount is required",
                ["Plugins.DiscountRules.ShippingCountry.Fields.CountryId.Required"] = "Country is required"
            });

            await base.InstallAsync();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task UninstallAsync()
        {
            //discount requirements
            var discountRequirements = (await _discountService.GetAllDiscountRequirementsAsync())
                .Where(discountRequirement => discountRequirement.DiscountRequirementRuleSystemName == DiscountRequirementDefaults.SYSTEM_NAME);
            foreach (var discountRequirement in discountRequirements)
            {
                await _discountService.DeleteDiscountRequirementAsync(discountRequirement, false);
            }

            //locales
            await _localizationService.DeleteLocaleResourcesAsync("Plugins.DiscountRules.ShippingCountry");

            await base.UninstallAsync();
        }

        #endregion
    }
}