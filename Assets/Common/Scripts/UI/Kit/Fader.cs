using UnityEngine;

namespace GameHub.Common
{
    public sealed class Fader
    {
        public const float DefaultSeconds = 0.35f;

        private float target;
        private float seconds;

        public float Value { get; private set; }

        public void To(bool visible, float duration)
        {
            target = visible ? 1f : 0f;
            seconds = duration;
            if (duration <= 0f)
                Value = target;
        }

        public void Tick(float deltaTime)
        {
            Value = seconds > 0f ? Mathf.MoveTowards(Value, target, deltaTime / seconds) : target;
        }
    }
}
