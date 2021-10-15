namespace CharacterEditor
{
    namespace Textures
    {
        public class Eyebrow : AbstractTexture
        {
            public Eyebrow(ITextureLoader loader, int x, int y, int width, int height,
                string characterRace) :
                base(loader, x, y, width, height, characterRace, 1, TextureType.Eyebrow) {
            }

            public override string GetFolderPath() {
                return Eyebrow.GetFolderPath(CharacterRace);
            }

            public static string GetFolderPath(string characterRace) {
                return "Assets/Character_Editor/Textures/Character/" + characterRace + "/Skin/Eyebrow";
            }

        }
    }
}
