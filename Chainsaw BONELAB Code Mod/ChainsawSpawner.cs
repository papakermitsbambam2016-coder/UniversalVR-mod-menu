using System;
using System.Collections.Generic;
using System.IO;
using MelonLoader;
using UnityEngine;

namespace ChainsawBONELABCodeMod
{
    internal static class ChainsawSpawner
    {
        private static AssetBundle bundle;
        private static GameObject prefab;
        private static readonly List<GameObject> spawnedChainsaws = new List<GameObject>();

        public static void Initialize()
        {
            if (prefab != null)
                return;

            try
            {
                string bundlePath = Path.Combine(
                    MelonEnvironment.GameRootDirectory,
                    "Mods",
                    "Chainsaw",
                    "chainsaw.bundle");

                if (!File.Exists(bundlePath))
                {
                    MelonLogger.Warning("[Chainsaw] chainsaw.bundle not found yet: " + bundlePath);
                    return;
                }

                bundle = AssetBundle.LoadFromFile(bundlePath);
                if (bundle == null)
                {
                    MelonLogger.Error("[Chainsaw] Failed to load chainsaw.bundle.");
                    return;
                }

                prefab = bundle.LoadAsset<GameObject>("Chainsaw.prefab");
                if (prefab == null)
                    prefab = bundle.LoadAsset<GameObject>("Chainsaw");

                if (prefab == null)
                {
                    GameObject[] allPrefabs = bundle.LoadAllAssets<GameObject>();
                    if (allPrefabs != null && allPrefabs.Length > 0)
                        prefab = allPrefabs[0];
                }

                if (prefab == null)
                {
                    MelonLogger.Error("[Chainsaw] No GameObject prefab was found in chainsaw.bundle.");
                    return;
                }

                MelonLogger.Msg("[Chainsaw] Spawn prefab loaded: " + prefab.name);
            }
            catch (Exception ex)
            {
                MelonLogger.Error("[Chainsaw] AssetBundle load failed: " + ex);
            }
        }

        public static void Spawn()
        {
            if (!Config.Enabled)
            {
                MelonLogger.Warning("[Chainsaw] Mod is disabled in BoneMenu.");
                return;
            }

            if (prefab == null)
                Initialize();

            if (prefab == null)
            {
                MelonLogger.Warning("[Chainsaw] Can't spawn yet because chainsaw.bundle is missing or invalid.");
                return;
            }

            Camera camera = Camera.main;
            if (camera == null)
            {
                MelonLogger.Warning("[Chainsaw] Main camera/player view is not ready.");
                return;
            }

            Vector3 position = camera.transform.position + camera.transform.forward * 1.5f;
            Quaternion rotation = Quaternion.LookRotation(camera.transform.forward, Vector3.up);

            GameObject chainsaw = UnityEngine.Object.Instantiate(prefab, position, rotation);
            chainsaw.name = "Chainsaw";
            spawnedChainsaws.Add(chainsaw);

            MelonLogger.Msg("[Chainsaw] Spawned Chainsaw from BoneMenu.");
        }

        public static void DespawnAll()
        {
            for (int i = spawnedChainsaws.Count - 1; i >= 0; i--)
            {
                GameObject chainsaw = spawnedChainsaws[i];
                if (chainsaw != null)
                    UnityEngine.Object.Destroy(chainsaw);
            }

            spawnedChainsaws.Clear();
            MelonLogger.Msg("[Chainsaw] Despawned all BoneMenu-spawned Chainsaws.");
        }
    }
}
