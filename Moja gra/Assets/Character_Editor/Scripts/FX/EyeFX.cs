using UnityEngine;

namespace CharacterEditor
{
    namespace FX
    {
        public class EyeFX : AbstractFXMesh
        {
            public EyeFX(IMeshLoader loader, Transform anchor, string characterRace) : base(loader, anchor, characterRace, FXType.Eye) {
            }

            public override string GetFolderPath() {
                return GetFolderPath(CharacterRace);
            }

            public static string GetFolderPath(string characterRace) {
                return "Assets/Character_Editor/Prefabs/" + characterRace + "/Eye FX";
            }
        }
    }
}