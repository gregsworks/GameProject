using UnityEngine;

namespace CharacterEditor
{
    namespace Mesh
    {
        public class Helm : AbstractMesh
        {
            public Helm(IMeshLoader loader, Transform anchor, string characterRace) : base(loader, anchor, characterRace, MeshType.Helm, GetMerheOrder()) {
            }

            public override string GetFolderPath() {
                return GetFolderPath(CharacterRace);
            }

            public static string GetFolderPath(string characterRace) {
                return "Assets/Character_Editor/Prefabs/" + characterRace + "/Head/";
            }

            public static int GetMerheOrder() {
                return 7;
            }
        }
    }
}