using System;
using System.Collections.Generic;
using BoneLib;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.AI;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(ChainsawBONELABCodeMod.Main), "Chainsaw BONELAB Code Mod", "1.3.0", "TankFullOfOofs Port")]
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

            ChainsawSpawner.Initialize();
            ChainsawMenu.Setup();

            MelonLogger.Msg("Chainsaw BONELAB Code Mod 1.3.0 initialized. BoneMenu spawning support enabled.");
        }

        public override void OnDeinitializeMelon()
        {
            Hooking.OnGrabObject -= OnGrab;
            Hooking.OnReleaseObject -= OnRelease;

            foreach (var state in held.Values)
                state.Stop();

            held.Clear();
            ChainsawSpawner.DespawnAll();
        }

        public override void OnUpdate()
        {
            if (!Config.Enabled)
                return;

            float dt = Time.deltaTime;
            damageTimer -= dt;

            foreach (var state in held.Values)
                state.Tick(dt);

            if (damageTimer > 0f)
                return;

            damageTimer = Mathf.Max(0.025f, Config.DamageInterval);

            foreach (var state in held.Values)
                state.DamageNearby();
        }

        private void OnGrab(GameObject objectToAttach, Hand hand)
        {
            if (!Config.Enabled || objectToAttach == null || hand == null)
                return;

            GameObject chainsawRoot = ResolveChainsawRoot(objectToAttach);
            if (chainsawRoot == null)
                return;

            ChainsawRuntime old;
            if (held.TryGetValue(hand, out old))
                old.Stop();

            var state = new ChainsawRuntime(chainsawRoot);
            held[hand] = state;
            state.Start();

            MelonLogger.Msg(
                "Chainsaw detected: " + chainsawRoot.name +
                ". Blades=" + state.BladeCount +
                ", BladeColliders=" + state.BladeColliderCount +
                ", MotorSounds=" + state.MotorSoundCount);
        }

        private void OnRelease(Hand hand)
        {
            if (hand == null)
                return;

            ChainsawRuntime state;
            if (!held.TryGetValue(hand, out state))
                return;

            state.Stop();
            held.Remove(hand);
            MelonLogger.Msg("Chainsaw released; runtime behavior disabled.");
        }

        private static bool LooksLikeChainsawName(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            return value.IndexOf("chainsaw", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   value.IndexOf("DoomHunterChainsaw", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   value.IndexOf("Doom Hunter Chainsaw", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool IsDoomHunterMarker(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            return value.IndexOf("slashTop", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   value.IndexOf("slashBottom", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   value.IndexOf("Pull Cord", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   value.IndexOf("StabPoint", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static GameObject ResolveChainsawRoot(GameObject grabbed)
        {
            Transform current = grabbed.transform;
            GameObject highestLikelyRoot = null;

            while (current != null)
            {
                if (LooksLikeChainsawName(current.name))
                    highestLikelyRoot = current.gameObject;

                current = current.parent;
            }

            if (highestLikelyRoot != null)
                return highestLikelyRoot;

            Transform searchRoot = grabbed.transform;
            while (searchRoot.parent != null && searchRoot.parent.parent != null)
                searchRoot = searchRoot.parent;

            Transform[] children = searchRoot.GetComponentsInChildren<Transform>(true);
            int doomMarkers = 0;
            bool hasBlade = false;
            bool hasGrip = false;

            foreach (Transform child in children)
            {
                if (child == null)
                    continue;

                string n = child.name ?? string.Empty;

                if (IsDoomHunterMarker(n))
                    doomMarkers++;

                if (n.IndexOf("Blade", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    n.IndexOf("slashTop", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    n.IndexOf("slashBottom", StringComparison.OrdinalIgnoreCase) >= 0)
                    hasBlade = true;

                if (n.IndexOf("Grip", StringComparison.OrdinalIgnoreCase) >= 0)
                    hasGrip = true;
            }

            if (doomMarkers >= 2 && hasBlade && hasGrip)
                return searchRoot.gameObject;

            Transform[] grabbedChildren = grabbed.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in grabbedChildren)
            {
                if (child == null)
                    continue;

                string name = child.name ?? string.Empty;
                if (name.IndexOf("BladeTransform", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    name.IndexOf("BladeCollider", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    name.IndexOf("IdleSound", StringComparison.OrdinalIgnoreCase) >= 0)
                    return grabbed;
            }

            return null;
        }

        private sealed class ChainsawRuntime
        {
            private readonly GameObject root;
            private readonly List<Transform> blades = new List<Transform>();
            private readonly List<Collider> bladeColliders = new List<Collider>();
            private readonly List<AudioSource> motorSounds = new List<AudioSource>();
            private bool running;

            public int BladeCount { get { return blades.Count; } }
            public int BladeColliderCount { get { return bladeColliders.Count; } }
            public int MotorSoundCount { get { return motorSounds.Count; } }

            public ChainsawRuntime(GameObject rootObject)
            {
                root = rootObject;

                Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
                foreach (Transform t in transforms)
                {
                    if (t == null)
                        continue;

                    string n = t.name ?? string.Empty;
                    if (n.IndexOf("BladeTransform", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        string.Equals(n, "Blade", StringComparison.OrdinalIgnoreCase) ||
                        n.IndexOf("slashTop", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        n.IndexOf("slashBottom", StringComparison.OrdinalIgnoreCase) >= 0)
                        blades.Add(t);
                }

                Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
                foreach (Collider c in colliders)
                {
                    if (c == null)
                        continue;

                    string n = c.name ?? string.Empty;
                    if (n.IndexOf("BladeCollider", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        n.IndexOf("Blade", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        n.IndexOf("slashTop", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        n.IndexOf("slashBottom", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        n.IndexOf("StabPoint", StringComparison.OrdinalIgnoreCase) >= 0)
                        bladeColliders.Add(c);
                }

                AudioSource[] audioSources = root.GetComponentsInChildren<AudioSource>(true);
                foreach (AudioSource audio in audioSources)
                {
                    if (audio == null)
                        continue;

                    string n = audio.name ?? string.Empty;
                    if (n.IndexOf("IdleSound", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        n.IndexOf("BladeAudio", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        n.IndexOf("Motor", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        n.IndexOf("Chainsaw", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        n.IndexOf("Idle", StringComparison.OrdinalIgnoreCase) >= 0)
                        motorSounds.Add(audio);
                }
            }

            public void Start()
            {
                running = true;

                if (!Config.MotorSound)
                    return;

                foreach (AudioSource audio in motorSounds)
                {
                    try
                    {
                        audio.loop = true;
                        if (audio.clip != null && !audio.isPlaying)
                            audio.Play();
                    }
                    catch (Exception e)
                    {
                        MelonLogger.Warning("Chainsaw audio start failed: " + e.Message);
                    }
                }
            }

            public void Stop()
            {
                running = false;

                foreach (AudioSource audio in motorSounds)
                {
                    try
                    {
                        if (audio != null && audio.isPlaying)
                            audio.Stop();
                    }
                    catch { }
                }
            }

            public void Tick(float dt)
            {
                if (!running || root == null)
                    return;

                foreach (Transform blade in blades)
                {
                    if (blade != null)
                        blade.Rotate(Vector3.forward, Config.BladeDegreesPerSecond * dt, Space.Self);
                }
            }

            public void DamageNearby()
            {
                if (!running || root == null)
                    return;

                var damagedBrains = new HashSet<AIBrain>();

                foreach (Collider collider in bladeColliders)
                {
                    if (collider == null || !collider.enabled)
                        continue;

                    Vector3 center = collider.bounds.center;
                    float radius = Mathf.Max(0.035f, collider.bounds.extents.magnitude * 0.45f);
                    Collider[] hits = Physics.OverlapSphere(center, radius);

                    foreach (Collider hit in hits)
                    {
                        if (hit == null || hit.transform.IsChildOf(root.transform))
                            continue;

                        AIBrain brain = hit.GetComponentInParent<AIBrain>();
                        if (brain == null || !damagedBrains.Add(brain))
                            continue;

                        try
                        {
                            brain.DealDamage(Config.Damage);
                        }
                        catch (Exception e)
                        {
                            MelonLogger.Warning("Chainsaw damage failed: " + e.Message);
                        }
                    }
                }
            }
        }
    }
}
