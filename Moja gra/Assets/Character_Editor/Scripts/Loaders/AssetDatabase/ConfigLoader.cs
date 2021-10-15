#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;

namespace CharacterEditor
{
    namespace AssetDatabaseLoader
    {
        /*
         * Parse and Load Configs (Only editor)
         */
        public class ConfigLoader : IConfigLoader
        {
            private const string CONFIG_PATHS = "Assets/Character_Editor/Configs";

            public void LoadConfigs(Action<Config[]> callback)
            {
                var paths = AssetDatabase.FindAssets("t:config", new string[] { CONFIG_PATHS });
                var configs = new Config[paths.Length];
                for (int i = 0; i < paths.Length; i++)
                {
                    configs[i] = AssetDatabase.LoadAssetAtPath<Config>(AssetDatabase.GUIDToAssetPath(paths[i]));
                    configs[i].prefab = AssetDatabase.LoadAssetAtPath<GameObject>(configs[i].prefabPath);
                }

                callback.Invoke(configs);
            }
        }
    }
}
#endif