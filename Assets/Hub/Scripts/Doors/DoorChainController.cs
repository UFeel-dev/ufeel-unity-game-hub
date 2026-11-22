using UnityEngine;

namespace GameHub.Hub
{
    public class DoorChainController : MonoBehaviour
    {
        [SerializeField] private Transform chain;
        [SerializeField] private Transform door;
        [SerializeField] private float chainExtensionSpeed = 5f;
        [SerializeField] private float maxChainLength = 5.9f;

        private float originalChainLength;

        public bool IsExtending { get; private set; }

        private void Start()
        {
            originalChainLength = chain.localScale.y;
        }

        private void Update()
        {
            float scaleDelta = chainExtensionSpeed * Time.deltaTime * 0.5f;
            Vector3 scale = chain.localScale;
            float targetScaleY = IsExtending
                ? Mathf.Min(scale.y + scaleDelta, maxChainLength)
                : Mathf.Max(scale.y - scaleDelta, originalChainLength);

            float appliedDelta = targetScaleY - scale.y;
            if (appliedDelta == 0f)
                return;

            chain.localScale = new Vector3(scale.x, targetScaleY, scale.z);
            chain.position += Vector3.down * appliedDelta;
            door.position += Vector3.down * (appliedDelta * 2f);
        }

        public void ToggleExtension()
        {
            IsExtending = !IsExtending;
        }
    }
}
