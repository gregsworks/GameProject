using AssetBundles;
using System;
using System.Collections;
using CharacterEditor.JSONMap;
using UnityEngine;

namespace CharacterEditor
{
    namespace AssetBundleLoader
    {
        public class TextureLoader : ITextureLoader
        {
            private Coroutine StartCoroutine(IEnumerator cor)
            {
                return LoaderManager.Instance.StartCoroutine(cor);
            }

            public void LoadTexture(string[] paths, Action<Texture2D[], string[]> callback)
            {
                StartCoroutine(LoadTextureCoroutine(paths, callback));
            }

            private IEnumerator LoadTextureCoroutine(string[] paths, Action<Texture2D[], string[]> callback)
            {
                var textures = new Texture2D[paths.Length];

                for (int i = 0; i < paths.Length; i++)
                {
                    var pathParts = paths[i].Split('/');

                    var assetBundleName = pathParts[0].ToLower();
                    var assetName = pathParts[pathParts.Length - 1];

                    if (!LoaderManager.Instance.IsReady)
                        yield return null;

                    AssetBundleLoadAssetOperation request = AssetBundleManager.LoadAssetAsync(assetBundleName, assetName, typeof(Texture2D));
                    if (request == null)
                    {
                        Debug.LogError("Failed AssetBundleLoadAssetOperation on " + assetName + " from the AssetBundle " + assetBundleName + ".");
                        yield break;
                    }
                    yield return StartCoroutine(request);
                    textures[i] = request.GetAsset<Texture2D>();
                }

                callback.Invoke(textures, paths);
            }

            public string[][] ParseTextures(AbstractTexture texture, string path = null)
            {
                return Parse(texture);
            }

            public void UnloadTextures() {}

            protected string[][] Parse(AbstractTexture texture)
            {
                TextAsset targetFile = Resources.Load<TextAsset>("assetBundleInfo");

                BundleMap map = JsonUtility.FromJson<BundleMap>(targetFile.text);
                string[][] textures = null;
                foreach (var raceInfo in map.races)
                {
                    if (raceInfo.race.Equals(texture.CharacterRace))
                    {
                        foreach (var textureInfo in raceInfo.textures)
                        {
                            if (textureInfo.type.Equals(texture.Type))
                            {
                                textures = new string[textureInfo.texturePaths.Count][];
                                int i = 0;
                                foreach (var textureColors in textureInfo.texturePaths)
                                {
                                    textures[i] = new string[textureColors.colors.Count];
                                    int j = 0;
                                    foreach (var texturePath in textureColors.colors)
                                    {
                                        textures[i][j] = texturePath.path;
                                        j++;
                                    }
                                    i++;
                                }
                            }
                        }
                    }
                }
                return textures;
            }
        }
    }
}