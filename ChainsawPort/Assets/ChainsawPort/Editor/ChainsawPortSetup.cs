using UnityEditor;
using UnityEngine;

namespace ChainsawPort
{
    public static class ChainsawPortSetup
    {
        [MenuItem("Chainsaw Port/Create Migration Folders")]
        public static void CreateMigrationFolders()
        {
            Ensure("Assets/ChainsawPort");
            Ensure("Assets/ChainsawPort/Source");
            Ensure("Assets/ChainsawPort/Prefabs");
            Ensure("Assets/ChainsawPort/Pallet");
            Ensure("Assets/ChainsawPort/Packed");
            AssetDatabase.Refresh();
            Debug.Log("Chainsaw migration folders created. Import the editable legacy Chainsaw assets into Source, then rebuild the Spawnable with the current MarrowSDK.");
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
