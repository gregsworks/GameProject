using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CharacterEditor
{

    [Serializable]
    public class TypeMask
    {
        [EnumFlag] public MeshType types;

    }

    /*
     * Mixes the selected textures and meshes.
     * Sets the same color and selected tmesha (hair, beard)
     * Enables and disables random skin mesh
     */
    public class RandomBtn : MonoBehaviour, IPointerClickHandler
    {
        [Header("Skinned Mesh settings")] [EnumFlag] public SkinMeshType skinMeshTypeMask;
        private SkinnedMeshRenderer[] longRobeMeshes;
        private SkinnedMeshRenderer[] shortRobeMeshes;
        private SkinnedMeshRenderer[] cloakMeshes;

        [Header("Mesh settings")] [EnumFlag] public MeshType meshTypeMask;
        public TypeMask[] sameMeshes;

        private MeshType[] randomMesheTypes;
        private MeshType[][] sameMesheTypes;

        [Header("Texture settings")] [EnumFlag] public TextureType textureTypeMask;
        private TextureType[] randomTextureTypes;
        private TextureType[] ignoreTextureTypes;

        [Header("Color settings")] [EnumFlag] public MeshType colorMeshTypeMask;
        [EnumFlag] public TextureType colorTextureTypeMask;

        private MeshType[] sameMeshColorTypes;
        private TextureType[] sameTextureColorTypes;

        [Header("FX settings")] [EnumFlag] public FXType fxTypeMask;
        private FXType[] randomFxTypes;


        private Action visibleCalback;
        private Action updateMeshCalback;

        public void Start()
        {
            PrepareTextureTypes();
            PrepareMeshTypes();
            PrepareFXTypes();
            PrepareSameColorTypes();

            ConfigManager.Instance.OnChangeCharacter += PrepareSkinMeshTypes;
        }


        public void OnPointerClick(PointerEventData eventData)
        {
            TextureManager.Instance.OnTextureChanged -= TextureChangeHandler;

            updateMeshCalback = () =>
            {
                MeshManager.Instance.OnRandom(randomMesheTypes, sameMesheTypes, sameTextureColorTypes.Length == 0);
                MeshManager.Instance.OnRandomFX(randomFxTypes);

                if (sameTextureColorTypes.Length > 0)
                {
                    MeshManager.Instance.SetMeshColor(sameMeshColorTypes,
                        TextureManager.Instance.currentCharacterTextures[sameTextureColorTypes[0]].SelectedColor);
                }

            };

            if (randomTextureTypes.Length == 0) //Update only meshes
            {
                updateMeshCalback.Invoke();
            }
            else
            {
                TextureManager.Instance.OnTextureChanged += TextureChangeHandler;
                ShuffleSkinendMesh();
                TextureManager.Instance.OnRandom(randomTextureTypes, sameTextureColorTypes, ignoreTextureTypes);
            }
        }

        protected void PrepareTextureTypes()
        {
            List<TextureType> list = new List<TextureType>();
            foreach (var enumValue in System.Enum.GetValues(typeof(TextureType)))
            {
                int checkBit = (int) textureTypeMask & (int) enumValue;
                if (checkBit != 0)
                    list.Add((TextureType) enumValue);
            }
            randomTextureTypes = list.ToArray();
        }

        protected void PrepareFXTypes()
        {
            List<FXType> list = new List<FXType>();
            foreach (var enumValue in System.Enum.GetValues(typeof(FXType)))
            {
                int checkBit = (int) fxTypeMask & (int) enumValue;
                if (checkBit != 0)
                    list.Add((FXType) enumValue);
            }
            randomFxTypes = list.ToArray();
        }

        protected void PrepareSameColorTypes()
        {
            List<MeshType> meshList = new List<MeshType>();
            foreach (var enumValue in Enum.GetValues(typeof(MeshType)))
            {
                int checkBit = (int) colorMeshTypeMask & (int) enumValue;
                if (checkBit != 0)
                    meshList.Add((MeshType) enumValue);
            }
            sameMeshColorTypes = meshList.ToArray();

            List<TextureType> textureList = new List<TextureType>();
            foreach (var enumValue in Enum.GetValues(typeof(TextureType)))
            {
                int checkBit = (int) colorTextureTypeMask & (int) enumValue;
                if (checkBit != 0)
                    textureList.Add((TextureType) enumValue);
            }
            sameTextureColorTypes = textureList.ToArray();
        }

        protected void PrepareMeshTypes()
        {
            List<MeshType> list = new List<MeshType>();
            foreach (var enumValue in Enum.GetValues(typeof(MeshType)))
            {
                int checkBit = (int) meshTypeMask & (int) enumValue;
                if (checkBit != 0)
                    list.Add((MeshType) enumValue);
            }
            randomMesheTypes = list.ToArray();

            List<MeshType> sameList = new List<MeshType>();
            sameMesheTypes = new MeshType[sameMeshes.Length][];
            for (int i = 0; i < sameMeshes.Length; i++)
            {
                sameList.Clear();
                foreach (var enumValue in Enum.GetValues(typeof(MeshType)))
                {
                    int checkBit = (int) sameMeshes[i].types & (int) enumValue;
                    if (checkBit != 0)
                        sameList.Add((MeshType) enumValue);
                }
                sameMesheTypes[i] = sameList.ToArray();
            }
        }

        protected void PrepareSkinMeshTypes(object sender, EventArgs e)
        {
            var config = ConfigManager.Instance.Config;
            foreach (var enumValue in Enum.GetValues(typeof(SkinMeshType)))
            {
                int checkBit = (int) skinMeshTypeMask & (int) enumValue;
                if (checkBit != 0)
                {
                    switch ((SkinMeshType) enumValue)
                    {
                        case SkinMeshType.RobeLong:
                            longRobeMeshes = config.GetLongRobeMeshes();
                            break;
                        case SkinMeshType.RobeShort:
                            shortRobeMeshes = config.GetShortRobeMeshes();
                            break;
                        case SkinMeshType.Cloak:
                            cloakMeshes = config.GetCloakMesshes();
                            break;
                    }
                }
            }
        }

        public void ShuffleSkinendMesh()
        {
            ignoreTextureTypes = null;

            bool showCloak = randomTextureTypes.Contains(TextureType.Cloak) && (UnityEngine.Random.Range(0, 2) == 1);

            if (randomTextureTypes.Contains(TextureType.Pants))
            {
                int rand = UnityEngine.Random.Range(0, 3);
                switch (rand)
                {
                    case 0:
                        visibleCalback = () =>
                        {
                            SetVisible(cloakMeshes, showCloak);
                            SetVisible(longRobeMeshes, true);
                            SetVisible(shortRobeMeshes, false);
                        };
                        ignoreTextureTypes = new TextureType[1] {TextureType.RobeShort};
                        break;
                    case 1:
                        visibleCalback = () =>
                        {
                            SetVisible(cloakMeshes, showCloak);
                            SetVisible(longRobeMeshes, false);
                            SetVisible(shortRobeMeshes, true);
                        };
                        ignoreTextureTypes = new TextureType[1] {TextureType.RobeLong};

                        break;
                    default:
                        visibleCalback = () =>
                        {
                            SetVisible(cloakMeshes, showCloak);
                            SetVisible(longRobeMeshes, false);
                            SetVisible(shortRobeMeshes, false);
                        };
                        ignoreTextureTypes = new TextureType[2] {TextureType.RobeLong, TextureType.RobeShort};
                        break;
                }
            }
        }

        private void SetVisible(SkinnedMeshRenderer[] meshes, bool visible)
        {
            if (meshes == null)
                return;

            for (int i = 0; i < meshes.Length; i++)
            {
                meshes[i].gameObject.SetActive(visible);
            }
        }

        private void TextureChangeHandler(object sender, EventArgs e)
        {
            TextureManager.Instance.OnTextureChanged -= TextureChangeHandler;
            if (visibleCalback != null)
                visibleCalback.Invoke();
            
            updateMeshCalback.Invoke();
        }
    }
}