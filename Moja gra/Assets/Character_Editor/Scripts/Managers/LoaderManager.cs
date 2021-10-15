using System.Collections;
using System.IO;
using AssetBundles;
using UnityEngine;
using UnityEngine.UI;

namespace CharacterEditor
{
    class LoaderFactory
    {
        public static IMeshLoader CreateMeshLoader(LoaderType type, MeshAtlasType atlasType) {
            switch (type)
            {
#if UNITY_EDITOR
                case LoaderType.AssetDatabase:
                    return new AssetDatabaseLoader.MeshLoader(atlasType);
#endif
                default:
                    return new AssetBundleLoader.MeshLoader();
            }
        }

        public static ITextureLoader CreateTextureLoader(LoaderType type) {
            switch (type)
            {
#if UNITY_EDITOR
                case LoaderType.AssetDatabase:
                    return new AssetDatabaseLoader.TextureLoader();
#endif
                default:
                    return new AssetBundleLoader.TextureLoader();
            }
        }

        public static IConfigLoader CreateConfigLoader(LoaderType type)
        {
            switch (type)
            {
#if UNITY_EDITOR
                case LoaderType.AssetDatabase:
                    return new AssetDatabaseLoader.ConfigLoader();
#endif
                default:
                    return new AssetBundleLoader.ConfigLoader();
            }
        }
    }
    

    public class LoaderManager : MonoBehaviour
    {
        [SerializeField]
        private LoaderType _type;
        public LoaderType Type
        {
            get { return _type; }
            private set { _type = value; }
        }

        [SerializeField]
        private MeshAtlasType _meshAtlasType;
        public MeshAtlasType MeshAtlasType {
            get { return _meshAtlasType; }
            private set { _meshAtlasType = value; }
        }

        [SerializeField]
        private Slider loadingSlider;
        [SerializeField]
        private GameObject loadingPanel;

        public bool IsReady { get; private set; }


        private IMeshLoader _meshLoader;
        public IMeshLoader MeshLoader
        {
            get
            {
                if (_meshLoader == null) {
                    _meshLoader = LoaderFactory.CreateMeshLoader(Type, MeshAtlasType);
                }
                return _meshLoader;
            }
        }

        private ITextureLoader _textureLoader;
        public ITextureLoader TextureLoader
        {
            get
            {
                if (_textureLoader == null) {
                    _textureLoader = LoaderFactory.CreateTextureLoader(Type);
                }
                return _textureLoader;
            }
        }

        private IConfigLoader _configLoader;
        public IConfigLoader ConfigLoader {
            get
            {
                if (_configLoader == null) {
                    _configLoader = LoaderFactory.CreateConfigLoader(Type);
                }
                return _configLoader;
            }
        }

        public static LoaderManager Instance { get; private set; }

        void Awake()
        {
            if (Instance != null) 
                Destroy(this.gameObject);
            
            Instance = this;
            IsReady = false;
        }

        void Start()
        {
            if (Type == LoaderType.AssetBundle) {
                StartCoroutine(InitializeAssetBundles());
                StartCoroutine(LoadingCoroutine());
            }
            else
            {
                IsReady = true;
                StartCoroutine(LoadingCoroutine());
            }
        }

     
        IEnumerator LoadingCoroutine()
        {
            SetLoading(0);
            while (!IsReady)
                yield return null;
            SetLoading(10);

            while (!TextureManager.Instance.IsReady)
                yield return null;
            SetLoading(30);

            while (!MeshManager.Instance.IsReady)
                yield return null;
            SetLoading(70);

            while (!ConfigManager.Instance.IsReady)
                yield return null;
            SetLoading(100);
        }

        private void SetLoading(float percentage)
        {
            if (loadingPanel == null)
                return;

            if (percentage >= 100)
            {
                loadingPanel.SetActive(false);
                return;
            }

            if (percentage < 100 && !loadingPanel.activeSelf)
                loadingPanel.SetActive(true);

            loadingSlider.value = percentage / 100f;
        }

        IEnumerator InitializeAssetBundles() {
            yield return StartCoroutine(Initialize());
            
            // Set active variants.
            AssetBundleManager.ActiveVariants = null;
        }

        // Initialize the downloading url and AssetBundleManifest object.
        protected IEnumerator Initialize() {
            // Don't destroy this gameObject as we depend on it to run the loading script.
            DontDestroyOnLoad(gameObject);

            //TODO ONLY LOCAL LOAD BANDLES
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            AssetBundleManager.SetSourceAssetBundleURL(Path.Combine(Application.streamingAssetsPath, Utility.AssetBundlesOutputPath) + "/");
            //AssetBundleManager.SetDevelopmentAssetBundleServer();
#else
            AssetBundleManager.SetSourceAssetBundleURL(Path.Combine(Application.streamingAssetsPath, Utility.AssetBundlesOutputPath) + "/");
         
            // Or customize the URL based on your deployment or configuration
            //AssetBundleManager.SetSourceAssetBundleURL("http://www.MyWebsite/MyAssetBundles");
#endif

            // Initialize AssetBundleManifest which loads the AssetBundleManifest object.
            var request = AssetBundleManager.Initialize();

            if (request != null)
                yield return StartCoroutine(request);

            IsReady = true;
        }
    }
}