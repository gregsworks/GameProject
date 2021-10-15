using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CharacterEditor
{
    public abstract class MeshTypeMaskSelector : MonoBehaviour, IPointerClickHandler
    {
        [EnumFlag]
        public MeshType typeMask;
        protected MeshType[] types;

        private Button button;

        protected abstract void OnClick();

        public void Start()
        {
            button = GetComponent<Button>();

            List<MeshType> list = new List<MeshType>();
            foreach (var enumValue in System.Enum.GetValues(typeof(MeshType))) {
                int checkBit = (int)typeMask & (int)enumValue;
                if (checkBit != 0)
                    list.Add((MeshType)enumValue);
            }
            types = list.ToArray();

            ConfigManager.Instance.OnChangeCharacter += DisableActionBtns;
        }

        private void DisableActionBtns(object sender, EventArgs e)
        {
            var interactable = false;
            foreach (MeshTypeBone bone in ConfigManager.Instance.Config.availableMeshes)
                if (Array.IndexOf(types, bone.mesh) != -1)
                {
                    interactable = true;
                    break;
                }

            button.interactable = interactable;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!button.interactable)
                return;

            OnClick();
        }
    }
}