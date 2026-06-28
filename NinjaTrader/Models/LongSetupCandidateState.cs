namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public enum LongSetupCandidateState
    {
        None,
        CandidateDetected,
        WaitingForConfirmation,
        InvalidContext,
        InvalidLocation,
        Disabled
    }
}
