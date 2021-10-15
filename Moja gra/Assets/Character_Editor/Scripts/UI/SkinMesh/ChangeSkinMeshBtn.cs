using System;
using System.Collections.Generic;
using UnityEngine;

namespace CharacterEditor
{
    public abstract class ChangeSkinMeshBtn : TextureTypeMaskSelector
    {
        [SerializeField]
        private SkinMeshType[] activeMeshes;
        [SerializeField]
        private SkinMeshType[] disableMeshes;
      

        protected override void OnClick()
        {
            ChangeMesh();
        }

        protected virtual void ChangeMesh()
        {
            TextureManager.Instance.OnTextureChanged += TextureChangeHandler;
            ChangeSkinTexture();
        }

        protected abstract void ChangeSkinTexture();

        protected void TextureChangeHandler(object sender, EventArgs e)
        {
            TextureManager.Instance.OnTextureChanged -= TextureChangeHandler;

            foreach (SkinMeshType mesh in disableMeshes)
                ActiveMeshes(mesh, false);

            foreach (SkinMeshType mesh in activeMeshes)
                ActiveMeshes(mesh, true);
        }

        private void ActiveMeshes(SkinMeshType type, bool active)
        {
            var config = ConfigManager.Instance.Config;
            var meshes = new List<SkinnedMeshRenderer>();
            switch (type)
            {
                case SkinMeshType.RobeLong:
                    meshes.AddRange(config.GetLongRobeMeshes());
                    break;
                case SkinMeshType.RobeShort:
                    meshes.AddRange(config.GetShortRobeMeshes());
                    break;
                case SkinMeshType.Cloak:
                    meshes.AddRange(config.GetCloakMesshes());
                    break;
            }
            
            foreach (SkinnedMeshRenderer mesh in meshes)
                mesh.gameObject.SetActive(active);
        }
    }
}
