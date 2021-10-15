namespace CharacterEditor
{
    namespace Textures
    {
        public class Hair : AbstractTexture
        {
            public Hair(ITextureLoader loader, int x, int y, int width, int height,
                string characterRace) :
                base(loader, x, y, width, height, characterRace, 4, TextureType.Hair) {
            }

            public override string GetFolderPath() {
                return GetFolderPath(CharacterRace);
            }

            public static string GetFolderPath(string characterRace) {
                return "Assets/Character_Editor/Textures/Character/" + characterRace + "/Skin/Hair";
            }
        }
    }
}
