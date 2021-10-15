using UnityEngine;

namespace CharacterEditor
{
    namespace Mesh
    {
        public class Leg : AbstractMesh
        {
            public Leg(IMeshLoader loader, Transform anchor, string characterRace, MeshType type) : base(loader, anchor, characterRace, type, GetMerheOrder(type)) {
            }

            public override string GetFolderPath() {
                return GetFolderPath(CharacterRace, MeshType);
            }

            public static string GetFolderPath(string characterRace, MeshType type) {
                return "Assets/Character_Editor/Prefabs/" + characterRace + "/Leg " + (type == MeshType.LegLeft ? 'L' : 'R') + "/";
            }

            public static int GetMerheOrder(MeshType type)
            {
                return type == MeshType.LegLeft ? 12 : 13;
            }
        }
    }
}