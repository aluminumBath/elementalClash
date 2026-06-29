using Elementborn.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Elementborn.Game
{
    /// <summary>
    /// Attach to the UI object/prefab used for a creature map marker.
    /// Assign sprites in the inspector so the last-ridden creature marker can
    /// visually communicate land/flying/swimming/amphibious/burrowing movement.
    /// </summary>
    public sealed class CreatureMapMarkerIconSet : MonoBehaviour
    {
        [Header("Marker Image")]
        [SerializeField] private Image iconImage;

        [Header("Creature Type Icons")]
        [SerializeField] private Sprite landIcon;
        [SerializeField] private Sprite flyingIcon;
        [SerializeField] private Sprite swimmingIcon;
        [SerializeField] private Sprite amphibiousIcon;
        [SerializeField] private Sprite burrowingIcon;
        [SerializeField] private Sprite unknownIcon;

        public void SetCreatureType(CreatureTraversalType type)
        {
            if (iconImage == null)
            {
                return;
            }

            iconImage.sprite = GetSprite(type);
        }

        public Sprite GetSprite(CreatureTraversalType type)
        {
            return type switch
            {
                CreatureTraversalType.Land => landIcon != null ? landIcon : unknownIcon,
                CreatureTraversalType.Flying => flyingIcon != null ? flyingIcon : unknownIcon,
                CreatureTraversalType.Swimming => swimmingIcon != null ? swimmingIcon : unknownIcon,
                CreatureTraversalType.Amphibious => amphibiousIcon != null ? amphibiousIcon : unknownIcon,
                CreatureTraversalType.Burrowing => burrowingIcon != null ? burrowingIcon : unknownIcon,
                _ => unknownIcon
            };
        }

        private void Reset()
        {
            iconImage = GetComponent<Image>();
            if (iconImage == null)
            {
                iconImage = GetComponentInChildren<Image>();
            }
        }
    }
}
