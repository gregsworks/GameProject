using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CharacterEditor;
using CharacterEditor.AssetDatabaseLoader;
using CharacterEditor.Mesh;
using UnityEditor;
using UnityEngine;

public class MeshUVManager
{

    [MenuItem("Tools/Character Editor/Update Mesh UV")]
    public static void UpdateMeshUV()
    {
        var configLoader = new ConfigLoader();
        configLoader.LoadConfigs((configs) => UpdateUVs(configs));
    }

    private static void UpdateUVs(Config[] configs)
    {
        foreach (var config in configs)
        {
            var armorMeshFolderPath = "Assets/Character_Editor/Meshes/" + config.folderName + "/Armor/";
            var weaponMeshFolderPath = "Assets/Character_Editor/Meshes/" + config.folderName + "/Weapon/";

            var meshPaths = new Dictionary<int, string>()
            {
                {Arm.GetMerheOrder(MeshType.ArmRight), armorMeshFolderPath + "Armor_Arm/ArmRight"},
                {Arm.GetMerheOrder(MeshType.ArmLeft), armorMeshFolderPath + "Armor_Arm/ArmLeft"},
                {Belt.GetMerheOrder(),armorMeshFolderPath + "Armor_Belt"},
                {BeltAdd.GetMerheOrder(), armorMeshFolderPath + "Armor_BeltAdd"},
                {Hair.GetMerheOrder(), armorMeshFolderPath + "Armor_Hair"},
                {Helm.GetMerheOrder(), armorMeshFolderPath + "Armor_Helm"},
                {Beard.GetMerheOrder(), armorMeshFolderPath + "Armor_Jaw"},
                {FaceFeature.GetMerheOrder(), armorMeshFolderPath + "Armor_Feature"},
                {Leg.GetMerheOrder(MeshType.LegRight), armorMeshFolderPath + "Armor_Leg/LegRight"},
                {Leg.GetMerheOrder(MeshType.LegLeft), armorMeshFolderPath + "Armor_Leg/LegLeft"},
                {Shoulder.GetMerheOrder(MeshType.ShoulderRight), armorMeshFolderPath + "Armor_Shoulder/ShoulderRight"},
                {Shoulder.GetMerheOrder(MeshType.ShoulderLeft), armorMeshFolderPath + "Armor_Shoulder/ShoulderLeft"},
                {Torso.GetMerheOrder(), armorMeshFolderPath + "Armor_Torso"},
                {TorsoAdd.GetMerheOrder(), armorMeshFolderPath + "Armor_TorsoAdd"},
                {Hand.GetMerheOrder(MeshType.HandRight), weaponMeshFolderPath + "HandRight"},
                {Hand.GetMerheOrder(MeshType.HandLeft), weaponMeshFolderPath + "HandLeft"},
            };


            var myList = meshPaths.ToList();
            myList.Sort((pair1, pair2) => pair1.Key.CompareTo(pair2.Key));

            int atlasSize = 4;
            float uvsStep = 1f / atlasSize;
            
            for (int itemNum = 0; itemNum < myList.Count; itemNum++)
            {
                if (!AssetDatabase.IsValidFolder(myList[itemNum].Value))
                    continue;

                var meshGUIDs = AssetDatabase.FindAssets("t:GameObject", new string[] { myList[itemNum].Value });
                for (int meshNum = 0; meshNum < meshGUIDs.Length; meshNum++)
                {
                    var tempMesh = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(meshGUIDs[meshNum]));
                    // Mesh tempMesh = UnityEngine.Object.Instantiate(meshObject);

                    //Update LOD parts for each armor item
                    var armorsParts = tempMesh.GetComponentsInChildren<MeshFilter>();
                    for (var armLOD = 0; armLOD < armorsParts.Length; armLOD++)
                    {
                        if (armorsParts[armLOD] != null)
                        {
                            var mTempMesh = (Mesh) GameObject.Instantiate(armorsParts[armLOD].sharedMesh);

                            //Update UVS for new atlas
                            Vector2[] uvs = mTempMesh.uv;
                            for (int i = 0; i < uvs.Length; i++)
                            {
                                uvs[i] = new Vector2(uvs[i].x / atlasSize + uvsStep * (itemNum % atlasSize),
                                    uvs[i].y / atlasSize + uvsStep * (atlasSize - 1 - (itemNum / atlasSize)));
                            }

                            mTempMesh.uv = uvs;
                            //assigne the selected LOD Mesh with new UV's to the new mesh to be exported
                            if (!Directory.Exists(myList[itemNum].Value + "/Meshes/"))
                            {
                                Directory.CreateDirectory(myList[itemNum].Value + "/Meshes/");
                            }

                            CreateOrReplaceAsset<Mesh>(mTempMesh,
                                myList[itemNum].Value + "/Meshes/" + armorsParts[armLOD].sharedMesh.name + "_New.asset");

                            AssetDatabase.SaveAssets();

                        }
                    }
                }
            }
            
            var prefabsPath = "Assets/Character_Editor/Prefabs/" + config.folderName;
            var prefabGUIDs = AssetDatabase.FindAssets("t:Prefab", new string[] { prefabsPath });
            for (int prefNum = 0; prefNum < prefabGUIDs.Length; prefNum++)
            {
                var pPath = AssetDatabase.GUIDToAssetPath(prefabGUIDs[prefNum]);
                if (pPath.Contains("/Model/"))
                {
                    var originalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(pPath);
                    var originalPrefabInstance = GameObject.Instantiate(originalPrefab);
                    originalPrefabInstance.name = originalPrefab.name;

                    foreach (var filter in originalPrefabInstance.GetComponentsInChildren<MeshFilter>())
                    {
                        var lodMeshPath = AssetDatabase.GetAssetPath(filter.sharedMesh);
                        var index = lodMeshPath.LastIndexOf("/");
                        if (index != -1)
                        {
                            lodMeshPath = lodMeshPath.Substring(0, index) + "/Meshes/" + filter.sharedMesh.name + "_New.asset";
                            
                            var changedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(lodMeshPath);
                            filter.mesh = changedMesh;

                            var newDirPath = pPath.Substring(0, pPath.IndexOf("Model/")) + "StaticModel/";
                            var fullDirPath = Application.dataPath.Substring(0, Application.dataPath.Length - 6) + newDirPath;
                            if (Directory.Exists(fullDirPath))
                                Directory.Delete(fullDirPath, true);

                            Directory.CreateDirectory(fullDirPath);

                            Object prefab = PrefabUtility.CreateEmptyPrefab(newDirPath + originalPrefabInstance.name + ".prefab");
                            PrefabUtility.ReplacePrefab(originalPrefabInstance, prefab, ReplacePrefabOptions.ConnectToPrefab);

                            AssetDatabase.SaveAssets();

                        }
                    }
                    GameObject.DestroyImmediate(originalPrefabInstance);
                }
            }
        }
   } 

    private static T CreateOrReplaceAsset<T>(T asset, string path) where T : Object {
        T existingAsset = AssetDatabase.LoadAssetAtPath<T>(path);

        if (existingAsset == null) {
            AssetDatabase.CreateAsset(asset, path);
            existingAsset = asset;
        }
        else {
            EditorUtility.CopySerialized(asset, existingAsset);
        }

        return existingAsset;
    }
}