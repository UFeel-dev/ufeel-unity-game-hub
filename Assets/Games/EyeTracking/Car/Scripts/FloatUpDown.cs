using UnityEngine;

public class FloatUpDown : MonoBehaviour
{
    public float Amplitude = 0.5f;
    public float Frequency = 1f;

    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        transform.position = startPos + (Amplitude * Mathf.Sin(Time.time * Frequency) * Vector3.up);
    }
}
