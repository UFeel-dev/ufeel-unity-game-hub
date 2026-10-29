using System.Collections;
using System.Globalization;
using UFeel;
using UnityEngine;
using UnityEngine.UI;

public class DisplayEffects : MonoBehaviour
{
    public AudioSource AudioSource;
    public Image RedFilter;
    private const uint MinBpm = 60;
    private const uint MaxBpm = 180;
    private uint bpm = 0;
    private Coroutine actionCoroutine;

    private static uint GetBPM()
    {
        uint currentBPM = UFeelAPI.HeartRate.Current;

        if (currentBPM == 0)
            return currentBPM;

        if (currentBPM < MinBpm)
            return MinBpm;

        if (currentBPM > MaxBpm)
            return MaxBpm;

        return currentBPM;
    }

    private void Start()
    {
        UFeelDebugHUD.Clear();

        bpm = GetBPM();
        RedFilter.color = new Color(1, 0, 0, 0);

        actionCoroutine = StartCoroutine(RunAudio());
    }

    private IEnumerator RunAudio()
    {
        while (true)
        {
            bpm = GetBPM();
            Debug.Log("BPM = " + bpm.ToString(CultureInfo.InvariantCulture));

            if (bpm == 0)
                yield break;

            AudioSource.Play();

            float newAlpha = RedFilter.color.a == 0 ? 0.25f : 0;
            RedFilter.color = new Color(1, 0, 0, newAlpha);

            Debug.Log(RedFilter.color);

            float wait = 1.0f / bpm * MinBpm;
            yield return new WaitForSeconds(wait);
        }
    }

    private void OnDestroy()
    {
        StopCoroutine(actionCoroutine);
    }
}
