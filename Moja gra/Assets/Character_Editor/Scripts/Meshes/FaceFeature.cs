using UnityEngine;

namespace CharacterEditor
{
    namespace Mesh
    {
        public class FaceFeature : AbstractMesh
        {
            public FaceFeature(IMeshLoader loader, Transform anchor, string characterRace):base(loader, anchor, characterRace, MeshType.FaceFeature, GetMerheOrder())
            {
            }

            public override string GetFolderPath() {
                return GetFolderPath(CharacterRace);
            }

            public static string GetFolderPath(string characterRace) {
                return "Assets/Character_Editor/Prefabs/" + characterRace + "/FaceFeature/";
            }

            public static int GetMerheOrder()
            {
                return 5;
            }
        }
    }
}