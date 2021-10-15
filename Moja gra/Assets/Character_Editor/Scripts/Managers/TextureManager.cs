using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace CharacterEditor
{

    public class TextureMergeComparer : IComparer<AbstractTexture>
    {
        public int Compare(AbstractTexture x, AbstractTexture y) {
            return (new CaseInsensitiveComparer()).Compare(x.MergeOrder, y.MergeOrder);
        }
    }

    [Serializable]
    public class MaterialInfo
    {
        [SerializeField]
        internal TextureShaderType shader;

        [SerializeField]
        internal Material material;

        [SerializeField]
        internal Material meshMaterial;

        [SerializeField]
        internal Material cloakMaterial;
    }


    public class TextureManager : MonoBehaviour
    {
        [SerializeField]
        private MaterialInfo[] _materials;
        public MaterialInfo[] Materials {get { return _materials; }}

        [SerializeField]
        private RawImage skinRawImage;

        private List<SkinnedMeshRenderer> modelRenderers;
        private List<SkinnedMeshRenderer> cloakRenderers;

        
        private string characterRace;
        private AbstractTexture baseTexture;
        private Dictionary<string, Dictionary<TextureType, AbstractTexture>> _characterTextures;

        public Dictionary<TextureType, AbstractTexture> currentCharacterTextures {
            get; private set;
        }

        private Color32[] basedPixels;
        private Color32[] changedPixels;
    

        private AbstractTexture[] sortTextures;
        private Texture2D mergedTexture;
        private TextureType[] ignoreTypes;

        private List<TextureType> mergeTypes = new List<TextureType>();
        private Coroutine mergeCoroutine;

        public EventHandler OnTextureChanged;

        public bool IsReady { get; private set; }
        public static TextureManager Instance { get; private set; }

        public Dictionary<string, TextureShaderType> CharacterShaders { get; private set; }
        public TextureShaderType CurrentCharacterShader { get; private set; }

        void Awake()
        {
            if (Instance != null) {
                Destroy(this.gameObject);
            }
            Instance = this;
            IsReady = false;

            _characterTextures = new Dictionary<string, Dictionary<TextureType, AbstractTexture>>();
            basedPixels = changedPixels = new Color32[1024 * 1024];
            modelRenderers = new List<SkinnedMeshRenderer>();
            cloakRenderers = new List<SkinnedMeshRenderer>();
            CharacterShaders = new Dictionary<string, TextureShaderType>();

        }

        IEnumerator Start()
        {
            if (!LoaderManager.Instance.IsReady)
                yield return null;
        }

        /*
         * Change Character. Update textures and skin meshes
         */
        public IEnumerator ApplyConfig(Config config)
        {
            var loader = LoaderManager.Instance.TextureLoader;

            modelRenderers.Clear();
            modelRenderers.AddRange(config.GetSkinMesshes());
            modelRenderers.AddRange(config.GetShortRobeMeshes());
            modelRenderers.AddRange(config.GetLongRobeMeshes());

            characterRace = config.folderName;
            if (!_characterTextures.ContainsKey(characterRace))
            {
                currentCharacterTextures = new Dictionary<TextureType, AbstractTexture>();

                foreach (var texture in config.availableTextures) {
                    currentCharacterTextures[texture] = TextureFactory.Create(texture, loader, characterRace);
                }
                _characterTextures[characterRace] = currentCharacterTextures;
            }

            currentCharacterTextures = _characterTextures[characterRace];

            cloakRenderers.Clear();
            cloakRenderers.AddRange(config.GetCloakMesshes());

            sortTextures = currentCharacterTextures.Values.ToArray();
            Array.Sort(sortTextures, new TextureMergeComparer());

            mergeTypes = new List<TextureType>(currentCharacterTextures.Keys.ToArray());
            mergeCoroutine = StartCoroutine(MergeTextures());
            yield return mergeCoroutine;

            if (!CharacterShaders.ContainsKey(characterRace) || CharacterShaders[characterRace] != CurrentCharacterShader)
                SetShader(CurrentCharacterShader);
        }

        public MaterialInfo GetShaderMaterial(TextureShaderType shader)
        {
            foreach (var mat in _materials)
            {
                if (mat.shader == shader)
                    return mat;
            }
            return null;
        }

        public MaterialInfo GetShaderMaterial()
        {
            return GetShaderMaterial(CurrentCharacterShader);
        }

        /*
        * Update skin mesh materials and shaders
        */
        public void SetShader(TextureShaderType shader)
        {
            var materialInfo = GetShaderMaterial(shader);
            if (materialInfo == null)
                return;

            CurrentCharacterShader = shader;
            CharacterShaders[characterRace] = shader;

            var material = materialInfo.material;
            material.mainTexture = mergedTexture;
            foreach (var render in modelRenderers)
                render.material = material;

            var cloakMaterial = materialInfo.cloakMaterial;
            cloakMaterial.mainTexture = GetCloakTexture();
            foreach (var render in cloakRenderers)
                render.material = cloakMaterial;
        }
      
        /*
         * Set up skin texture on character skin meshes. Update materials
         */
        public void LoadTexture(Texture2D texture, Dictionary<TextureType, TextureInfo> textureInfo, int cloakTexture, bool equipShortRobe, bool equipLongRobe)
        {
            var shortRobeRenderers = ConfigManager.Instance.Config.GetShortRobeMeshes();
            var longRobeRenderers = ConfigManager.Instance.Config.GetLongRobeMeshes();

            foreach (var info in textureInfo)
            {
                currentCharacterTextures[info.Key].SelectedTexture = info.Value.selectedTexture;
                currentCharacterTextures[info.Key].SelectedColor = info.Value.selectedColor;
            }
            mergedTexture = texture;
            currentCharacterTextures[TextureType.Cloak].SelectedTexture = cloakTexture;

            UpdateModelRenderers();
            UpdateCloakTexture();

            if (cloakTexture != 0)
            {
                for (int i = 0; i < cloakRenderers.Count; i++)
                    cloakRenderers[i].gameObject.SetActive(true);
            }

            if (equipShortRobe)
            {
                for (int i = 0; i < shortRobeRenderers.Length; i++)
                    shortRobeRenderers[i].gameObject.SetActive(true);
            }

            if (equipLongRobe)
            {
                for (int i = 0; i < longRobeRenderers.Length; i++)
                    longRobeRenderers[i].gameObject.SetActive(true);
            }
        }

        /*
         * Combining the texture of the character
         */
        private IEnumerator MergeTextures()
        {
            IsReady = false;
            foreach (var type in mergeTypes)
            {
                while (!currentCharacterTextures[type].IsReady) {
                    yield return null;
                }
            }

            //Main skin texture
            basedPixels = sortTextures[0].GetPixels32();
            Array.Copy(basedPixels, changedPixels, basedPixels.Length);

            int index;
            int c = 0;
            //Udpate texture pixels, alternately drawing each texture
            for (int i = 1; i < sortTextures.Length; i++) {
                if (ignoreTypes != null && ignoreTypes.Contains(sortTextures[i].Type)) { continue; }
                if (sortTextures[i].Type == TextureType.Cloak) { continue; }
          
                var currentPixels = sortTextures[i].GetPixels32();
                c += currentPixels.Length;
                for (int j = 0; j < currentPixels.Length; j++) {
                    if (currentPixels[j].a != 0) {
                        index = (sortTextures[0].width * sortTextures[i].y + sortTextures[i].x) + j + ((int)j / sortTextures[i].width) * (sortTextures[0].width - sortTextures[i].width);
                        changedPixels[index] = Color32.LerpUnclamped(changedPixels[index], currentPixels[j], currentPixels[j].a / 255f);
                    }
                }
                
                if (c > 500000)
                {
                    c = 0;
                    yield return null;
                }
            }
            mergedTexture = new Texture2D(sortTextures[0].width, sortTextures[0].height);
            mergedTexture.SetPixels32(changedPixels);
            mergedTexture.Apply();

            UpdateModelRenderers();

            if (OnTextureChanged != null) OnTextureChanged(this, null);

            ignoreTypes = null;
            mergeCoroutine = null;

            mergeTypes.Clear();
            Resources.UnloadUnusedAssets();
            IsReady = true;

#if UNITY_EDITOR
            yield return null;
            UnityEditor.EditorUtility.UnloadUnusedAssetsImmediate();
#endif
        }

        protected void UpdateModelRenderers()
        {
            foreach (var render in modelRenderers)
                render.material.mainTexture = mergedTexture;

            if (skinRawImage != null)
                skinRawImage.texture = mergedTexture;
            
        }

        protected IEnumerator UpdateCloakTexture()
        {
            while (!currentCharacterTextures[TextureType.Cloak].IsReady) {
                yield return null;
            }

            foreach (SkinnedMeshRenderer render in cloakRenderers)
                render.material.mainTexture = currentCharacterTextures[TextureType.Cloak].Current;
        }

        public Texture2D GetMergedTexture() {
            return mergedTexture;
        }

        public Texture2D GetCloakTexture() {
            return currentCharacterTextures[TextureType.Cloak].Current;
        }

        public void UpdateMaterial(Material mat)
        {
            foreach (var render in modelRenderers)
                render.material = mat;
        }

        public void UpdateCloakMaterial(Material mat)
        {
            foreach (SkinnedMeshRenderer render in cloakRenderers)
                render.material = mat;
        }

        public void OnPrevTexture(TextureType[] types, TextureType[] clearTypes = null) {
            if (!IsReady) return;

            AbstractTexture mainTexture = null;
            for (int i = 0; i < types.Length; i++) {
                if (!currentCharacterTextures.ContainsKey(types[i]))
                    continue;

                if (mainTexture == null) {
                    mainTexture = currentCharacterTextures[types[i]];
                    mainTexture.MovePrev();
                } else {
                    currentCharacterTextures[types[i]].SelectedTexture = mainTexture.SelectedTexture;
                }
            }

            ResetTexture(clearTypes);
            OnChangeTexture(types);
        }

        public void OnPrevColor(TextureType[] types)
        {
            if (!IsReady) return;

            AbstractTexture mainTexture = null;
            for (int i = 0; i < types.Length; i++)
            {
                if (!currentCharacterTextures.ContainsKey(types[i]))
                    continue;

                if (mainTexture == null) {
                    mainTexture = currentCharacterTextures[types[i]];
                    mainTexture.MovePrevColor();
                } else {
                    currentCharacterTextures[types[i]].SelectedColor = mainTexture.SelectedColor;
                }
            }
            OnChangeTexture(types);
        }

        public void OnNextTexture(TextureType[] types, TextureType[] clearTypes = null)
        {
            if (!IsReady) return;

            AbstractTexture mainTexture = null;
            for (int i = 0; i < types.Length; i++) {
                if (!currentCharacterTextures.ContainsKey(types[i]))
                    continue;

                if (mainTexture == null)
                {
                    mainTexture = currentCharacterTextures[types[i]];
                    mainTexture.MoveNext();
                }
                else {
                    currentCharacterTextures[types[i]].SelectedTexture = mainTexture.SelectedTexture;
                }
            }
            ResetTexture(clearTypes);
            OnChangeTexture(types);
        }

        public void OnNextColor(TextureType[] types)
        {
            if (!IsReady) return;

            AbstractTexture mainTexture = null;
            for (int i = 0; i < types.Length; i++) {
                if (!currentCharacterTextures.ContainsKey(types[i]))
                    continue;

                if (mainTexture == null) {
                    mainTexture = currentCharacterTextures[types[i]];
                    mainTexture.MoveNextColor();
                } else {
                    currentCharacterTextures[types[i]].SelectedColor = mainTexture.SelectedColor;
                }
            }

            OnChangeTexture(types);
        }

        public void OnResetTexure(TextureType[] types) {
            if (!IsReady) return;
            ResetTexture(types);
            OnChangeTexture(types);
        }

        private void ResetTexture(TextureType[] types)
        {
            if (types == null)
                return;

            for (int i = 0; i < types.Length; i++)
            {
                if (!currentCharacterTextures.ContainsKey(types[i]))
                    continue;

                currentCharacterTextures[types[i]].Reset();
            }
        }

        public void OnResetColor(TextureType[] types) {
            if (!IsReady) return;
            for (int i = 0; i < types.Length; i++) {
                if (!currentCharacterTextures.ContainsKey(types[i]))
                    continue;

                currentCharacterTextures[types[i]].ResetColor();
            }
            OnChangeTexture(types);
        }

        public void OnClear(TextureType[] types) {
            if (!IsReady) return;

            for (int i = 0; i < types.Length; i++) {
                if (!currentCharacterTextures.ContainsKey(types[i]))
                    continue;

                currentCharacterTextures[types[i]].ResetColor();
                currentCharacterTextures[types[i]].Reset();
            }
            OnChangeTexture(types);
        }

        public void OnRandom(TextureType[] types, TextureType[] sameColors, TextureType[] ignoreTypes = null) {
            if (!IsReady) return;

            foreach (TextureType type in types)
            {
                if (!currentCharacterTextures.ContainsKey(type))
                    continue;

                currentCharacterTextures[type].Shuffle();
                currentCharacterTextures[type].ShuffleColor();
            }

            for (int i = 1; i < sameColors.Length; i++) {
                if (!currentCharacterTextures.ContainsKey(sameColors[i]))
                    continue;

                currentCharacterTextures[sameColors[i]].SelectedColor = currentCharacterTextures[sameColors[0]].SelectedColor;
            }
            this.ignoreTypes = ignoreTypes;
            OnChangeTexture(types);
        }

        /*
         * Prepare skin meshes and check merge texture
         */
        private void OnChangeTexture(TextureType[] changedTypes)
        {
            var types = new List<TextureType>();
            foreach (var type in changedTypes)
            {
                if (currentCharacterTextures.ContainsKey(type))
                    types.Add(type);
            }

            if (types.Count == 0)
                return;

            PrepareSkinMeshTextures(types);
            mergeTypes.AddRange(types);

            if (mergeCoroutine != null)
            {
                StopCoroutine(mergeCoroutine);
                mergeCoroutine = null;
            }
            mergeCoroutine = StartCoroutine(MergeTextures());
        }

        private void PrepareSkinMeshTextures(List<TextureType> types)
        {
            if (types.Contains(TextureType.Cloak)) {
                StartCoroutine(UpdateCloakTexture());
                if (types.Count == 1)
                    return;
            }

            if (ignoreTypes == null)
                return;

            List< TextureType> skined = new List<TextureType>()
            {
                TextureType.RobeLong,
                TextureType.RobeShort,
                TextureType.Pants
            };
            for (int i = 0; i < ignoreTypes.Length; i++)
            {
                skined.Remove(ignoreTypes[i]);
                currentCharacterTextures[ignoreTypes[i]].Reset();
            }
            TextureType type = skined.First();
            if (currentCharacterTextures[type].SelectedTexture == 0)
            {
                currentCharacterTextures[type].MoveNext();
            }
        }
    }
}
