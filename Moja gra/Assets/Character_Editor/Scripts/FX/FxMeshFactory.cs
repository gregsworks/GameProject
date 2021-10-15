using CharacterEditor.FX;

namespace CharacterEditor
{
    public class FxMeshFactory
    {
        public static AbstractFXMesh Create(IMeshLoader loader, FxMeshTypeBone meshType, Config config)
        {
            var anchor = Helper.FindTransform(config.GetCharacter().transform, meshType.boneName);
            switch (meshType.mesh)
            {
                case FXType.Eye:
                    return new EyeFX(loader, anchor, config.folderName);
                case FXType.Torso:
                    return new TorsoFX(loader, anchor, config.folderName);
            }
            return null;
        }
    }
}
