# Brick Breaker — C#

A complete 3D Brick Breaker with the same table, controls, 120 Hz swept collision mechanics, scoring, three lives, and round states as the Visual Script, C++ and AngelScript versions. Gameplay runs in C#. The table is an authored scene; the render pipeline and GameUI HUD are editable assets.

## Open in the editor

Requires Windows, Plasma Engine 0.12.0, and a .NET 10 SDK. `global.json` permits newer .NET 10 feature bands.

Get the project from the Plasma Launcher's **Samples** page (BrickBreaker, C#), or clone this repository and use **Add Existing Project**. The first time the editor opens it, it offers to install the packages listed in `Editor/Packages.ddl` (GameUI 1.4.1 and C# 1.3.2). Accept, and they are installed into the project's `Plugins` folder. Then open `Scenes/Main.plScene` and play.

## Build and play without the editor

```powershell
.\Build.ps1 -Test -Run
```

Or double-click `Play.cmd`. No C++ build is required: the script compiles the C# project, transforms the assets, optionally runs the mechanics tests, and launches the engine's `Player.exe`.

The script uses the engine the launcher assigned to this project and the packages the editor installed. Pass `-SdkRoot 'C:\path\to\engine'` to use another engine, or `-GameUiRoot` and `-ScriptPackageRoot` to use built package folders. The C# package must include its `CSharp` managed payload and bundled `DotNet` runtime. Engine and package files are staged into this project's `Intermediate/Runtime/Binaries`; building the sample does not overwrite the shared SDK Player payload. Close this project's Player before rebuilding it.

## Controls

| Action | Control |
| --- | --- |
| Move | A / D or left / right arrows |
| Launch | Space or left click |
| Pause / resume | P or Escape |
| Restart | R |
| Exit | Close window or Alt+F4 |

Clear 24 bricks for 2,400 points. Each hit increases ball speed up to a cap. Missing the ball costs a life and resets its speed; score and surviving bricks remain. Paddle contact position controls the rebound angle.

## Editing

- `Scripts/BrickBreaker.cs`: engine component, scene references, input, fixed-step scheduling, and GameUI blackboard values.
- `Scripts/BrickBreakerSimulation.cs`: engine-independent layout constants, movement, swept collisions, scoring, and round state.
- `CSharpProject.plCSharpProjectAsset` and `BrickBreaker.csproj`: compiled script project and generated binding configuration. Keep the `PlasmaScript` persistent ID stable; the scene references its script subasset.
- `Scenes/Main.plScene`: editable table, camera, lights, input, script component, and GameUI canvas.
- `Meshes/`, `Materials/`: the box and sphere assets used by the 3D table.
- `RenderPipelines/BrickBreaker.plRenderPipelineAsset`: forward pipeline with the GUI stage.
- `UI/`: GameUI document, blackboard contract, and font.

The simulation uses kinematic sweeps against expanded boxes on the horizontal plane, so no physics plugin is required. Keep authored object names and transforms aligned with the simulation when changing the layout. Generated bindings and build outputs live in ignored directories.

## Reload while playing

Compatible method-body edits can update the existing script instance in editor simulation. Development uses debug-compatible portable symbols; Shipping uses the normal optimized compiler. Structural edits use a full reload. `IScriptReloadState` preserves the simulation's score, lives, bricks and accumulator across that replacement, and scene references are reacquired without resetting the round. Compile errors retain the last successful code. Attached managed debuggers use full reload.

The C# asset's Problems tab opens diagnostics in your IDE. Its Setup tab checks the SDK and packaged tools. Disable Use Persistent Compiler to use the standard build path. `Directory.Build.props` resolves the C# payload version from `Editor/Packages.ddl`; update the package declaration when upgrading.

## Validation

`Build.ps1 -Test` runs 19 standalone mechanics checks and transforms `Tests/Mechanics.plScene`. To rerun just the engine-independent checks:

```powershell
dotnet run --project Tests/BrickBreakerTests.csproj -c Development
```

For integration checks, open `Tests/Mechanics.plScene` and play, or pass that scene to Player. Its script runs the same 19 checks and logs `Brick Breaker: 19 mechanics checks passed.` It throws if a check fails. The checks cover bounds, docking, launch, pause, paddle angles, wall/brick sweeps, speed, lives, win/loss, reset, and a reproducible complete game.
