using System;
using UnityEngine;

namespace CharacterEditor
{
    public class FollowCamera : MonoBehaviour
    {
        public Vector3 offset;
        private Transform target;

        void Start()
        {
            ConfigManager.Instance.OnChangeCharacter += ChangeTarget;
        }

        void LateUpdate()
        {
            if (target != null) transform.position = target.transform.position + offset;
        }

        void ChangeTarget(object sender, EventArgs e)
        {
            target = ConfigManager.Instance.Config.GetHead();
        }
    }
}