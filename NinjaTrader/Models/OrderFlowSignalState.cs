namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public enum OrderFlowSignalState
    {
        DataMissing,
        WaitingForConfirmation,
        Rejected,
        ConfirmedLong
    }
}
