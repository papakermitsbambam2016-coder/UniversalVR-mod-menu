#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace ChainsawPort.Editor
{
    public static class ChainsawPrefabBuilder
    {
        [MenuItem("Chainsaw Port/Create Reconstructed Chainsaw")]
        public static void CreateChainsaw()
        {
            GameObject root = new GameObject("Chainsaw");

            Rigidbody rb = root.AddComponent<Rigidbody>();

            rb.mass = 5f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode =
                CollisionDetectionMode.ContinuousDynamic;

            // -------------------------------------
            // MAIN COLLIDER
            // -------------------------------------

            BoxCollider mainCollider =
                root.AddComponent<BoxCollider>();

            mainCollider.center = new Vector3(
                0.0278568268f,
                0.08067175f,
                -0.4257303f
            );

            // Old pallet stored EXTENTS.
            // Unity BoxCollider uses full SIZE.
            mainCollider.size = new Vector3(
                0.26797703f,
                0.35492572f,
                1.04866111f
            );

            // -------------------------------------
            // GRIP
            // -------------------------------------

            GameObject gripPoint =
                new GameObject("GripPoint");

            gripPoint.transform.SetParent(
                root.transform,
                false
            );

            GameObject gripCollider =
                new GameObject("GripCollider");

            gripCollider.transform.SetParent(
                gripPoint.transform,
                false
            );

            BoxCollider gripBox =
                gripCollider.AddComponent<BoxCollider>();

            gripBox.size = new Vector3(
                0.12f,
                0.14f,
                0.28f
            );

            // -------------------------------------
            // BLADE
            // -------------------------------------

            GameObject bladeTransform =
                new GameObject("BladeTransform");

            bladeTransform.transform.SetParent(
                root.transform,
                false
            );

            GameObject bladeCollider =
                new GameObject("BladeCollider");

            bladeCollider.transform.SetParent(
                bladeTransform.transform,
                false
            );

            BoxCollider bladeBox =
                bladeCollider.AddComponent<BoxCollider>();

            bladeBox.isTrigger = true;

            bladeBox.size = new Vector3(
                0.10f,
                0.10f,
                0.75f
            );

            ChainsawDamage damage =
                bladeCollider.AddComponent<ChainsawDamage>();

            // -------------------------------------
            // STAB POINT
            // -------------------------------------

            GameObject stabPoint =
                new GameObject("StabPoint");

            stabPoint.transform.SetParent(
                bladeTransform.transform,
                false
            );

            // -------------------------------------
            // AUDIO
            // -------------------------------------

            GameObject idleSound =
                new GameObject("IdleSound");

            idleSound.transform.SetParent(
                root.transform,
                false
            );

            AudioSource idleAudio =
                idleSound.AddComponent<AudioSource>();

            idleAudio.loop = true;
            idleAudio.playOnAwake = false;
            idleAudio.spatialBlend = 1f;

            GameObject bladeAudio =
                new GameObject("BladeAudio");

            bladeAudio.transform.SetParent(
                root.transform,
                false
            );

            AudioSource bladeAudioSource =
                bladeAudio.AddComponent<AudioSource>();

            bladeAudioSource.loop = true;
            bladeAudioSource.playOnAwake = false;
            bladeAudioSource.spatialBlend = 1f;

            // -------------------------------------
            // IMPACT PROPERTIES PLACEHOLDER
            // -------------------------------------

            GameObject impactProperties =
                new GameObject("ImpactProperties");

            impactProperties.transform.SetParent(
                root.transform,
                false
            );

            GameObject soundsExt =
                new GameObject("SoundsExt");

            soundsExt.transform.SetParent(
                root.transform,
                false
            );

            // -------------------------------------
            // MOTOR
            // -------------------------------------

            ChainsawMotor motor =
                root.AddComponent<ChainsawMotor>();

            motor.bladeTransform =
                bladeTransform.transform;

            motor.idleSound =
                idleAudio;

            motor.bladeAudio =
                bladeAudioSource;

            damage.motor = motor;

            Selection.activeGameObject = root;

            Debug.Log(
                "[ChainsawPort] Reconstructed Chainsaw hierarchy created."
            );
        }
    }
}

#endif
