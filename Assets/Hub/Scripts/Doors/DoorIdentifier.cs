using UnityEngine;

namespace GameHub.Hub
{
    public class DoorIdentifier : MonoBehaviour
    {
        [SerializeField] private int doorId;
        [SerializeField] private string doorName;

        public int DoorId => doorId;

        public string DoorName => doorName;
    }
}
