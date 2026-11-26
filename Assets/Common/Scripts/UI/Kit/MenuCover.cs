using UnityEngine;

namespace GameHub.Common
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class MenuCover : MonoBehaviour
    {
        private const float FadeSpeed = 8f;

        private CanvasGroup group;

        private void Awake()
        {
            group = GetComponent<CanvasGroup>();
        }

        private void LateUpdate()
        {
            group.alpha = Mathf.MoveTowards(group.alpha, UiFocus.IsCaptured ? 0f : 1f, FadeSpeed * Time.unscaledDeltaTime);
        }
    }
}
