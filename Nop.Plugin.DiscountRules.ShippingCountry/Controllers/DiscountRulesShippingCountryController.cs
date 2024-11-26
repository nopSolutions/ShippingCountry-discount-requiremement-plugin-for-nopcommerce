using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Discounts;
using Nop.Plugin.DiscountRules.ShippingCountry.Models;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.DiscountRules.ShippingCountry.Controllers;

[AuthorizeAdmin]
[Area(AreaNames.ADMIN)]
[AutoValidateAntiforgeryToken]
public class DiscountRulesShippingCountryController : BasePluginController
{
    #region Fields

    private readonly ICountryService _countryService;
    private readonly IDiscountService _discountService;
    private readonly ILocalizationService _localizationService;
    private readonly ISettingService _settingService;

    #endregion

    #region Ctor

    public DiscountRulesShippingCountryController(ICountryService countryService,
        IDiscountService discountService,
        ILocalizationService localizationService,
        ISettingService settingService)
    {
        _countryService = countryService;
        _discountService = discountService;
        _localizationService = localizationService;
        _settingService = settingService;
    }

    #endregion

    #region Utilities

    private IEnumerable<string> GetErrorsFromModelState()
    {
        return ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
    }

    #endregion

    #region Methods

    [CheckPermission(StandardPermission.Promotions.DISCOUNTS_VIEW)]
    public async Task<IActionResult> Configure(int discountId, int? discountRequirementId)
    {
        var discount = await _discountService.GetDiscountByIdAsync(discountId) ?? throw new ArgumentException("Discount could not be loaded");

        DiscountRequirement discountRequirement = null;

        if (discountRequirementId.HasValue)
        {
            discountRequirement = await _discountService.GetDiscountRequirementByIdAsync(discountRequirementId.Value);
            if (discountRequirement == null)
                return Content("Failed to load requirement.");
        }

        var shippingCountryId = await _settingService.GetSettingByKeyAsync<int>(string.Format(DiscountRequirementDefaults.SETTINGS_KEY, discountRequirementId ?? 0));

        var model = new RequirementModel
        {
            RequirementId = discountRequirementId ?? 0,
            DiscountId = discount.Id,
            CountryId = shippingCountryId
        };

        //countries
        model.AvailableCountries.Add(new SelectListItem { Text = await _localizationService.GetResourceAsync("Plugins.DiscountRules.ShippingCountry.Fields.SelectCountry"), Value = "0" });

        foreach (var c in await _countryService.GetAllCountriesAsync(showHidden: true))
            model.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString(), Selected = discountRequirement != null && c.Id == shippingCountryId });

        //add a prefix
        ViewData.TemplateInfo.HtmlFieldPrefix = string.Format(DiscountRequirementDefaults.HTML_FIELD_PREFIX, discountRequirementId ?? 0);

        return View("~/Plugins/DiscountRules.ShippingCountry/Views/Configure.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Promotions.DISCOUNTS_CREATE_EDIT_DELETE)]
    public async Task<IActionResult> Configure(RequirementModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { Errors = GetErrorsFromModelState() });

        //load the discount
        var discount = await _discountService.GetDiscountByIdAsync(model.DiscountId);
        if (discount == null)
            return NotFound(new { Errors = new[] { "Discount could not be loaded" } });

        //get the discount requirement
        var discountRequirement = await _discountService.GetDiscountRequirementByIdAsync(model.RequirementId);

        //the discount requirement does not exist, so create a new one
        if (discountRequirement == null)
        {
            discountRequirement = new DiscountRequirement
            {
                DiscountId = discount.Id,
                DiscountRequirementRuleSystemName = DiscountRequirementDefaults.SYSTEM_NAME
            };

            await _discountService.InsertDiscountRequirementAsync(discountRequirement);
        }

        await _settingService.SetSettingAsync(string.Format(DiscountRequirementDefaults.SETTINGS_KEY, discountRequirement.Id), model.CountryId);

        return Ok(new { NewRequirementId = discountRequirement.Id });
    }

    #endregion
}