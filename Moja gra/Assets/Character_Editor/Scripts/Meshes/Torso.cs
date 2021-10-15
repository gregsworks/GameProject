using UnityEngine;

namespace CharacterEditor
{
    namespace Mesh
    {
        public class Torso : AbstractMesh
        {
            public Torso(IMeshLoader loader, Transform anchor, string characterRace) : base(loader, anchor, characterRace, MeshType.Torso, GetMerheOrder()) {
            }

            public override string GetFolderPath() {
                return GetFolderPath(CharacterRace);
            }

            public static string GetFolderPath(string characterRace) {
                return "Assets/Character_Editor/Prefabs/" + characterRace + "/Torso/";
            }

            public static int GetMerheOrder()
            {
                return 2;
            }
        }
    }
}