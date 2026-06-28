namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public enum ValueAcceptanceState
    {
        Unknown,
        RejectedAboveValue,
        RejectedBelowValue,
        AcceptedInsideValue,
        AcceptedAboveValue,
        AcceptedBelowValue,
        RotationalInsideValue,
        NoAcceptance
    }
}
