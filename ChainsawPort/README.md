# Chainsaw BONELAB Quest Port

Current-SDK reconstruction workspace for rebuilding the legacy TankFullOfOofs Chainsaw as a BONELAB standalone Quest spawnable.

- Target Unity: `2021.3.16f1`
- Target platform: BONELAB standalone Quest
- Current pallet reference: SDK `1.2.0`, pallet format version `2`
- Target pallet barcode: `TankFullOfOofs.Chainsaw`
- Target spawnable barcode: `TankFullOfOofs.Chainsaw.Spawnable.Chainsaw`

## Current reference

The uploaded `Frictic.DoomHunterChainsaw` Quest mod is used only as a known-current chainsaw pallet reference. Its packed metadata confirms:

- pallet format version `2`
- `sdkVersion: 1.2.0`
- spawnable crate type `crate-spawnable#0`
- tags `Weapon`, `Melee`, `Blade`
- PreviewMesh + Spawnable + MonoScripts bundle layout
- a normal Unity prefab as the Spawnable main asset

Reference metadata is stored under `CurrentReference/`. The DOOM Hunter model/assets are not copied into this project.

## Rebuild pipeline

The editor side is now split into numbered steps:

1. `Chainsaw Port -> Create Current SDK Migration Folders`
2. `Chainsaw Port -> Create Chainsaw Prefab Skeleton`
3. `Chainsaw Port -> 3 - Bind Recovered Model Materials Audio`
4. `Chainsaw Port -> 4 - Add Current Marrow Components`
5. `Chainsaw Port -> 5 - Create Current Pallet And Spawnable Crate`
6. `Chainsaw Port -> 6 - Write Current Pallet GUID Report`
7. Open the generated Pallet/SpawnableCrate assets in the Inspector, verify the prefab and preview fields, then use the compatible SDK's real `Pack for Quest` operation.

The prefab hierarchy is designed to match both the recovered legacy structure and the companion DLL detector:

`Chainsaw -> GripPoint/GripCollider, Blade/BladeTransform/BladeCollider/slashTop/slashBottom/StabPoint, Audio/IdleSound/BladeAudio, Pull Cord, ImpactProperties, SoundsExt`

## Recovered assets

The original standalone release only contains already-packed Unity AssetBundles. We preserved those bundles and recovered their hierarchy/metadata, but a packed `.bundle` is not the same thing as editable FBX/material/audio source assets.

When editable extracted assets are placed into:

- `Assets/ChainsawPort/Source/Models`
- `Assets/ChainsawPort/Source/Materials`
- `Assets/ChainsawPort/Source/Audio`

`ChainsawRecoveredAssetBinder.cs` automatically places the recovered model under `Visuals`, reuses recovered materials where names match, assigns motor/blade audio by filename, wires `ChainsawMotor`, and adds `ChainsawDamage` to the blade collider.

## Current Marrow setup

`ChainsawCurrentMarrowBuilder.cs` uses the actual SDK types when they are present in Unity rather than compiling against one guessed SDK assembly layout. It resolves and adds the available Marrow Poolee/Spawnable, Grip and StabSlash types, then creates real `SLZ.Marrow.Warehouse.Pallet` and `SLZ.Marrow.Warehouse.SpawnableCrate` ScriptableObject assets when those types are exposed by the installed compatible SDK.

If the installed SDK uses a different/internal type shape, the builder stops and reports the missing type instead of generating a fake pallet.

The current metadata template remains at `CurrentReference/Chainsaw.current-pallet-template.json`. It mirrors the working 1.2.0 pallet structure and original Chainsaw barcode/bounds. GUIDs are generated from the reconstructed Unity assets, not copied from the DOOM Chainsaw.

## What is still required for a real Spawn Gun release

The source project and Marrow wiring are now prepared, but the final Spawn Gun mod is not produced until all of these happen:

- editable Chainsaw mesh/material/audio extraction is available and bound into the prefab;
- the compatible current Marrow/Extended SDK successfully creates/accepts the Pallet + SpawnableCrate;
- a PreviewMesh is assigned;
- the SDK successfully performs **Pack for Quest**;
- the resulting pallet is installed and tested on standalone Quest.

Do not hand-rename the preserved legacy `.bundle` files and call them current output; the packed catalog/bundles must come from the current SDK build.

## Code-mod companion

The separate `Chainsaw BONELAB Code Mod` DLL recognizes this hierarchy and provides runtime motor/blade/damage behavior. Once this rebuilt spawnable is packed and tested, it can replace the temporary DOOM Hunter Chainsaw base.
