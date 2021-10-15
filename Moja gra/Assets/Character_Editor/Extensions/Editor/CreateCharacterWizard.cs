using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CharacterEditor;
using UnityEditor;

public class CreateCharacterWizard : ScriptableWizard
{
    public string folderName = "";
    public GameObject model;
    public string headBone;

    public SkinnedMeshRenderer[] skinnedMeshes;
    public SkinnedMeshRenderer[] shortRobeMeshes;
    public SkinnedMeshRenderer[] longRobeMeshes;
    public SkinnedMeshRenderer[] cloakMeshes;

    [EnumFlag]
    public TextureType textureMask;
    private TextureType[] _availableTextures;


    public MeshTypeBone[] availableMeshes;
    public FxMeshTypeBone[] availableFxMeshes;

    private Config _selectedObject;


    [MenuItem("Tools/Character Editor/Create Character Wizard...")]
    static void CreateWizard()
    {
        ScriptableWizard.DisplayWizard<CreateCharacterWizard>("Create Character", "Save new", "Update selected");
    }

    void Awake()
    {
        var selectedObject = Selection.activeObject;
        if (selectedObject != null && selectedObject is Config)
        {
            _selectedObject = selectedObject as Config;
            InitValues(_selectedObject);
        }
        else
        {
            //Default bones and meshes
            availableMeshes = new MeshTypeBone[]
            {
                new MeshTypeBone(MeshType.Hair, "Bip01_Head"),
                new MeshTypeBone(MeshType.Beard, "Bip01_Jaw"),
                new MeshTypeBone(MeshType.FaceFeature, "Bip01_Jaw"),
                new MeshTypeBone(MeshType.Helm, "Bip01_Head"),
                new MeshTypeBone(MeshType.Torso, "Bip01_Spine3"),
                new MeshTypeBone(MeshType.TorsoAdd, "Bip01_Spine3"),
                new MeshTypeBone(MeshType.LegLeft, "Bip01_L_Calf"),
                new MeshTypeBone(MeshType.LegRight, "Bip01_R_Calf"),
                new MeshTypeBone(MeshType.ShoulderLeft, "Bip01_L_UpperArm"),
                new MeshTypeBone(MeshType.ShoulderRight, "Bip01_R_UpperArm"),
                new MeshTypeBone(MeshType.ArmLeft, "Bip01_L_Forearm"),
                new MeshTypeBone(MeshType.ArmRight, "Bip01_R_Forearm"),
                new MeshTypeBone(MeshType.Belt, "Bip01_Pelvis"),
                new MeshTypeBone(MeshType.BeltAdd, "Bip01_Pelvis"),
                new MeshTypeBone(MeshType.HandLeft, "Bip01_L_Weapon"),
                new MeshTypeBone(MeshType.HandRight, "Bip01_R_Weapon"),
            };

            availableFxMeshes = new FxMeshTypeBone[]
            {
                new FxMeshTypeBone(FXType.Torso, "Bip01_Spine3"), 
                new FxMeshTypeBone(FXType.Eye, "Bip01_Head"),
            };

            headBone = "Bip01_Head";
        }
    }

    void OnWizardCreate()
    {
        var config = ScriptableObject.CreateInstance<Config>();

        SetValues(config);

        if (!System.IO.Directory.Exists(Application.dataPath + "/Character_Editor/Configs"))
        {
            AssetDatabase.CreateFolder("Assets/Character_Editor", "Configs");
        }

        AssetDatabase.CreateAsset(config, GetAssetpath(folderName));
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = config;
    }

    void OnWizardOtherButton()
    {
        if (_selectedObject == null)
            return;

        SetValues(_selectedObject);

        EditorUtility.CopySerialized(_selectedObject, _selectedObject);
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = _selectedObject;
    }

    void OnWizardUpdate()
    {
        helpString = "Enter character details";
    }

    private void SetValues(Config config)
    {
        InitTextureTypes();

        config.prefabPath = AssetDatabase.GetAssetPath(model);
        if (config.prefabPath == "")
        {
            var prefab = PrefabUtility.GetPrefabParent(model);
            config.prefabPath = AssetDatabase.GetAssetPath(prefab);

        }
        config.folderName = folderName;
        config.availableMeshes = availableMeshes;
        config.availableFxMeshes = availableFxMeshes;
        config.availableTextures = _availableTextures;
        config.skinnedMeshes = new string[skinnedMeshes.Length];
        config.headBone = headBone;

        for (int i = 0; i < skinnedMeshes.Length; i++)
        {
            config.skinnedMeshes[i] = skinnedMeshes[i].name;
        }

        config.cloakMeshes = new string[cloakMeshes.Length];
        for (int i = 0; i < cloakMeshes.Length; i++)
        {
            config.cloakMeshes[i] = cloakMeshes[i].name;
        }

        config.shortRobeMeshes = new string[shortRobeMeshes.Length];
        for (int i = 0; i < shortRobeMeshes.Length; i++)
        {
            config.shortRobeMeshes[i] = shortRobeMeshes[i].name;
        }
        
        config.longRobeMeshes = new string[longRobeMeshes.Length];
        for (int i = 0; i < longRobeMeshes.Length; i++)
        {
            config.longRobeMeshes[i] = longRobeMeshes[i].name;
        }
    }

    private void InitValues(Config config)
    {
        folderName = config.folderName;
        model = AssetDatabase.LoadAssetAtPath<GameObject>(config.prefabPath);
        headBone = config.headBone;
        availableMeshes = config.availableMeshes;
        availableFxMeshes = config.availableFxMeshes;
        _availableTextures = config.availableTextures;

        if (config.skinnedMeshes != null) {
            skinnedMeshes = new SkinnedMeshRenderer[config.skinnedMeshes.Length];
            for (int i = 0; i < config.skinnedMeshes.Length; i++) {
                skinnedMeshes[i] = model.transform.Find(config.skinnedMeshes[i]).GetComponent<SkinnedMeshRenderer>();
            }
        }

        if (config.cloakMeshes != null) { 
            cloakMeshes = new SkinnedMeshRenderer[config.cloakMeshes.Length];
            for (int i = 0; i < config.cloakMeshes.Length; i++) {
                cloakMeshes[i] = model.transform.Find(config.cloakMeshes[i]).GetComponent<SkinnedMeshRenderer>();
            }
        }

        if (config.shortRobeMeshes != null)
        {
            shortRobeMeshes = new SkinnedMeshRenderer[config.shortRobeMeshes.Length];
            for (int i = 0; i < config.shortRobeMeshes.Length; i++)
            {
                shortRobeMeshes[i] = model.transform.Find(config.shortRobeMeshes[i]).GetComponent<SkinnedMeshRenderer>();
            }
        }

        if (config.longRobeMeshes != null)
        {
            longRobeMeshes = new SkinnedMeshRenderer[config.longRobeMeshes.Length];
            for (int i = 0; i < config.longRobeMeshes.Length; i++)
            {
                longRobeMeshes[i] = model.transform.Find(config.longRobeMeshes[i]).GetComponent<SkinnedMeshRenderer>();
            }
        }
        InitTextureMask();
    }

    private void InitTextureTypes()
    {
        List<TextureType> list = new List<TextureType>();
        foreach (var enumValue in System.Enum.GetValues(typeof(TextureType))) {
            int checkBit = (int)textureMask & (int)enumValue;
            if (checkBit != 0)
                list.Add((TextureType)enumValue);
        }
        _availableTextures = list.ToArray();
    }

    private void InitTextureMask()
    {
        textureMask = 0;
        for (int i = 0; i < _availableTextures.Length; i++) {
            textureMask |= _availableTextures[i];
        }
    }


    private string GetAssetpath(string name)
    {
        return "Assets/Character_Editor/Configs/" + name  + ".asset";
    }
}