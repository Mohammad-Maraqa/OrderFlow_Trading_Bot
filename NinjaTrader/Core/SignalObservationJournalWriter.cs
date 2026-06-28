using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public sealed class SignalObservationJournalWriter
    {
        public const string DefaultFileName = "orderflow_signal_observations.jsonl";
        public const string OutputPrefix = "SIGNAL_OBSERVATION_JSON=";

        public bool Write(
            SignalObservationRecord record,
            string journalFileName,
            Action<string> print)
        {
            if (record == null)
            {
                return false;
            }

            string jsonLine = ToJsonLine(record);
            string safeFileName = string.IsNullOrEmpty(journalFileName)
                ? DefaultFileName
                : journalFileName;

            try
            {
                string path = ResolvePath(safeFileName);
                File.AppendAllText(path, jsonLine + Environment.NewLine, Encoding.UTF8);
                return true;
            }
            catch (Exception error)
            {
                if (print != null)
                {
                    print("Signal observation journal warning: file write failed: "
                        + error.Message);
                    print(OutputPrefix + jsonLine);
                }

                return false;
            }
        }

        public string ToJsonLine(SignalObservationRecord record)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("{");
            Append(builder, "record_id", record.RecordId).Append(",");
            Append(builder, "timestamp", record.Timestamp.ToString("O", CultureInfo.InvariantCulture)).Append(",");
            Append(builder, "instrument", record.Instrument).Append(",");
            Append(builder, "current_bar", record.CurrentBar).Append(",");
            Append(builder, "price", record.Price).Append(",");
            Append(builder, "context_state", record.ContextState).Append(",");
            Append(builder, "location_state", record.LocationState).Append(",");
            Append(builder, "vwap", record.Vwap).Append(",");
            Append(builder, "poc", record.Poc).Append(",");
            Append(builder, "vah", record.Vah).Append(",");
            Append(builder, "val", record.Val).Append(",");
            Append(builder, "value_state", record.ValueState).Append(",");
            Append(builder, "candidate_setup_type", record.CandidateSetupType).Append(",");
            Append(builder, "candidate_state", record.CandidateState).Append(",");
            Append(builder, "candidate_reward_risk", record.CandidateRewardRisk).Append(",");
            Append(builder, "candidate_reason", record.CandidateReason).Append(",");
            Append(builder, "order_flow_bias", record.OrderFlowBias).Append(",");
            Append(builder, "bar_delta", record.BarDelta).Append(",");
            Append(builder, "cumulative_delta", record.CumulativeDelta).Append(",");
            Append(builder, "is_high_volume_bar", record.IsHighVolumeBar).Append(",");
            Append(builder, "confirmation_type", record.ConfirmationType).Append(",");
            Append(builder, "confirmation_state", record.ConfirmationState).Append(",");
            Append(builder, "confirmation_score", record.ConfirmationScore).Append(",");
            Append(builder, "confirmation_reason", record.ConfirmationReason).Append(",");
            Append(builder, "execution_disabled", record.ExecutionDisabled).Append(",");
            Append(builder, "evaluation_only_mode", record.EvaluationOnlyMode).Append(",");
            Append(builder, "decision_state", record.DecisionState).Append(",");
            Append(builder, "notes", record.Notes);
            builder.Append("}");
            return builder.ToString();
        }

        private static string ResolvePath(string journalFileName)
        {
            if (Path.IsPathRooted(journalFileName))
            {
                return journalFileName;
            }

            string folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            return Path.Combine(folder, journalFileName);
        }

        private static StringBuilder Append(StringBuilder builder, string key, string value)
        {
            builder.Append("\"").Append(Escape(key)).Append("\":");
            builder.Append("\"").Append(Escape(value)).Append("\"");
            return builder;
        }

        private static StringBuilder Append(StringBuilder builder, string key, double value)
        {
            builder.Append("\"").Append(Escape(key)).Append("\":");
            builder.Append(value.ToString("0.########", CultureInfo.InvariantCulture));
            return builder;
        }

        private static StringBuilder Append(StringBuilder builder, string key, int value)
        {
            builder.Append("\"").Append(Escape(key)).Append("\":");
            builder.Append(value.ToString(CultureInfo.InvariantCulture));
            return builder;
        }

        private static StringBuilder Append(StringBuilder builder, string key, bool value)
        {
            builder.Append("\"").Append(Escape(key)).Append("\":");
            builder.Append(value ? "true" : "false");
            return builder;
        }

        private static string Escape(string value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }
    }
}
