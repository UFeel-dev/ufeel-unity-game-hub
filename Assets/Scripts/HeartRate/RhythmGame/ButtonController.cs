using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ButtonController : MonoBehaviour
{
    private SpriteRenderer sr;
    public Sprite Image;
    public Sprite PressedImage;
    public KeyCode KeyToPress;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (LayoutInput.GetKeyDown(KeyToPress))
        {
            sr.sprite = PressedImage;
        }

        if (LayoutInput.GetKeyUp(KeyToPress))
        {
            sr.sprite = Image;
        }
    }
}
