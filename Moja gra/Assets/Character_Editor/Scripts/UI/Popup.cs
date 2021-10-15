using UnityEngine;

namespace CharacterEditor
{
    public class Popup : MonoBehaviour
    {
        public void Toggle()
        {
            gameObject.SetActive(!gameObject.activeSelf);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }
    }
}