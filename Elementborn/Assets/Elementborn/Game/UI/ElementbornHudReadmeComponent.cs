using UnityEngine;

namespace Elementborn.Game
{
    public sealed class ElementbornHudReadmeComponent : MonoBehaviour
    {
        [TextArea(8, 16)]
        [SerializeField] private string notes =
@"Elementborn HUD Prefab

Default controls:
- J toggles quest log.
- F9 toggles debug panel.
- 1-4 cast spell loadout slots if assigned.
- Right Mouse Button blocks.
- Left Alt dodges.

Run HudPanelAutoBinder after scene/player creation if references are missing.";
    }
}
