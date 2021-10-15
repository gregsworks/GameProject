using UnityEngine;
using UnityEngine.EventSystems;

namespace CharacterEditor
{
    public class NextCharacterBtn : MonoBehaviour, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            ConfigManager.Instance.OnNextCharacter();
        }
    }
}
