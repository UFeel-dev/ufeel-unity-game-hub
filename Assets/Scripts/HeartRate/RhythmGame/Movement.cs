using UnityEngine;

public class Movement : MonoBehaviour
{
    private static float LimitRotation(float rotation, float incrementor)
    {
        if ((rotation <= 90f && incrementor == -1f) ||
            (rotation >= 270f && incrementor == 1f))
        {
            return rotation;
        }

        return rotation + incrementor;
    }

    private void Update()
    {
        float direction = LayoutInput.GetHorizontalAxis();
        if (direction != 0f)
        {
            transform.rotation = Quaternion.Euler(0f, LimitRotation(transform.localEulerAngles.y, direction), 0f);
        }
    }
}
