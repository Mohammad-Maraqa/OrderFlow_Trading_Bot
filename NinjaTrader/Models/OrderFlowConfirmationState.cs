namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public enum OrderFlowConfirmationState
    {
        None,
        WaitingForConfirmation,
        ConfirmationObserved,
        WeakConfirmation,
        NoConfirmation,
        InvalidCandidate,
        Disabled,
        ApproximationOnly
    }
}
