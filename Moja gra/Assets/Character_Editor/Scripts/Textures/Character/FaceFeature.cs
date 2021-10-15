namespace CharacterEditor
{
    namespace Textures
    {
        public class FaceFeature : AbstractTexture
        {
            public FaceFeature(ITextureLoader loader, int x, int y, int width, int height,
                string characterRace) :
                base(loader, x, y, width, height, characterRace, 3, TextureType.FaceFeature) {
            }

            public override string GetFolderPath() {
                return FaceFeature.GetFolderPath(CharacterRace);
            }

            public static string GetFolderPath(string characterRace) {
                return "Assets/Character_Editor/Textures/Character/" + characterRace + "/Skin/FaceFeature";
            }
        }
    }
}
