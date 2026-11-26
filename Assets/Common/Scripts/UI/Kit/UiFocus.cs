using System.Collections.Generic;
using UnityEngine;

namespace GameHub.Common
{
    public static class UiFocus
    {
        private static readonly List<object> Owners = new();
        private static int changedFrame = -1;

        public static bool IsFree => Owners.Count == 0 && Time.frameCount != changedFrame;

        public static bool IsCaptured => Owners.Count > 0;

        public static void Capture(object owner)
        {
            if (Owners.Contains(owner))
                return;
            Owners.Add(owner);
            changedFrame = Time.frameCount;
        }

        public static void Release(object owner)
        {
            if (Owners.Remove(owner))
                changedFrame = Time.frameCount;
        }

        public static bool IsTop(object owner)
        {
            return Owners.Count > 0 && Owners[^1] == owner;
        }

        public static bool Has(object owner)
        {
            return IsTop(owner) && Time.frameCount != changedFrame;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            Owners.Clear();
            changedFrame = -1;
        }
    }
}
