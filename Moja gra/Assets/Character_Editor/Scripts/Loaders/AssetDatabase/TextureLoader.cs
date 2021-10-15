#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CharacterEditor
{
    namespace AssetDatabaseLoader
    {
        /*
         * Parse and Load Meshes (Only editor)
         */
        public class TextureLoader : ITextureLoader
        {
            public void LoadTexture(string[] paths, Action<Texture2D[], string[]> callback)
            {
                Texture2D[] textures = new Texture2D[paths.Length];
                for (int i = 0; i < paths.Length; i++)
                {
                    textures[i] = AssetDatabase.LoadAssetAtPath(paths[i], typeof(Texture2D)) as Texture2D;
                }
                callback.Invoke(textures, paths);
            }

            public void UnloadTextures() {}

            public string[][] ParseTextures(AbstractTexture texture, string path = null)
            {
                return Parse(texture, path ?? texture.GetFolderPath());
            }

            private string[][] Parse(AbstractTexture texture, string path)
            {
                var dirPath = Path.Combine(Application.dataPath, path.Substring(7));
                if (!Directory.Exists(dirPath))
                    return new string[0][];
                

                var folders = Directory.GetDirectories(dirPath);
                bool withColor = folders.Length != 0;

                string[][] texturePaths;
                if (withColor)
                {
                    texturePaths = new string[folders.Length][];

                    for (int i = 0; i < folders.Length; i++)
                    {
                        var textures = AssetDatabase.FindAssets("t:texture2D", new string[]
                            {
                                folders[i].Substring(Application.dataPath.Length - 6)
                            }
                        );
                        texturePaths[i] = new string[textures.Length];
                        for (int j = 0; j < textures.Length; j++)
                            texturePaths[i][j] = AssetDatabase.GUIDToAssetPath(textures[j]);
                    }
                }
                else
                {
                    var textures = AssetDatabase.FindAssets("t:texture2D", new string[] {path});
                    texturePaths = new string[textures.Length][];

                    for (int i = 0; i < textures.Length; i++)
                        texturePaths[i] = new string[] {AssetDatabase.GUIDToAssetPath(textures[i])};
                }
                return texturePaths;
            }
        }
    }
}
#endif