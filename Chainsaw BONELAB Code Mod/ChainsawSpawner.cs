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
                string bundlePath = FindBundlePath();
                if (string.IsNullOrEmpty(bundlePath))
                {
                    MelonLogger.Warning("[Chainsaw] chainsaw.bundle was not found in any expected Quest Mods path.");
                    MelonLogger.Warning("[Chainsaw] Expected: " + Path.Combine(Application.persistentDataPath, "Mods", "Chainsaw", "chainsaw.bundle"));
                    return;
                }

                MelonLogger.Msg("[Chainsaw] Loading bundle from: " + bundlePath);

                bundle = AssetBundle.LoadFromFile(bundlePath);
                if (bundle == null)
                {
                    MelonLogger.Error("[Chainsaw] Failed to load chainsaw.bundle. The old bundle may be incompatible with the current BONELAB/Unity runtime.");
                    return;
                }

                string[] assetNames = bundle.GetAllAssetNames();
                if (assetNames != null)
                {
                    MelonLogger.Msg("[Chainsaw] Bundle asset count: " + assetNames.Length);
                    foreach (string assetName in assetNames)
                        MelonLogger.Msg("[Chainsaw] Bundle asset: " + assetName);
                }

                prefab = TryLoadPrefab("Chainsaw.prefab");
                if (prefab == null)
                    prefab = TryLoadPrefab("Chainsaw");

                if (prefab == null && assetNames != null)
                {
                    foreach (string assetName in assetNames)
                    {
                        if (string.IsNullOrEmpty(assetName))
                            continue;

                        GameObject candidate = TryLoadPrefab(assetName);
                        if (candidate != null)
                        {
                            prefab = candidate;
                            MelonLogger.Msg("[Chainsaw] Using GameObject asset: " + assetName);
                            break;
                        }
                    }
                }

                if (prefab == null)
                {
                    MelonLogger.Error("[Chainsaw] Bundle loaded, but no GameObject prefab could be loaded from it.");
                    return;
                }

                MelonLogger.Msg("[Chainsaw] Spawn prefab loaded: " + prefab.name);
            }
            catch (Exception ex)
            {
                MelonLogger.Error("[Chainsaw] AssetBundle load failed: " + ex);
            }
        }

        private static string FindBundlePath()
        {
            string persistentPath = Path.Combine(
                Application.persistentDataPath,
                "Mods",
                "Chainsaw",
                "chainsaw.bundle");

            if (File.Exists(persistentPath))
                return persistentPath;

            string currentPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Mods",
                "Chainsaw",
                "chainsaw.bundle");

            if (File.Exists(currentPath))
                return currentPath;

            string explicitQuestPath = "/storage/emulated/0/Android/data/com.StressLevelZero.BONELAB/files/Mods/Chainsaw/chainsaw.bundle";
            if (File.Exists(explicitQuestPath))
                return explicitQuestPath;

            return null;
        }

        private static GameObject TryLoadPrefab(string assetName)
        {
            try
            {
                return bundle.LoadAsset<GameObject>(assetName);
            }
            catch
            {
                return null;
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
                MelonLogger.Warning("[Chainsaw] Can't spawn because chainsaw.bundle is missing, incompatible, or contains no loadable GameObject prefab.");
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
            if (chainsaw == null)
            {
                MelonLogger.Error("[Chainsaw] Instantiate returned null.");
                return;
            }

            chainsaw.name = "Chainsaw";
            chainsaw.SetActive(true);
            spawnedChainsaws.Add(chainsaw);

            MelonLogger.Msg("[Chainsaw] Spawned Chainsaw from BoneMenu at " + position);
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
