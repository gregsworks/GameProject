using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

/*
 * Save character data in runtime (feature work)
 */
namespace CharacterEditor
{
    [Serializable]
    public class MeshInfo
    {
        public int selectedMesh;
        public int selectedTexture;
    }

    [Serializable]
    public class TextureInfo
    {
        public int selectedTexture;
        public int selectedColor;
    }

    [Serializable]
    public class SaveData: ISerializable
    {
        public Dictionary<MeshType, MeshInfo> meshes;
        public Dictionary<FXType, MeshInfo> fxMeshes;
        public Dictionary<TextureType, TextureInfo> textures;

        public bool equipLongRobe;
        public bool equipShortRobe;

        public int selectedCloak;

        public SaveData()
        {
            
        }

        public SaveData(SerializationInfo info, StreamingContext context) {
            meshes = (Dictionary<MeshType, MeshInfo>)info.GetValue("meshes", typeof(Dictionary<MeshType, MeshInfo>));
            fxMeshes = (Dictionary<FXType, MeshInfo>)info.GetValue("fxMeshes", typeof(Dictionary<FXType, MeshInfo>));
            textures = (Dictionary<TextureType, TextureInfo>)info.GetValue("textures", typeof(Dictionary<TextureType, TextureInfo>));
            equipLongRobe = info.GetBoolean("equipLongRobe");
            equipShortRobe = info.GetBoolean("equipShortRobe");
            selectedCloak = info.GetInt32("selectedCloak");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("meshes", meshes);
            info.AddValue("fxMeshes", fxMeshes);
            info.AddValue("textures", textures);
            info.AddValue("equipLongRobe", equipLongRobe);
            info.AddValue("equipShortRobe", equipShortRobe);
            info.AddValue("selectedCloak", selectedCloak);
        }

        public void InitData()
        {
            meshes = new Dictionary<MeshType, MeshInfo>();
            fxMeshes = new Dictionary<FXType, MeshInfo>();
            textures = new Dictionary<TextureType, TextureInfo>();

            foreach (var mesh in MeshManager.Instance.GetMeshes())
            {
                var info = new MeshInfo();
                info.selectedMesh = mesh.SelectedMesh;
                info.selectedTexture = mesh.Texture.SelectedTexture;
                meshes.Add(mesh.MeshType, info);
            }

            foreach (var mesh in MeshManager.Instance.GetFxMeshes())
            {
                var info = new MeshInfo();
                info.selectedMesh = mesh.SelectedMesh;
                fxMeshes.Add(mesh.FxType, info);
            }

            foreach (var texture in TextureManager.Instance.currentCharacterTextures) {
                var info = new TextureInfo();
                info.selectedColor = texture.Value.SelectedColor;
                info.selectedTexture = texture.Value.SelectedTexture;
                textures.Add(texture.Key, info);
            }

            equipLongRobe = TextureManager.Instance.currentCharacterTextures[TextureType.RobeLong].SelectedTexture != 0;
            equipShortRobe = TextureManager.Instance.currentCharacterTextures[TextureType.RobeShort].SelectedTexture != 0;
            selectedCloak = TextureManager.Instance.currentCharacterTextures[TextureType.Cloak].SelectedTexture;
        }
    }
}
