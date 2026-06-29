using UnityEngine;

namespace Elementborn.Game
{
    [CreateAssetMenu(menuName = "Elementborn/Abilities/Ability Definition", fileName = "AbilityDefinition")]
    public sealed class AbilityDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string abilityId = "";
        [SerializeField] private string displayName = "Ability";
        [TextArea]
        [SerializeField] private string description = "";

        [Header("Classification")]
        [SerializeField] private AbilityElementType element = AbilityElementType.Neutral;
        [SerializeField] private AbilityCategory category = AbilityCategory.ActiveCombat;
        [SerializeField] private AbilitySlotType defaultSlot = AbilitySlotType.Primary;
        [SerializeField] private bool passive;
        [SerializeField] private bool ultimate;

        [Header("Unlock")]
        [SerializeField] private AbilityUnlockRequirement unlockRequirement = new AbilityUnlockRequirement();
        [SerializeField] private AbilityUnlockSource defaultUnlockSource = AbilityUnlockSource.SkillPoints;

        [Header("Effect")]
        [SerializeField] private AbilityEffectDefinition effect = new AbilityEffectDefinition();

        [Header("Presentation")]
        [SerializeField] private Sprite icon;
        [SerializeField] private string animationTrigger = "";
        [SerializeField] private string vfxResourcePath = "";
        [SerializeField] private string sfxResourcePath = "";

        public string AbilityId => string.IsNullOrWhiteSpace(abilityId) ? name : abilityId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? AbilityId : displayName;
        public string Description => description;
        public AbilityElementType Element => element;
        public AbilityCategory Category => category;
        public AbilitySlotType DefaultSlot => defaultSlot;
        public bool Passive => passive || category == AbilityCategory.Passive;
        public bool Ultimate => ultimate || category == AbilityCategory.Ultimate;
        public AbilityUnlockRequirement UnlockRequirement => unlockRequirement;
        public AbilityUnlockSource DefaultUnlockSource => defaultUnlockSource;
        public AbilityEffectDefinition Effect => effect;
        public Sprite Icon => icon;
        public string AnimationTrigger => animationTrigger;
        public string VfxResourcePath => vfxResourcePath;
        public string SfxResourcePath => sfxResourcePath;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(abilityId))
            {
                abilityId = name;
            }

            if (category == AbilityCategory.Passive)
            {
                passive = true;
            }

            if (category == AbilityCategory.Ultimate)
            {
                ultimate = true;
                defaultSlot = AbilitySlotType.Ultimate;
            }
        }
    }
}
