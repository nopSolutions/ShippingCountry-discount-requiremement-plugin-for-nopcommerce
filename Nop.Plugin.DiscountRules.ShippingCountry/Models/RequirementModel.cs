﻿using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.DiscountRules.ShippingCountry.Models
{
    public record RequirementModel
    {
        public RequirementModel()
        {
            AvailableCountries = new List<SelectListItem>();
        }

        public IList<SelectListItem> AvailableCountries { get; set; }

        [NopResourceDisplayName("Plugins.DiscountRules.ShippingCountry.Fields.Country")]
        public int CountryId { get; set; }

        public int DiscountId { get; set; }

        public int RequirementId { get; set; }
    }
}