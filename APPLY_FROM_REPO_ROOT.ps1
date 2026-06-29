# Run this from C:\Users\steel\Desktop\Code\elementalClash
# after extracting this patch zip somewhere.
param(
  [string]$PatchRoot = "."
)

$repo = Get-Location
$target = Join-Path $repo "Elementborn"
$source = Join-Path (Resolve-Path $PatchRoot) "Elementborn"

if (-not (Test-Path $target)) { throw "Could not find target Elementborn folder under $repo" }
if (-not (Test-Path $source)) { throw "Could not find source Elementborn folder under $PatchRoot" }

Copy-Item "$source\*" $target -Recurse -Force
Write-Host "Copied boat/content patch into $target"
Write-Host "Open Unity and let it recompile."
