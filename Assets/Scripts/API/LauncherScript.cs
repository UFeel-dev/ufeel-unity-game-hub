using UnityEditor;
using UnityEngine;

public class LauncherScript : MonoBehaviour
{
    private int currentStep = 0;

    private void Start()
    {
        ApplyStep();
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.N))
        {
            currentStep++;
            currentStep = Mathf.Clamp(currentStep, 0, 6);

            ApplyStep();
        }

        if (Input.GetKeyUp(KeyCode.B))
        {
            currentStep--;
            currentStep = Mathf.Clamp(currentStep, 0, 6);

            ApplyStep();
        }
    }

    private static void StopUnity()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void ApplyStep()
    {
        switch (currentStep)
        {
            case 0:
                break;

            case 1:
                Debug.Log("=== Eye Tracking Detection ===");
                break;

            case 2:
                Debug.Log("=== Emotion Detection ===");
                break;

            case 3:
                Debug.Log("=== Speech Detection ===");
                break;

            case 4:
                Debug.Log("=== Heart Rate Detection ===");
                break;

            case 5:
                break;

            case 6:
                Debug.Log("=== End ===");
                StopUnity();
                break;
        }
    }
}