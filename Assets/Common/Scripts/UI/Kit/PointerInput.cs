using UnityEngine;
using UnityEngine.InputSystem;

namespace GameHub.Common
{
    public sealed class PointerInput
    {
        private const float MoveThreshold = 0.5f;

        public Vector2 Screen { get; private set; }

        public bool Moved { get; private set; }

        public bool Pressed { get; private set; }

        public bool Held { get; private set; }

        public float Scroll { get; private set; }

        public static void ShowCursor(bool visible)
        {
            Cursor.visible = visible;
            Cursor.lockState = CursorLockMode.None;
        }

        public void Tick()
        {
            Mouse? mouse = Mouse.current;
            if (mouse == null)
            {
                Moved = false;
                Pressed = false;
                Held = false;
                Scroll = 0f;
                return;
            }

            Vector2 position = mouse.position.ReadValue();
            Moved = (position - Screen).sqrMagnitude > MoveThreshold * MoveThreshold;
            Screen = position;
            Pressed = mouse.leftButton.wasPressedThisFrame;
            Held = mouse.leftButton.isPressed;
            Scroll = mouse.scroll.ReadValue().y;
        }

        public bool IsOver(RectTransform rect)
        {
            return rect.gameObject.activeInHierarchy && RectTransformUtility.RectangleContainsScreenPoint(rect, Screen, null);
        }

        public Vector2 LocalIn(RectTransform rect)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, Screen, null, out Vector2 local);
            return local;
        }
    }
}
