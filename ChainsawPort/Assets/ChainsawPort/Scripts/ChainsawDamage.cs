using System.Collections.Generic;
using UnityEngine;

namespace ChainsawPort
{
    public class ChainsawDamage : MonoBehaviour
    {
        public ChainsawMotor motor;
        public float damagePerHit = 7.5f;
        public float hitInterval = 0.075f;
        public float impulse = 1.5f;

        private readonly Dictionary<int, float> nextHitTimes = new Dictionary<int, float>();

        private void Awake()
        {
            if (motor == null)
                motor = GetComponentInParent<ChainsawMotor>();
        }

        private void OnTriggerStay(Collider other)
        {
            if (motor == null || !motor.motorRunning || other == null)
                return;

            int id = other.GetInstanceID();
            float nextTime;
            if (nextHitTimes.TryGetValue(id, out nextTime) && Time.time < nextTime)
                return;

            nextHitTimes[id] = Time.time + hitInterval;

            Rigidbody rb = other.attachedRigidbody;
            if (rb != null)
            {
                Vector3 direction = (other.bounds.center - transform.position).normalized;
                rb.AddForce(direction * impulse, ForceMode.Impulse);
            }

            // Current BONELAB/Marrow NPC damage should be wired here only after
            // the exact SDK/Extended SDK assemblies used by the target game build
            // are present. This avoids compiling against obsolete legacy APIs.
        }
    }
}
