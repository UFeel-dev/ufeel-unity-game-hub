using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    private bool playerInRange;
    private DoorController _doorController;

    [Tooltip("Name of the scene to load additively.")]
    public string targetSceneName;

    [Tooltip("Shows 'In Progress' on the door and keeps it closed. Also applied when the target scene is not in the build.")]
    public bool inProgress;

    [Tooltip("The wall GameObject (quad) to disable when loading the new scene.")]
    public GameObject wallToDisable;

    public const string InProgressLabel = "In Progress";

    public bool IsInProgress =>
        inProgress ||
        (!string.IsNullOrEmpty(targetSceneName) && !Application.CanStreamedLevelBeLoaded(targetSceneName));

    private void Start()
    {
        if (TryGetComponent(out DoorController doorController))
        {
            _doorController = doorController;
        }
        else
        {
            Debug.LogError("DoorController component is missing on this GameObject.");
            return;
        }

        if (IsInProgress)
        {
            _doorController.ShowStatusLabel(InProgressLabel);
        }
    }

    private void Update()
    {
        if (SceneLoader.Instance.IsLoading()) return;

        if (playerInRange && LayoutInput.GetKeyDown(KeyCode.E))
        {
            if (IsInProgress)
            {
                return;
            }

            if (string.IsNullOrEmpty(targetSceneName))
            {
                Debug.LogWarning("Target scene name is not set on DoorController.");
                return;
            }

            if (wallToDisable != null)
            {
                wallToDisable.SetActive(false);
            }
            else
            {
                Debug.LogWarning("Wall to disable is not set.");
            }

            // SceneLoader.Instance.LoadSceneAsync(targetSceneName, 3.0f);

            SceneLoader.Instance.LoadAdditiveSceneAtPosition(
                targetSceneName,
                new Vector3(0.0f, 0.0f, 20f),
                () =>
                {
                    _doorController.ToggleDoor();
                }
            );

            // TODO: start loading when the door touches the ground
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    private void OnDrawGizmos()
    {
        if (TryGetComponent(out BoxCollider box) && box.isTrigger)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
            Gizmos.matrix = Matrix4x4.identity;
        }
    }
}

// TODO: clean public -» check good practive between private SerializedField and public

// TODO: reorder architecture, put everything where it should be
