using CharacterEditor.Mesh;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace CharacterEditor
{
    /*
     * Preservation of characters and characters in runtime
     */
    public class SaveManager : MonoBehaviour
    {
        const int ARMOR_IMAGE_SIZE = 512;

        private Dictionary<string, MeshFilter> meshes = new Dictionary<string, MeshFilter>();

        public string SavePath
        {
            get { return Application.persistentDataPath + "/Saves/"; }
        }

        public static SaveManager Instance { get; private set; }

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(this.gameObject);
            }
            Instance = this;
        }

        public void OnSaveClick(string fileName = "")
        {
            if (LoaderManager.Instance.Type == LoaderType.AssetBundle)
            {
                Save(fileName);
            }
        }

        public void OnSavePrefabClick()
        {
#if UNITY_EDITOR
            SavePrefab();
#endif
        }

        public void OnLoadClick(string fileName)
        {
            if (LoaderManager.Instance.Type == LoaderType.AssetBundle)
            {
                Load(fileName);
            }
        }

        /*
         * Save in runtime
         */
        private void Save(string characterName)
        {
            var mergedTexture = TextureManager.Instance.GetMergedTexture();
            var cloakTexture = TextureManager.Instance.GetCloakTexture();
            var armorAtlas = MeshManager.Instance.GetMergedTexture();

            if (!Directory.Exists(Application.dataPath + "/Resources/Character/"))
                Directory.CreateDirectory(Application.dataPath + "/Resources/Character/");
            
            if (!Directory.Exists(Application.dataPath + "/Resources/Character/" + characterName))
                Directory.CreateDirectory(Application.dataPath + "/Resources/Character/" + characterName);
            
            if (!Directory.Exists(SavePath + characterName))
                Directory.CreateDirectory(SavePath + characterName);
            

            File.WriteAllBytes(Application.dataPath + "/Resources/Character/" + characterName + "/Character_texture.png", mergedTexture.EncodeToPNG());
            File.WriteAllBytes(Application.dataPath + "/Resources/Character/" + characterName + "/Armor_texture.png", armorAtlas.EncodeToPNG());
            File.WriteAllBytes(Application.dataPath + "/Resources/Character/" + characterName + "/Cloak_texture.png", cloakTexture.EncodeToPNG());
            
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(SavePath + characterName + "/FileName.dat", FileMode.Create);

            SaveData newData = new SaveData();
            newData.InitData();
            bf.Serialize(file, newData);
            file.Close();
        }

        /*
         * load in runtime
         */
        private void Load(string characterName)
        {
            if (!File.Exists(SavePath + characterName + "/FileName.dat"))
                return;

            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(SavePath + characterName + "/FileName.dat", FileMode.Open);
            SaveData newData = (SaveData) bf.Deserialize(file);
            file.Close();

            Texture2D characterTexture = Resources.Load<Texture2D>("Character/" + characterName + "/Character_texture");
            Texture2D armortTexture = Resources.Load<Texture2D>("Character/" + characterName + "/Armor_texture");

            TextureManager.Instance.LoadTexture(characterTexture, newData.textures, newData.selectedCloak, newData.equipShortRobe, newData.equipLongRobe);

            MeshManager.Instance.LoadMeshes(armortTexture, newData.meshes, newData.fxMeshes);
        }

        public string[] GetSaves()
        {
            if (!Directory.Exists(SavePath))
            {
                Directory.CreateDirectory(SavePath);
                return new string[0];
            }

            var folders = Directory.GetDirectories(SavePath);
            var saveNames = new string[folders.Length];
            for (int i = 0; i < saveNames.Length; i++)
            {
                var folderParts = folders[i].Split('/');
                saveNames[i] = folderParts[folderParts.Length - 1];
            }

            return saveNames;
        }



#if UNITY_EDITOR
        /*
         * Create Prefab folder for a character
         */
        private string CreateFolder()
        {
            var config = ConfigManager.Instance.Config;
            string prefabNum = System.DateTime.Now.ToString("yyyyMMddHHmmssfff");

            if (!System.IO.Directory.Exists(Application.dataPath + "/Character_Editor/NewCharacter"))
                AssetDatabase.CreateFolder("Assets/Character_Editor", "NewCharacter");

            if (!System.IO.Directory.Exists(
                Application.dataPath + "/Character_Editor/NewCharacter/" + config.folderName))
                AssetDatabase.CreateFolder("Assets/Character_Editor/NewCharacter", config.folderName);

            var folderPath = "Assets/Character_Editor/NewCharacter/" + config.folderName;
            var folderName = config.folderName + "_" + prefabNum;

            AssetDatabase.CreateFolder(folderPath, folderName);

            folderPath += '/' + folderName;
            AssetDatabase.CreateFolder(folderPath, "Mesh");
            return folderPath;
        }

        private void SavePrefab()
        {
            var folderName = CreateFolder();

            if (LoaderManager.Instance.Type == LoaderType.AssetDatabase &&
                LoaderManager.Instance.MeshAtlasType == MeshAtlasType.Dynamic)
            {
                CreateArmorAtlas(folderName);
                CreateCloakAtlas(folderName);
                CreateSkinAtlas(folderName);

                // quit the editor because the mesh you export have now new UW'S and quitting the editor play mode will remove the new UW's 
                EditorApplication.isPlaying = false;
            }
            else
            {
                CreatePrefab(folderName);
            }
        }

        private void CreateSkinAtlas(string folderName)
        {
            var config = ConfigManager.Instance.Config;
            var race = config.folderName;
            var materialSkin = GetObjectMaterial();
            AssetDatabase.CreateAsset(materialSkin, folderName + "/" + race + "_SkinMat.mat");

            Texture2D mergedTexture = TextureManager.Instance.GetMergedTexture();

            var texturPath = folderName + "/" + race + "_CharacterSkin.png";
            File.WriteAllBytes(texturPath, mergedTexture.EncodeToPNG());
            AssetDatabase.Refresh();
            materialSkin.mainTexture = AssetDatabase.LoadAssetAtPath(texturPath, typeof(Texture2D)) as Texture2D;

            TextureManager.Instance.UpdateMaterial(materialSkin);

            PrefabUtility.CreatePrefab(folderName + "/" + race + ".prefab", config.GetCharacter(),
                ReplacePrefabOptions.Default);
        }

        /*
         * Create prefab for static atlas
         */
        private void CreatePrefab(string folderName)
        {
            var config = ConfigManager.Instance.Config;
            var materialArmor = GetObjectMaterial();
            AssetDatabase.CreateAsset(materialArmor, folderName + "/" + config.folderName + "_ArmorMat.mat");

            var armorAtlas = MeshManager.Instance.GetMergedTexture();
            var texturPath = folderName + "/" + config.folderName + "_Armor.png";
            File.WriteAllBytes(texturPath, armorAtlas.EncodeToPNG());
            AssetDatabase.Refresh();
            materialArmor.mainTexture = AssetDatabase.LoadAssetAtPath(texturPath, typeof(Texture2D)) as Texture2D;
            MeshManager.Instance.UpdateMaterial(materialArmor);

            CreateCloakAtlas(folderName);
            CreateSkinAtlas(folderName);
        }

        /*
         * Forming a dynamic atlas for armor. Convert a model UV
         */
        private void CreateArmorAtlas(string folderName)
        {
            var config = ConfigManager.Instance.Config;
            var materialArmor = GetObjectMaterial();
            AssetDatabase.CreateAsset(materialArmor, folderName + "/" + config.folderName + "_ArmorMat.mat");
            AssetDatabase.Refresh();

            Texture2D armorAtlas = MeshManager.Instance.GetMergedTexture();

            if (armorAtlas == null)
                return;

            int atlasSize = armorAtlas.width / ARMOR_IMAGE_SIZE;
            float uvsStep = 1f / atlasSize;

            List<AbstractMesh> selectedMeshes = MeshManager.Instance.SelectedMeshes;

            int itemNum = 0;
            for (int armNum = 0; armNum < selectedMeshes.Count; armNum++, itemNum++)
            {
                //Update LOD parts for each armor item
                var armorsParts = selectedMeshes[armNum].GetMesh().GetComponentsInChildren<MeshFilter>();
                for (var armLOD = 0; armLOD < armorsParts.Length; armLOD++)
                {
                    if (armorsParts[armLOD] != null)
                    {
                        if (!meshes.ContainsKey(armorsParts[armLOD].name))
                        {
                            armorsParts[armLOD].GetComponent<Renderer>().material = materialArmor;


                            //Update UVS for new atlas
                            Vector2[] uvs = armorsParts[armLOD].GetComponent<MeshFilter>().mesh.uv;
                            for (int i = 0; i < uvs.Length; i++)
                            {
                                uvs[i] = new Vector2(uvs[i].x / atlasSize + uvsStep * (itemNum % atlasSize),
                                    uvs[i].y / atlasSize + uvsStep * (atlasSize - 1 - (itemNum / atlasSize)));
                            }

                            armorsParts[armLOD].mesh.uv = uvs;

                            //assigne the selected LOD Mesh with new UV's to the new mesh to be exported
                            AssetDatabase.CreateAsset(
                                armorsParts[armLOD].mesh, folderName + "/Mesh/" + armorsParts[armLOD].name + "_New" + armLOD + ".asset"
                            ); //exporte asset to new project folder

                            meshes.Add(armorsParts[armLOD].name, armorsParts[armLOD]);
                        }
                        else
                        {
                            armorsParts[armLOD].mesh = meshes[armorsParts[armLOD].name].mesh;
                            armorsParts[armLOD].GetComponent<Renderer>().material = materialArmor;
                        }
                    }
                }
            }
            var texturPath = folderName + "/" + config.folderName + "_Armor.png";
            File.WriteAllBytes(texturPath, armorAtlas.EncodeToPNG());
            AssetDatabase.Refresh();
            materialArmor.mainTexture = AssetDatabase.LoadAssetAtPath(texturPath, typeof(Texture2D)) as Texture2D;
        }

        protected void CreateCloakAtlas(string folderName)
        {
            var config = ConfigManager.Instance.Config;
            Texture2D cloak = TextureManager.Instance.GetCloakTexture();
            if (cloak != null)
            {
                var materialCloak = GetObjectMaterial();
                AssetDatabase.CreateAsset(materialCloak, folderName + "/" + config.folderName + "_Cloak.mat");
                var texturPath = folderName + "/" + config.folderName + "_Cloak.png";

                File.WriteAllBytes(texturPath, cloak.EncodeToPNG());
                AssetDatabase.Refresh();
                materialCloak.mainTexture = AssetDatabase.LoadAssetAtPath(texturPath, typeof(Texture2D)) as Texture2D;
                TextureManager.Instance.UpdateCloakMaterial(materialCloak);
            }
        }
#endif

        private Material GetObjectMaterial()
        {
            var material = TextureManager.Instance.GetShaderMaterial();
            return new Material(material.material);
        }
    }
}
