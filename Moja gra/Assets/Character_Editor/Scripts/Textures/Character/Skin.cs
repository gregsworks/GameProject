namespace CharacterEditor
{
    namespace Textures
    {
        public class Skin : AbstractTexture
        {
            public Skin(ITextureLoader loader, int x, int y, int width, int height, 
                string characterRace) :
                base(loader, x, y, width, height, characterRace, 0, TextureType.Skin)
            {
            }

            public override string GetFolderPath()
            {
                return Skin.GetFolderPath(CharacterRace);
            }

            public static string GetFolderPath(string characterRace)
            {
                return "Assets/Character_Editor/Textures/Character/" + characterRace + "/Skin/Skin";
            }
        }
    }
}

