using UnityEditor;
using UnityEngine;

namespace ChainsawPort
{
    public static class ChainsawPortSetup
    {
        [MenuItem("Chainsaw Port/Create Current SDK Migration Folders")]
        public static void CreateMigrationFolders()
        {
            Ensure("Assets/ChainsawPort");
            Ensure("Assets/ChainsawPort/Source");
            Ensure("Assets/ChainsawPort/Source/Models");
            Ensure("Assets/ChainsawPort/Source/Materials");
            Ensure("Assets/ChainsawPort/Source/Audio");
            Ensure("Assets/ChainsawPort/Prefabs");
            Ensure("Assets/ChainsawPort/Pallet");
            Ensure("Assets/ChainsawPort/PreviewMesh");
            Ensure("Assets/ChainsawPort/Packed");
            AssetDatabase.Refresh();
            Debug.Log("Chainsaw current-SDK migration folders created. Target pallet format: version 2 / SDK 1.2.0 reference.");
        }

        [MenuItem("Chainsaw Port/Create Chainsaw Prefab Skeleton")]
        public static void CreatePrefabSkeleton()
        {
            CreateMigrationFolders();

            var root = new GameObject("Chainsaw");
            var body = root.AddComponent<Rigidbody>();
            body.mass = 4f;

            var bodyCollider = root.AddComponent<BoxCollider>();
            bodyCollider.center = new Vector3(0.0278568268f, 0.08067175f, -0.4257303f);
            bodyCollider.size = new Vector3(0.26797703f, 0.35492572f, 1.0486611f);

            CreateChild(root.transform, "Visuals");

            var gripPoint = CreateChild(root.transform, "GripPoint");
            CreateChild(gripPoint.transform, "GripCollider").AddComponent<BoxCollider>();

            var blade = CreateChild(root.transform, "Blade");
            CreateChild(blade.transform, "BladeTransform");
            CreateChild(blade.transform, "BladeCollider").AddComponent<BoxCollider>();
            CreateChild(blade.transform, "slashTop");
            CreateChild(blade.transform, "slashBottom");
            CreateChild(blade.transform, "StabPoint");

            var audioRoot = CreateChild(root.transform, "Audio");
            CreateChild(audioRoot.transform, "IdleSound").AddComponent<AudioSource>();
            CreateChild(audioRoot.transform, "BladeAudio").AddComponent<AudioSource>();

            CreateChild(root.transform, "Pull Cord");
            CreateChild(root.transform, "ImpactProperties");
            CreateChild(root.transform, "SoundsExt");

            const string prefabPath = "Assets/ChainsawPort/Prefabs/Chainsaw.prefab";
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Created Chainsaw prefab skeleton at " + prefabPath + ". Next: assign recovered model/audio and add the current Marrow SDK Spawnable/Grip/interaction components before Pack for Quest.");
        }

        private static GameObject CreateChild(Transform parent, string name)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent, false);
            return child;
        }

        private static void Ensure(string path)
        {
            var parts = path.Split('/');
            var current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
