param(
  [string]$SdkRoot = $env:PLASMA_SDK_ROOT,
  [string]$GameUiRoot = $env:PLASMA_GAMEUI_ROOT,
  [string]$ScriptPackageRoot,
  [switch]$Run,
  [switch]$Test
)
$ErrorActionPreference = 'Stop'

# The engine is, in order: -SdkRoot or PLASMA_SDK_ROOT, an engine checkout this project sits inside,
# or the engine the Plasma Launcher assigned to this project (recorded in plLauncher).
function Resolve-PlasmaSdk([string]$Root) {
  if ($Root) { return (Resolve-Path -LiteralPath $Root).Path }
  for ($dir = Get-Item -LiteralPath $PSScriptRoot; $dir; $dir = $dir.Parent) {
    if (Test-Path -LiteralPath (Join-Path $dir.FullName 'Code/Engine/Foundation/Basics.h')) { return $dir.FullName }
  }
  $launcherFile = Join-Path $PSScriptRoot 'plLauncher'
  $engines = Join-Path $env:APPDATA 'com.plasmatech.launcher/engines.json'
  if ((Test-Path -LiteralPath $launcherFile) -and (Test-Path -LiteralPath $engines)) {
    $version = (Get-Content -LiteralPath $launcherFile -Raw | ConvertFrom-Json).engine_version
    # Windows PowerShell returns a JSON array as one object; assigning it first lets the pipeline enumerate it.
    $installed = Get-Content -LiteralPath $engines -Raw | ConvertFrom-Json
    $engine = $installed | Where-Object { $_.version -eq $version } | Select-Object -First 1
    if ($engine) { return $engine.install_path }
  }
  throw 'Could not find the engine. Add this project in the Plasma Launcher, or pass -SdkRoot with the engine folder.'
}

# Editor/Packages.ddl is the list the editor installs from, so the build uses the same versions.
function Get-ProjectPackages {
  $ddl = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'Editor/Packages.ddl') -Raw
  [regex]::Matches($ddl, '%Id\s*\{\s*"([^"]+)"\s*\}\s*(?:string\s*)?(?:%Version\s*\{\s*"([^"]+)"\s*\})?') |
    ForEach-Object { @{ Id = $_.Groups[1].Value; Version = $_.Groups[2].Value } }
}

$SdkRoot = Resolve-PlasmaSdk $SdkRoot
Write-Host "Engine: $SdkRoot"
$sdkBinaryDirectory = Join-Path $SdkRoot 'Binaries/Windows_Development'
$binaryDirectory = Join-Path $PSScriptRoot 'Intermediate/Runtime/Binaries'
New-Item -ItemType Directory -Path $binaryDirectory -Force | Out-Null
# Keep all mutable package deployment project-local. Do not overwrite a shared SDK's Player payload.
foreach ($engineFile in Get-ChildItem -LiteralPath $sdkBinaryDirectory -File) {
  if ($engineFile.Extension -notin @('.dll', '.exe', '.json', '.plPluginBundle', '.ini', '.qss', '.txt')) { continue }
  $runtimeFile = Join-Path $binaryDirectory $engineFile.Name
  if (-not (Test-Path -LiteralPath $runtimeFile) -or (Get-Item -LiteralPath $runtimeFile).LastWriteTimeUtc -lt $engineFile.LastWriteTimeUtc -or (Get-Item -LiteralPath $runtimeFile).Length -ne $engineFile.Length) {
    Copy-Item -LiteralPath $engineFile.FullName -Destination $runtimeFile -Force
  }
}
foreach ($qtFolder in @('platforms', 'imageformats', 'iconengines', 'styles', 'tls', 'D3D12', 'Aftermath')) {
  $sdkQtFolder = Join-Path $sdkBinaryDirectory $qtFolder
  if (Test-Path -LiteralPath $sdkQtFolder) { Copy-Item -LiteralPath $sdkQtFolder -Destination $binaryDirectory -Recurse -Force }
}
Set-Content -LiteralPath (Join-Path $binaryDirectory 'plSdkRoot.txt') -Value ([IO.Path]::GetRelativePath($binaryDirectory, $SdkRoot)) -NoNewline
$scriptPackage = 'plasma.csharp'
$sources = @{ 'plasma.gameui' = $GameUiRoot; $scriptPackage = $ScriptPackageRoot }

foreach ($package in Get-ProjectPackages) {
  $destination = Join-Path $PSScriptRoot "Plugins/$($package.Id)/$($package.Version)"
  $source = $sources[$package.Id]
  if (-not $source -and -not (Test-Path -LiteralPath (Join-Path $destination 'Package.plPackage'))) {
    # Engine developers keep package checkouts beside the engine.
    $sibling = Join-Path (Split-Path $SdkRoot -Parent) "LocalPackages/$($package.Id)"
    if (Test-Path -LiteralPath (Join-Path $sibling 'Package.plPackage')) { $source = $sibling }
  }
  if ($source) {
    $source = (Resolve-Path -LiteralPath $source).Path
    $manifest = Get-Content -LiteralPath (Join-Path $source 'Package.plPackage') -Raw
    if ($manifest -notmatch ('%Version\s*\{\s*"' + [regex]::Escape($package.Version) + '"')) { throw "Expected $($package.Id) $($package.Version) in $source." }
    New-Item -ItemType Directory -Path $destination -Force | Out-Null
    Copy-Item -LiteralPath (Join-Path $source 'Package.plPackage') -Destination $destination -Force
    foreach ($folder in @('Assets', 'Bin')) {
      if (Test-Path -LiteralPath (Join-Path $source $folder)) { Copy-Item -LiteralPath (Join-Path $source $folder) -Destination $destination -Recurse -Force }
    }
  }
  if (-not (Test-Path -LiteralPath (Join-Path $destination 'Package.plPackage'))) {
    throw "$($package.Id) $($package.Version) is not installed. Open the project in the editor once and accept the package install, or pass its built package folder."
  }
  $packageBinaries = Join-Path $destination 'Bin/Windows_x64_Development'
  if (-not (Test-Path -LiteralPath $packageBinaries)) { throw "$($package.Id) $($package.Version) has no Development binaries." }
  # Player resolves runtime imports and the managed host payload beside the engine binaries.
  Get-ChildItem -LiteralPath $packageBinaries -File -Filter '*.dll' | Copy-Item -Destination $binaryDirectory -Force
  if ($package.Id -eq $scriptPackage) {
    foreach ($folder in @('CSharp', 'DotNet')) {
      if (Test-Path -LiteralPath (Join-Path $packageBinaries $folder)) { Copy-Item -LiteralPath (Join-Path $packageBinaries $folder) -Destination $binaryDirectory -Recurse -Force }
    }
  }
}

# plasma.csharp rewrites this stamp while transforming the C# project, but refuses to transform while it
# is missing, so a fresh clone needs a placeholder. The transform replaces it with the real one.
$sourceStamp = Join-Path $PSScriptRoot 'Intermediate/PlasmaBuild/CSharpBindings/CSharpSourceInputs.stamp'
if (-not (Test-Path -LiteralPath $sourceStamp)) {
  New-Item -ItemType Directory -Path (Split-Path $sourceStamp -Parent) -Force | Out-Null
  Set-Content -LiteralPath $sourceStamp -Value '' -NoNewline
}
$processor = Join-Path $binaryDirectory 'EditorProcessor.exe'
if (-not (Test-Path -LiteralPath $processor)) { throw 'EditorProcessor is required to compile scripts and transform assets.' }
$assets = 'CSharpProject.plCSharpProjectAsset;Meshes/Box.plMeshAsset;Meshes/Sphere.plMeshAsset;RenderPipelines/BrickBreaker.plRenderPipelineAsset;UI/HudFont.plUiFontAsset;UI/HudData.plBlackboardTemplateAsset;UI/BrickBreakerHUD.plUiDocumentAsset;Scenes/Main.plScene'
if ($Test) { $assets += ';Tests/Mechanics.plScene' }
# Use absolute asset paths so SDK assets with the same relative name cannot shadow this project.
$assets = (($assets -split ';') | ForEach-Object { Join-Path $PSScriptRoot $_ }) -join ';'
$transform = Start-Process -FilePath $processor -ArgumentList '-project', ('"' + $PSScriptRoot + '"'), '-transform', 'Default', '-assets', ('"' + $assets + '"'), '-outputDir', ('"' + (Join-Path $PSScriptRoot 'AssetCache/TransformLog') + '"') -WindowStyle Hidden -PassThru
# Wait for the processor itself; the C# compiler server can outlive its parent.
$transform.WaitForExit()
if ($transform.ExitCode -ne 0) { throw "Asset transformation failed ($($transform.ExitCode)). See AssetCache/TransformLog." }
if ($Test) {
  Push-Location $PSScriptRoot
  try {
    & dotnet run --project Tests/BrickBreakerTests.csproj -c Development
    if ($LASTEXITCODE -ne 0) { throw 'Mechanics tests failed.' }
  } finally { Pop-Location }
}
if ($Run) {
  $player = Start-Process -FilePath (Join-Path $binaryDirectory 'Player.exe') -ArgumentList '-project', ('"' + $PSScriptRoot + '"'), '-scene', 'Scenes/Main.plScene' -WindowStyle Hidden -Wait -PassThru
  if ($player.ExitCode -ne 0) { throw "Player exited with code $($player.ExitCode)." }
}
