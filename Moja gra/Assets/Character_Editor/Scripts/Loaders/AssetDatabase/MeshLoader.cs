#if UNITY_EDITOR

using System;
using CharacterEditor.Mesh;
using UnityEngine;
using System.IO;
using CharacterEditor.FX;
using UnityEditor;

namespace CharacterEditor
{
    namespace AssetDatabaseLoader
    {
        /*
         * Parse and Load Meshes (Only editor)
         */
        public class MeshLoader : IMeshLoader
        {
            private MeshAtlasType meshAtlasType;

            public MeshLoader(MeshAtlasType atlasType)
            {
                meshAtlasType = atlasType;
            }

            public void ParseMeshes(AbstractFXMesh mesh, Action<GameObject[]> callback)
            {
                var objects = AssetDatabase.FindAssets("t:GameObject", new string[]
                    {
                        mesh.GetFolderPath()
                    }
                );

                var meshObjects = new GameObject[objects.Length];
                for (int i = 0; i < objects.Length; i++)
                    meshObjects[i] = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(objects[i]), typeof(GameObject)) as GameObject;
                
                callback.Invoke(meshObjects);
            }

            public void ParseMeshes(AbstractMesh mesh, Action<GameObject[], AbstractTexture[]> callback)
            {
                var textureLoader = new TextureLoader();
                var folders = Directory.GetDirectories(Path.Combine(Application.dataPath, mesh.GetFolderPath().Substring(7)));
                var meshObjects = new GameObject[folders.Length];
                var textures = new AbstractTexture[folders.Length];

                var modelType = meshAtlasType == MeshAtlasType.Static ? "StaticModel" : "Model";
                for (int i = 0; i < folders.Length; i++)
                {
                    string path = folders[i].Substring(Application.dataPath.Length - 6);
                    var meshGUIDs = AssetDatabase.FindAssets("t:GameObject", new string[]
                        {
                            path + "/" + modelType
                        }
                    );

                    meshObjects[i] = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(meshGUIDs[0]), typeof(GameObject)) as GameObject;
                    textures[i] = new MeshTexture(textureLoader, mesh.CharacterRace, path + "/Textures");
                }

                callback.Invoke(meshObjects, textures);
            }
        }
    }
}
#endif