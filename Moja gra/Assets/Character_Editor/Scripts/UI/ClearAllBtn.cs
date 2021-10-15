using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CharacterEditor
{
    /*
     * Clears selected textures and meshes.
     */
    public class ClearAllBtn : MonoBehaviour, IPointerClickHandler
    {
        [EnumFlag] public SkinMeshType skinedMeshesMask;

        [EnumFlag] public TextureType textureMask;
        [EnumFlag] public FXType fxMask;
        [EnumFlag] public MeshType meshMask;

        private TextureType[] textures;
        private FXType[] fxMeshes;
        private MeshType[] meshes;
        private SkinnedMeshRenderer[] skinMeshes;

        public void Start()
        {
            PrepareTextureTypes();
            PrepareMeshTypes();
            PrepareFXTypes();

            ConfigManager.Instance.OnChangeCharacter += PrepareSkinMeshTypes;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (textures.Length > 0)
            {
                TextureManager.Instance.OnClear(textures);
                TextureManager.Instance.OnTextureChanged += ResetMeshes;
            }
            else
            {
                ResetMeshes();
            }
        }

        private void ResetMeshes(object sender, EventArgs e)
        {
            TextureManager.Instance.OnTextureChanged -= ResetMeshes;
            ResetMeshes();
        }

        private void ResetMeshes()
        {
            MeshManager.Instance.OnClearMesh(meshes);
            MeshManager.Instance.OnClearFX(fxMeshes);
            for (int i = 0; i < skinMeshes.Length; i++)
            {
                skinMeshes[i].gameObject.SetActive(false);
            }
        }

        protected void PrepareTextureTypes()
        {
            List<TextureType> list = new List<TextureType>();
            foreach (var enumValue in Enum.GetValues(typeof(TextureType)))
            {
                int checkBit = (int) textureMask & (int) enumValue;
                if (checkBit != 0)
                    list.Add((TextureType) enumValue);
            }
            textures = list.ToArray();
        }

        protected void PrepareFXTypes()
        {
            List<FXType> list = new List<FXType>();
            foreach (var enumValue in Enum.GetValues(typeof(FXType)))
            {
                int checkBit = (int) fxMask & (int) enumValue;
                if (checkBit != 0)
                    list.Add((FXType) enumValue);
            }
            fxMeshes = list.ToArray();
        }
        
        protected void PrepareMeshTypes()
        {
            List<MeshType> list = new List<MeshType>();
            foreach (var enumValue in Enum.GetValues(typeof(MeshType)))
            {
                int checkBit = (int) meshMask & (int) enumValue;
                if (checkBit != 0)
                    list.Add((MeshType) enumValue);
            }
            meshes = list.ToArray();
        }

        protected void PrepareSkinMeshTypes(object sender, EventArgs e)
        {
            var config = ConfigManager.Instance.Config;

            List<SkinnedMeshRenderer> list = new List<SkinnedMeshRenderer>();
            foreach (var enumValue in Enum.GetValues(typeof(SkinMeshType)))
            {
                int checkBit = (int) skinedMeshesMask & (int) enumValue;
                if (checkBit != 0)
                {
                    switch ((SkinMeshType) enumValue)
                    {
                        case SkinMeshType.RobeLong:
                            list.AddRange(config.GetLongRobeMeshes());
                            break;
                        case SkinMeshType.RobeShort:
                            list.AddRange(config.GetShortRobeMeshes());
                            break;
                        case SkinMeshType.Cloak:
                            list.AddRange(config.GetCloakMesshes());
                            break;
                    }
                }
            }
            skinMeshes = list.ToArray();
        }
    }
}