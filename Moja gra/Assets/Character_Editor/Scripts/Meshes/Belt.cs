using UnityEngine;

namespace CharacterEditor
{
    namespace Mesh
    {
        public class Belt : AbstractMesh
        {
            public Belt(IMeshLoader loader, Transform anchor, string characterRace) : base(loader, anchor, characterRace, MeshType.Belt, GetMerheOrder()) {
            }

            public override string GetFolderPath() {
                return GetFolderPath(CharacterRace);
            }

            public static string GetFolderPath(string characterRace) {
                return "Assets/Character_Editor/Prefabs/" + characterRace + "/Belt/";
            }

            public static int GetMerheOrder() {
                return 10;
            }
        }
    }
}