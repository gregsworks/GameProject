using UnityEngine;

namespace CharacterEditor
{
    namespace Mesh
    {
        public class Beard : AbstractMesh
        {
            public Beard(IMeshLoader loader, Transform anchor, string characterRace):base(loader, anchor, characterRace, MeshType.Beard, GetMerheOrder())
            {
            }

            public override string GetFolderPath() {
                return GetFolderPath(CharacterRace);
            }

            public static string GetFolderPath(string characterRace) {
                return "Assets/Character_Editor/Prefabs/" + characterRace + "/Beard/";
            }

            public static int GetMerheOrder()
            {
                return 4;
            }
        }
    }
}