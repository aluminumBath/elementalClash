# 04 — Unity Import and Prefab Setup

Recommended Unity folder structure:

```text
Assets/
  Elementborn/
    Art/
      Models/
        GeneratedRaw/
        BlenderCleaned/
        UnityReady/
      Textures/
      Materials/
      Sprites/
    Prefabs/
      NPCs/
      Channelers/
      Creatures/
      Plants/
      Items/
    Scripts/
    Editor/
```

## Import settings

- Scale Factor: normalize per asset, usually 1 unit = 1 meter.
- Rig: Humanoid for human characters when possible; Generic for creatures/plants.
- Materials: use a toon shader or URP Lit with ramped/flat lighting.
- Mesh Compression: test visually; avoid aggressive compression on faces.
- Read/Write: off unless runtime mesh operations need it.
- Generate Colliders: off for characters; create custom colliders manually.

## Prefab components

NPC / Channeler:

- Animator
- CharacterController or CapsuleCollider
- FactionMember / NPC controller
- Dialogue or combat controller
- ElementbornGeneratedModelMetadata

Creature:

- Animator
- Rigidbody if physics-driven
- Collider set
- CreatureController / EnemyController / Tameable as needed

Plant:

- Collider trigger zones
- Gameplay component, e.g. PlantFlytrap, VinePatch, WillowGate
- Optional Animator for jaw/vine/petal motion
