using GameHub.Common;
using UnityEngine;

namespace GameHub.RhythmGame
{
    public class BeatScroller : MonoBehaviour
    {
        private const float UnitsPerBeat = 4f;

        public uint Bpm { get; set; } = HeartRateTempo.MinBpm;

        public bool IsScrolling { get; set; }

        private void Update()
        {
            if (IsScrolling)
                transform.position += Vector3.down * (Bpm / 60f * UnitsPerBeat * Time.deltaTime);
        }
    }
}
