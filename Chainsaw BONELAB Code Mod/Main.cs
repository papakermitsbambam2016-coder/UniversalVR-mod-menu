using System;
using System.Collections.Generic;
using BoneLib;
using Il2CppSLZ.Marrow.AI;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(ChainsawBONELABCodeMod.Main), "Chainsaw BONELAB Code Mod", "1.0.0", "TankFullOfOofs Port")]
[assembly: MelonGame("Stress Level Zero", "BONELAB")]

namespace ChainsawBONELABCodeMod
{
    public sealed class Main : MelonMod
    {
        private readonly Dictionary<Hand, ChainsawRuntime> held = new Dictionary<Hand, ChainsawRuntime>();
        private float damageTimer;

        public override void OnInitializeMelon()
        {
            Hooking.OnGrabObject += OnGrab;
            Hooking.OnReleaseObject += OnRelease;
            MelonLogger.Msg("Chainsaw BONELAB Code Mod 1.0.0 initialized.");
        }

        public override void OnDeinitializeMelon()
        {
            Hooking.OnGrabObject -= OnGrab;
            Hooking.OnReleaseObject -= OnRelease;
            foreach (var state in held.Values) state.Stop();
            held.Clear();
        }

        public override void OnUpdate()
        {
            float dt = Time.deltaTime;
            damageTimer -= dt;
            foreach (var state in held.Values) state.Tick(dt);
            if (damageTimer <= 0f)
            {
                damageTimer = 0.075f;
                foreach (var state in held.Values) state.DamageNearby();
            }
        }

        private void OnGrab(GameObject objectToAttach, Hand hand)
        {
            if (objectToAttach == null || hand == null || !LooksLikeChainsaw(objectToAttach)) return;
            if (held.TryGetValue(hand, out var old)) old.Stop();
            var state = new ChainsawRuntime(objectToAttach);
            held[hand] = state;
            state.Start();
            MelonLogger.Msg("Chainsaw grabbed; runtime behavior enabled.");
        }

        private void OnRelease(Hand hand)
        {
            if (hand == null) return;
            if (held.TryGetValue(hand, out var state))
            {
                state.Stop();
                held.Remove(hand);
                MelonLogger.Msg("Chainsaw released; runtime behavior disabled.");
            }
        }

        private static bool LooksLikeChainsaw(GameObject go)
        {
            string name = go.name ?? string.Empty;
            if (name.IndexOf("chainsaw", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            foreach (var t in go.GetComponentsInChildren<Transform>(true))
            {
                if (t == null) continue;
                if ((t.name ?? string.Empty).IndexOf("chainsaw", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            }
            return false;
        }

        private sealed class ChainsawRuntime
        {
            private readonly GameObject root;
            private readonly List<Transform> blades = new List<Transform>();
            private readonly List<Collider> bladeColliders = new List<Collider>();
            private readonly List<AudioSource> motorSounds = new List<AudioSource>();
            private bool running;

            public ChainsawRuntime(GameObject rootObject)
            {
                root = rootObject;
                foreach (var t in root.GetComponentsInChildren<Transform>(true))
                {
                    if (t == null) continue;
                    string n = t.name ?? string.Empty;
                    if (n.IndexOf("BladeTransform", StringComparison.OrdinalIgnoreCase) >= 0 || n.IndexOf("Blade", StringComparison.OrdinalIgnoreCase) >= 0)
                        blades.Add(t);
                }
                foreach (var c in root.GetComponentsInChildren<Collider>(true))
                {
                    if (c == null) continue;
                    string n = c.name ?? string.Empty;
                    if (n.IndexOf("BladeCollider", StringComparison.OrdinalIgnoreCase) >= 0 || n.IndexOf("Blade", StringComparison.OrdinalIgnoreCase) >= 0)
                        bladeColliders.Add(c);
                }
                foreach (var audio in root.GetComponentsInChildren<AudioSource>(true))
                {
                    if (audio == null) continue;
                    string n = audio.name ?? string.Empty;
                    if (n.IndexOf("IdleSound", StringComparison.OrdinalIgnoreCase) >= 0 || n.IndexOf("Motor", StringComparison.OrdinalIgnoreCase) >= 0 || n.IndexOf("Chainsaw", StringComparison.OrdinalIgnoreCase) >= 0)
                        motorSounds.Add(audio);
                }
            }

            public void Start()
            {
                running = true;
                foreach (var audio in motorSounds)
                {
                    try
                    {
                        audio.loop = true;
                        if (audio.clip != null && !audio.isPlaying) audio.Play();
                    }
                    catch { }
                }
            }

            public void Stop()
            {
                running = false;
                foreach (var audio in motorSounds)
                {
                    try { if (audio.isPlaying) audio.Stop(); } catch { }
                }
            }

            public void Tick(float dt)
            {
                if (!running || root == null) return;
                foreach (var blade in blades)
                    if (blade != null) blade.Rotate(Vector3.forward, 1800f * dt, Space.Self);
            }

            public void DamageNearby()
            {
                if (!running) return;
                foreach (var collider in bladeColliders)
                {
                    if (collider == null || !collider.enabled) continue;
                    Vector3 center = collider.bounds.center;
                    float radius = Mathf.Max(0.035f, collider.bounds.extents.magnitude * 0.45f);
                    foreach (var hit in Physics.OverlapSphere(center, radius))
                    {
                        if (hit == null) continue;
                        var brain = hit.GetComponentInParent<AIBrain>();
                        if (brain == null) continue;
                        try { brain.DealDamage(7.5f); }
                        catch (Exception e) { MelonLogger.Warning("Chainsaw damage failed: " + e.Message); }
                    }
                }
            }
        }
    }
}
