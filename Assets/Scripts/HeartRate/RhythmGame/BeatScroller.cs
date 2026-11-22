using UnityEngine;

public class BeatScroller : MonoBehaviour
{
    public float BeatTempo;
    public uint Bpm;
    public bool Started;

    private void Start()
    {
        BeatTempo = 2;
        Bpm = 60;
    }

    private void Update()
    {
        if (Started)
        {
            BeatTempo = Bpm / 15f; // 60 / 4
            transform.position -= new Vector3(0f, BeatTempo * Time.deltaTime, 0f);
        }
    }
}
