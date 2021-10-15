using System;
using UnityEngine;

namespace CharacterEditor
{
    public interface ITextureLoader
    {
        void LoadTexture(string[] paths, Action<Texture2D[], string[]> callback);

        void UnloadTextures();

        string[][] ParseTextures(AbstractTexture texture, string path = null);

    }
}
