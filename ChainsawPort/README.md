# Chainsaw BONELAB Quest Port

GitHub-ready reconstruction workspace for the legacy TankFullOfOofs Chainsaw pallet.

- Target Unity: `2021.3.16f1`
- Target platform: BONELAB standalone Quest
- Legacy SDK metadata: `0.3.6-1705`
- Spawnable barcode: `TankFullOfOofs.Chainsaw.Spawnable.Chainsaw`

## Chromebook workflow

This project is set up so the Unity preparation step can run in GitHub Actions instead of requiring Unity on the Chromebook.

Run the root workflow:

`Actions -> Chainsaw Quest Unity CI -> Run workflow`

The workflow opens `ChainsawPort` with Unity `2021.3.16f1`, targets Android/ARM64/IL2CPP, runs `ChainsawPort.Editor.ChainsawQuestCI.PrepareQuestBuild`, and uploads `Chainsaw-Quest-CI-Output`.

GitHub/Unity licensing secrets are still required for the hosted Unity editor:

- `UNITY_LICENSE`
- `UNITY_EMAIL`
- `UNITY_PASSWORD`

## Important limitation

The supplied legacy releases contain packed AssetBundles rather than the original editable Unity prefab/project. The current official MarrowSDK documentation also lists custom Spawnables as unsupported in its current release, so an appropriate compatible Extended SDK/custom-spawnable solution is required for the final item build.

The GitHub workflow therefore does **not** pretend that a generic Android Unity build is a BONELAB pallet. It prepares/validates the Quest Unity project and emits a CI report. A real Mod.io release still needs:

1. Reconstructed/extracted Chainsaw model, materials and audio.
2. A working Chainsaw prefab with Marrow interaction components.
3. A compatible Marrow/Extended SDK Spawnable Crate and Pallet.
4. The SDK's real **Pack for Quest** operation.
5. A standalone Quest test before publishing.

## Source layout

- `Assets/ChainsawPort/Scripts/ChainsawMotor.cs` - visual blade rotation and motor audio.
- `Assets/ChainsawPort/Scripts/ChainsawDamage.cs` - blade contact/physics hook; BONELAB-specific NPC damage is intentionally left for the exact target SDK API.
- `Assets/ChainsawPort/Editor/ChainsawQuestCI.cs` - headless GitHub Actions preparation entry point.
- `Assets/ChainsawPort/Editor/ChainsawPortSetup.cs` - migration folder setup.
- `LegacyInput/pallet.json` - legacy pallet metadata reference.
- `.github/workflows/build-chainsaw-quest.yml` at repository root - Chromebook-friendly Unity CI.
