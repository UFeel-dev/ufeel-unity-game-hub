using GameHub.Common;
using UnityEngine;

namespace GameHub.RhythmGame
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class ButtonController : MonoBehaviour
    {
        [SerializeField] private Sprite idleSprite;
        [SerializeField] private Sprite pressedSprite;
        [SerializeField] private KeyCode key;

        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            if (LayoutInput.GetKeyDown(key))
                spriteRenderer.sprite = pressedSprite;

            if (LayoutInput.GetKeyUp(key))
                spriteRenderer.sprite = idleSprite;
        }
    }
}
