# Chainsaw BONELAB Code Mod

This folder contains the MelonLoader C# code-mod side of the Chainsaw project. A separate `Mod.io Version` folder is reserved for the MarrowSDK pallet/content version.

The code mod is an initial runtime scaffold. It discovers Chainsaw/blade transforms and provides blade animation. Damage, trigger/grip behavior and audio integration require testing against the exact current Quest IL2CPP reference assemblies before they can be implemented reliably.

The original release supplied packed AssetBundles rather than an editable Unity project, so the Mod.io version still requires a current MarrowSDK rebuild.

Note: the loader used here is **MelonLoader**, not LemonLoader.