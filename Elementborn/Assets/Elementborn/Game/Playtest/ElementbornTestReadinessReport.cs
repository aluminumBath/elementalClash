using System;
using System.Collections.Generic;
using System.Text;

namespace Elementborn.Game
{
    [Serializable]
    public class ElementbornTestReadinessReport
    {
        public List<ElementbornTestReadinessIssue> Issues = new List<ElementbornTestReadinessIssue>();
        public int ErrorCount;
        public int WarningCount;
        public int InfoCount;

        public bool IsReady => ErrorCount == 0;

        public void Add(ElementbornTestReadinessIssue issue)
        {
            if (issue == null)
            {
                return;
            }

            Issues.Add(issue);
            switch (issue.Severity)
            {
                case ElementbornTestReadinessSeverity.Error:
                    ErrorCount++;
                    break;
                case ElementbornTestReadinessSeverity.Warning:
                    WarningCount++;
                    break;
                default:
                    InfoCount++;
                    break;
            }
        }

        public string ToMarkdown()
        {
            var sb = new StringBuilder();
            sb.AppendLine("# Elementborn Test Readiness Report");
            sb.AppendLine();
            sb.AppendLine($"Ready: {(IsReady ? "Yes" : "No")}");
            sb.AppendLine();
            sb.AppendLine("```text");
            sb.AppendLine($"Errors: {ErrorCount}");
            sb.AppendLine($"Warnings: {WarningCount}");
            sb.AppendLine($"Info: {InfoCount}");
            sb.AppendLine("```");
            sb.AppendLine();
            sb.AppendLine("## Findings");
            sb.AppendLine();

            if (Issues.Count == 0)
            {
                sb.AppendLine("- No readiness issues detected.");
            }

            foreach (ElementbornTestReadinessIssue issue in Issues)
            {
                sb.AppendLine($"- **{issue.Severity}** — `{issue.Area}`: {issue.Message}");
                if (!string.IsNullOrWhiteSpace(issue.SuggestedFix))
                {
                    sb.AppendLine($"  - Fix: {issue.SuggestedFix}");
                }
            }

            sb.AppendLine();
            sb.AppendLine("## Recommended test order");
            sb.AppendLine();
            sb.AppendLine("```text");
            sb.AppendLine("1. Build Rounded Playable Scene.");
            sb.AppendLine("2. Run Test Readiness scan.");
            sb.AppendLine("3. Press Play.");
            sb.AppendLine("4. Press F8 for wrist admin UI.");
            sb.AppendLine("5. Use the Playtest Harness panel for teleport/loop checks.");
            sb.AppendLine("6. Run EditMode tests.");
            sb.AppendLine("7. Run PlayMode tests.");
            sb.AppendLine("```");
            return sb.ToString();
        }
    }
}
