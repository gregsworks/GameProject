using UnityEngine;

namespace CharacterEditor
{
    namespace Mesh
    {
        public class Arm : AbstractMesh
        {
            public Arm(IMeshLoader loader, Transform anchor, string characterRace, MeshType type) : base(loader, anchor, characterRace, type, GetMerheOrder(type)) {
            }

            public override string GetFolderPath() {
                return GetFolderPath(CharacterRace, MeshType);
            }

            public static string GetFolderPath(string characterRace, MeshType type) {
                return "Assets/Character_Editor/Prefabs/" + characterRace + "/Arm " + (type == MeshType.ArmLeft ? 'L' : 'R') + "/";
            }

            public static int GetMerheOrder(MeshType type)
            {
                return type == MeshType.ArmLeft ? 8 : 9;
            }
        }
    }
}