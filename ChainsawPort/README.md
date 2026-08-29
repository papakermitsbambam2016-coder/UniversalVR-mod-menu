# Chainsaw BONELAB Port

GitHub-ready migration workspace for the legacy TankFullOfOofs Chainsaw pallet.

Target Unity: 2021.3.16f1
Target: BONELAB Quest
Legacy SDK metadata: 0.3.6-1705

The supplied legacy release contains packed AssetBundles rather than the original editable Unity project. Changing pallet metadata alone cannot rebuild those bundles. The final Quest pallet therefore requires the Chainsaw source prefab/assets to be re-imported and packed with the current MarrowSDK.

See `LegacyInput/pallet.json` for the migration metadata and `.github/workflows/chainsaw-build.yml` for the validation/build entry point.
