using System;
using UnityEngine;

namespace CharacterEditor
{
    namespace Mesh
    {
        public abstract class AbstractMesh
        {
            private readonly GameObject parent;  //An empty object for grouping meshes
            private readonly Transform anchor;   //Root bone

            public readonly int MergeOrder;      //Order in static atlas

            private AbstractTexture[] _textures;
            public AbstractTexture Texture
            {
                get { return _textures[SelectedMesh != -1 ? SelectedMesh : 0]; }
            }

            public MeshType MeshType { get; private set; }
            public GameObject[] Meshes { get; private set; }

            public int MeshesCount
            {
                get { return Meshes != null ? Meshes.Length : 0; }
            }

            private int _selectedMesh = -1;
            public int SelectedMesh
            {
                get { return _selectedMesh; }
                set
                {
                    if (_selectedMesh != -1) {
                        Meshes[_selectedMesh].SetActive(false);
                        Meshes[_selectedMesh].transform.SetParent(parent.transform);
                    }

                    if (value >= MeshesCount) {
                        value = -1;
                    }
                    if (value < -1) {
                        value = MeshesCount - 1;
                    }
                    _selectedMesh = value;
                }
            }

            private Action<GameObject[], AbstractTexture[]> loadCallback;
            private bool _isReady;
            public bool IsReady {
                get { return _isReady && Texture.IsReady; }
                private set { _isReady = value; }
            }

            public readonly string CharacterRace;

            protected AbstractMesh(IMeshLoader loader, Transform anchor, string characterRace, MeshType type, int order)
            {
                this.anchor = anchor;
                this.CharacterRace = characterRace;
                this.MeshType = type;
                this.MergeOrder = order;

                parent = GameObject.Find("ModularMesh");
                if (parent == null) parent = new GameObject("ModularMesh");

                IsReady = false;
                loadCallback = (meshes, texures) => LoadingMeshes(meshes, texures);
              
                loader.ParseMeshes(this, loadCallback);
            }

            public abstract string GetFolderPath();

            private void LoadingMeshes(GameObject[] meshObjects, AbstractTexture[] textures)
            {
                this._textures = textures;
                this.Meshes = meshObjects;

                for (int i = 0; i < MeshesCount; i++) {
                    this.Meshes[i] = GameObject.Instantiate(Meshes[i]);
                    this.Meshes[i].transform.SetParent(parent.transform);
                    this.Meshes[i].SetActive(false);
                }

                IsReady = true;
            }

            public GameObject GetMesh() {
                return SelectedMesh == -1 ? null : Meshes[SelectedMesh];
            }

            private void UpdateMesh()
            {
                if (SelectedMesh != -1) {
                    Meshes[SelectedMesh].transform.SetParent(anchor);
                    Meshes[SelectedMesh].transform.position = anchor.position;
                    Meshes[SelectedMesh].transform.rotation = anchor.rotation;

                    foreach (MeshRenderer render in Meshes[SelectedMesh].GetComponentsInChildren<MeshRenderer>()) {
                        render.material.mainTexture = Texture.Current;
                    }
                    Meshes[SelectedMesh].SetActive(true);
                }
            }

            private void UnsetMesh()
            {
                if (SelectedMesh != -1) {
                    Meshes[SelectedMesh].SetActive(false);
                    Meshes[SelectedMesh].transform.SetParent(parent.transform);
                }
                SelectedMesh = -1;
            }

            public void MoveNext()
            {
                if (SelectedMesh != -1 && Texture.HasNext()) {
                    Texture.MoveNext();
                }
                else {
                    SelectedMesh++;
                    //Show first texture for mesh
                    Texture.Reset();
                }
                UpdateTextureListeners();
            }

            public void MovePrev()
            {
                if (SelectedMesh != -1 && Texture.HasPrev()) {
                    Texture.MovePrev();
                }
                else {
                    SelectedMesh--;
                    //Show last texture for mesh
                    Texture.Reset();
                    Texture.MovePrev();
                }
                UpdateTextureListeners();
            }

            public void Reset()
            {
                UnsetMesh();
            }

            public void Shuffle()
            {
                SelectedMesh = UnityEngine.Random.Range(-1, MeshesCount);
                Texture.Shuffle();
                UpdateTextureListeners();
            }

            public void SetMesh(int mesh)
            {
                SelectedMesh = mesh;
                UpdateTextureListeners();
            }

            public void SetTexture(int texture)
            {
                Texture.SelectedTexture = texture;
                UpdateTextureListeners();
            }

            public void MoveNextColor()
            {
                Texture.MoveNextColor();
                UpdateTexturesColor();
                UpdateTextureListeners();
            }

            public void MovePrevColor()
            {
                Texture.MovePrevColor();
                UpdateTexturesColor();
                UpdateTextureListeners();
            }

            public void SetColor(int color)
            {
                Texture.SelectedColor = color;
                UpdateTexturesColor();
                UpdateTextureListeners();
            }

            public void ResetColor()
            {
                Texture.ResetColor();
                UpdateTexturesColor();
                UpdateTextureListeners();
            }

            private void OnTextureChanged(object sender, EventArgs e)
            {
                Texture.OnTextureLoaded -= OnTextureChanged;
                UpdateMesh();
            }

            /*
             * Updating the mesh after loading the texture
             */
            private void UpdateTextureListeners()
            {
                if (Texture.IsReady)
                {
                    UpdateMesh();
                    return;
                }

                Texture.OnTextureLoaded -= OnTextureChanged;
                Texture.OnTextureLoaded += OnTextureChanged;
            }

            /*
             * Updates the color of all textures associated with the mesh. To the next switch of the texture, there was a correct color (for example, hair)
             */
            private void UpdateTexturesColor()
            {
                foreach (AbstractTexture texture in _textures)
                    texture.SelectedColor = Texture.SelectedColor;
            }
        }
    }
}
