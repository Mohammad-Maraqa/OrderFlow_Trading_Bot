using System;

namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public sealed class SignalObservationRecord
    {
        public SignalObservationRecord()
        {
            RecordId = string.Empty;
            Instrument = string.Empty;
            ContextState = string.Empty;
            LocationState = string.Empty;
            ValueState = string.Empty;
            CandidateSetupType = string.Empty;
            CandidateState = string.Empty;
            CandidateReason = string.Empty;
            OrderFlowBias = string.Empty;
            ConfirmationType = string.Empty;
            ConfirmationState = string.Empty;
            ConfirmationReason = string.Empty;
            DecisionState = string.Empty;
            Notes = string.Empty;
        }

        public string RecordId { get; set; }
        public DateTime Timestamp { get; set; }
        public string Instrument { get; set; }
        public int CurrentBar { get; set; }
        public double Price { get; set; }
        public string ContextState { get; set; }
        public string LocationState { get; set; }
        public double Vwap { get; set; }
        public double Poc { get; set; }
        public double Vah { get; set; }
        public double Val { get; set; }
        public string ValueState { get; set; }
        public string CandidateSetupType { get; set; }
        public string CandidateState { get; set; }
        public double CandidateRewardRisk { get; set; }
        public string CandidateReason { get; set; }
        public string OrderFlowBias { get; set; }
        public double BarDelta { get; set; }
        public double CumulativeDelta { get; set; }
        public bool IsHighVolumeBar { get; set; }
        public string ConfirmationType { get; set; }
        public string ConfirmationState { get; set; }
        public double ConfirmationScore { get; set; }
        public string ConfirmationReason { get; set; }
        public bool ExecutionDisabled { get; set; }
        public bool EvaluationOnlyMode { get; set; }
        public string DecisionState { get; set; }
        public string Notes { get; set; }
    }
}
