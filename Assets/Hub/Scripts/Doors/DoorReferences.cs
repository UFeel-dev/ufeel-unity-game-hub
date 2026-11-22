using UnityEngine;

namespace GameHub.Hub
{
    public class DoorReferences : MonoBehaviour
    {
        [SerializeField] private DoorTrigger trigger;
        [SerializeField] private DoorController controller;

        public DoorTrigger Trigger => trigger;

        public DoorController Controller => controller;
    }
}
