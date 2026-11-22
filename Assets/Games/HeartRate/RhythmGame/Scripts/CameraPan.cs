using GameHub.Common;
using UnityEngine;

namespace GameHub.RhythmGame
{
    public class CameraPan : MonoBehaviour
    {
        private const float MinYaw = 90f;
        private const float MaxYaw = 270f;

        [SerializeField] private float degreesPerSecond = 60f;

        private void Update()
        {
            float direction = LayoutInput.GetHorizontalAxis(includeArrows: false);
            if (direction == 0f)
                return;

            float yaw = transform.localEulerAngles.y;
            if ((direction < 0f && yaw <= MinYaw) || (direction > 0f && yaw >= MaxYaw))
                return;

            transform.rotation = Quaternion.Euler(0f, yaw + (direction * degreesPerSecond * Time.deltaTime), 0f);
        }
    }
}
