using UnityEngine;
using UnityEngine.EventSystems;

namespace CharacterEditor
{
    public class PrevCharacterBtn : MonoBehaviour, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            ConfigManager.Instance.OnPrevCharacter();
        }
    }
}

