namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public sealed class SetupPerformanceStats
    {
        public SetupPerformanceStats()
        {
            SetupType = string.Empty;
            BestR = double.MinValue;
            WorstR = double.MaxValue;
        }

        public string SetupType { get; set; }
        public int Total { get; set; }
        public int TargetHits { get; set; }
        public int StopHits { get; set; }
        public int Timeouts { get; set; }
        public double WinRate { get; set; }
        public double AverageR { get; set; }
        public double TotalR { get; set; }
        public double AverageMfeR { get; set; }
        public double AverageMaeR { get; set; }
        public double BestR { get; set; }
        public double WorstR { get; set; }
    }
}
