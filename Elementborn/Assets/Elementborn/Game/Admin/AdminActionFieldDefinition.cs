using System;
using System.Collections.Generic;

namespace Elementborn.Game
{
    [Serializable]
    public class AdminActionFieldDefinition
    {
        public string FieldId = "";
        public string Label = "";
        public AdminFieldType FieldType = AdminFieldType.Text;
        public string DefaultValue = "";
        public List<string> Options = new List<string>();

        public AdminActionFieldDefinition() { }

        public AdminActionFieldDefinition(string id, string label, AdminFieldType type, string defaultValue = "", params string[] options)
        {
            FieldId = id;
            Label = label;
            FieldType = type;
            DefaultValue = defaultValue ?? "";
            if (options != null)
            {
                Options.AddRange(options);
            }
        }
    }
}
