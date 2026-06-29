# v53 Functionality: Compile Fix Pass 4

This patch addresses the Editor.log findings after v52.

## What the uploaded Editor.log showed

Unity was compiling separate assemblies:

```text
Elementborn.Core.dll
Elementborn.Game.dll
Elementborn.Editor.dll
```

The patch series is currently intended to compile without custom Elementborn asmdefs. The many editor errors were mostly caused by stale local assembly definition/reference files splitting Elementborn code into assemblies whose references did not include the runtime/game code.

## Main fix

The v53 apply script deletes stale local Elementborn files matching:

```text
*.asmdef
*.asmref
```

under:

```text
Elementborn/Assets/Elementborn
```

This returns Elementborn to Unity's default compilation layout:
- runtime code under `Assets/Elementborn/...`
- editor code under `Assets/Elementborn/Editor/...`

## Source fix included

`SiteInteriorController.cs` now imports:

```text
using Elementborn.Core;
```

so its `MapMarkerType` compatibility overload can compile.

## Timestamp warning fix

The Editor.log also showed Unity/Bee warnings such as:

```text
Cannot trust contents of ... because its timestamp is in the future
```

The v53 apply script normalizes `LastWriteTime` for files under `Assets/Elementborn` to the current local time after copy.

## Apply script

Use:

```text
FIX_V53_COMPILE_ERRORS.ps1
```

Recommended command from repo root:

```powershell
cd C:\Users\steel\Desktop\Code\elementalClash

powershell -ExecutionPolicy Bypass -File "C:\Users\steel\Downloads\elementborn_v53_patch\FIX_V53_COMPILE_ERRORS.ps1" `
  -PatchRoot "C:\Users\steel\Downloads\elementborn_v53_patch"
```

## If Unity still reports stale assembly errors

Close Unity, then delete Bee's script compilation cache:

```powershell
cd C:\Users\steel\Desktop\Code\elementalClash\Elementborn
Remove-Item ".\Library\Bee" -Recurse -Force
```

Then reopen Unity.
