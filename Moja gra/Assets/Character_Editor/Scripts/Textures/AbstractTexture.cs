using System;
using System.Collections.Generic;
using UnityEngine;

namespace CharacterEditor
{
    public abstract class AbstractTexture
    {
        public readonly int x; //Start X position in atlas
        public readonly int y; //Start Y position in atlas
        public readonly int width;
        public readonly int height;

        public readonly string CharacterRace;
        public readonly TextureType Type;
        public readonly int MergeOrder; //Order in atlas (so that the armor is drawn on top of the skin)

        public bool IsReady { get; private set; }

        private readonly ITextureLoader textureLoader;
        protected readonly string[][] textures;

        private int _selectedColor;
        private int _selectedTexture;

        private Action<Texture2D[], string[]> loadCallback;

        private string lastLoadPath;
        public EventHandler OnTextureLoaded;

        public int SelectedTexture
        {
            get { return _selectedTexture; }
            set
            {
                if (value >= textures.Length) {
                    value = 0;
                }
                else if (value < 0) {
                    value = textures.Length - 1;
                }
                _selectedTexture = GetTextureNumber(value);
                LoadTexture();
            }
        }

        public int SelectedColor
        {
            get { return _selectedColor; }
            set
            {
                if (value >= textures[_selectedTexture].Length) {
                    value = 0;
                }
                else if (value < 0) {
                    value = textures[_selectedTexture].Length - 1;
                }
                _selectedColor = GetColorNumber(value);
                LoadTexture();
            }
        }


        public Texture2D Current
        {
            get
            {
                var path = textures[_selectedTexture][_selectedColor];
                if (!textureCache.ContainsKey(path))
                    return null;
                
                return textureCache[path];
            }
        }

        //storage of an array of pixels of the texture (memory savings)
        private Dictionary<string, Color32[]> cache = new Dictionary<string, Color32[]>();
        //storage for already loaded textures
        private Dictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>();


        protected AbstractTexture(ITextureLoader loader, string characterRace, int width, int height)
        {
            this.textureLoader = loader;
            this.CharacterRace = characterRace;
            this.height = height;
            this.width = width;

            loadCallback = (textures, paths) => LoadingTexture(textures, paths);
        }

        protected AbstractTexture(ITextureLoader loader, string characterRace, int width, int height, string[][] texturePaths) : this(loader, characterRace, width, height)
        {
            textures = texturePaths;
            LoadTexture();
        }

        protected AbstractTexture(ITextureLoader loader, string characterRace, int width, int height, string path) : this(loader, characterRace, width, height) {
            textures = loader.ParseTextures(this, path);
            LoadTexture();
        }

        protected AbstractTexture(ITextureLoader loader, int x, int y, int width, int height, string characterRace, int order, TextureType type = TextureType.Skin): this(loader, characterRace, width, height) {
            this.x = x;
            this.y = y;

            this.MergeOrder = order;
            this.Type = type;

            textures = textureLoader.ParseTextures(this);
            LoadTexture();
        }

        public abstract string GetFolderPath();

        private int GetTextureNumber(int value)
        {
            if (value >= textures.Length) {
                value = 0;
            }
            else if (value < 0) {
                value = textures.Length - 1;
            }
            return value;
        }

        private int GetColorNumber(int value)
        {
            if (value >= textures[_selectedTexture].Length) {
                value = 0;
            }
            else if (value < 0) {
                value = textures[_selectedTexture].Length - 1;
            }
            return value;
        }

        protected void LoadTexture()
        {
            IsReady = false;
            lastLoadPath = textures[_selectedTexture][_selectedColor];
            if (!textureCache.ContainsKey(lastLoadPath)) {
                textureLoader.LoadTexture(new string[] {lastLoadPath}, loadCallback);
            }
            else {
                IsReady = true;
            }

            //Loading near textures for smoother work
            var requiredPaths = new List<string>()
            {
                textures[_selectedTexture][_selectedColor],
                textures[_selectedTexture][GetColorNumber(_selectedColor + 1)],
                textures[_selectedTexture][GetColorNumber(_selectedColor - 1)],
                textures[GetTextureNumber(_selectedTexture + 1)][_selectedColor],
                textures[GetTextureNumber(_selectedTexture - 1)][_selectedColor],
            };

            var neededPaths = new List<string>();
            for (int i = 0; i < requiredPaths.Count; i++)
            {
                if (!textureCache.ContainsKey(requiredPaths[i]))
                    neededPaths.Add(requiredPaths[i]);
            }
            if (neededPaths.Count > 0)
            {
                lastLoadPath = neededPaths[0];
                textureLoader.LoadTexture(neededPaths.ToArray(), loadCallback);
            }
        }

        private void LoadingTexture(Texture2D[] textures, string[] paths)
        {
            for (int i = 0; i < textures.Length; i++)
                textureCache[paths[i]] = textures[i];
            
            if (lastLoadPath.Equals(paths[0]))
            {
                IsReady = true;
                if (OnTextureLoaded != null) OnTextureLoaded(this, null);
            }
        }

        public virtual void MoveNext() {
            SelectedTexture++;
        }

        public bool HasNext() {
            return SelectedTexture != textures.Length - 1;
        }

        public virtual void MovePrev() {
            SelectedTexture--;
        }

        public bool HasPrev() {
            return SelectedTexture != 0;
        }

        public void Reset() {
            SelectedTexture = 0;
        }

        public void Shuffle() {
            SelectedTexture = UnityEngine.Random.Range(0, textures.Length);
        }

        public void MoveNextColor() {
            SelectedColor++;
        }

        public void MovePrevColor() {
            SelectedColor--;
        }

        public void ResetColor() {
            SelectedColor = 0;
        }

        public void ShuffleColor() {
            SelectedColor = UnityEngine.Random.Range(0, textures[SelectedTexture].Length);
        }

        public Color32[] GetPixels32()
        {
            string key = textures[_selectedTexture][_selectedColor];

            if (!cache.ContainsKey(key)) {
           
                cache[key] = Current.GetPixels32();
                textureLoader.UnloadTextures();
            }

            return cache[key];
        }
    }
}