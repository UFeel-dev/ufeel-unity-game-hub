using GameHub.Common;
using UnityEngine;

namespace GameHub.Maze
{
    public class FogTrigger : MonoBehaviour
    {
        [SerializeField] private Direction direction;
        [SerializeField] private float checkInterval = 0.5f;

        private RoomController room;
        private float timer;

        private void Awake()
        {
            room = GetComponentInParent<RoomController>();
            if (room == null)
                Debug.LogError($"No RoomController found for fog {name}.", this);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (room != null && other.CompareTag(Tags.Adventurer))
                MazeManager.Instance.TryMove(room, direction);
        }

        private void OnTriggerStay(Collider other)
        {
            if (room == null || !other.CompareTag(Tags.Adventurer))
                return;

            timer += Time.deltaTime;
            if (timer < checkInterval)
                return;

            timer = 0f;
            MazeManager.Instance.TryMove(room, direction);
        }
    }
}
