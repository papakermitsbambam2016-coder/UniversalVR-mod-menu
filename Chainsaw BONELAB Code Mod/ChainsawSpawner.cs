using System;
using System.Collections.Generic;
using System.IO;
using Il2CppInterop.Runtime;
using MelonLoader;
using UnityEngine;

namespace ChainsawBONELABCodeMod
{
    internal static class ChainsawSpawner
    {
        private const string LegacyQuestAssetKey = "ddc0b40df5ea8ad44aaac0d714b4eb8e";
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

                MelonLogger.Msg("[Chainsaw] Trying known legacy Quest asset key: " + LegacyQuestAssetKey);
                prefab = TryLoadPrefab(LegacyQuestAssetKey);
                if (prefab == null)
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
                UnityEngine.Object loaded = bundle.LoadAsset(assetName, Il2CppType.Of<GameObject>());
                if (loaded == null)
                {
                    MelonLogger.Warning("[Chainsaw] Asset entry returned null: " + assetName);
                    return null;
                }

                GameObject gameObject = loaded.TryCast<GameObject>();
                if (gameObject == null)
                {
                    MelonLogger.Warning("[Chainsaw] Asset was not a GameObject: " + assetName + " (type=" + loaded.GetType().FullName + ")");
                    return null;
                }

                MelonLogger.Msg("[Chainsaw] Loaded GameObject entry: " + assetName + " -> " + gameObject.name);
                return gameObject;
            }
            catch (Exception ex)
            {
                MelonLogger.Warning("[Chainsaw] Failed loading asset entry " + assetName + ": " + ex.GetType().Name + ": " + ex.Message);
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
