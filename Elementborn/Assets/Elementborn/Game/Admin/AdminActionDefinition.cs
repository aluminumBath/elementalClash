using System;
using System.Collections.Generic;

namespace Elementborn.Game
{
    [Serializable]
    public class AdminActionDefinition
    {
        public string ActionId = "";
        public string DisplayName = "";
        public string Description = "";
        public AdminActionCategory Category = AdminActionCategory.General;
        public List<AdminActionFieldDefinition> Fields = new List<AdminActionFieldDefinition>();

        public AdminActionDefinition() { }

        public AdminActionDefinition(string id, string displayName, AdminActionCategory category, string description = "")
        {
            ActionId = id;
            DisplayName = displayName;
            Category = category;
            Description = description ?? "";
        }

        public AdminActionDefinition AddField(AdminActionFieldDefinition field)
        {
            if (field != null)
            {
                Fields.Add(field);
            }

            return this;
        }
    }
}
