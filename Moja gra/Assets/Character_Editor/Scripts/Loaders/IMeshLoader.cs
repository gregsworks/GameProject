using System;
using CharacterEditor.FX;
using CharacterEditor.Mesh;
using UnityEngine;

namespace CharacterEditor
{
    /*
     * Loading and parsing armor/weapon meshes and Fx meshes
     */
    public interface IMeshLoader
    {
        void ParseMeshes(AbstractFXMesh mesh, Action<GameObject[]> callback);
        void ParseMeshes(AbstractMesh mesh, Action<GameObject[], AbstractTexture[]> callback);
    }
}
