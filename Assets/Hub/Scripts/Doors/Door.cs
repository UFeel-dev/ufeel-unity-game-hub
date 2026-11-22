using UnityEngine;

namespace GameHub.Hub
{
    [RequireComponent(typeof(AudioSource))]
    public class Door : MonoBehaviour
    {
        private const float OpenAngle = -90f;
        private const float ClosedAngle = 0f;
        private const float BaseSpeed = 5f;

        [SerializeField] private float speed = 1f;
        [SerializeField] private AudioClip openClip;
        [SerializeField] private AudioClip closeClip;

        private AudioSource audioSource;
        private bool isOpen;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        private void Update()
        {
            Quaternion target = Quaternion.Euler(0f, isOpen ? OpenAngle : ClosedAngle, 0f);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, target, Time.deltaTime * BaseSpeed * speed);
        }

        public void Toggle()
        {
            isOpen = !isOpen;
            audioSource.clip = isOpen ? openClip : closeClip;
            audioSource.Play();
        }
    }
}
