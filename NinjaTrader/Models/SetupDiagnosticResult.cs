namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public sealed class SetupDiagnosticResult
    {
        public SetupDiagnosticResult()
        {
            SetupType = string.Empty;
            DiagnosticReason = string.Empty;
            RecommendedAction = "KeepTesting";
        }

        public string SetupType { get; set; }
        public int Total { get; set; }
        public double WinRate { get; set; }
        public double AverageR { get; set; }
        public double TotalR { get; set; }
        public bool ShouldDisable { get; set; }
        public bool ShouldTighten { get; set; }
        public bool ShouldKeepTesting { get; set; }
        public string DiagnosticReason { get; set; }
        public string RecommendedAction { get; set; }
    }
}
