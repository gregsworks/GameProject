using System;
using System.Collections;
using UnityEngine;

namespace CharacterEditor
{
    /*
     * Manages character switching
     */
    public class ConfigManager : MonoBehaviour
    {
        public EventHandler OnChangeCharacter;
        private Coroutine currentCoroutine;

        public Config Config
        {
            get { return _configs[CurrentConfigIndex]; }
        }

        private int _currentConfigIndex;
        private int CurrentConfigIndex
        {
            get { return _currentConfigIndex; }
            set
            {
                if (value < 0)
                {
                    value = _configs.Length - 1;
                }
                if (value >= _configs.Length)
                {
                    value = 0;
                }
                _currentConfigIndex = value;
            }
        }

        private Config[] _configs;

        public bool IsReady { get; private set; }
        public static ConfigManager Instance { get; private set; }

        void Awake()
        {
            if (Instance != null)
                Destroy(this.gameObject);
            
            Instance = this;
            IsReady = false;
        }

        private IEnumerator Start()
        {
            if (!LoaderManager.Instance.IsReady)
                yield return null;

            LoaderManager.Instance.ConfigLoader.LoadConfigs((configs) => StartCoroutine(LoadingConfigs(configs)));
        }

        private IEnumerator LoadingConfigs(Config[] configs)
        {
            for (int i = 0; i < configs.Length; i++)
            {
                var character = GameObject.Instantiate(configs[i].prefab);
                character.SetActive(false);
                configs[i].SetCharacter(character);
            }

            _configs = configs;
            CurrentConfigIndex = 0;
            for (int i = 0; i < _configs.Length; i++)
            {
                Config.GetCharacter().SetActive(false);
                CurrentConfigIndex++;
                yield return StartCoroutine(ChangeCharacter());
            }
            IsReady = true;
        }

        public void OnNextCharacter()
        {
            if (_configs.Length == 1 && Config.GetCharacter().activeSelf)
                return;

            Config.GetCharacter().SetActive(false);
            CurrentConfigIndex++;

            currentCoroutine = StartCoroutine(ChangeCharacter());
        }

        public void OnPrevCharacter()
        {
            if (_configs.Length == 1 && Config.GetCharacter().activeSelf)
                return;

            Config.GetCharacter().SetActive(false);
            CurrentConfigIndex--;

            currentCoroutine = StartCoroutine(ChangeCharacter());
        }

        private IEnumerator ChangeCharacter()
        {
            if (currentCoroutine != null)
                StopCoroutine(currentCoroutine);

            yield return StartCoroutine(TextureManager.Instance.ApplyConfig(Config));
            yield return StartCoroutine(MeshManager.Instance.ApplyConfig(Config));
            while (!TextureManager.Instance.IsReady || !MeshManager.Instance.IsReady)
                yield return null;

            if (OnChangeCharacter != null) OnChangeCharacter(this, null);

            Config.GetCharacter().SetActive(true);
            currentCoroutine = null;
        }
    }
}
