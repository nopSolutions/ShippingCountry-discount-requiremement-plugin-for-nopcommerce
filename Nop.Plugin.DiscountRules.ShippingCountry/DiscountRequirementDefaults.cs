namespace Nop.Plugin.DiscountRules.ShippingCountry
{
    /// <summary>
    /// Represents constants for the discount requirement rule
    /// </summary>
    public static class DiscountRequirementDefaults
    {
        /// <summary>
        /// The system name of the discount requirement rule
        /// </summary>
        public const string SYSTEM_NAME = "DiscountRequirement.ShippingCountryIs";

        /// <summary>
        /// The key of the settings to save restricted customer roles
        /// </summary>
        public const string SETTINGS_KEY = "DiscountRequirement.ShippingCountry-{0}";

        /// <summary>
        /// The HTML field prefix for discount requirements
        /// </summary>
        public const string HTML_FIELD_PREFIX = "DiscountRulesShippingCountry{0}";
    }
}
