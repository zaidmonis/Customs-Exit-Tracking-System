namespace CustomsExitTracking.ServiceA.Api.Settings;

public sealed class ScreeningRulesOptions
{
    public const string SectionName = "ScreeningRules";

    public int FrequentTravelThreshold { get; set; } = 3;
}
