using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CharacterEditor.Mesh;
using UnityEngine;
using CharacterEditor.FX;
using UnityEngine.UI;

namespace CharacterEditor
{

    public class MeshManager : MonoBehaviour
    {
        private string characterRace;

        private Dictionary<string, Dictionary<MeshType, AbstractMesh>> characterMeshes;
        private Dictionary<MeshType, AbstractMesh> currentCharacterMeshes;

        private Dictionary<string, Dictionary<FXType, AbstractFXMesh>> characterFxMeshes;
        private Dictionary<FXType, AbstractFXMesh> currentCharacterFxMeshes;

        private AbstractMesh[] sortMeshes;
        private Texture2D armorTexture;

        [SerializeField]
        private RawImage armorRawImage;

        public List<AbstractMesh> SelectedMeshes {
            get; private set;
        }

        private Coroutine buildCoroutine;

        public bool IsReady { get; private set; }
        public static MeshManager Instance { get; private set; }

        private int meshTextureWidth = 512;
        private int meshTexturHeight = 512;
        private Color32[] emptyPixels;

        void Awake()
        {
            if (Instance != null)
                Destroy(this.gameObject);
            
            Instance = this;

            characterMeshes = new Dictionary<string, Dictionary<MeshType, AbstractMesh>>();
            characterFxMeshes = new Dictionary<string, Dictionary<FXType, AbstractFXMesh>>();
            SelectedMeshes = new List<AbstractMesh>();
        }

        IEnumerator Start()
        {
            if (!LoaderManager.Instance.IsReady)
                yield return null;
        }

        /*
         * Change Character. Update armor/weapon meshes and fx meshes
         */
        public IEnumerator ApplyConfig(Config config)
        {
            var loader = LoaderManager.Instance.MeshLoader;

            characterRace = config.folderName;
            if (!characterMeshes.ContainsKey(characterRace))
            {
                currentCharacterMeshes = new Dictionary<MeshType, AbstractMesh>();
                foreach (var meshBone in config.availableMeshes)
                    currentCharacterMeshes[meshBone.mesh] = MeshFactory.Create(loader, meshBone, config);
                
                characterMeshes[characterRace] = currentCharacterMeshes;
            }

            if (!characterFxMeshes.ContainsKey(characterRace))
            {
                currentCharacterFxMeshes = new Dictionary<FXType, AbstractFXMesh>();
                foreach (var fxMeshBone in config.availableFxMeshes)
                    currentCharacterFxMeshes[fxMeshBone.mesh] = FxMeshFactory.Create(loader, fxMeshBone, config);
                
                characterFxMeshes[characterRace] = currentCharacterFxMeshes;
            }

            currentCharacterMeshes = characterMeshes[characterRace];
            currentCharacterFxMeshes = characterFxMeshes[characterRace];

            sortMeshes = currentCharacterMeshes.Values.ToArray();

            foreach (AbstractMesh mesh in sortMeshes)
            {
                while (!mesh.IsReady)
                {
                    yield return null;
                }
            }

            emptyPixels = new Color32[meshTextureWidth * meshTexturHeight];
            for (int i = 0; i < emptyPixels.Length; i++)
                emptyPixels[i] = new Color32(0, 0, 0, 0);

            armorTexture = new Texture2D(meshTextureWidth, meshTexturHeight);
            armorTexture.SetPixels32(emptyPixels);
            armorTexture.Apply();

            yield return StartCoroutine(BuildTexture());

            if (!TextureManager.Instance.CharacterShaders.ContainsKey(characterRace) || TextureManager.Instance.CharacterShaders[characterRace] != TextureManager.Instance.CurrentCharacterShader)
                SetShader(TextureManager.Instance.CurrentCharacterShader);
            
            IsReady = true;
        }

        /*
         * Update mesh materials and shaders
         */
        public void SetShader(TextureShaderType shader)
        {
            var materialInfo = TextureManager.Instance.GetShaderMaterial(shader);
            if (materialInfo == null)
                return;

            var material = materialInfo.meshMaterial;
            material.mainTexture = armorTexture;

            foreach (var mesh in currentCharacterMeshes.Values)
            {
                foreach (var meshObject in mesh.Meshes)
                {
                    foreach (var render in meshObject.GetComponentsInChildren<MeshRenderer>())
                    {
                        if (IsDynamicTextureAtlas())
                        {
                            var texture = render.sharedMaterial.mainTexture;
                            render.material = material;
                            render.material.mainTexture = texture;
                        }
                        else
                        {
                            render.material = material;
                        }
                    }
                }
            }
        }

        private bool IsDynamicTextureAtlas()
        {
#if UNITY_EDITOR
            return LoaderManager.Instance.Type == LoaderType.AssetDatabase && LoaderManager.Instance.MeshAtlasType == MeshAtlasType.Dynamic;
#else
            return false;
#endif
        }

        /*
         * Create mesh atlas from selected meshes
         */
        private IEnumerator BuildTexture()
        {
            if (!IsReady)
                yield break;

            SelectedMeshes.Clear();
            foreach (var mesh in sortMeshes)
            {
                if (mesh.GetMesh() != null)
                {
                    SelectedMeshes.Add(mesh);
                    while (!mesh.IsReady)
                        yield return null;
                }
            }
            //Create empty atlas
            int size = 2048;
            if (IsDynamicTextureAtlas())
                size = SelectedMeshes.Count > 4 ? 2048 : (SelectedMeshes.Count > 1 ? 1024 : 512);

            armorTexture = new Texture2D(size, size);
            if (SelectedMeshes.Count == 0)
            {
                if (armorRawImage != null)
                {
                    armorRawImage.enabled = false;
                    armorRawImage.texture = armorTexture;
                }
                yield break;

            }

            //Insert mesh textures in atlas
            int j = 0;
            foreach (var selectedMesh in SelectedMeshes)
            {
                int position = IsDynamicTextureAtlas() ? j++ : selectedMesh.MergeOrder;
                int x = meshTextureWidth * (position % (size / meshTexturHeight));
                int y = (size - meshTexturHeight) - (meshTexturHeight * position / size) * meshTexturHeight;

                if (IsDynamicTextureAtlas() && selectedMesh.GetMesh() == null)
                    continue;
                

                if (selectedMesh.GetMesh() != null) {
                    armorTexture.SetPixels32(x, y, meshTextureWidth, meshTexturHeight, selectedMesh.Texture.GetPixels32());
                }
                else {
                    armorTexture.SetPixels32(x, y, meshTextureWidth, meshTexturHeight, emptyPixels);
                }
            }

            armorTexture.Apply();
            if (armorRawImage != null)
            {
                armorRawImage.texture = armorTexture;
                armorRawImage.enabled = true;
            }
            UpdateModelRenderers();
        }


        public void UpdateMaterial(Material material)
        {
            for (int i = 0; i < sortMeshes.Length; i++)
            {
                GameObject meshObject = sortMeshes[i].GetMesh();
                if (meshObject != null)
                {
                    foreach (var meshRenderer in meshObject.GetComponentsInChildren<MeshRenderer>())
                        meshRenderer.material = material;
                }
            }
        }

        private void UpdateModelRenderers()
        {
            if (!IsDynamicTextureAtlas())
            {
                for (int i = 0; i < sortMeshes.Length; i++)
                {
                    GameObject meshObject = sortMeshes[i].GetMesh();
                    if (meshObject != null)
                    {
                        foreach (var meshRenderer in meshObject.GetComponentsInChildren<MeshRenderer>())
                            foreach (var material in meshRenderer.materials)
                                material.mainTexture = armorTexture;
                    }
                }
            }
        }

        public Texture2D GetMergedTexture()
        {
            return armorTexture;
        }

        #region Mesh Actions
        public void OnNextMesh(MeshType[] types)
        {
            if (!IsReady) { return; }
            for (int i = 0; i < types.Length; i++)
            {
                if (currentCharacterMeshes.ContainsKey(types[i]))
                    currentCharacterMeshes[types[i]].MoveNext();
            }
            OnChangeMesh();
        }

        public void OnPrevMesh(MeshType[] types)
        {
            if (!IsReady) { return; }
            for (int i = 0; i < types.Length; i++)
            {
                if (currentCharacterMeshes.ContainsKey(types[i]))
                    currentCharacterMeshes[types[i]].MovePrev();
            }
            OnChangeMesh();
        }

        public void OnClearMesh(MeshType[] types)
        {
            if (!IsReady) { return; }
            for (int i = 0; i < types.Length; i++)
            {
                if (currentCharacterMeshes.ContainsKey(types[i]))
                    currentCharacterMeshes[types[i]].Reset();
            }
            OnChangeMesh();
        }

        public void OnRandom(MeshType[] types, MeshType[][] sameTypes = null, bool updateMesh = true)
        {
            if (!IsReady) { return; }
            //Remove same meshes from random list
            if (sameTypes != null && sameTypes.Length > 0)
            {
                List<MeshType> typesList = new List<MeshType>(types);
                foreach (MeshType[] typeGroup in sameTypes)
                {
                    for (int i = 1; i < typeGroup.Length; i++)
                    {
                        typesList.Remove(typeGroup[i]);
                    }
                }
                types = typesList.ToArray();
            }

            //Shuffle available types
            for (int i = 0; i < types.Length; i++)
            {
                if (currentCharacterMeshes.ContainsKey(types[i]))
                    currentCharacterMeshes[types[i]].Shuffle();
            }

            if (sameTypes != null)
            {
                foreach (MeshType[] typeGroup in sameTypes)
                {
                    for (int i = 1; i < typeGroup.Length; i++)
                    {
                        if (currentCharacterMeshes.ContainsKey(typeGroup[i])) {
                            currentCharacterMeshes[typeGroup[i]].SetMesh(currentCharacterMeshes[typeGroup[0]].SelectedMesh);
                            currentCharacterMeshes[typeGroup[i]].SetTexture(currentCharacterMeshes[typeGroup[0]].Texture.SelectedTexture);
                            currentCharacterMeshes[typeGroup[i]].SetColor(currentCharacterMeshes[typeGroup[0]].Texture.SelectedColor);
                        }
                    }
                }
            }
            if (updateMesh) OnChangeMesh();
            
        }
        #endregion

        #region Color Actions
        public void OnNextColor(MeshType[] types)
        {
            if (!IsReady) { return; }
            for (int i = 0; i < types.Length; i++)
            {
                if (currentCharacterMeshes.ContainsKey(types[i]))
                    currentCharacterMeshes[types[i]].MoveNextColor();
            }
            OnChangeMesh();
        }

        public void OnPrevColor(MeshType[] types)
        {
            if (!IsReady) { return; }
            for (int i = 0; i < types.Length; i++)
            {
                if (currentCharacterMeshes.ContainsKey(types[i]))
                    currentCharacterMeshes[types[i]].MovePrevColor();
            }
            OnChangeMesh();
        }

        public void SetMeshColor(MeshType[] types, int color)
        {
            if (!IsReady) { return; }
            for (int i = 0; i < types.Length; i++)
            {
                if (currentCharacterMeshes.ContainsKey(types[i]))
                    currentCharacterMeshes[types[i]].SetColor(color);
            }
            OnChangeMesh();
        }

        public void OnClearMeshColor(MeshType[] types)
        {
            if (!IsReady) { return; }
            for (int i = 0; i < types.Length; i++)
            {
                if (currentCharacterMeshes.ContainsKey(types[i]))
                    currentCharacterMeshes[types[i]].ResetColor();
            }
            OnChangeMesh();
        }
        #endregion

        #region FX Actions
        public void OnNextFX(FXType[] types)
        {
            if (!IsReady) { return; }
            for (int i = 0; i < types.Length; i++)
            {
                if (currentCharacterFxMeshes.ContainsKey(types[i]))
                    currentCharacterFxMeshes[types[i]].MoveNext();
            }
        }

        public void OnPrevFX(FXType[] types)
        {
            for (int i = 0; i < types.Length; i++)
            {
                if (currentCharacterFxMeshes.ContainsKey(types[i]))
                    currentCharacterFxMeshes[types[i]].MovePrev();
            }
        }

        public void OnClearFX(FXType[] types)
        {
            if (!IsReady) { return; }
            for (int i = 0; i < types.Length; i++)
            {
                if (currentCharacterFxMeshes.ContainsKey(types[i]))
                    currentCharacterFxMeshes[types[i]].Reset();
            }
        }

        public void OnRandomFX(FXType[] types)
        {
            if (!IsReady) { return; }
            for (int i = 0; i < types.Length; i++)
            {
                if (currentCharacterFxMeshes.ContainsKey(types[i]))
                    currentCharacterFxMeshes[types[i]].Shuffle();
            }
        }
        #endregion

        private void OnChangeMesh()
        {
            if (buildCoroutine != null)
            {
                StopCoroutine(buildCoroutine);
            }
            buildCoroutine = StartCoroutine(BuildTexture());
        }

        public void LoadMeshes(Texture2D meshTexture, Dictionary<MeshType, MeshInfo> meshData, Dictionary<FXType, MeshInfo> fxMeshData)
        {
            foreach (var meshInfo in meshData)
            {
                currentCharacterMeshes[meshInfo.Key].SetMesh(meshInfo.Value.selectedMesh);
                currentCharacterMeshes[meshInfo.Key].SetTexture(meshInfo.Value.selectedTexture);
            }
            foreach (var meshInfo in fxMeshData)
            {
                currentCharacterFxMeshes[meshInfo.Key].SetMesh(meshInfo.Value.selectedMesh);
            }
            armorTexture = meshTexture;
            UpdateModelRenderers();
        }

        public AbstractMesh[] GetMeshes()
        {
            return sortMeshes;
        }

        public AbstractFXMesh[] GetFxMeshes()
        {
            return currentCharacterFxMeshes.Values.ToArray();
        }
    }
}
