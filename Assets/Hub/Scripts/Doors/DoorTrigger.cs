using GameHub.Common;
using UnityEngine;

namespace GameHub.Hub
{
    [RequireComponent(typeof(DoorController))]
    public class DoorTrigger : MonoBehaviour
    {
        public const string InProgressLabel = "In Progress";

        private static readonly Vector3 LoadedScenePosition = new(0f, 0f, 20f);
        private static readonly Color GizmoColor = new(0f, 1f, 0f, 0.25f);

        [SerializeField] private string targetSceneName;
        [Tooltip("Shows 'In Progress' on the door and keeps it closed. Also applied when the target scene is not in the build.")]
        [SerializeField] private bool inProgress;
        [SerializeField] private GameObject wallToDisable;

        private DoorController doorController;
        private bool playerInRange;

        public string TargetSceneName => targetSceneName;

        public bool InProgress => inProgress;

        public bool IsInProgress =>
            inProgress ||
            (!string.IsNullOrEmpty(targetSceneName) && !Application.CanStreamedLevelBeLoaded(targetSceneName));

        private void Awake()
        {
            doorController = GetComponent<DoorController>();
        }

        private void Start()
        {
            if (IsInProgress)
                doorController.ShowStatusLabel(InProgressLabel);
        }

        private void Update()
        {
            if (!playerInRange || SceneLoader.Instance.IsLoading || !LayoutInput.GetKeyDown(KeyCode.E))
                return;

            if (IsInProgress)
                return;

            if (string.IsNullOrEmpty(targetSceneName))
            {
                Debug.LogWarning($"{name} has no target scene.", this);
                return;
            }

            if (wallToDisable != null)
                wallToDisable.SetActive(false);

            SceneLoader.Instance.LoadAdditiveSceneAtPosition(targetSceneName, LoadedScenePosition, doorController.ToggleDoor);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(Tags.Player))
                playerInRange = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(Tags.Player))
                playerInRange = false;
        }

        private void OnDrawGizmos()
        {
            if (!TryGetComponent(out BoxCollider box) || !box.isTrigger)
                return;

            Gizmos.color = GizmoColor;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
            Gizmos.matrix = Matrix4x4.identity;
        }
    }
}
