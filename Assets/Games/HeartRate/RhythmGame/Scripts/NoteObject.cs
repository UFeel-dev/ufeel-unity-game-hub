using System.Collections;
using GameHub.Common;
using UnityEngine;

namespace GameHub.RhythmGame
{
    public class NoteObject : MonoBehaviour
    {
        private const float SpawnHeight = 10.5f;
        private const int LaneChangeOdds = 11;
        private const float LaneMoveDuration = 0.5f;
        private const float LaneRotateDuration = 1f;

        private static readonly float[] LaneX = { -1.5f, -0.5f, 0.5f, 1.5f };
        private static readonly float[] LaneRotation = { 180f, -90f, 90f, 0f };
        private static readonly KeyCode[] LaneKey = { KeyCode.RightArrow, KeyCode.DownArrow, KeyCode.UpArrow, KeyCode.LeftArrow };

        [SerializeField] private KeyCode key;

        private int lane;
        private bool canBePressed;

        private void Start()
        {
            lane = System.Array.FindIndex(LaneX, x => Mathf.Approximately(x, transform.position.x));
        }

        private void Update()
        {
            if (!canBePressed || !LayoutInput.GetKeyDown(key))
                return;

            canBePressed = false;
            RhythmGameManager.Instance.NoteHit();
            Respawn();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag(Tags.Activator))
                canBePressed = true;

            if (collision.CompareTag(Tags.PerturbatorDetector) && Random.Range(0, LaneChangeOdds) == 0)
                ChangeLane();
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (!gameObject.activeInHierarchy || !collision.CompareTag(Tags.Activator))
                return;

            if (canBePressed)
            {
                canBePressed = false;
                RhythmGameManager.Instance.NoteMissed();
            }

            Respawn();
        }

        private static IEnumerator MoveTo(Transform target, Vector3 end, float duration)
        {
            for (float elapsed = 0f; elapsed < duration;)
            {
                elapsed += Time.deltaTime;
                target.position = Vector3.Lerp(target.position, end, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }

            target.position = end;
        }

        private static IEnumerator RotateTo(Transform target, Quaternion end, float duration)
        {
            for (float elapsed = 0f; elapsed < duration;)
            {
                elapsed += Time.deltaTime;
                target.rotation = Quaternion.Slerp(target.rotation, end, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }

            target.rotation = end;
        }

        private void Respawn()
        {
            lane = Random.Range(0, LaneX.Length);
            transform.SetPositionAndRotation(new Vector3(LaneX[lane], SpawnHeight, 0f), Quaternion.Euler(0f, 0f, LaneRotation[lane]));
            key = LaneKey[lane];
        }

        private void ChangeLane()
        {
            int newLane;
            do
            {
                newLane = Random.Range(0, LaneX.Length);
            }
            while (newLane == lane);

            lane = newLane;
            key = LaneKey[lane];
            StartCoroutine(MoveTo(transform, new Vector3(LaneX[lane], transform.position.y, transform.position.z), LaneMoveDuration));
            StartCoroutine(RotateTo(transform, Quaternion.Euler(0f, 0f, LaneRotation[lane]), LaneRotateDuration));
        }
    }
}
