using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    [CreateAssetMenu(menuName = "Elementborn/Social NPC/Social Group", fileName = "SocialNpcGroup")]
    public sealed class SocialNpcGroupDefinition : ScriptableObject
    {
        [SerializeField] private string groupId = "";
        [SerializeField] private string displayName = "Social Group";
        [SerializeField] private CapitalId primaryCapital = CapitalId.Unknown;
        [SerializeField] private Vector3 groupCenter;
        [TextArea]
        [SerializeField] private string summary = "";
        [SerializeField] private List<SocialGroupMemberRecord> members = new List<SocialGroupMemberRecord>();
        [SerializeField] private List<SocialNpcRelationshipRecord> relationships = new List<SocialNpcRelationshipRecord>();

        public string GroupId => string.IsNullOrWhiteSpace(groupId) ? name : groupId;
        public string DisplayName => displayName;
        public CapitalId PrimaryCapital => primaryCapital;
        public Vector3 GroupCenter => groupCenter;
        public string Summary => summary;
        public IReadOnlyList<SocialGroupMemberRecord> Members => members;
        public IReadOnlyList<SocialNpcRelationshipRecord> Relationships => relationships;
    }
}
