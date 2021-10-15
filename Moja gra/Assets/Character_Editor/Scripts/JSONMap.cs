using System;
using System.Collections.Generic;

namespace CharacterEditor
{
    namespace JSONMap
    {
        [Serializable]
        public class BundleMap
        {
            public List<RaceMap> races = new List<RaceMap>();
        }

        [Serializable]
        public class RaceMap
        {
            public List<BundleTextures> textures = new List<BundleTextures>();
            public List<BundleMeshes> meshes = new List<BundleMeshes>();
            public List<BundleFxMeshes> fxs = new List<BundleFxMeshes>();
            public string race;
            public string configPath;
            public string prefabPath;
        }

        #region Mesh

        [Serializable]
        public class BundleMeshes
        {
            public List<BundleMesh> meshPaths = new List<BundleMesh>();
            public MeshType type;
        }

        [Serializable]
        public class BundleFxMeshes
        {
            public List<BundleColor> meshPaths = new List<BundleColor>();
            public FXType type;
        }

        [Serializable]
        public class BundleMesh
        {
            public List<BundleTexture> textures = new List<BundleTexture>();
            public string modelPath;
        }

        #endregion

        #region Texture

        [Serializable]
        public class BundleTextures
        {
            public List<BundleTexture> texturePaths = new List<BundleTexture>();
            public TextureType type;
        }

        [Serializable]
        public class BundleTexture
        {
            public List<BundleColor> colors = new List<BundleColor>();
        }

        [Serializable]
        public class BundleColor
        {
            public string path;
        }
        #endregion
    }
}