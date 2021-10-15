using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace CharacterEditor
{
    public class SaveItem : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField]
        private Text text;

        public string Text
        {
            get { return text.text; }
            set { text.text = value; }
        }

        public EventHandler ClickHandler;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (ClickHandler != null)
            {
                ClickHandler(this, null);
            }
        }
    }
}