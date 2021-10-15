using System;
using System.Collections;
using AssetBundles;
using CharacterEditor.FX;
using CharacterEditor.Mesh;
using UnityEngine;
using CharacterEditor.JSONMap;

namespace CharacterEditor
{
    namespace AssetBundleLoader
    {
        public class MeshLoader : IMeshLoader
        {
            private Coroutine StartCoroutine(IEnumerator cor)
            {
                return LoaderManager.Instance.StartCoroutine(cor);
            }

            public void ParseMeshes(AbstractFXMesh mesh, Action<GameObject[]> callback)
            {
                StartCoroutine(ParseFx(mesh, callback));
            }

            public void ParseMeshes(AbstractMesh mesh, Action<GameObject[], AbstractTexture[]> callback)
            {
                StartCoroutine(Parse(mesh, callback));
            }

            private IEnumerator LoadMeshCoroutine(string path, Action<GameObject> callback)
            {
                var pathParts = path.Split('/');

                var assetBundleName = pathParts[0].ToLower();
                var assetName = pathParts[pathParts.Length - 1];

                if (!LoaderManager.Instance.IsReady)
                    yield return null;

                AssetBundleLoadAssetOperation request = AssetBundleManager.LoadAssetAsync(assetBundleName, assetName, typeof(GameObject));
                if (request == null)
                {
                    Debug.LogError("Failed AssetBundleLoadAssetOperation on " + assetName + " from the AssetBundle " + assetBundleName + ".");
                    yield break;
                }
                yield return StartCoroutine(request);

                callback.Invoke(request.GetAsset<GameObject>());
            }

            /*
             * Parse Fx meshes from AssetBundle
             */
            private IEnumerator ParseFx(AbstractFXMesh mesh, Action<GameObject[]> callback)
            {
                TextAsset targetFile = Resources.Load<TextAsset>("assetBundleInfo");

                GameObject[] meshObjects = null;

                BundleMap map = JsonUtility.FromJson<BundleMap>(targetFile.text);
                foreach (var raceInfo in map.races)
                {
                    if (raceInfo.race.Equals(mesh.CharacterRace))
                    {
                        foreach (var meshInfo in raceInfo.fxs)
                        {
                            if (meshInfo.type.Equals(mesh.FxType))
                            {
                                meshObjects = new GameObject[meshInfo.meshPaths.Count];

                                int i = 0;
                                foreach (var meshPathInfo in meshInfo.meshPaths)
                                {
                                    yield return LoadMeshCoroutine(meshPathInfo.path, (GameObject meshGameObject) =>
                                    {
                                        meshObjects[i] = meshGameObject;
                                    });
                                    i++;
                                }
                            }
                        }
                    }
                }
                callback.Invoke(meshObjects);
            }

            /*
             * Parse Armor and weapon meshes from AssetBundle
             */
            private IEnumerator Parse(AbstractMesh mesh, Action<GameObject[], AbstractTexture[]> callback)
            {
                TextAsset targetFile = Resources.Load<TextAsset>("assetBundleInfo");

                GameObject[] meshObjects = null;
                AbstractTexture[] textures = null;

                var textureLoader = new TextureLoader();

                BundleMap map = JsonUtility.FromJson<BundleMap>(targetFile.text);
                foreach (var raceInfo in map.races)
                {
                    if (raceInfo.race.Equals(mesh.CharacterRace))
                    {
                        foreach (var meshInfo in raceInfo.meshes)
                        {
                            if (meshInfo.type.Equals(mesh.MeshType))
                            {
                                meshObjects = new GameObject[meshInfo.meshPaths.Count];
                                textures = new AbstractTexture[meshInfo.meshPaths.Count];

                                int i = 0;
                                foreach (var meshPathInfo in meshInfo.meshPaths)
                                {
                                    yield return LoadMeshCoroutine(meshPathInfo.modelPath,
                                        (GameObject meshGameObject) =>
                                        {
                                            meshObjects[i] = meshGameObject;
                                        });

                                    var texturePaths = new string[meshPathInfo.textures.Count][];
                                    int j = 0;
                                    foreach (var texture in meshPathInfo.textures)
                                    {

                                        texturePaths[j] = new string[texture.colors.Count];
                                        int k = 0;
                                        foreach (var color in texture.colors)
                                        {
                                            texturePaths[j][k] = color.path;
                                            k++;
                                        }
                                        j++;
                                    }

                                    textures[i] = new MeshTexture(textureLoader, mesh.CharacterRace, texturePaths);
                                    i++;
                                }
                            }
                        }
                    }
                }
                callback.Invoke(meshObjects, textures);
            }
        }
    }
}