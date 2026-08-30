using System.Collections.Generic;
using UnityEngine;

namespace ChainsawPort
{
    public class ChainsawDamage : MonoBehaviour
    {
        public ChainsawMotor motor;

        [Header("Damage")]
        public float damagePerHit = 7.5f;
        public float hitInterval = 0.075f;

        [Header("Detection")]
        public float hitRadius = 0.08f;

        private readonly Dictionary<GameObject, float> nextHitTimes =
            new Dictionary<GameObject, float>();

        private void Awake()
        {
            if (motor == null)
                motor = GetComponentInParent<ChainsawMotor>();
        }

        private void OnTriggerStay(Collider other)
        {
            if (motor == null || !motor.motorRunning)
                return;

            if (other == null)
                return;

            GameObject target = other.gameObject;

            float nextTime;

            if (nextHitTimes.TryGetValue(target, out nextTime))
            {
                if (Time.time < nextTime)
                    return;
            }

            nextHitTimes[target] = Time.time + hitInterval;

            Rigidbody rb = other.attachedRigidbody;

            if (rb != null)
            {
                Vector3 direction =
                    (other.transform.position - transform.position).normalized;

                rb.AddForce(
                    direction * 1.5f,
                    ForceMode.Impulse
                );
            }

            // BONELAB-specific NPC damage is added through
            // the Marrow/BoneLib component once the current
            // BONELAB SDK references are present.
        }
    }
}
