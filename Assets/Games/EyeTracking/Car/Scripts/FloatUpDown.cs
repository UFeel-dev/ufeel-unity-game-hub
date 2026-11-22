using UnityEngine;

namespace GameHub.Car
{
    public class FloatUpDown : MonoBehaviour
    {
        [SerializeField] private float amplitude = 0.5f;
        [SerializeField] private float frequency = 1f;

        private Vector3 startPosition;

        private void Start()
        {
            startPosition = transform.position;
        }

        private void Update()
        {
            transform.position = startPosition + (amplitude * Mathf.Sin(Time.time * frequency) * Vector3.up);
        }
    }
}
