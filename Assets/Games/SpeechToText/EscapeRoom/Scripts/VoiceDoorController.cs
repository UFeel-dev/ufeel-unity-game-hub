using UnityEngine;

namespace GameHub.EscapeRoom
{
    public class VoiceDoorController : MonoBehaviour
    {
        [SerializeField] private Transform pivot;
        [SerializeField] private float openAngle = 90f;
        [SerializeField] private float speed = 2f;

        private bool isOpen;

        private void Update()
        {
            Quaternion target = Quaternion.Euler(0f, isOpen ? openAngle : 0f, 0f);
            pivot.localRotation = Quaternion.Lerp(pivot.localRotation, target, Time.deltaTime * speed);
        }

        public void Open()
        {
            isOpen = true;
        }
    }
}
