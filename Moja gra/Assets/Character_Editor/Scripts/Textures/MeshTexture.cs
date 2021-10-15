namespace CharacterEditor
{
    /*
     * Basic texture for meshes (without texture type)
     */
    public class MeshTexture : AbstractTexture
    {
        private readonly string folderPath;

        #region EditorMeshLoader
        public MeshTexture(ITextureLoader loader, string characterRace, string path) : base(loader, characterRace, 512, 512, path) {
            folderPath = path;
        }
        #endregion

        #region BundleMeshLoader
        public MeshTexture(ITextureLoader loader, string characterRace, string[][] texturePaths) : base(loader, characterRace, 512, 512, texturePaths)
        {
            folderPath = null;
        }
        #endregion

        public override string GetFolderPath()
        {
            return folderPath;
        }
    }
}
