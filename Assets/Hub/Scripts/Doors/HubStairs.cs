using System.Collections.Generic;
using UnityEngine;

namespace GameHub.Hub
{
    public sealed class HubStairs
    {
        private const float StepSeconds = 0.35f;
        private const float StepStagger = 0.05f;
        private const float SinkMargin = 0.05f;
        private const float CarryTolerance = 0.1f;

        private readonly List<Transform> steps = new();
        private readonly List<float> raisedHeights = new();
        private readonly List<Collider> walkways = new();
        private float[] previousTops = new float[0];
        private float progress;
        private float target;

        public HubStairs(Transform root)
        {
            foreach (Transform child in root)
            {
                if (child.TryGetComponent(out MeshRenderer _))
                {
                    steps.Add(child);
                    raisedHeights.Add(child.localPosition.y);
                }
                else if (child.TryGetComponent(out Collider walkway))
                {
                    walkways.Add(walkway);
                }
            }

            previousTops = new float[steps.Count];
            Apply();
            SetWalkable(false);
        }

        public bool IsHidden => progress <= 0f;

        private float TotalSeconds => StepSeconds + (StepStagger * Mathf.Max(0, steps.Count - 1));

        public void Tick(float deltaTime, CharacterController? rider)
        {
            if (Mathf.Approximately(progress, target))
                return;

            for (int i = 0; i < steps.Count; i++)
                previousTops[i] = Top(steps[i]);

            progress = Mathf.MoveTowards(progress, target, deltaTime / TotalSeconds);
            Apply();
            if (rider != null && rider.enabled)
                Carry(rider);

            if (progress >= 1f)
                SetWalkable(true);
        }

        public void Rise()
        {
            target = 1f;
        }

        public void Sink()
        {
            target = 0f;
            SetWalkable(false);
        }

        private void Apply()
        {
            float elapsed = progress * TotalSeconds;
            for (int i = 0; i < steps.Count; i++)
            {
                Transform step = steps[i];
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((elapsed - (i * StepStagger)) / StepSeconds));
                float hidden = raisedHeights[i] - step.localScale.y - SinkMargin;
                Vector3 position = step.localPosition;
                step.localPosition = new Vector3(position.x, Mathf.Lerp(hidden, raisedHeights[i], t), position.z);
                step.gameObject.SetActive(t > 0f);
            }
        }

        private static float Top(Transform step)
        {
            return step.position.y + (step.lossyScale.y * 0.5f);
        }

        private void Carry(CharacterController rider)
        {
            Bounds body = rider.bounds;
            float foot = body.min.y;
            float lift = 0f;
            for (int i = 0; i < steps.Count; i++)
            {
                Transform step = steps[i];
                if (!step.gameObject.activeSelf)
                    continue;

                Vector3 center = step.position;
                Vector3 half = step.lossyScale * 0.5f;
                bool under = body.max.x > center.x - half.x && body.min.x < center.x + half.x
                    && body.max.z > center.z - half.z && body.min.z < center.z + half.z;
                float top = Top(step);
                if (under && top > foot && foot >= previousTops[i] - CarryTolerance)
                    lift = Mathf.Max(lift, top - foot);
            }

            if (lift <= 0f)
                return;

            rider.enabled = false;
            rider.transform.position += Vector3.up * lift;
            rider.enabled = true;
        }

        private void SetWalkable(bool walkable)
        {
            foreach (Collider walkway in walkways)
                walkway.enabled = walkable;
        }
    }
}
