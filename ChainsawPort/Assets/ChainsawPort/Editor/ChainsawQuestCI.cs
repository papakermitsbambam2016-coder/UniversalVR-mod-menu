#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ChainsawPort.Editor
{
    public static class ChainsawQuestCI
    {
        public static void PrepareQuestBuild()
        {
            const string root = "Assets/ChainsawPort";
            const string source = root + "/Source";
            const string prefabs = root + "/Prefabs";
            const string pallet = root + "/Pallet";
            const string packed = root + "/Packed";

            EnsureFolder(root);
            EnsureFolder(source);
            EnsureFolder(prefabs);
            EnsureFolder(pallet);
            EnsureFolder(packed);

            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

            string report =
                "Chainsaw Quest CI preparation completed.\n" +
                "Unity: " + Application.unityVersion + "\n" +
                "Target: Android / ARM64 / IL2CPP\n" +
                "Expected spawnable barcode: TankFullOfOofs.Chainsaw.Spawnable.Chainsaw\n\n" +
                "IMPORTANT:\n" +
                "This CI step validates/prepares the Unity project, but it does NOT pretend to create a current BONELAB pallet when a current compatible MarrowSDK/Extended SDK pallet and reconstructed prefab are missing.\n" +
                "After the reconstructed model/materials/audio and compatible Marrow pallet are present, the final step must invoke the SDK's Pack for Quest operation.\n";

            File.WriteAllText(Path.Combine(packed, "QUEST-CI-REPORT.txt"), report);
            AssetDatabase.Refresh();

            Debug.Log("[ChainsawPort] Quest CI preparation complete.");
        }

        private static void EnsureFolder(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
#endif
