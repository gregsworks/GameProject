using UnityEngine;

namespace CharacterEditor
{
    namespace Mesh
    {
        public class BeltAdd : AbstractMesh
        {
            public BeltAdd(IMeshLoader loader, Transform anchor, string characterRace) : base(loader, anchor, characterRace, MeshType.BeltAdd, GetMerheOrder()) {
            }

            public override string GetFolderPath() {
                return GetFolderPath(CharacterRace);
            }

            public static string GetFolderPath(string characterRace) {
                return "Assets/Character_Editor/Prefabs/" + characterRace + "/BeltAdd/";
            }

            public static int GetMerheOrder()
            {
                return 11;
            }
        }
    }
}