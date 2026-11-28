using UnityEngine;

namespace GameHub.Hub
{
    public static class HubLayout
    {
        public const float EntryZ = 20.6f;
        public const float DoorSill = 5.4f;

        public static readonly Vector3 HubEntry = new(0f, DoorSill, EntryZ);
        public static readonly Vector3 RoomEntry = new(0f, 0f, 20f);
    }
}
