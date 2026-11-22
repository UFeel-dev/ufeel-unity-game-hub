using UnityEngine;

public class FogTrigger : MonoBehaviour
{
    [Header("Fog Settings")]
    public Direction Direction;

    private RoomController room;

    private void Awake()
    {
        room = GetComponentInParent<RoomController>();

        if (room == null)
        {
            Debug.LogError(
                $"No RoomController found for fog {gameObject.name}"
            );
        }
    }

    private float timer;
    [SerializeField] private float checkInterval = 0.5f;

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Adventurer"))
            return;

        timer += Time.deltaTime;

        if (timer >= checkInterval)
        {
            timer = 0f;
            MazeManager.Instance.TryMove(room, Direction);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Adventurer"))
            return;

        if (room == null)
            return;

        MazeManager.Instance.TryMove(room, Direction);
    }
}
