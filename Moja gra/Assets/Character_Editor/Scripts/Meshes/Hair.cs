using UnityEngine;

namespace CharacterEditor
{
    namespace Mesh
    {
        public class Hair : AbstractMesh
        {
            public Hair(IMeshLoader loader, Transform anchor, string characterRace) : base(loader, anchor, characterRace, MeshType.Hair, GetMerheOrder()) {
            }

            public override string GetFolderPath() {
                return GetFolderPath(CharacterRace);
            }

            public static string GetFolderPath(string characterRace) {
                return "Assets/Character_Editor/Prefabs/" + characterRace + "/Hair/";
            }

            public static int GetMerheOrder()
            {
                return 6;
            }
        }
    }
}