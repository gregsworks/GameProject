using System.Collections.Generic;
using System.IO;
using System.Text;
using CharacterEditor;
using CharacterEditor.AssetDatabaseLoader;
using CharacterEditor.JSONMap;
using UnityEditor;
using UnityEngine;

using Textures = CharacterEditor.Textures;
using Meshes = CharacterEditor.Mesh;
using FX = CharacterEditor.FX;


public class EditorBundleManager
{
    [MenuItem("Tools/Character Editor/Update AssetsBundle")]
    public static void UpdateAssetsBundle()
    {
        ConfigLoader loader = new ConfigLoader();
        loader.LoadConfigs((configs) => UpdateBundles(configs));
    }

    protected static void UpdateBundles(Config[] configs)
    {
        var bundleMap = new BundleMap();

        for (int i = 0; i < configs.Length; i++)
        {
            var raceName = configs[i].folderName;

            var raceMap = new RaceMap();
            raceMap.race = raceName;
            raceMap.configPath = ParseConfigPath(configs[i]);
            raceMap.prefabPath = ParsePrefabPath(configs[i]);
            raceMap.textures = ParseBundleTextures(raceName);
            raceMap.meshes = ParseBundleMeshes(raceName);
            raceMap.fxs = ParseBundleFXMeshes(raceName);

            bundleMap.races.Add(raceMap);
        }

        if (!Directory.Exists(Application.dataPath + "/Resources/")) {
            Directory.CreateDirectory(Application.dataPath + "/Resources/");
        }

        using (FileStream fs = new FileStream("Assets/Resources/assetBundleInfo.json", FileMode.Create))
        {
            using (StreamWriter writer = new StreamWriter(fs))
            {
                writer.Write(JsonUtility.ToJson(bundleMap));
            }
        }
        AssetDatabase.Refresh();
    }

    protected static string ParseConfigPath(Config config)
    {
        string bundleName, assetPath;

        var configPath = AssetDatabase.GetAssetPath(config);

        var bundlePath = configPath.Substring("Assets/Character_Editor/".Length);
        ParsePathToBundle(bundlePath, out bundleName, out assetPath);

        AssetImporter.GetAtPath(configPath).SetAssetBundleNameAndVariant(bundleName, "");
        return assetPath;
    }

    protected static string ParsePrefabPath(Config config)
    {
        string bundleName, assetPath;

        var bundlePath = config.prefabPath.Substring("Assets/Character_Editor/".Length);
        ParsePathToBundle(bundlePath, out bundleName, out assetPath);

        AssetImporter.GetAtPath(config.prefabPath).SetAssetBundleNameAndVariant(bundleName, "");
        return assetPath;
    }

    protected static List<BundleMeshes> ParseBundleMeshes(string raceName)
    {
        var textureLoader = new TextureLoader();
        var bundleMeshList = new List<BundleMeshes>();

        int substringLength = "Assets/Character_Editor/Prefabs/".Length;

        var meshTypes = new Dictionary<MeshType, string>
        {
            {MeshType.Torso, Meshes.Torso.GetFolderPath(raceName)},
            {MeshType.Beard, Meshes.Beard.GetFolderPath(raceName)},
            {MeshType.FaceFeature, Meshes.FaceFeature.GetFolderPath(raceName)},
            {MeshType.Hair, Meshes.Hair.GetFolderPath(raceName)},
            {MeshType.Helm, Meshes.Helm.GetFolderPath(raceName)},
            {MeshType.TorsoAdd, Meshes.TorsoAdd.GetFolderPath(raceName)},
            {MeshType.LegRight, Meshes.Leg.GetFolderPath(raceName, MeshType.LegRight)},
            {MeshType.LegLeft, Meshes.Leg.GetFolderPath(raceName, MeshType.LegLeft)},
            {MeshType.ShoulderLeft, Meshes.Shoulder.GetFolderPath(raceName, MeshType.ShoulderLeft)},
            {MeshType.ShoulderRight, Meshes.Shoulder.GetFolderPath(raceName, MeshType.ShoulderRight)},
            {MeshType.ArmLeft, Meshes.Arm.GetFolderPath(raceName, MeshType.ArmLeft)},
            {MeshType.ArmRight, Meshes.Arm.GetFolderPath(raceName, MeshType.ArmRight)},
            {MeshType.Belt, Meshes.Belt.GetFolderPath(raceName)},
            {MeshType.BeltAdd, Meshes.BeltAdd.GetFolderPath(raceName)},
            {MeshType.HandLeft, Meshes.Hand.GetFolderPath(raceName, MeshType.HandLeft)},
            {MeshType.HandRight, Meshes.Hand.GetFolderPath(raceName, MeshType.HandRight)},
        };

        foreach (var path in meshTypes)
        {
            var bundleMeshes = new BundleMeshes();
            bundleMeshes.type = path.Key;

            var dirPath = Path.Combine(Application.dataPath, path.Value.Substring(7));
            if (!Directory.Exists(dirPath)) 
                continue;

            var folders = Directory.GetDirectories(dirPath);

            for (int i = 0; i < folders.Length; i++)
            {
                var bundleMesh = new BundleMesh();

                string gameObjectsPath = folders[i].Substring(Application.dataPath.Length - 6);
                var meshGUIDs = AssetDatabase.FindAssets("t:GameObject", new string[]
                    {
                        gameObjectsPath + "/StaticModel"
                    }
                );
                if (meshGUIDs.Length == 0)
                {
                    continue;
                }
                var bundleModelPath = AssetDatabase.GUIDToAssetPath(meshGUIDs[0]);

                string bundleName, assetPath;
                ParsePathToBundle(bundleModelPath.Substring(substringLength), out bundleName, out assetPath, 2);
                AssetImporter.GetAtPath(bundleModelPath).SetAssetBundleNameAndVariant(bundleName, "");

                bundleMesh.modelPath = assetPath;

                foreach (var texturePath in textureLoader.ParseTextures(null, gameObjectsPath + "/Textures"))
                {
                    var bundleTexture = new BundleTexture();

                    foreach (var colorPath in texturePath)
                    {
                        ParsePathToBundle(colorPath.Substring(substringLength), out bundleName, out assetPath, 2);
                        AssetImporter.GetAtPath(colorPath).SetAssetBundleNameAndVariant(bundleName, "");

                        var bundleColor = new BundleColor();
                        bundleColor.path = assetPath;
                        bundleTexture.colors.Add(bundleColor);
                    }

                    bundleMesh.textures.Add(bundleTexture);
                }
                bundleMeshes.meshPaths.Add(bundleMesh);
            }
            bundleMeshList.Add(bundleMeshes);
        }
        return bundleMeshList;
    }


    protected static List<BundleFxMeshes> ParseBundleFXMeshes(string raceName)
    {
        var bundleMeshList = new List<BundleFxMeshes>();
        int substringLength = "Assets/Character_Editor/Prefabs/".Length;

        var meshTypes = new Dictionary<FXType, string>
        {
            {FXType.Torso, FX.TorsoFX.GetFolderPath(raceName)},
            {FXType.Eye, FX.EyeFX.GetFolderPath(raceName)},
        };

        foreach (var path in meshTypes)
        {
            var bundleMeshes = new BundleFxMeshes();
            bundleMeshes.type = path.Key;

            if (!Directory.Exists(path.Value))
                continue;

            var meshGUIDs = AssetDatabase.FindAssets("t:GameObject", new string[]
                {
                        path.Value
                }
            );
            for (int i = 0; i < meshGUIDs.Length; i++)
            {
                var bundleMesh = new BundleColor();
                var bundleModelPath = AssetDatabase.GUIDToAssetPath(meshGUIDs[i]);
                string bundleName, assetPath;

                ParsePathToBundle(bundleModelPath.Substring(substringLength), out bundleName, out assetPath);
                AssetImporter.GetAtPath(bundleModelPath).SetAssetBundleNameAndVariant(bundleName, "");

                bundleMesh.path = assetPath;
                bundleMeshes.meshPaths.Add(bundleMesh);
            }
            bundleMeshList.Add(bundleMeshes);
        }

        return bundleMeshList;
    }

    protected static List<BundleTextures> ParseBundleTextures(string raceName)
    {
        var loader = new TextureLoader();

        var bundleTextures = new List<BundleTextures>();
        var textureTypes = new Dictionary<TextureType, string>
        {
            {TextureType.Skin, Textures.Skin.GetFolderPath(raceName)},
            {TextureType.Eyebrow, Textures.Eyebrow.GetFolderPath(raceName)},
            {TextureType.Scar, Textures.Scar.GetFolderPath(raceName)},
            {TextureType.Beard, Textures.Beard.GetFolderPath(raceName)},
            {TextureType.FaceFeature, Textures.FaceFeature.GetFolderPath(raceName)},
            {TextureType.Hair, Textures.Hair.GetFolderPath(raceName)},
            {TextureType.Eye, Textures.Eye.GetFolderPath(raceName)},
            {TextureType.Head, Textures.Head.GetFolderPath(raceName)},
            {TextureType.Pants, Textures.Pants.GetFolderPath(raceName)},
            {TextureType.Torso, Textures.Torso.GetFolderPath(raceName)},
            {TextureType.Shoe, Textures.Shoes.GetFolderPath(raceName)},
            {TextureType.Glove, Textures.Gloves.GetFolderPath(raceName)},
            {TextureType.RobeLong, Textures.RobeLong.GetFolderPath(raceName)},
            {TextureType.RobeShort, Textures.RobeShort.GetFolderPath(raceName)},
            {TextureType.Belt, Textures.Belt.GetFolderPath(raceName)},
            {TextureType.Cloak, Textures.Cloak.GetFolderPath(raceName)},
        };

        foreach (var path in textureTypes)
        {
            BundleTextures textures = new BundleTextures();
            textures.type = path.Key;
            var paths = loader.ParseTextures(null, path.Value);
            foreach (string[] texturePaths in paths)
            {
                var texture = new BundleTexture();
                foreach (string colorPath in texturePaths)
                {
                    string bundleName, assetPath;

                    var bundlePath = colorPath.Substring("Assets/Character_Editor/Textures/Character/".Length);
                    ParsePathToBundle(bundlePath, out bundleName, out assetPath);

                    AssetImporter.GetAtPath(colorPath).SetAssetBundleNameAndVariant(bundleName, "");

                    var color = new BundleColor();
                    color.path = assetPath;

                    texture.colors.Add(color);
                }
                textures.texturePaths.Add(texture);
            }
            bundleTextures.Add(textures);

        }
        return bundleTextures;
    }

    protected static void ParsePathToBundle(string bundlePath, out string bundleName, out string assetPath, int rootDepth = 1)
    {
        var builder = new StringBuilder();

        var pathParts = bundlePath.Split('/');
        builder.Length = 0;
        for (int j = 0; j < pathParts.Length - rootDepth; j++)
        {
            builder.Append(pathParts[j]).Append("_");
        }

        bundleName = builder.ToString().Trim('_');

        var assetNameParts = pathParts[pathParts.Length - 1].Split('.');
        builder.Length = 0;
        builder.Append(bundleName).Append("/");

        for (int j = 0; j < assetNameParts.Length - 1; j++)
        {
            builder.Append(assetNameParts[j]);
        }
        assetPath = builder.ToString();
    }
}
