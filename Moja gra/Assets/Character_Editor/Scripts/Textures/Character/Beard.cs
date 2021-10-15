using System;

namespace CharacterEditor
{
    namespace Textures
    {
        public class Beard : AbstractTexture
        {
            public Beard(ITextureLoader loader, int x, int y, int width, int height,
                string characterRace) :
                base(loader, x, y, width, height, characterRace, 3, TextureType.Beard) {
            }

            public override string GetFolderPath() {
                return Beard.GetFolderPath(CharacterRace);
            }

            public static string GetFolderPath(string characterRace) {
                return "Assets/Character_Editor/Textures/Character/" + characterRace + "/Skin/Beard";
            }
        }
    }
}
