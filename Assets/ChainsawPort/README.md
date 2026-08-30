# BONELAB Chainsaw Quest Reconstruction

Target:

- BONELAB standalone Quest
- Unity 2021.3.16f1
- Current MarrowSDK / compatible Extended SDK
- Quest pallet build

Original spawnable barcode:

TankFullOfOofs.Chainsaw.Spawnable.Chainsaw

Recovered original component/object names:

- Chainsaw
- BladeCollider
- BladeTransform
- IdleSound
- BladeAudio
- StabPoint
- GripPoint
- GripCollider
- ImpactProperties
- SoundsExt
- fleshCut
- metal
- damage

## Setup

1. Open the current BONELAB MarrowSDK Unity project.
2. Add these scripts to Assets/ChainsawPort.
3. Let Unity compile.
4. Use:

   Chainsaw Port > Create Reconstructed Chainsaw

5. Import the extracted Chainsaw model.
6. Place the model under the Chainsaw root.
7. Position GripPoint on the rear handle.
8. Position BladeTransform over the chain/blade.
9. Position BladeCollider over the cutting portion.
10. Assign the recovered engine audio to IdleSound.
11. Assign the recovered cutting/blade audio to BladeAudio.
12. Add the current Marrow Grip/Interactable components.
13. Create the Spawnable Crate.
14. Use barcode:

TankFullOfOofs.Chainsaw.Spawnable.Chainsaw

15. Pack the pallet using Pack for Quest.
16. Test it on standalone Quest before publishing.
