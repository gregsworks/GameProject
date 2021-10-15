using UnityEngine;

namespace CharacterEditor
{
    namespace FX
    {
        public class TorsoFX : AbstractFXMesh
        {
            public TorsoFX(IMeshLoader loader, Transform anchor, string characterRace) : base(loader, anchor, characterRace, FXType.Torso) {
            }

            public override string GetFolderPath() {
                return GetFolderPath(CharacterRace);
            }

            public static string GetFolderPath(string characterRace) {
                return "Assets/Character_Editor/Prefabs/" + characterRace + "/Torso FX";
            }
        }
    }
}