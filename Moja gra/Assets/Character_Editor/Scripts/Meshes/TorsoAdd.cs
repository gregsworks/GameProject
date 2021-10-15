using UnityEngine;

namespace CharacterEditor
{
    namespace Mesh
    {
        public class TorsoAdd : AbstractMesh
        {
            public TorsoAdd(IMeshLoader loader, Transform anchor, string characterRace) : base(loader, anchor, characterRace, MeshType.TorsoAdd, GetMerheOrder()) {
            }

            public override string GetFolderPath() {
                return GetFolderPath(CharacterRace);
            }

            public static string GetFolderPath(string characterRace) {
                return "Assets/Character_Editor/Prefabs/" + characterRace + "/TorsoAdd/";
            }

            public static int GetMerheOrder() {
                return 3;
            }
        }
    }
}