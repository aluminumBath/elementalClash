#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Elementborn.Game.EditorTools
{
    public static class ElementbornGeneratedAssetApprovalDatabase
    {
        public const string ApprovalPath = "Assets/Elementborn/Data/GeneratedAssets/ElementbornGeneratedAssetApproval_v97.json";

        [System.Serializable]
        public sealed class ApprovalData
        {
            public int catalogVersion = 97;
            public string notes = "Generated asset approvals.";
            public string[] approved = new string[0];
            public string[] rejected = new string[0];
            public ReviewNote[] reviewNotes = new ReviewNote[0];
        }

        [System.Serializable]
        public sealed class ReviewNote
        {
            public string safeName;
            public string note;
        }

        public static ApprovalData Load()
        {
            EnsureFolder("Assets/Elementborn");
            EnsureFolder("Assets/Elementborn/Data");
            EnsureFolder("Assets/Elementborn/Data/GeneratedAssets");

            if (!File.Exists(ApprovalPath))
            {
                ApprovalData created = new ApprovalData();
                Save(created);
                return created;
            }

            try
            {
                string json = File.ReadAllText(ApprovalPath);
                ApprovalData data = JsonUtility.FromJson<ApprovalData>(json);
                if (data == null)
                {
                    data = new ApprovalData();
                }

                if (data.approved == null) data.approved = new string[0];
                if (data.rejected == null) data.rejected = new string[0];
                if (data.reviewNotes == null) data.reviewNotes = new ReviewNote[0];
                return data;
            }
            catch
            {
                return new ApprovalData();
            }
        }

        public static void Save(ApprovalData data)
        {
            EnsureFolder("Assets/Elementborn");
            EnsureFolder("Assets/Elementborn/Data");
            EnsureFolder("Assets/Elementborn/Data/GeneratedAssets");

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(ApprovalPath, json);
            AssetDatabase.ImportAsset(ApprovalPath);
            AssetDatabase.SaveAssets();
        }

        public static bool IsApproved(string safeName)
        {
            ApprovalData data = Load();
            return Contains(data.approved, safeName);
        }

        public static bool IsRejected(string safeName)
        {
            ApprovalData data = Load();
            return Contains(data.rejected, safeName);
        }

        public static void Approve(string safeName, string note)
        {
            ApprovalData data = Load();
            List<string> approved = new List<string>(data.approved ?? new string[0]);
            List<string> rejected = new List<string>(data.rejected ?? new string[0]);

            if (!approved.Contains(safeName))
            {
                approved.Add(safeName);
            }

            rejected.Remove(safeName);

            data.approved = approved.ToArray();
            data.rejected = rejected.ToArray();
            SetNote(data, safeName, note);
            Save(data);
        }

        public static void Reject(string safeName, string note)
        {
            ApprovalData data = Load();
            List<string> approved = new List<string>(data.approved ?? new string[0]);
            List<string> rejected = new List<string>(data.rejected ?? new string[0]);

            if (!rejected.Contains(safeName))
            {
                rejected.Add(safeName);
            }

            approved.Remove(safeName);

            data.approved = approved.ToArray();
            data.rejected = rejected.ToArray();
            SetNote(data, safeName, note);
            Save(data);
        }

        public static void ClearReview(string safeName)
        {
            ApprovalData data = Load();
            List<string> approved = new List<string>(data.approved ?? new string[0]);
            List<string> rejected = new List<string>(data.rejected ?? new string[0]);

            approved.Remove(safeName);
            rejected.Remove(safeName);

            data.approved = approved.ToArray();
            data.rejected = rejected.ToArray();
            SetNote(data, safeName, "");
            Save(data);
        }

        public static string GetNote(string safeName)
        {
            ApprovalData data = Load();
            if (data.reviewNotes == null)
            {
                return "";
            }

            for (int i = 0; i < data.reviewNotes.Length; i++)
            {
                ReviewNote note = data.reviewNotes[i];
                if (note != null && note.safeName == safeName)
                {
                    return note.note ?? "";
                }
            }

            return "";
        }

        public static string BuildApprovalReport()
        {
            ApprovalData data = Load();
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Elementborn Generated Asset Approval Report");
            builder.AppendLine("Approved: " + (data.approved != null ? data.approved.Length : 0));
            builder.AppendLine("Rejected: " + (data.rejected != null ? data.rejected.Length : 0));
            builder.AppendLine();

            builder.AppendLine("Approved assets:");
            if (data.approved != null)
            {
                for (int i = 0; i < data.approved.Length; i++)
                {
                    builder.AppendLine("- " + data.approved[i] + " :: " + GetNote(data.approved[i]));
                }
            }

            builder.AppendLine();
            builder.AppendLine("Rejected assets:");
            if (data.rejected != null)
            {
                for (int i = 0; i < data.rejected.Length; i++)
                {
                    builder.AppendLine("- " + data.rejected[i] + " :: " + GetNote(data.rejected[i]));
                }
            }

            return builder.ToString();
        }

        private static void SetNote(ApprovalData data, string safeName, string noteText)
        {
            List<ReviewNote> notes = new List<ReviewNote>(data.reviewNotes ?? new ReviewNote[0]);
            bool found = false;

            for (int i = 0; i < notes.Count; i++)
            {
                ReviewNote note = notes[i];
                if (note != null && note.safeName == safeName)
                {
                    note.note = noteText ?? "";
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                notes.Add(new ReviewNote { safeName = safeName, note = noteText ?? "" });
            }

            data.reviewNotes = notes.ToArray();
        }

        private static bool Contains(string[] values, string value)
        {
            if (values == null)
            {
                return false;
            }

            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] == value)
                {
                    return true;
                }
            }

            return false;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
            string name = System.IO.Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
#endif
