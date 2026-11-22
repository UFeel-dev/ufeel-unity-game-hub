using UnityEngine;

namespace GameHub.Survivor
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float forwardSpeed = 6f;
        [SerializeField] private float lateralSpeed = 5f;

        private CharacterController controller;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if (SurvivorGameManager.Instance == null || !SurvivorGameManager.Instance.IsPlaying)
                return;

            Vector3 move = Vector3.forward * forwardSpeed;

            if (EyeInput.Instance != null && EyeInput.Instance.LeftPressed)
                move += Vector3.left * lateralSpeed;

            if (EyeInput.Instance != null && EyeInput.Instance.RightPressed)
                move += Vector3.right * lateralSpeed;

            controller.Move(move * Time.deltaTime);
        }
    }
}
