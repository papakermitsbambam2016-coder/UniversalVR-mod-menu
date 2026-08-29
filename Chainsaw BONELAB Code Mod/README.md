# Chainsaw BONELAB Code Mod

This is the **code-mod side** of the Chainsaw port.

It uses the MelonLoader C# API and BoneLib. BoneLib provides grab/release hooks and an AIBrain damage helper. On Quest, LemonLoader is the Android loader used for compatible MelonLoader-style mods.

Current runtime source:
- detects the legacy Chainsaw when grabbed
- rotates blade transforms
- starts/stops motor/idle AudioSources when present
- checks blade colliders for nearby AIBrain targets
- applies damage through BoneLib

The source still needs the target Quest reference DLLs before a real DLL can be compiled. The old Chainsaw AssetBundles are separate from this code mod.
