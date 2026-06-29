using System;
using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    [Serializable]
    public class SkillTreeNodeDefinition
    {
        public string NodeId = "";
        public AbilityDefinition Ability;
        public Vector2 UiPosition;
        public List<string> PrerequisiteNodeIds = new List<string>();

        public string AbilityId => Ability != null ? Ability.AbilityId : "";
    }
}
