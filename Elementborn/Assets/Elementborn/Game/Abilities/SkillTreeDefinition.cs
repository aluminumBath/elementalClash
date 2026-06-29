using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    [CreateAssetMenu(menuName = "Elementborn/Abilities/Skill Tree", fileName = "SkillTree")]
    public sealed class SkillTreeDefinition : ScriptableObject
    {
        [SerializeField] private string treeId = "";
        [SerializeField] private string displayName = "Skill Tree";
        [TextArea]
        [SerializeField] private string description = "";
        [SerializeField] private AbilityElementType element = AbilityElementType.Neutral;
        [SerializeField] private List<SkillTreeNodeDefinition> nodes = new List<SkillTreeNodeDefinition>();

        public string TreeId => string.IsNullOrWhiteSpace(treeId) ? name : treeId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? TreeId : displayName;
        public string Description => description;
        public AbilityElementType Element => element;
        public IReadOnlyList<SkillTreeNodeDefinition> Nodes => nodes;

        public SkillTreeNodeDefinition FindNode(string nodeId)
        {
            return nodes.Find(n => n != null && n.NodeId == nodeId);
        }

        public SkillTreeNodeDefinition FindNodeByAbilityId(string abilityId)
        {
            return nodes.Find(n => n != null && n.AbilityId == abilityId);
        }

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(treeId))
            {
                treeId = name;
            }
        }
    }
}
