using System;

namespace Elementborn.Game
{
    [Serializable]
    public class ElementbornTestReadinessIssue
    {
        public ElementbornTestReadinessSeverity Severity = ElementbornTestReadinessSeverity.Info;
        public string Area = "";
        public string Message = "";
        public string SuggestedFix = "";

        public ElementbornTestReadinessIssue() { }

        public ElementbornTestReadinessIssue(ElementbornTestReadinessSeverity severity, string area, string message, string suggestedFix = "")
        {
            Severity = severity;
            Area = area ?? "";
            Message = message ?? "";
            SuggestedFix = suggestedFix ?? "";
        }

        public override string ToString()
        {
            return $"[{Severity}] {Area}: {Message}" + (string.IsNullOrWhiteSpace(SuggestedFix) ? "" : $" — {SuggestedFix}");
        }
    }
}
