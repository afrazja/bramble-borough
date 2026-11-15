# Bramble Borough

An original, lightweight 2D tower-defense prototype in Unity 6000.6.9f1. Protect Lantern Grove from three waves of dustlings. No Plants vs. Zombies characters, artwork, or assets are used.

## Play

Open this folder with Unity Hub, then open `Assets/Scenes/LanternGrove.unity` and press Play. Alternatively run `Builds/Windows/BrambleBorough.exe` after building.

Choose Acorn Scout (60 amber, fast damage) or Dew Keeper (85 amber, damage plus slowing), then click/tap a mossy + pad. Start each wave manually. Defeated enemies award amber; completed waves award 40 more. Keep the grove above zero lives through wave three. Pause and restart are available. A good starting formation puts scouts on the two pads inside the first bend and the next pad to their right.

The layout fits a 1280 x 720 landscape canvas and letterboxes on other aspect ratios. Mouse and touch use the same GUI placement controls. Android is landscape, ARM64, Android 8 or later; phone performance and touch need device verification.

## Edit and verify

* `Assets/Scripts/GroveGame.cs`: deterministic game state, costs, enemy path, pad coordinates, waves, and procedural drawing.
* `Assets/Editor/ProjectBuild.cs`: scene preparation, gameplay verification, and build entry points.
* Unity menu: **Bramble Borough > Run gameplay checks**.
* **Prepare playable scene** recreates the scene; use it only when you intend to reset scene edits.

The initial art is generated from simple original shapes at runtime. This is a functional art pass, not finished production illustration. The prototype uses IMGUI for quick input and rendering; a later polish milestone should migrate to sprites, atlases, prefabs, and a Canvas UI. There are no paid assets, external art downloads, 3D models, post effects, or runtime package dependencies beyond Unity modules.

## Reproducible local builds

Close the Unity editor for this project before invoking batch builds. In PowerShell, from this folder:

```powershell
./Tools/Build.ps1 -Target Verify
./Tools/Build.ps1 -Target Windows
./Tools/Build.ps1 -Target Web
./Tools/Build.ps1 -Target Android
```

Logs go to `Builds/Logs`. Build output is intentionally ignored by Git. The Android APK uses Unity's local debug signing for testing; it is not a store-ready release. Never commit signing keys or passwords.

The build script detects the isolated Android dependencies prepared in this task's sibling `work` folder. On another machine, install Unity's Android SDK/NDK components through Hub, or pass `-AndroidSdk` and `-AndroidNdk` paths. SDK/NDK overrides apply only during the build and restore the prior editor preferences afterwards.

For Web testing, serve `Builds/Web` over HTTP rather than opening `index.html` as a file. Web compression is disabled to avoid custom server-header requirements on GitHub Pages.

## Git and Pages

Commit `Assets` (including every `.meta`), `Packages`, and `ProjectSettings`. The `.gitignore` excludes regenerated caches, build output, and personal editor settings. Never copy assets between projects without their `.meta` files.

`Tools/StageWeb.ps1` copies a successful Web build into `docs` for static hosting. The Pages workflow deploys `docs` from `main`. Build Web, stage it, commit and push, then choose **GitHub Actions** as the repository's Pages source. No Unity license secret is required in GitHub Actions because compilation happens locally. Use a new repository called `bramble-borough`; this project has no relationship to sky-island-defense.

## Next milestone

Visually inspect the Windows and browser builds, test touch on a phone, then polish this one level: replace shapes with original layered cartoon sprites, add clear placement range previews, hit feedback, sound, and tune difficulty. Expand only after that level feels good.
