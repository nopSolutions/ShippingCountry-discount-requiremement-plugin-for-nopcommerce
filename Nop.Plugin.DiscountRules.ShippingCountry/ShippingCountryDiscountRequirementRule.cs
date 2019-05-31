using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Services.Plugins;
using Nop.Services.Configuration;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Core;

namespace Nop.Plugin.DiscountRules.ShippingCountry
{
    public partial class ShippingCountryDiscountRequirementRule : BasePlugin, IDiscountRequirementRule
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IWebHelper _webHelper;

        #endregion

        #region Ctor

        public ShippingCountryDiscountRequirementRule(ISettingService settingService,
            ILocalizationService localizationService,
            IActionContextAccessor actionContextAccessor,
            IUrlHelperFactory urlHelperFactory,
            IWebHelper webHelper)
        {
            _settingService = settingService;
            _localizationService = localizationService;
            _actionContextAccessor = actionContextAccessor;
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

            if (request.Customer.ShippingAddress == null)
                return result;

            var shippingCountryId = _settingService.GetSettingByKey<int>($"DiscountRequirement.ShippingCountry-{request.DiscountRequirementId}");

            if (shippingCountryId == 0)
                return result;

            result.IsValid = request.Customer.ShippingAddress.CountryId == shippingCountryId;

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
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.ShippingCountry.Fields.SelectCountry", "Select country");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.ShippingCountry.Fields.Country", "Shipping country");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.ShippingCountry.Fields.Country.Hint", "Select required shipping country.");
            base.Install();
        }

        public override void Uninstall()
        {
            //locales
            _localizationService.DeletePluginLocaleResource("Plugins.DiscountRules.ShippingCountry.Fields.SelectCountry");
            _localizationService.DeletePluginLocaleResource("Plugins.DiscountRules.ShippingCountry.Fields.Country");
            _localizationService.DeletePluginLocaleResource("Plugins.DiscountRules.ShippingCountry.Fields.Country.Hint");
            base.Uninstall();
        }

        #endregion
    }
}