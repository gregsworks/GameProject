using UnityEngine;

namespace CharacterEditor
{
    namespace Mesh
    {
        public class Shoulder : AbstractMesh
        {
            public Shoulder(IMeshLoader loader, Transform anchor, string characterRace, MeshType meshType) : base(loader, anchor, characterRace, meshType, GetMerheOrder(meshType)) {
            }

            public override string GetFolderPath() {
                return GetFolderPath(CharacterRace, MeshType);
            }

            public static string GetFolderPath(string characterRace, MeshType type) {
                return "Assets/Character_Editor/Prefabs/" + characterRace + "/Shoulder " + (type == MeshType.ShoulderLeft ? 'L' : 'R') + "/";
            }

            public static int GetMerheOrder(MeshType type) {
                return type == MeshType.ShoulderLeft ? 14 : 15;
            }
        }
    }
}