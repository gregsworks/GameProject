using UnityEngine;
using UnityEngine.EventSystems;

namespace CharacterEditor
{
    public class TogglePopup : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField]
        private Popup popup;

        private void Start()
        {
            if (LoaderManager.Instance.Type != LoaderType.AssetBundle)
                transform.parent.parent.gameObject.SetActive(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            popup.Toggle();
        }
    }
}