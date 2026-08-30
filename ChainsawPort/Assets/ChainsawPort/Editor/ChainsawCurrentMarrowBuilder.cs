#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ChainsawPort.Editor
{
    public static class ChainsawCurrentMarrowBuilder
    {
        private const string PrefabPath = "Assets/ChainsawPort/Prefabs/Chainsaw.prefab";
        private const string PalletFolder = "Assets/ChainsawPort/Pallet";
        private const string PreviewFolder = "Assets/ChainsawPort/PreviewMesh";
        private const string PalletBarcode = "TankFullOfOofs.Chainsaw";
        private const string SpawnableBarcode = "TankFullOfOofs.Chainsaw.Spawnable.Chainsaw";

        [MenuItem("Chainsaw Port/4 - Add Current Marrow Components")]
        public static void AddCurrentMarrowComponents()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
            {
                Debug.LogError("[ChainsawPort] Chainsaw.prefab is missing.");
                return;
            }

            GameObject root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                AddFirstAvailableComponent(root,
                    "SLZ.Marrow.Pool.Poolee",
                    "SLZ.Marrow.Pool.Spawnable",
                    "SLZ.Marrow.Warehouse.Spawnable");

                Transform gripPoint = Find(root.transform, "GripPoint");
                if (gripPoint != null)
                {
                    AddFirstAvailableComponent(gripPoint.gameObject,
                        "SLZ.Marrow.Interaction.Grip",
                        "SLZ.Interaction.Grip");
                }

                Transform blade = Find(root.transform, "Blade");
                if (blade != null)
                {
                    AddFirstAvailableComponent(blade.gameObject,
                        "SLZ.Combat.StabSlash",
                        "SLZ.Marrow.Combat.StabSlash");
                }

                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                AssetDatabase.SaveAssets();
                Debug.Log("[ChainsawPort] Marrow component pass finished. Check the Console: every resolved type was added; unavailable types are reported instead of breaking the project.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        [MenuItem("Chainsaw Port/5 - Create Current Pallet And Spawnable Crate")]
        public static void CreateWarehouseAssets()
        {
            EnsureFolder(PalletFolder);
            EnsureFolder(PreviewFolder);

            Type palletType = FindType("SLZ.Marrow.Warehouse.Pallet");
            Type crateType = FindType("SLZ.Marrow.Warehouse.SpawnableCrate");

            if (palletType == null || crateType == null)
            {
                Debug.LogError("[ChainsawPort] Current Marrow Warehouse types were not found. Install/load the current compatible Marrow/Extended SDK first. Expected: SLZ.Marrow.Warehouse.Pallet and SLZ.Marrow.Warehouse.SpawnableCrate.");
                return;
            }

            if (!typeof(ScriptableObject).IsAssignableFrom(palletType) || !typeof(ScriptableObject).IsAssignableFrom(crateType))
            {
                Debug.LogError("[ChainsawPort] Warehouse types were found but are not ScriptableObject assets in this SDK build. Use the SDK's Warehouse editor and the generated metadata reference instead.");
                return;
            }

            string palletPath = PalletFolder + "/ChainsawPallet.asset";
            string cratePath = PalletFolder + "/ChainsawSpawnableCrate.asset";

            ScriptableObject pallet = AssetDatabase.LoadAssetAtPath<ScriptableObject>(palletPath);
            if (pallet == null)
            {
                pallet = ScriptableObject.CreateInstance(palletType);
                pallet.name = "Chainsaw Pallet";
                AssetDatabase.CreateAsset(pallet, palletPath);
            }

            ScriptableObject crate = AssetDatabase.LoadAssetAtPath<ScriptableObject>(cratePath);
            if (crate == null)
            {
                crate = ScriptableObject.CreateInstance(crateType);
                crate.name = "Chainsaw Spawnable Crate";
                AssetDatabase.CreateAsset(crate, cratePath);
            }

            SetStringLike(pallet, "barcode", PalletBarcode);
            SetStringLike(pallet, "title", "Chainsaw");
            SetStringLike(pallet, "author", "TankFullOfOofs Port");
            SetStringLike(pallet, "version", "1.0.0");
            SetStringLike(crate, "barcode", SpawnableBarcode);
            SetStringLike(crate, "title", "Chainsaw");
            SetStringLike(crate, "description", "Spawnable Chainsaw for BONELAB Quest.");

            TryAssignObjectLike(crate, "mainAsset", AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath));
            TryAssignObjectLike(crate, "spawnable", AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath));
            TryAssignObjectLike(crate, "prefab", AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath));
            TryAddCrateToPallet(pallet, crate);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            WriteGuidReport();
            Debug.Log("[ChainsawPort] Created/updated current Marrow Pallet + SpawnableCrate assets. Open them in the Inspector, confirm the prefab/preview fields, then use the SDK's actual Pack for Quest command.");
        }

        [MenuItem("Chainsaw Port/6 - Write Current Pallet GUID Report")]
        public static void WriteGuidReport()
        {
            string prefabGuid = AssetDatabase.AssetPathToGUID(PrefabPath);
            string previewGuid = FindPreviewMeshGuid();
            string path = "ChainsawPort/CurrentReference/GENERATED-GUIDS.txt";
            string absolute = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "CurrentReference/GENERATED-GUIDS.txt"));

            Directory.CreateDirectory(Path.GetDirectoryName(absolute));
            File.WriteAllText(absolute,
                "TankFullOfOofs Chainsaw current rebuild GUIDs\n" +
                "PrefabPath=" + PrefabPath + "\n" +
                "PrefabGUID=" + prefabGuid + "\n" +
                "PreviewMeshGUID=" + previewGuid + "\n" +
                "PalletBarcode=" + PalletBarcode + "\n" +
                "SpawnableBarcode=" + SpawnableBarcode + "\n" +
                "TargetSdkVersion=1.2.0\n" +
                "TargetPalletFormat=2\n");

            AssetDatabase.Refresh();
            Debug.Log("[ChainsawPort] Wrote GUID report: " + path);
        }

        private static void AddFirstAvailableComponent(GameObject target, params string[] typeNames)
        {
            foreach (string typeName in typeNames)
            {
                Type type = FindType(typeName);
                if (type == null || !typeof(Component).IsAssignableFrom(type))
                    continue;

                if (target.GetComponent(type) == null)
                    target.AddComponent(type);
                Debug.Log("[ChainsawPort] Added " + type.FullName + " to " + target.name + ".");
                return;
            }
            Debug.LogWarning("[ChainsawPort] Could not resolve a compatible Marrow component for " + target.name + ". Candidates: " + string.Join(", ", typeNames));
        }

        private static Type FindType(string fullName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(fullName, false);
                if (type != null)
                    return type;
            }
            return null;
        }

        private static Transform Find(Transform root, string name)
        {
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                if (string.Equals(t.name, name, StringComparison.OrdinalIgnoreCase))
                    return t;
            return null;
        }

        private static void SetStringLike(UnityEngine.Object obj, string hint, string value)
        {
            SerializedObject so = new SerializedObject(obj);
            SerializedProperty it = so.GetIterator();
            bool enter = true;
            while (it.NextVisible(enter))
            {
                enter = false;
                if (it.propertyType != SerializedPropertyType.String)
                    continue;
                string normalized = it.name.ToLowerInvariant();
                if (normalized == hint.ToLowerInvariant() || normalized.EndsWith(hint.ToLowerInvariant()) || normalized.Contains(hint.ToLowerInvariant()))
                {
                    it.stringValue = value;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(obj);
                    return;
                }
            }
        }

        private static void TryAssignObjectLike(UnityEngine.Object obj, string hint, UnityEngine.Object value)
        {
            if (value == null)
                return;

            SerializedObject so = new SerializedObject(obj);
            SerializedProperty it = so.GetIterator();
            bool enter = true;
            while (it.NextVisible(enter))
            {
                enter = false;
                if (it.propertyType != SerializedPropertyType.ObjectReference)
                    continue;
                string normalized = it.name.ToLowerInvariant();
                if (!normalized.Contains(hint.ToLowerInvariant()))
                    continue;
                it.objectReferenceValue = value;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(obj);
                return;
            }
        }

        private static void TryAddCrateToPallet(UnityEngine.Object pallet, UnityEngine.Object crate)
        {
            SerializedObject so = new SerializedObject(pallet);
            SerializedProperty it = so.GetIterator();
            bool enter = true;
            while (it.NextVisible(enter))
            {
                enter = false;
                if (!it.isArray || it.propertyType == SerializedPropertyType.String)
                    continue;
                if (!it.name.ToLowerInvariant().Contains("crate"))
                    continue;

                int index = it.arraySize;
                it.InsertArrayElementAtIndex(index);
                SerializedProperty element = it.GetArrayElementAtIndex(index);
                if (element.propertyType == SerializedPropertyType.ObjectReference)
                {
                    element.objectReferenceValue = crate;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(pallet);
                    return;
                }
            }
        }

        private static string FindPreviewMeshGuid()
        {
            string[] guids = AssetDatabase.FindAssets("t:Mesh", new[] { PreviewFolder });
            return guids.Length > 0 ? guids[0] : "MISSING_PREVIEW_MESH";
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
