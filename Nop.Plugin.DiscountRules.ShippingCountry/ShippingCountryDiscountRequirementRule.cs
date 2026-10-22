using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.DiscountRules.ShippingCountry;

public class ShippingCountryDiscountRequirementRule : BasePlugin, IDiscountRequirementRule
{
    #region Fields

    private readonly IAddressService _addressService;
    private readonly ICountryService _countryService;
    private readonly IDiscountService _discountService;
    private readonly ILocalizationService _localizationService;
    private readonly INopUrlHelper _nopUrlHelper;
    private readonly ISettingService _settingService;
    private readonly IWebHelper _webHelper;

    #endregion

    #region Ctor

    public ShippingCountryDiscountRequirementRule(IAddressService addressService,
        ICountryService countryService,
        IDiscountService discountService,
        ILocalizationService localizationService,
        INopUrlHelper nopUrlHelper,
        ISettingService settingService,
        IWebHelper webHelper)
    {
        _addressService = addressService;
        _countryService = countryService;
        _discountService = discountService;
        _localizationService = localizationService;
        _nopUrlHelper = nopUrlHelper;
        _settingService = settingService;
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
        ArgumentNullException.ThrowIfNull(request);

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
        return _nopUrlHelper.RouteUrl(DiscountRequirementDefaults.ConfigurationRouteName,
            new { discountId, discountRequirementId }, _webHelper.GetCurrentRequestProtocol());
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
            await _discountService.DeleteDiscountRequirementAsync(discountRequirement, false);

        //locales
        await _localizationService.DeleteLocaleResourcesAsync("Plugins.DiscountRules.ShippingCountry");

        await base.UninstallAsync();
    }

    #endregion
}