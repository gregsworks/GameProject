using System;
using UnityEngine;

namespace CharacterEditor
{
    namespace FX
    {
        public abstract class AbstractFXMesh
        {
            private readonly GameObject parent;  //An empty object for grouping meshes
            private readonly Transform anchor;   //Root bone

            public FXType FxType { get; private set; }

            protected GameObject[] meshes;
            public int MeshesCount
            {
                get { return meshes != null ? meshes.Length : 0; }
            }

            private int _selectedMesh = -1;
            public int SelectedMesh
            {
                get { return _selectedMesh; }
                private set
                {
                    if (SelectedMesh != -1) {
                        meshes[_selectedMesh].SetActive(false);
                        meshes[_selectedMesh].transform.SetParent(parent.transform);
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


            private Action<GameObject[]> loadCallback;
            public bool IsReady { get; private set; }

            public readonly string CharacterRace;


            protected AbstractFXMesh(IMeshLoader loader, Transform anchor, string characterRace, FXType type) {
                this.anchor = anchor;
                this.CharacterRace = characterRace;
                this.FxType = type;

                parent = GameObject.Find("ModularMesh");
                if (parent == null) {
                    parent = new GameObject("ModularMesh");
                }

                loadCallback = (meshes) => LoadingMeshes(meshes);
                loader.ParseMeshes(this, loadCallback);
            }

            protected void LoadingMeshes(GameObject[] meshObjects) {
                this.meshes = meshObjects;

                for (int i = 0; i < MeshesCount; i++) {
                    this.meshes[i] = GameObject.Instantiate(meshes[i]);
                    this.meshes[i].transform.SetParent(parent.transform);
                    this.meshes[i].SetActive(false);
                }

                IsReady = true;
            }

            public abstract string GetFolderPath();

            public GameObject GetMesh() {
                return SelectedMesh == -1 ? null : meshes[SelectedMesh];
            }

            public void UpdateMesh() {
                if (SelectedMesh != -1) {
                    meshes[SelectedMesh].transform.SetParent(anchor);
                    meshes[SelectedMesh].transform.position = anchor.position;
                    meshes[SelectedMesh].transform.rotation = anchor.rotation;
                    meshes[SelectedMesh].SetActive(true);
                }
            }

            private void UnsetMesh() {
                if (SelectedMesh != -1) {
                    meshes[SelectedMesh].SetActive(false);
                    meshes[SelectedMesh].transform.SetParent(parent.transform);
                }
                SelectedMesh = -1;
            }

            public void MoveNext() {
                SelectedMesh++;
                UpdateMesh();
            }

            public void MovePrev() {
                SelectedMesh--;
                UpdateMesh();
            }

            public void Reset() {
                UnsetMesh();
            }

            public void Shuffle() {
                SelectedMesh = UnityEngine.Random.Range(-1, MeshesCount);
                UpdateMesh();
            }

            public void SetMesh(int mesh) {
                SelectedMesh = mesh;
                UpdateMesh();
            }
        }
    }
}
