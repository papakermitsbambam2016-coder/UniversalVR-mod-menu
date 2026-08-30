using UnityEngine;

namespace ChainsawPort
{
    public class ChainsawMotor : MonoBehaviour
    {
        [Header("Blade")]
        public Transform bladeTransform;

        [Tooltip("Visual blade rotation speed.")]
        public float bladeSpeed = 1800f;

        [Header("Audio")]
        public AudioSource idleSound;
        public AudioSource bladeAudio;

        [Header("State")]
        public bool motorRunning;

        private void Awake()
        {
            if (bladeTransform == null)
            {
                Transform found = transform.Find("BladeTransform");

                if (found != null)
                    bladeTransform = found;
            }

            if (idleSound == null)
            {
                Transform audioObject = transform.Find("IdleSound");

                if (audioObject != null)
                    idleSound = audioObject.GetComponent<AudioSource>();
            }

            if (bladeAudio == null)
            {
                Transform audioObject = transform.Find("BladeAudio");

                if (audioObject != null)
                    bladeAudio = audioObject.GetComponent<AudioSource>();
            }

            StopMotor();
        }

        private void Update()
        {
            if (!motorRunning)
                return;

            if (bladeTransform != null)
            {
                bladeTransform.Rotate(
                    Vector3.right,
                    bladeSpeed * Time.deltaTime,
                    Space.Self
                );
            }
        }

        public void StartMotor()
        {
            motorRunning = true;

            if (idleSound != null)
            {
                idleSound.loop = true;

                if (!idleSound.isPlaying)
                    idleSound.Play();
            }

            if (bladeAudio != null)
            {
                bladeAudio.loop = true;

                if (!bladeAudio.isPlaying)
                    bladeAudio.Play();
            }
        }

        public void StopMotor()
        {
            motorRunning = false;

            if (idleSound != null && idleSound.isPlaying)
                idleSound.Stop();

            if (bladeAudio != null && bladeAudio.isPlaying)
                bladeAudio.Stop();
        }

        public void ToggleMotor()
        {
            if (motorRunning)
                StopMotor();
            else
                StartMotor();
        }
    }
}
