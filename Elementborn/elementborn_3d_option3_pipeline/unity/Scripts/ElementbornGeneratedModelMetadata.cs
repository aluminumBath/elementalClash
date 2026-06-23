// ElementbornGeneratedModelMetadata.cs
using UnityEngine;

public enum ElementbornModelCategory
{
    NPC,
    Channeler,
    Creature,
    Plant,
    Item,
    Weapon,
    Environment
}

public class ElementbornGeneratedModelMetadata : MonoBehaviour
{
    public string slug;
    public string displayName;
    public ElementbornModelCategory category;
    [TextArea] public string sourceConceptNotes;
    public bool needsRetopology = true;
    public bool needsRigging = true;
    public bool approvedForGameplay = false;
}
