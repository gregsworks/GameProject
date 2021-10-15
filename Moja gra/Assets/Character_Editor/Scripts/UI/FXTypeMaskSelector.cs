using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CharacterEditor
{
    public abstract class FXTypeMaskSelector : MonoBehaviour, IPointerClickHandler
    {
        [EnumFlag]
        public FXType typeMask;
        protected FXType[] types;

        private Button button;

        protected abstract void OnClick();

        public void Start()
        {
            button = GetComponent<Button>();

            List<FXType> list = new List<FXType>();
            foreach (var enumValue in System.Enum.GetValues(typeof(FXType))) {
                int checkBit = (int)typeMask & (int)enumValue;
                if (checkBit != 0)
                    list.Add((FXType)enumValue);
            }
            types = list.ToArray();

            ConfigManager.Instance.OnChangeCharacter += DisableActionBtns;
        }

        private void DisableActionBtns(object sender, EventArgs e)
        {
            var interactable = false;
            foreach (FxMeshTypeBone bone in ConfigManager.Instance.Config.availableFxMeshes)
            { 
                if (Array.IndexOf(types, bone.mesh) != -1)
                {
                    interactable = true;
                    break;
                }
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