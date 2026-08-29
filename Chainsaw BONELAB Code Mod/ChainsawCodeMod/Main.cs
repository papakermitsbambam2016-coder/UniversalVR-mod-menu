using System;
using System.Collections.Generic;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(ChainsawBONELABCodeMod.Main), "Chainsaw BONELAB Code Mod", "0.1.0", "TankFullOfOofs Port")]
[assembly: MelonGame("Stress Level Zero", "BONELAB")]

namespace ChainsawBONELABCodeMod
{
    public class Main : MelonMod
    {
        private readonly List<Transform> blades = new List<Transform>();
        private float scanTimer;
        private bool active;

        public override void OnInitializeMelon()
        {
            MelonLogger.Msg("Chainsaw BONELAB Code Mod initialized.");
            MelonLogger.Msg("Waiting for Chainsaw objects...");
        }

        public override void OnUpdate()
        {
            scanTimer -= Time.deltaTime;
            if (scanTimer <= 0f)
            {
                scanTimer = 2f;
                ScanForChainsaws();
            }

            if (!active) return;

            for (int i = blades.Count - 1; i >= 0; i--)
            {
                if (blades[i] == null)
                {
                    blades.RemoveAt(i);
                    continue;
                }
                blades[i].Rotate(Vector3.forward, 1800f * Time.deltaTime, Space.Self);
            }
        }

        private void ScanForChainsaws()
        {
            blades.Clear();
            try
            {
                var transforms = UnityEngine.Object.FindObjectsOfType<Transform>();
                foreach (var t in transforms)
                {
                    if (t == null) continue;
                    string n = t.name ?? string.Empty;
                    if (n.IndexOf("chainsaw", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        n.IndexOf("blade", StringComparison.OrdinalIgnoreCase) >= 0)
                        blades.Add(t);
                }
                active = blades.Count > 0;
                if (active) MelonLogger.Msg($"Found {blades.Count} possible Chainsaw/blade transform(s).");
            }
            catch (Exception e)
            {
                active = false;
                MelonLogger.Warning("Chainsaw scan failed: " + e.Message);
            }
        }
    }
}
