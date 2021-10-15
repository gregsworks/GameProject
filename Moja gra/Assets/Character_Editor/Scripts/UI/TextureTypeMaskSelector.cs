using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CharacterEditor
{
    public abstract class TextureTypeMaskSelector : MonoBehaviour, IPointerClickHandler
    {
        [EnumFlag] public TextureType typeMask;
        protected TextureType[] types;

        [EnumFlag] public TextureType clearTypeMask;
        protected TextureType[] clearTypes;

        private Button button;

        protected abstract void OnClick();

        public virtual void Start()
        {
            button = GetComponent<Button>();

            List<TextureType> list = new List<TextureType>();
            foreach (var enumValue in System.Enum.GetValues(typeof(TextureType)))
            {
                int checkBit = (int) typeMask & (int) enumValue;
                if (checkBit != 0)
                    list.Add((TextureType) enumValue);
            }
            types = list.ToArray();

            list.Clear();
            foreach (var enumValue in System.Enum.GetValues(typeof(TextureType)))
            {
                int checkBit = (int) clearTypeMask & (int) enumValue;
                if (checkBit != 0)
                    list.Add((TextureType) enumValue);
            }
            clearTypes = list.ToArray();


            ConfigManager.Instance.OnChangeCharacter += DisableActionBtns;
        }

        private void DisableActionBtns(object sender, EventArgs e)
        {
            var interactable = false;
            foreach (var type in types)
            {
                if (Array.IndexOf(ConfigManager.Instance.Config.availableTextures, type) != -1)
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