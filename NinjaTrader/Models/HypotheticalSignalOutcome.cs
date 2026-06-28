using System;

namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public sealed class HypotheticalSignalOutcome
    {
        public HypotheticalSignalOutcome()
        {
            RecordId = string.Empty;
            Instrument = string.Empty;
            CandidateSetupType = string.Empty;
            ConfirmationState = string.Empty;
            ConfirmationType = string.Empty;
            OutcomeState = HypotheticalOutcomeState.Open.ToString();
            OutcomeReason = string.Empty;
            ExecutionDisabled = true;
            EvaluationOnlyMode = true;
        }

        public string RecordId { get; set; }
        public string Instrument { get; set; }
        public DateTime SignalTime { get; set; }
        public int SignalBar { get; set; }
        public string CandidateSetupType { get; set; }
        public string ConfirmationState { get; set; }
        public string ConfirmationType { get; set; }
        public double EntryPrice { get; set; }
        public double StopPrice { get; set; }
        public double TargetPrice { get; set; }
        public double RewardRisk { get; set; }
        public int BarsOpen { get; set; }
        public int MaxBarsToTrack { get; set; }
        public double MaxFavorableExcursion { get; set; }
        public double MaxAdverseExcursion { get; set; }
        public double MaxFavorableR { get; set; }
        public double MaxAdverseR { get; set; }
        public string OutcomeState { get; set; }
        public DateTime? OutcomeTime { get; set; }
        public int? OutcomeBar { get; set; }
        public double? OutcomePrice { get; set; }
        public string OutcomeReason { get; set; }
        public bool IsClosed { get; set; }
        public bool ExecutionDisabled { get; set; }
        public bool EvaluationOnlyMode { get; set; }
    }
}
