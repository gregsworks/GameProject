using System;
using System.Collections;
using System.Collections.Generic;
using AssetBundles;
using UnityEngine;
using CharacterEditor.JSONMap;

namespace CharacterEditor
{
    namespace AssetBundleLoader
    {
        public class ConfigLoader : IConfigLoader
        {

            private Coroutine StartCoroutine(IEnumerator cor)
            {
                return LoaderManager.Instance.StartCoroutine(cor);
            }

            public void LoadConfigs(Action<Config[]> callback)
            {
                StartCoroutine(LoadConfigsCoroutine(callback));
            }

            private IEnumerator LoadConfigsCoroutine(Action<Config[]> callback)
            {
                TextAsset targetFile = Resources.Load<TextAsset>("assetBundleInfo");

                var configs = new List<Config>();
                BundleMap map = JsonUtility.FromJson<BundleMap>(targetFile.text);
                foreach (var raceInfo in map.races)
                {
                    yield return LoadConfigCoroutine(raceInfo.configPath, raceInfo.prefabPath, (Config config) =>
                    {
                        configs.Add(config);
                    });
                }
                callback.Invoke(configs.ToArray());
            }

            private IEnumerator LoadConfigCoroutine(string configPath, string prefabPath, Action<Config> callback)
            {
                //Load Config
                var pathParts = configPath.Split('/');
                var assetBundleName = pathParts[0].ToLower();
                var assetName = pathParts[pathParts.Length - 1];

                if (!LoaderManager.Instance.IsReady)
                    yield return null;

                AssetBundleLoadAssetOperation request =
                    AssetBundleManager.LoadAssetAsync(assetBundleName, assetName, typeof(Config));
                if (request == null)
                {
                    Debug.LogError("Failed AssetBundleLoadAssetOperation on " + assetName + " from the AssetBundle " + assetBundleName + ".");
                    yield break;
                }
                yield return StartCoroutine(request);
                var config = request.GetAsset<Config>();

                //Load Prefab
                pathParts = prefabPath.Split('/');
                assetBundleName = pathParts[0].ToLower();
                assetName = pathParts[pathParts.Length - 1];

                request = AssetBundleManager.LoadAssetAsync(assetBundleName, assetName, typeof(GameObject));
                if (request == null)
                {
                    Debug.LogError("Failed AssetBundleLoadAssetOperation on " + assetName + " from the AssetBundle " + assetBundleName + ".");
                    yield break;
                }
                yield return StartCoroutine(request);

                config.prefab = request.GetAsset<GameObject>();
                callback.Invoke(config);
            }
        }
    }
}
