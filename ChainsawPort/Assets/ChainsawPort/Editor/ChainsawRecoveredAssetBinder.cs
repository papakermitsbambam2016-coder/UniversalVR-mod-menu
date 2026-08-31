#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ChainsawPort.Editor
{
    public static class ChainsawRecoveredAssetBinder
    {
        private const string PrefabPath = "Assets/ChainsawPort/Prefabs/Chainsaw.prefab";
        private const string ModelFolder = "Assets/ChainsawPort/Source/Models";
        private const string MaterialFolder = "Assets/ChainsawPort/Source/Materials";
        private const string TextureFolder = "Assets/ChainsawPort/Source/Textures";
        private const string AudioFolder = "Assets/ChainsawPort/Source/Audio";

        [MenuItem("Chainsaw Port/3 - Bind Recovered Model Materials Audio")]
        public static void BindRecoveredAssets()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) == null)
            {
                Debug.LogError("[ChainsawPort] Missing Chainsaw.prefab. Run 'Create Chainsaw Prefab Skeleton' first.");
                return;
            }

            GameObject root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                Transform visuals = Find(root.transform, "Visuals");
                Transform idle = Find(root.transform, "IdleSound");
                Transform bladeAudio = Find(root.transform, "BladeAudio");
                Transform bladeTransform = Find(root.transform, "BladeTransform");

                int modelCount = BindModel(visuals);
                int materialCount = BindMaterials(visuals);
                int audioCount = BindAudio(idle, bladeAudio);

                ChainsawMotor motor = root.GetComponent<ChainsawMotor>();
                if (motor == null)
                    motor = root.AddComponent<ChainsawMotor>();

                motor.bladeTransform = bladeTransform;
                motor.idleSound = idle != null ? idle.GetComponent<AudioSource>() : null;
                motor.bladeAudio = bladeAudio != null ? bladeAudio.GetComponent<AudioSource>() : null;

                Transform bladeCollider = Find(root.transform, "BladeCollider");
                if (bladeCollider != null && bladeCollider.GetComponent<ChainsawDamage>() == null)
                {
                    ChainsawDamage damage = bladeCollider.gameObject.AddComponent<ChainsawDamage>();
                    damage.motor = motor;
                }

                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log("[ChainsawPort] Recovered asset bind complete. Models=" + modelCount + ", Materials=" + materialCount + ", Audio=" + audioCount + ".");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static int BindModel(Transform visuals)
        {
            if (visuals == null)
                return 0;

            string[] guids = AssetDatabase.FindAssets("t:GameObject", new[] { ModelFolder });
            if (guids.Length == 0)
            {
                Debug.LogWarning("[ChainsawPort] No extracted model/prefab found in " + ModelFolder + ". The preserved legacy .bundle cannot be used as an editable prefab by itself.");
                return 0;
            }

            foreach (Transform child in visuals.Cast<Transform>().ToArray())
                UnityEngine.Object.DestroyImmediate(child.gameObject);

            int count = 0;
            foreach (string guid in guids.OrderBy(g => AssetDatabase.GUIDToAssetPath(g)))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (source == null)
                    continue;

                GameObject instance = PrefabUtility.InstantiatePrefab(source) as GameObject;
                if (instance == null)
                    instance = UnityEngine.Object.Instantiate(source);

                instance.name = "Recovered_" + source.name;
                instance.transform.SetParent(visuals, false);
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one;
                count++;
            }
            return count;
        }

        private static int BindMaterials(Transform visuals)
        {
            if (visuals == null)
                return 0;

            string[] guids = AssetDatabase.FindAssets("t:Material", new[] { MaterialFolder });
            Material[] materials = guids
                .Select(g => AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(m => m != null)
                .ToArray();

            if (materials.Length == 0)
            {
                Material recovered = CreateRecoveredMaterial();
                if (recovered != null)
                    materials = new[] { recovered };
            }

            if (materials.Length == 0)
                return 0;

            int assignments = 0;
            Renderer[] renderers = visuals.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                Material[] slots = renderer.sharedMaterials;
                for (int i = 0; i < slots.Length; i++)
                {
                    string wanted = slots[i] != null ? Normalize(slots[i].name) : Normalize(renderer.name);
                    Material best = materials.FirstOrDefault(m => Normalize(m.name) == wanted)
                                    ?? materials.FirstOrDefault(m => Normalize(m.name).Contains(Normalize(renderer.name)))
                                    ?? (materials.Length == 1 ? materials[0] : null);
                    if (best == null)
                        continue;
                    slots[i] = best;
                    assignments++;
                }
                renderer.sharedMaterials = slots;
            }
            return assignments;
        }

        private static Material CreateRecoveredMaterial()
        {
            string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { TextureFolder });
            if (textureGuids.Length == 0)
                return null;

            if (!AssetDatabase.IsValidFolder(MaterialFolder))
                AssetDatabase.CreateFolder("Assets/ChainsawPort/Source", "Materials");

            const string materialPath = MaterialFolder + "/chainsaw_recovered.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                if (shader == null)
                {
                    Debug.LogError("[ChainsawPort] No compatible lit shader was found for the recovered material.");
                    return null;
                }
                material = new Material(shader) { name = "chainsaw_recovered" };
                AssetDatabase.CreateAsset(material, materialPath);
            }

            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(textureGuids[0]));
            if (material.HasProperty("_BaseMap"))
                material.SetTexture("_BaseMap", texture);
            if (material.HasProperty("_MainTex"))
                material.SetTexture("_MainTex", texture);
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();
            return material;
        }

        private static int BindAudio(Transform idleTransform, Transform bladeTransform)
        {
            string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { AudioFolder });
            AudioClip[] clips = guids
                .Select(g => AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(c => c != null)
                .ToArray();

            if (clips.Length == 0)
                return 0;

            AudioClip idleClip = FindClip(clips, "idle", "loop", "motor", "engine");
            AudioClip bladeClip = FindClip(clips, "blade", "saw", "chain", "rev", "cut");

            int count = 0;
            if (idleTransform != null)
            {
                AudioSource source = idleTransform.GetComponent<AudioSource>() ?? idleTransform.gameObject.AddComponent<AudioSource>();
                source.clip = idleClip ?? clips[0];
                source.loop = true;
                source.playOnAwake = false;
                count++;
            }

            if (bladeTransform != null)
            {
                AudioSource source = bladeTransform.GetComponent<AudioSource>() ?? bladeTransform.gameObject.AddComponent<AudioSource>();
                source.clip = bladeClip ?? idleClip ?? clips[0];
                source.loop = true;
                source.playOnAwake = false;
                count++;
            }
            return count;
        }

        private static AudioClip FindClip(AudioClip[] clips, params string[] terms)
        {
            foreach (string term in terms)
            {
                AudioClip clip = clips.FirstOrDefault(c => Normalize(c.name).Contains(Normalize(term)));
                if (clip != null)
                    return clip;
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

        private static string Normalize(string value)
        {
            return (value ?? string.Empty).ToLowerInvariant().Replace(" ", string.Empty).Replace("_", string.Empty).Replace("-", string.Empty);
        }
    }
}
#endif
