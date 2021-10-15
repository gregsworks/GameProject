namespace CharacterEditor
{
    namespace Textures
    {
        public class Scar : AbstractTexture
        {
            public Scar(ITextureLoader loader, int x, int y, int width, int height,
                string characterRace) :
                base(loader, x, y, width, height, characterRace, 2, TextureType.Scar) {
            }

            public override string GetFolderPath() {
                return Scar.GetFolderPath(CharacterRace);
            }

            public static string GetFolderPath(string characterRace) {
                return "Assets/Character_Editor/Textures/Character/" + characterRace + "/Skin/Scars";
            }
        }
    }
}
