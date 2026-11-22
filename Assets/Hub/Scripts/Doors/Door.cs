using UnityEngine;
namespace DoorScript
{
    [RequireComponent(typeof(AudioSource))]

    public class Door : MonoBehaviour
    {
        public bool open;
        public float smooth = 1.0f;
        private readonly float doorOpenAngle = -90.0f;
        private readonly float doorCloseAngle;
        public AudioSource asource;
        public AudioClip openDoor;
        public AudioClip closeDoor;

        void Start()
        {
            asource = GetComponent<AudioSource>();
        }

        void Update()
        {
            if (open)
            {
                Quaternion target = Quaternion.Euler(0, doorOpenAngle, 0);
                transform.localRotation = Quaternion.Slerp(transform.localRotation, target, Time.deltaTime * 5 * smooth);
            }
            else
            {
                Quaternion target1 = Quaternion.Euler(0, doorCloseAngle, 0);
                transform.localRotation = Quaternion.Slerp(transform.localRotation, target1, Time.deltaTime * 5 * smooth);
            }
        }

        public void ToggleDoor()
        {
            open = !open;
            asource.clip = open ? openDoor : closeDoor;
            asource.Play();
        }
    }
}
