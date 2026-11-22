using UnityEngine;

namespace GameHub.EscapeRoom
{
    public class VoiceWindowController : MonoBehaviour
    {
        [SerializeField] private Transform leftShutter;
        [SerializeField] private Transform rightShutter;

        [SerializeField] private float openLeftX = -7.5f;
        [SerializeField] private float openRightX = 7f;
        [SerializeField] private float closedLeftX = -2.85f;
        [SerializeField] private float closedRightX = 2.15f;
        [SerializeField] private float speed = 2f;

        private bool isOpen = true;

        private void Update()
        {
            MoveShutter(leftShutter, isOpen ? openLeftX : closedLeftX);
            MoveShutter(rightShutter, isOpen ? openRightX : closedRightX);
        }

        public void Close()
        {
            isOpen = false;
        }

        private void MoveShutter(Transform shutter, float targetX)
        {
            Vector3 position = shutter.localPosition;
            position.x = Mathf.Lerp(position.x, targetX, Time.deltaTime * speed);
            shutter.localPosition = position;
        }
    }
}
