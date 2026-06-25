# Elementborn compile fix notes — social/UI/VR pass

This patch resolves the second wave of Unity compiler errors reported after the WorldMap rename fix.

## Fixed errors

- `CoopAllies.cs`: added `using Elementborn.Game.Social;` so `SocialHub` resolves from `Elementborn.Game.Social`.
- `PlayerInventory.cs`: added `using Elementborn.Game.Social;` so `GuildController` resolves from `Elementborn.Game.Social`.
- `GrimoireController.cs`: changed `ContentSizeFitter.Fit.PreferredSize` to Unity's correct enum `ContentSizeFitter.FitMode.PreferredSize`.
- `UserDirectory.cs`: added a compatibility alias `UserRef.Name => DisplayName` so existing social UI code compiles while preserving `DisplayName` as the canonical field.
- `GuildController.cs`: replaced the nonexistent `SocialHub.Instance.Invites.InviteFriend(...)` call with a helper that calls `InviteService.Invite(fromId, toId, sessionId)`.
- `PartyController.cs`: same invite API fix as guild.
- `VrOverlayHub.cs`: qualified `UnityEngine.XR.CommonUsages.menuButton` to avoid ambiguity with `UnityEngine.InputSystem.CommonUsages`.

## WorldMap fix still included

- `Assets/Elementborn/Core/WorldMap.cs` remains an empty compatibility shim so ZIP-overwrite installs replace the stale static `WorldMap` class.
- `Assets/Elementborn/Core/WorldMapLayout.cs` contains the static map layout.
- `Assets/Elementborn/Core/World/World.cs` contains the runtime instance `WorldMap` class.

