using System.Collections;
using UnityEngine;

public class NoteObject : MonoBehaviour
{
    public KeyCode KeyToPress;
    private bool canBePressed = false;
    private int index = 0;
    private float[] orientations = { 180f, -90f, 90f, 0f };
    private float[] positionsX = { -1.5f, -0.5f, 0.5f, 1.5f };
    private KeyCode[] keys = { KeyCode.RightArrow, KeyCode.DownArrow, KeyCode.UpArrow, KeyCode.LeftArrow };

    private void Start()
    {
        index = System.Array.FindIndex(positionsX, v => Mathf.Approximately(v, transform.position.x));
    }

    private void ManageNote(int randomX, bool hit)
    {
        if (!hit && canBePressed)
        {
            GameManager.Instance.NoteMissed();
            canBePressed = false;
        }

        transform.SetPositionAndRotation(
            new Vector3(positionsX[0] + randomX, 10.5f, 0f),
            Quaternion.Euler(0f, 0f, orientations[randomX])
        );
        KeyToPress = keys[randomX];
    }

    private void Update()
    {
        if (LayoutInput.GetKeyDown(KeyToPress) && canBePressed)
        {
            GameManager.Instance.NoteHit();
            canBePressed = false;
            ManageNote(Random.Range(0, 4), true);
        }
    }

    private static IEnumerator MoveTo(Transform start, Vector3 end, float duration)
    {
        for (float elapsed = 0f; elapsed < duration;)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / duration);
            start.position = Vector3.Lerp(start.position, end, alpha);
            yield return null;
        }
        start.position = end;
    }

    private static IEnumerator RotateTo(Transform start, Quaternion end, float duration)
    {
        for (float elapsed = 0f; elapsed < duration;)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / duration);
            start.rotation = Quaternion.Slerp(start.rotation, end, alpha);
            yield return null;
        }
        start.rotation = end;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Activator"))
        {
            canBePressed = true;
        }

        if (!collision.CompareTag("PerturbatorDetector"))
        {
            return;
        }
        if (Random.Range(0, 11) == 0)
        {
            int rc = 0;

            do
            {
                rc = Random.Range(0, 4);
            }
            while (rc == index);

            StartCoroutine(MoveTo(transform, new Vector3(positionsX[rc], transform.position.y, transform.position.z), 0.5f));
            StartCoroutine(RotateTo(transform, Quaternion.Euler(0f, 0f, orientations[rc]), 1f));
            KeyToPress = keys[rc];
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (gameObject.activeInHierarchy && collision.CompareTag("Activator"))
        {
            ManageNote(Random.Range(0, 4), false);
        }
    }
}
